using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GStringCompiler
{
    public class Return : Stmt
    {
        public Expr Expr { get; set; }

        public static Stmt Synthesize(ref int index, IList<GStringObject> tokens)
        {
            List<Stmt> stmtList = new List<Stmt>();
            Return ret = new Return();
            stmtList.Add(ret);

            index++;
            if (tokens[index].Object != Scanner.Colon)
                throw new Error("require :[Colon] after 'Return' [At line: " + tokens[index].Line + "]");

            index++;
            if (tokens[index].Object != Scanner.Left_Braces)
                throw new Error("require {[Left braces] after :[Colon] [At line: " + tokens[index].Line + "]");

            index++;
            if (tokens[index].Object.Equals("Read"))
            {
                index--;
                stmtList.Remove(ret);
                stmtList.Add(BracketReadOperation.Synthesize(ref index,  tokens, ret));
            }
            else
            {
                if (OrderController.IsSeqExpr(index,  tokens))
                {
                    ret.Expr = OrderController.GetSequenceOfOperation(ref index,  tokens, Parser.Getinstance().CurrentSynMethod.ReturnType);
                }
                else
                {
                    ret.Expr = Parser.Getinstance().ParseExpr();
                }
            }

            if (tokens[index].Object != Scanner.Right_Braces || index == tokens.Count)
                throw new Error("require }[Right braces] for 'Return' statement [At line: " + tokens[index].Line + "]");

            index++;
            if (tokens[index].Object != Scanner.FullStop || index == tokens.Count)
            {
                throw new Error("require .[Full stop] [At line: " + tokens[index].Line + "]");
            }

            if (index < tokens.Count - 1 && !tokens[index].Object.Equals("end"))
                return QueueTreeController.AssignSequence(stmtList, ref index,  tokens);
            else
                return QueueTreeController.AssignSequence(stmtList);
        }
    }
}
