using Collections = System.Collections.Generic;
using Text = System.Text;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GStringCompiler
{
    public sealed class Parser
    {
        public Dictionary<Method, List<DeclareLocalVar>> mainDeclaredLocalVar { get; set; }
        private int index;
        private Collections.IList<GStringObject> tokens;
        private readonly Stmt result = new Sequence();
        private static Parser parser;
        private static string CurrentNamespace;
        public Project Proj { get; private set; }
        public Class CurrentSynClass;
        public Method CurrentSynMethod;

        public Parser(Collections.IList<GStringObject> tokens)
        {
            this.tokens = tokens;
            this.index = 0;
            parser = this;
            Proj = new Project();
            mainDeclaredLocalVar = new Dictionary<Method, List<DeclareLocalVar>>();

            do
            {
                ParseStmtIni();
                if (tokens[index].Object == Scanner.FullStop)
                    index++;
            } while (this.index < this.tokens.Count);

            if (this.index != this.tokens.Count)
                throw new Error("require .[Full stop] [At line: " + (tokens[index].Line - 1) + "]");
        }

        public static Parser Getinstance()
        {
            return parser;
        }

        public Stmt Result
        {
            get { return result; }
        }

        public void ContinueParse()
        {
            this.ParseStmt();
        }

        public void ParseStmtIni()
        {
            if (this.index == this.tokens.Count)
            {
                throw new Error("expected statement, got EOF [At line: " + tokens[index].Line + "]");
            }

            switch (this.tokens[this.index].Object.ToString())
            {
                case "Namespace":
                    CurrentNamespace = "";
                    SetNamespace();
                    break;
                case "Class":
                    Proj.Classes.Add(Class.SynthesizeClass(ref index, tokens, CurrentNamespace));
                    break;
                case "Main":
                    CurrentSynClass.Methods.Add(Method.EntryMethod(ref index, tokens));
                    break;
                case "Method":
                    CurrentSynClass.Methods.Add(Method.VirMethod(ref index, tokens));
                    break;
                default:
                    throw new Error("Parse error at line " + this.tokens[this.index].Line + ": " + this.tokens[this.index].Object);
            }
        }

        public Stmt ParseStmt()
        {
            Stmt result = null;

            if (this.index == this.tokens.Count)
            {
                throw new Error("expected statement, got EOF [At line: " + tokens[index].Line + "]");
            }

            switch (this.tokens[this.index].Object.ToString())
            {
                case "Display":
                    result = Display.Synthesize(ref this.index, tokens);
                    break;
                case "Declare":
                    result = DeclareLocalVar.Synthesize(ref this.index, tokens);
                    break;
                case "Assign":
                    result = Assign.Synthesize(ref index, ref tokens);
                    break;
                case "If":
                    result = ConditionHolder.Synthesize(ref index, tokens);
                    break;
                case "if":
                    result = ConditionHolder.Synthesize(ref index, tokens);
                    break;
                case "Else":
                    return result;
                case "Repeat":
                    this.index++;
                    if (this.tokens[this.index].Object.Equals("since"))
                        result = ForLoop.Synthesize(ref index, ref tokens);
                    else if (this.tokens[this.index].Object.Equals("while"))
                    {
                        if (Repeater.IsBodyDoLoop)
                            return result;
                        return WhileLoop.Synthesize(ref index, ref tokens);
                    }
                    else
                        throw new Error("invalid repeat type [At line: " + tokens[index].Line + "]");
                    break;
                case "Do":
                    this.index++;
                    if (this.tokens[this.index].Object != Scanner.Colon)
                        throw new Error("require :[Colon] after Do [At line: " + tokens[index].Line + "]");
                    index++;
                    result = DoLoop.Synthesize(ref index, ref tokens);
                    break;
                case "End":
                    return result;
                case "loop":
                    index--;
                    return result;
                case "method":
                    index--;
                    return result;
                case "Return":
                    result = Return.Synthesize(ref index, tokens);
                    break;
                case "Call":
                    index++;
                    if (this.tokens[this.index].Object.Equals("method"))
                        result = CallMethod.Synthesize(ref index, tokens);
                    else
                        throw new Error("'Call " + this.tokens[this.index].Object + "' is undefined [At line: " + tokens[index].Line + "]");
                    break;
                default:
                    throw new Error("Parse error at line " + this.tokens[this.index].Line + ": " + this.tokens[this.index].Object);
            }

            if (this.index < this.tokens.Count && this.tokens[this.index].Object == Scanner.FullStop)
            {
                this.index++;
                if (this.index < this.tokens.Count &&
                    !this.tokens[this.index].Object.Equals("end"))
                {
                    Sequence sequence = new Sequence();
                    sequence.First = result;
                    sequence.Second = this.ParseStmt();
                    result = sequence;
                }
            }

            return result;
        }

        private void SetNamespace()
        {
            this.index++;
            if (this.tokens[this.index].Object != Scanner.Colon)
                throw new Error("require :[Colon] after Namespace [At line: " + tokens[index].Line + "]");

            this.index++;
            if (this.tokens[this.index].Object != Scanner.Left_Braces)
                throw new Error("require {[Left Braces] after :[Colon] [At line: " + tokens[index].Line + "]");

            string ns = DotMerger.MergeDot(ref index, tokens);

            if (string.IsNullOrEmpty(ns))
                throw new Error("require a name of Namespace [At line: " + tokens[index].Line + "]");

            index++;
            if (this.tokens[this.index].Object != Scanner.FullStop)
                throw new Error("require .[Full Stop] after :[Colon] [At line: " + (tokens[index].Line - 1) + "]");

            CurrentNamespace = ns;
            Parser.Getinstance().CurrentSynClass = null;
            Parser.Getinstance().CurrentSynMethod = null;

            index++;

            if (!tokens[index].Object.Equals("End"))
            {
                do
                {
                    Parser.Getinstance().ParseStmtIni();
                    index++;
                }
                while (!tokens[index].Object.Equals("End"));
            }
            index--;

            index++;
            if (index == tokens.Count || !tokens[index].Object.Equals("End"))
            {
                if (index == tokens.Count)
                    index--;
                throw new Error("unterminated 'Namespace' body, require 'End namespace.' [At line: " + tokens[index].Line + "]");
            }

            index++;
            if (index == tokens.Count || !tokens[index].Object.Equals("namespace"))
            {
                if (index == tokens.Count)
                    index--;
                throw new Error("unterminated 'Namespace' body, require 'End namespace.' [At line: " + tokens[index].Line + "]");
            }

            index++;
            if (index == tokens.Count || tokens[index].Object != Scanner.FullStop)
            {
                if (index == tokens.Count)
                    index--;
                throw new Error("require .[Full stop] [At line: " + tokens[index].Line + "]");
            }
        }

        public Expr ParseExpr()
        {
            return ParseExprInner(ref this.index);
        }

        public Expr ParseExpr(ref int index)
        {
            return ParseExprInner(ref index);
        }

        private Expr ParseExprInner(ref int index)
        {
            if (this.index == this.tokens.Count)
            {
                throw new System.Exception("expected expression, got EOF");
            }

            if (this.tokens[index].Object is Text.StringBuilder)
            {
                string value = ((Text.StringBuilder)this.tokens[index++].Object).ToString();
                StringLiteral stringLiteral = new StringLiteral();
                stringLiteral.Value = value;
                return stringLiteral;
            }
            else if (this.tokens[index].Object is int)
            {
                int intValue = (int)this.tokens[index++].Object;
                IntLiteral intLiteral = new IntLiteral();
                intLiteral.Value = intValue;
                return intLiteral;
            }
            else if (this.tokens[index].Object is float)
            {
                float floatValue = (float)this.tokens[index++].Object;
                FloatLiteral floatLiteral = new FloatLiteral();
                floatLiteral.Value = floatValue;
                return floatLiteral;
            }
            else if (this.tokens[index].Object is double)
            {
                double doubleValue = (double)this.tokens[index++].Object;
                DoubleLiteral doubleLiteral = new DoubleLiteral();
                doubleLiteral.Value = doubleValue;
                return doubleLiteral;
            }

            else if (this.tokens[index].Object is string)
            {
                if (this.tokens[index].Object.Equals("true") || this.tokens[index].Object.Equals("false"))// true/false
                {
                    BooleanLiteral booleanLiteral = new BooleanLiteral();
                    if (this.tokens[index].Object.Equals("true"))
                        booleanLiteral.Value = true;
                    index++;
                    return booleanLiteral;
                }
                else if (tokens[index].Object.Equals("Call"))// Call method
                {
                    index++;
                    if (tokens[index].Object.Equals("method"))
                    {
                        return (Expr)CallMethod.Synthesize(ref index, tokens, false, true);
                    }
                    else
                        throw new Error("'Call " + tokens[index].Object + "' is undefined [At line: " + tokens[index].Line + "]");
                }
                else
                {
                    Type type = GSType.GetTypeByTypeText(tokens[index].Object.ToString());

                    if (tokens[index].Object.ToString() == "string" && type == typeof(System.Text.StringBuilder))
                        type = typeof(string);

                    if (type != null)
                    {
                        index++;
                        List<Ornament> orm = new List<Ornament>();
                        Variable.FindVariableOrnament(ref index, tokens, orm, "", type);
                        if (orm.FirstOrDefault() is CallMethod)
                        {
                            CallMethod cm = (CallMethod)orm.First();
                            cm.IsStatic = true;
                            return (Expr)cm;
                        }
                        return null;
                    }
                    else
                    {
                        string ident = (string)this.tokens[index++].Object;
                        Variable var = new Variable();
                        var.Ident = ident;
                        if (this.tokens[index].Object == Scanner.Comma)
                            var.Ornaments = Variable.FindVariableOrnament(ref index, tokens, new List<Ornament>(), var.Ident);
                        return var;
                    }
                }
            }
            else if (this.tokens[index].Object is LogicalSymbol)
            {
                return null;
            }
            else
            {
                throw new Error("Expected string literal, int literal, or variable [At line: " + tokens[index].Line + "]");
            }
        }

        //Just used for reference.
        public Expr ParseExpr(Type type)
        {
            if (this.index == this.tokens.Count)
            {
                throw new Error("expected expression, got EOF [At line: " + tokens[index].Line + "]");
            }

            if (type == typeof(Text.StringBuilder))
            {
                StringLiteral stringLiteral = new StringLiteral();
                stringLiteral.Value = String.Empty;
                return stringLiteral;
            }
            else if (type == typeof(int))
            {
                IntLiteral intLiteral = new IntLiteral();
                intLiteral.Value = 0;
                return intLiteral;
            }
            else if (type == typeof(float))
            {
                FloatLiteral fLiteral = new FloatLiteral();
                fLiteral.Value = 0;
                return fLiteral;
            }
            else if (type == typeof(double))
            {
                DoubleLiteral dLiteral = new DoubleLiteral();
                dLiteral.Value = 0;
                return dLiteral;
            }
            else if (type == typeof(bool))
            {
                BooleanLiteral boolLiteral = new BooleanLiteral();
                boolLiteral.Value = false;
                return boolLiteral;
            }
            else if (type == typeof(string))
            {
                Variable var = new Variable();
                return var;
            }
            else
            {
                throw new Error("expected string literal, int literal, or variable [At line: " + tokens[index].Line + "]");
            }
        }
    }
}
