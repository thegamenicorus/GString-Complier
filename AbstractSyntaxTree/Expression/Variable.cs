using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GStringCompiler
{
    // <ident> := <char> <ident_rest>*
    // <ident_rest> := <char> | <digit>
    public class Variable : VarDecExpr
    {
        public string Ident { get; set; }
        public List<Ornament> Ornaments { get; set; }
        public Variable() { }

        public static List<Ornament> FindVariableOrnament(ref int index, IList<GStringObject> tokens, List<Ornament> ornament, string ident = "", Type refType = null)
        {
            while (tokens[index].Object != Scanner.Right_Braces)
            {
                index++;
                if (tokens[index].Object == Scanner.Comma) { }
                else if (tokens[index].Object == Scanner.Right_Braces) break;
                else if (tokens[index].Object.Equals("position"))
                {
                    if (!TypeIdentifier.IsGStringCollection(ident, tokens[index].Line))
                        throw new Error("keyword 'position' is not require for variable: " + ident + " [At line: " + tokens[index].Line + "]");

                    index++;
                    if (tokens[index].Object != Scanner.Colon)
                        throw new Error("require :[Colon] after 'position' [At line: " + tokens[index].Line + "]");

                    index++;
                    if (tokens[index].Object != Scanner.Left_Braces)
                        throw new Error("require {[Left Braces] after :[Colon] [At line: " + tokens[index].Line + "]");

                    ornament.Add(InstanceVariableIndex(ref index, tokens, ident));
                }
                else if (tokens[index].Object.Equals("call"))
                {
                    index++;
                    if (tokens[index].Object.Equals("method"))
                        ornament.Add((Ornament)CallMethod.Synthesize(ref index, tokens, true, true, refType));
                    else
                        throw new Error("'call " + tokens[index].Object + "' is undefined [At line: " + tokens[index].Line + "]");
                    index--;
                }
                else
                {
                    index--;
                    return ornament;
                }
            }
            if (ornament.Count > 0)
                index++;
            return ornament;
        }

        private static VariableIndex InstanceVariableIndex(ref int index, IList<GStringObject> tokens, string ident)
        {
            VariableIndex variableIndex = new VariableIndex();
            while (tokens[index].Object != Scanner.Right_Braces)
            {
                index++;
                if (tokens[index].Object == Scanner.Comma) { }
                else if (tokens[index].Object == Scanner.Right_Braces)
                    break;
                variableIndex.Index.Add(OrderController.GetSequenceOfOperation(ref index, tokens, new object[] { Scanner.Right_SquareBracket }, null));
            }
            return variableIndex;
        }
    }
}
