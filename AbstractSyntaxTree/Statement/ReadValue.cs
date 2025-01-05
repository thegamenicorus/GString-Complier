using Collections = System.Collections.Generic;
using System;
using System.Collections.Generic;

namespace GStringCompiler
{
    class ReadValue : Stmt, Expr
    {
        public string Ident;
        public Type ReadType;
        public System.Collections.Generic.List<object> Index { get; set; }

        public static ReadValue Synthesize(ref int index, Collections.IList<object> tokens)
        {
            ReadValue readVal = new ReadValue() { ReadType = typeof(string) };

            return readVal;
        }

        public static List<Stmt> Synthesize(ref int index, IList<GStringObject> tokens, Stmt stmt, bool isCheckedSyntax = true)
        {
            List<Stmt> stmtList = new List<Stmt>();
            ReadValue readVal = new ReadValue(); ;

            // Check input.
            if (!isCheckedSyntax)
            {
                index++;
                if (!tokens[index].Object.Equals("input"))
                    throw new Error("require 'input' after 'Read' [At line: " + tokens[index].Line + "]");
            }

            if (stmt is DeclareLocalVar)
            {
                DeclareLocalVar declareLocalVar = stmt as DeclareLocalVar;
                readVal = new ReadValue { Ident = declareLocalVar.Ident };
                stmtList.Add(readVal);
                index++;
                if (declareLocalVar.FixedType != null)
                {
                    if (declareLocalVar.FixedType == typeof(System.Text.StringBuilder))
                        readVal.ReadType = typeof(string);
                    else if (declareLocalVar.FixedType == typeof(int))
                        readVal.ReadType = typeof(int);
                    else
                    {
                        throw new Error("type '" + declareLocalVar.FixedType.ToString() + "' was not support to use Read from user function [At line: " + tokens[index].Line + "]");
                    }
                }
                else
                {
                    declareLocalVar.FixedType = typeof(System.Text.StringBuilder);
                    readVal.ReadType = typeof(string);
                }
            }
            else if (stmt is Assign)
            {
                Assign assign = stmt as Assign;
                readVal = new ReadValue { Ident = assign.Ident, Index = assign.Index, ReadType = assign.TypeOfReferencedVar };
                stmtList.Add(readVal);
                index++;
            }
            // Read without save.
            else if (stmt == null)
            {
                readVal = new ReadValue();
                stmtList.Add(readVal);
                index++;
            }
            if (tokens[index].Object == Scanner.Comma)
            {
                index++;
                Display display = Display.SynthesizeSpecialRead(ref index, ref tokens);
                stmtList.Insert(0, display);
                if (display.Expr is Variable)
                    if (((display.Expr) as Variable).Ornaments.Count > 0)
                        index--;

            }
            return stmtList;
        }
    }
}
