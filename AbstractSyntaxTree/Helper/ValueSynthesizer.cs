using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GStringCompiler
{
    class ValueSynthesizer
    {
        public static void DeclareLocalVarValueSynthesize(ref DeclareLocalVar declareLocal, ref List<Stmt> stmtList, ref int index, IList<GStringObject> tokens)
        {
            // Read from user.
            if (tokens[index].Object.Equals("Read"))
            {
                Parser.Getinstance().mainDeclaredLocalVar[Parser.Getinstance().CurrentSynMethod].Add(declareLocal);
                index--;
                stmtList.Add(BracketReadOperation.Synthesize(ref index, tokens, declareLocal));
            }

            // Check bracket, read while instance or array.
            else if ((declareLocal.Expr is LocalArray) && (declareLocal.Expr as LocalArray) != null)
            {
                (declareLocal.Expr as LocalArray).InstanceElement(ref index, tokens);
                if ((declareLocal.Expr as LocalArray).Element.Count == 0 || index == tokens.Count)
                    throw new Error("wrong initial value for array: " + (declareLocal.Expr as LocalArray).Ident + " [At line: " + tokens[index].Line + "]");
                index++;
            }

            else if (tokens[index].Object is LogicalSymbol)
            {
                LogicalSymbol ls = tokens[index].Object as LogicalSymbol;
                if (ls.Symbol == "!")
                {
                    index++;
                    var expr = OrderController.GetSequenceOfOperation(ref index, tokens, declareLocal.FixedType);
                    if (tokens[index].Object != Scanner.Right_Braces || index == tokens.Count)
                        throw new Error("require }[Right braces] after'" + tokens[index - 1].Object + "' [At line: " + tokens[index].Line + "]");
                    declareLocal.Expr = new NotSymbolSequenceExpr { Value = expr.Value};
                    index++;
                }
                else if (ls.Symbol == "-")//use as normal
                {
                    declareLocal.Expr = OrderController.GetSequenceOfOperation(ref index, tokens, declareLocal.FixedType);
                    if (tokens[index].Object != Scanner.Right_Braces || index == tokens.Count)
                        throw new Error("require }[Right braces] after'" + tokens[index - 1].Object + "' [At line: " + tokens[index].Line + "]");
                    index++;
                }
            }

            // Sequence value.
            else if (OrderController.IsSeqExpr(index, tokens))
            {
                declareLocal.Expr = OrderController.GetSequenceOfOperation(ref index, tokens,declareLocal.FixedType);
                if (tokens[index].Object != Scanner.Right_Braces || index == tokens.Count)
                    throw new Error("require }[Right braces] after'" + tokens[index - 1].Object + "' [At line: " + tokens[index].Line + "]");
                index++;
            }

            // Normal value of unfixed(var) type.
            else if (declareLocal.FixedType == null || tokens[index].Object is string)
            {
                declareLocal.Expr = Parser.Getinstance().ParseExpr();
                if (tokens[index].Object != Scanner.Right_Braces || index == tokens.Count)
                    throw new Error("require }[Right braces] after'" + tokens[index - 1].Object + "' [At line: " + tokens[index].Line + "]");
                index++;
            }

            // Normal value of fixed type.
            else if (declareLocal.FixedType == tokens[index].Object.GetType() || tokens[index].Object is string)
            {
                declareLocal.Expr = Parser.Getinstance().ParseExpr();
                if (tokens[index].Object != Scanner.Right_Braces || index == tokens.Count)
                    throw new Error("require }[Right braces] after'" + tokens[index - 1].Object + "' [At line: " + tokens[index].Line + "]");
                index++;
            }
            else
                throw new Error("value of '" + declareLocal.Ident + "' is not match with type '" + declareLocal.FixedType.ToString() + "' [At line: " + tokens[index].Line + "]");
        }

        public static void AssignValueSynthesize(Assign assign, List<Stmt> stmtList, ref int index, IList<GStringObject> tokens)
        {
            // Read input from user.
            if (tokens[index].Object.Equals("Read"))
            {
                index--;
                stmtList.Remove(assign);
                stmtList.Add(BracketReadOperation.Synthesize(ref index, tokens, assign));
            }
        }
    }
}
