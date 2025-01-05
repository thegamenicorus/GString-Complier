using Collections = System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;

namespace GStringCompiler
{
    public enum LoopRunningExpr { Increment, Decrement };

    public class Repeater : Stmt
    {
        public static bool IsBodyDoLoop = false;
        public string Ident { get; set; }
        public Stmt Body { get; set; }
    }

    // for <ident> = <expr> to <expr> do <stmt> end
    public class ForLoop : Repeater
    {
        public Expr From { get; private set; }
        public Expr To { get; private set; }
        public Expr runningValue { get; private set; }
        public LoopRunningExpr runnerType { get; private set; }
        public bool IsToBefore { get; private set; }

        public static Stmt Synthesize(ref int index, ref Collections.IList<GStringObject> tokens)
        {
            ForLoop forLoop = new ForLoop();
            List<Stmt> stmtList = new List<Stmt>();
            stmtList.Add(forLoop);

            index++;
            if (index < tokens.Count && tokens[index].Object is string)
            {
                forLoop.Ident = (string)tokens[index].Object;
                if (Parser.Getinstance().mainDeclaredLocalVar[Parser.Getinstance().CurrentSynMethod].SingleOrDefault(i => i.Ident == forLoop.Ident) == null)
                    Parser.Getinstance().mainDeclaredLocalVar[Parser.Getinstance().CurrentSynMethod].Add(new DeclareLocalVar { Ident = forLoop.Ident, Expr = new IntLiteral { Value = 0 } });
            }
            else
                throw new Error("require variable name after 'Repeat since' [At line: " + tokens[index].Line + "]");

            index++;
            if (tokens[index].Object != Scanner.Colon || index == tokens.Count)
                throw new Error("require :[Colon] after '" + forLoop.Ident + "' [At line: " + tokens[index].Line + "]");

            index++;
            if (tokens[index].Object != Scanner.Left_Braces || index == tokens.Count)
                throw new Error("require {[Left braces] after :[Colon] [At line: " + tokens[index].Line + "]");

            index++;
            // Detect what ever have {} or not in from.
            if (tokens[index].Object == Scanner.Left_Braces)
            {
                index++;
                if (OrderController.IsSeqExpr(index, tokens))
                    forLoop.From = OrderController.GetSequenceOfOperation(ref index, tokens, null);
                else
                    forLoop.From = Parser.Getinstance().ParseExpr();

                if (tokens[index].Object != Scanner.Right_Braces || index == tokens.Count)
                    throw new Error("require }[Right braces] after loop's start value [At line: " + tokens[index].Line + "]");
                index++;
            }
            else
                forLoop.From = Parser.Getinstance().ParseExpr();

            if (!tokens[index].Object.Equals("to") || index == tokens.Count)
                throw new Error("require 'to' after loop's start value [At line: " + tokens[index].Line + "]");

            index++;
            if (tokens[index].Object.Equals("before"))
            {
                forLoop.IsToBefore = true;
                index++;
            }

            // Detect what ever use {} or not in from.
            if (tokens[index].Object == Scanner.Left_Braces)
            {
                index++;
                if (OrderController.IsSeqExpr(index, tokens))
                    forLoop.To = OrderController.GetSequenceOfOperation(ref index, tokens, null);
                else
                    forLoop.To = Parser.Getinstance().ParseExpr();

                if (tokens[index].Object != Scanner.Right_Braces || index == tokens.Count)
                    throw new Error("require }[Right braces] after loop's end value [At line: " + tokens[index].Line + "]");
                index++;
            }
            else
                forLoop.To = Parser.Getinstance().ParseExpr();

            if (tokens[index].Object != Scanner.Right_Braces || index == tokens.Count)
                throw new Error("require }[Right braces] after loop's start value [At line: " + tokens[index].Line + "]");

            index++;
            if (!tokens[index].Object.Equals(Scanner.Comma) || index == tokens.Count)
            {
                throw new Error("require loop type identify [increase by {value}, decrease by {value}] [At line: " + tokens[index].Line + "]");
            }

            index++;
            if (tokens[index].Object.Equals("increase"))
                forLoop.runnerType = LoopRunningExpr.Increment;
            else if (tokens[index].Object.Equals("decrease"))
                forLoop.runnerType = LoopRunningExpr.Decrement;
            else
                throw new Error("wrong loop type identify [increase by {value}, decrease by {value}] [At line: " + tokens[index].Line + "]");

            index++;
            if (!tokens[index].Object.Equals("by") || index == tokens.Count)
            {
                throw new Error("require 'by' after identify loop type [At line: " + tokens[index].Line + "]");
            }

            index++;
            if (tokens[index].Object != Scanner.Colon || index == tokens.Count)
                throw new Error("require :[Colon] after 'by' [At line: " + tokens[index].Line + "]");

            index++;
            if (tokens[index].Object != Scanner.Left_Braces || index == tokens.Count)
                throw new Error("require {[Left braces] after :[Colon] [At line: " + tokens[index].Line + "]");


            index++;
            if (OrderController.IsSeqExpr(index, tokens))
            {
                forLoop.runningValue = OrderController.GetSequenceOfOperation(ref index, tokens, null);
            }
            else
            {
                forLoop.runningValue = Parser.Getinstance().ParseExpr();
            }

            if (tokens[index].Object != Scanner.Right_Braces || index == tokens.Count)
                throw new Error("require }[Right braces] after loop's counter  value [At line: " + tokens[index].Line + "]");

            index++;
            if (tokens[index].Object != Scanner.Colon || index == tokens.Count)
                throw new Error("require :[Colon] after loop declaration [At line: " + tokens[index].Line + "]");

            index++;
            forLoop.Body = Parser.Getinstance().ParseStmt();

            if (!tokens[index].Object.Equals("End") || index == tokens.Count)
                throw new Error("unterminated 'Repeat since' loop body, require 'End loop.' [At line: " + tokens[index].Line + "]");

            index++;
            if (!tokens[index].Object.Equals("loop") || index == tokens.Count)
            {
                throw new Error("unterminated 'Repeat since' loop body, require 'End loop.' [At line: " + tokens[index].Line + "]");
            }

            index++;
            if (tokens[index].Object != Scanner.FullStop || index == tokens.Count)
            {
                throw new Error("require .[Full stop] [At line: " + tokens[index].Line + "]");
            }

            if (index < tokens.Count - 1 && !tokens[index].Object.Equals("end"))
                return QueueTreeController.AssignSequence(stmtList, ref index, tokens);
            else
                return QueueTreeController.AssignSequence(stmtList);
        }
    }

    public class WhileLoop : Repeater
    {
        public ConditionHolder ConditionHolder { get; set; }

        public static Stmt Synthesize(ref int index, ref Collections.IList<GStringObject> tokens)
        {
            WhileLoop whileLoop = new WhileLoop();
            List<Stmt> stmtList = new List<Stmt>();
            stmtList.Add(whileLoop);

            index++;
            ConditionHolder conditionHolder = new ConditionHolder();
            whileLoop.ConditionHolder = conditionHolder;
            conditionHolder.IsLoop = true;
            conditionHolder.Condition = OrderController.GetSequenceOfOperation(ref index, tokens, Scanner.Colon, null);

            index++;
            conditionHolder.IfTrue = Parser.Getinstance().ParseStmt();

            whileLoop.Body = Parser.Getinstance().ParseStmt();

            if (!tokens[index].Object.Equals("End") || index == tokens.Count)
                throw new Error("unterminated 'Repeat since' loop body, require 'End loop.' [At line: " + tokens[index].Line + "]");

            index++;
            if (!tokens[index].Object.Equals("loop") || index == tokens.Count)
            {
                throw new Error("unterminated 'Repeat since' loop body, require 'End loop.' [At line: " + tokens[index].Line + "]");
            }

            index++;
            if (tokens[index].Object != Scanner.FullStop || index == tokens.Count)
            {
                throw new Error("require .[Full stop] [At line: " + tokens[index].Line + "]");
            }

            if (index < tokens.Count - 1 && !tokens[index].Object.Equals("end"))
                return QueueTreeController.AssignSequence(stmtList, ref index, tokens);
            else
                return QueueTreeController.AssignSequence(stmtList);
        }
    }

    public class DoLoop : Repeater
    {
        public Expr Condition { get; set; }

        public static Stmt Synthesize(ref int index, ref Collections.IList<GStringObject> tokens)
        {
            DoLoop doLoop = new DoLoop();
            List<Stmt> stmtList = new List<Stmt>();
            stmtList.Add(doLoop);

            Repeater.IsBodyDoLoop = true;
            doLoop.Body = Parser.Getinstance().ParseStmt();
            Repeater.IsBodyDoLoop = false;

            index++;
            doLoop.Condition = OrderController.GetSequenceOfOperation(ref index, tokens, Scanner.FullStop, null);

            if (tokens[index].Object != Scanner.FullStop || index == tokens.Count)
            {
                throw new Error("require .[Full stop] [At line: " + tokens[index].Line + "]");
            }

            if (index < tokens.Count - 1 && !tokens[index].Object.Equals("end"))
                return QueueTreeController.AssignSequence(stmtList, ref index, tokens);
            else
                return QueueTreeController.AssignSequence(stmtList);
        }
    }
}
