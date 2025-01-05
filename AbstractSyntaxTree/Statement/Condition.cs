using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GStringCompiler
{
    public class ConditionHolder : Stmt
    {
        public Stmt IfTrue { get; set; }
        public Stmt IfFalse { get; set; }
        public Expr Condition { get; set; }
        public bool IsLoop { get; set; }
        public bool IsDoWhileStyle { get; set; }

        public static Stmt Synthesize(ref int index, IList<GStringObject> tokens)
        {
            bool isIfFalse = false;
            if (tokens[index].Object.ToString().Equals("If"))
                index++;
            if (tokens[index].Object.ToString().Equals("if"))
            {
                index++;
                isIfFalse = true;
            }
            List<Stmt> stmtList = new List<Stmt>();
            ConditionHolder conditionHolder = new ConditionHolder();
            stmtList.Add(conditionHolder);

            conditionHolder.Condition = OrderController.GetSequenceOfOperation(ref index, tokens, Scanner.Colon, null);
            index++;
            conditionHolder.IfTrue = Parser.Getinstance().ParseStmt();

            if (tokens.Count == index)
                throw new Error("require 'End condition' statement [At line: " + tokens[index - 1].Line + "]");

            if (tokens[index].Object.Equals("Else"))
            {
                index++;
                if (tokens[index].Object.Equals("if"))
                    conditionHolder.IfFalse = Parser.Getinstance().ParseStmt();
                else
                {
                    // Skip :.
                    index++;
                    conditionHolder.IfFalse = Parser.Getinstance().ParseStmt();
                }
            }

            if (isIfFalse)
            {
                var res = QueueTreeController.AssignSequence(stmtList);
                if (tokens.Count == index)
                    throw new Error("require 'End condition' statement [At line: " + tokens[index - 1].Line + "]");
                return res;
            }

            if (!tokens[index].Object.Equals("End") || index == tokens.Count)
                throw new Error("require 'End condition' statement [At line: " + tokens[index].Line + "]");

            index++;
            if (!tokens[index].Object.Equals("condition") || index == tokens.Count)
            {
                throw new Error("require 'End condition' statement [At line: " + tokens[index].Line + "]");
            }

            index++;
            if (index == tokens.Count)
            {
                throw new Error("require .[Full stop] [At line: " + tokens[index - 1].Line + "]");
            }

            if (index < tokens.Count - 1 && !tokens[index].Object.Equals("end"))
                return QueueTreeController.AssignSequence(stmtList, ref index, tokens);
            else
                return QueueTreeController.AssignSequence(stmtList);
        }
    }
}
