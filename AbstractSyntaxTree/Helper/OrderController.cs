using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GStringCompiler.Extension;

namespace GStringCompiler
{
    class OrderController
    {
        public static AddSymbol addSymbol = new AddSymbol();
        public static SubSymbol subSymbol = new SubSymbol();
        public static MulSymbol mulSymbol = new MulSymbol();
        public static DivSymbol divSymbol = new DivSymbol();
        public static ModSymbol modSymbol = new ModSymbol();
        public static bool isNonNumber = false;

        public static bool IsSeqExpr(int index, IList<GStringObject> tokens)
        {
            return IsSeqExpr(index, tokens, new object[] { });
        }

        public static bool IsSeqExpr(int index, IList<GStringObject> tokens, object stopper)
        {
            return IsSeqExpr(index, tokens, new object[] { stopper });
        }

        public static bool IsSeqExpr(int index, IList<GStringObject> tokens, object[] stopper, bool isCallMethodIdentity = false)
        {
            if (tokens[index - 1].Object == Scanner.Comma || tokens[index - 1].Object == Scanner.Left_Bracket)
                return true;
            else if (tokens[index].Object is LogicalSymbol)
            {
                LogicalSymbol ls = tokens[index].Object as LogicalSymbol;
                if (ls.Symbol == "!")
                {
                    return false;
                }
            }
            index++;
            if (tokens[index].Object == Scanner.Right_Braces || tokens[index + 1].Object == Scanner.Right_Braces || stopper.Contains(tokens[index + 1].Object)
                || stopper.Contains(tokens[index].Object))
                return false;
            return true;
        }

        public static SequenceExpr GetSequenceOfOperation(ref int index, IList<GStringObject> tokens, Type fixedType)
        {
            SequenceExpr sExpr = new SequenceExpr();
            sExpr.Value = GetInSquare(new List<Expr>(), ref index, tokens, new object[] { Scanner.Right_Braces }, fixedType);
            return sExpr;
        }

        public static SequenceExpr GetSequenceOfOperation(ref int index, IList<GStringObject> tokens, object stopper, Type fixedType)
        {
            SequenceExpr sExpr = new SequenceExpr();
            sExpr.Value = GetInSquare(new List<Expr>(), ref index, tokens, new object[] { stopper }, fixedType);
            return sExpr;
        }

        public static SequenceExpr GetSequenceOfOperation(ref int index, IList<GStringObject> tokens, object[] stopper, Type fixedType)
        {
            SequenceExpr sExpr = new SequenceExpr();
            sExpr.Value = GetInSquare(new List<Expr>(), ref index, tokens, stopper, fixedType);
            return sExpr;
        }

        private static TreeNode GetInSquare(List<Expr> expr, ref int index, IList<GStringObject> tokens, object[] stopper, Type fixedType)
        {
            isNonNumber = false;
            bool isWillUnbalanceParentheses = false;
            List<Expr> queue = expr;
            Stack<object> stack = new Stack<object>();
            Type type = null;
            do
            {
                isWillUnbalanceParentheses = false;
                bool isNextRequire = true;
                bool isNoMoreQueue = false;
                if (tokens[index].Object is int || tokens[index].Object is string || tokens[index].Object is StringBuilder)
                {
                    isNextRequire = false;
                    if (tokens[index].Object is StringBuilder)
                        isNonNumber = true;
                    else if (tokens[index].Object is string)
                    {
                        if (tokens[index].Object.Equals("Call"))
                        {
                            isNonNumber = false;
                            isNoMoreQueue = true;
                            // Continue Call methid HERE.

                            queue.Add(Parser.Getinstance().ParseExpr(ref index));
                        }
                        else if (tokens[index].Object.Equals("Read"))
                        {
                            isNoMoreQueue = true;
                            // Read input, "text": add element in to queue.
                            var read = ReadValue.Synthesize(ref index, tokens, null, false);
                            read.ForEach(readExpr => queue.Add((Expr)readExpr));
                            if (read.Where(x => x is Display).Count() > 0)
                                queue.Add(new EmptySymbol());
                        }
                        else
                        {
                            type = GSType.GetTypeByTypeText(tokens[index].Object.ToString());
                            if (type != null)
                            {
                                isNonNumber = type == typeof(string);
                            }
                            else
                                isNonNumber = TypeIdentifier.FindTypeOfVariable((string)tokens[index].Object, tokens[index].Line) == typeof(string);
                        }
                        if (!isNonNumber && (fixedType == typeof(StringBuilder) || fixedType == typeof(string)))
                        {
                            if (stack.Contains(Scanner.Left_Bracket))
                            {
                                if (tokens[index].Object is string)
                                {
                                    isWillUnbalanceParentheses = !TypeIdentifier.IsGStringCollection(tokens[index].Object.ToString(), tokens[index].Line);
                                }
                                if (OrderController.IsSeqExpr(index, tokens))
                                {
                                    OptStrSequenceExpr ext = new OptStrSequenceExpr { Value = OrderController.GetSequenceOfOperation(ref index, tokens, new object[] { Scanner.Right_Bracket }, null).Value };
                                    queue.Add(ext);
                                    isNoMoreQueue = true;
                                }
                            }
                            isNonNumber = true;
                        }
                    }
                    if (!isNoMoreQueue)
                    {
                        queue.Add(Parser.Getinstance().ParseExpr(ref index));
                        if (isWillUnbalanceParentheses)
                            index--;
                    }
                }
                else if (tokens[index].Object == Scanner.Comma) { }
                else if (tokens[index].Object == Scanner.FullStop) { index--; isNextRequire = false; }
                else if (tokens[index].Object == Scanner.Left_Bracket)
                    stack.Push(tokens[index].Object);
                else if (tokens[index].Object is LogicalSymbol)
                {
                    if (tokens[index].Object == Scanner.Sub)
                    {
                        if (!tokens[index - 1].Object.IsNumeric() && tokens[index - 1].Object != Scanner.Right_Braces && tokens[index - 1].Object != Scanner.Right_Bracket && !(tokens[index - 1].Object is string))
                        // Left braces for function.
                        {
                            tokens[index].Object = Scanner.Neg;
                        }
                    }
                    bool repeat;
                    do
                    {
                        repeat = false;
                        if (stack.Count == 0)
                            stack.Push(tokens[index].Object);
                        else if (stack.Peek() == Scanner.Left_Bracket)
                            stack.Push(tokens[index].Object);
                        else if (Precedence(tokens[index].Object) > Precedence(stack.Peek()))
                            stack.Push(tokens[index].Object);
                        else
                        {
                            queue.Add(ConvertToExpr(stack.Pop()));
                            repeat = true;
                        }
                    } while (repeat);
                }
                else if (tokens[index].Object == Scanner.Right_Bracket)
                {
                    bool ok = false;
                    while (stack.Count > 0)
                    {
                        object p = stack.Pop();
                        if (p == Scanner.Left_Bracket)
                        {
                            ok = true;
                            break;
                        }
                        else
                            queue.Add(ConvertToExpr(p));
                    }

                    if (!ok)
                        throw new Error("unbalanced parentheses" + " [At line: " + tokens[index].Line + "]");
                }
                else
                    throw new Error("invalid character encountered: " + tokens[index].Object + " [At line: " + tokens[index].Line + "]");
                if (isNextRequire) index++;
            } while (!stopper.Contains(tokens[index].Object) && tokens[index].Object != Scanner.Right_Braces);

            while (stack.Count > 0)
            {
                object p = stack.Pop();
                if (p == Scanner.Left_Bracket)
                    throw new Error("unbalanced parentheses" + " [At line: " + tokens[index].Line + "]");
                queue.Add(ConvertToExpr(p));
            }

            if (stack.Count == 0 && queue.Count == 0)
                return null;

            var res = ToTree(expr);
            res.ShuntingYardArtmValue = expr;
            if (isNonNumber)
            {
                expr.RemoveAll(ex => ex is AddSymbol);
                CutTreeMathSymbol(res);
            }
            return res;
        }

        private static void CutTreeMathSymbol(TreeNode tree)
        {
            if (tree.Op != null)
            {
                tree.Op = null;
                CutTreeMathSymbol(tree.Left);
                CutTreeMathSymbol(tree.Right);
            }
        }

        private static Expr ConvertToExpr(object symbol)
        {
            if (isNonNumber && (symbol == Scanner.Sub || symbol == Scanner.Mul || symbol == Scanner.Div || symbol == Scanner.Mod))
                throw new Error("cannot use operator " + GetSymbolString(symbol) + " operate with variable type of string");

            LogicalSymbol sym = (LogicalSymbol)symbol;
            Type t = Type.GetType(sym.Name + "Symbol");
            return Activator.CreateInstance(t) as Expr;
        }

        private static string GetSymbolString(object symbol)
        {
            LogicalSymbol sym = (LogicalSymbol)symbol;
            return "'" + sym.Symbol + "'[" + sym.Name + "]";
        }

        private static int Precedence(object c)
        {
            // Set up negation to have a precedence higher than multiply and divide,
            // but lower than exponentiation. You can also set it up to be right associative.
            if (c == Scanner.Neg)
                return 13;
            else if (c == Scanner.Mul || c == Scanner.Div || c == Scanner.Mod)
                return 12;
            else if (c == Scanner.Add || c == Scanner.Sub)
                return 11;
            else if (c == Scanner.MoreThan || c == Scanner.MoreThanAndEqual || c == Scanner.LessThan || c == Scanner.LessThanAndEqual)
                return 10;
            else if (c == Scanner.LogicalEqual || c == Scanner.LogicalNotEqual)
                return 9;
            else if (c == Scanner.And || c == Scanner.AndAlso)
                return 8;
            else if (c == Scanner.Or || c == Scanner.OrElse)
                return 7;
            return 0;
        }

        static bool IsInConditionType(Expr expr)
        {
            return expr is VarDecExpr || expr is CallMethod || expr is ReadValue || expr is Display;
        }

        static TreeNode ToTree(List<Expr> q)
        {
            Stack<TreeNode> stack = new Stack<TreeNode>();

            foreach (Expr mv in q)
            {
                if (IsInConditionType(mv))
                    stack.Push(new TreeNode(mv));
                else if (mv is SubNegSymbol)
                {
                    var nseq = new NegSymbolSequenceExpr { Value = stack.Pop() };
                    nseq.Value.ShuntingYardArtmValue = q;
                    stack.Push(new TreeNode(nseq));
                }
                else if (mv is LogicalNotSymbol)
                {
                    var nseq = new NotSymbolSequenceExpr { Value = stack.Pop() };
                    nseq.Value.ShuntingYardArtmValue = q;
                    stack.Push(new TreeNode(nseq));
                }
                else
                {
                    TreeNode right = stack.Pop();
                    TreeNode left = stack.Pop();
                    stack.Push(new TreeNode((mv as LogicalSymbolExpr), left, right));
                }
            }

            return stack.Pop();
        }
    }
}
