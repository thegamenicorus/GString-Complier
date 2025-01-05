using System;
using Collections = System.Collections.Generic;
using IO = System.IO;
using Text = System.Text;

namespace GStringCompiler
{
    public sealed class Scanner
    {
        private readonly Collections.IList<GStringObject> result;
        private int line = 1;
        private Collections.List<string> NameVarWithNumExcept = new Collections.List<string> { "to", "before"};

        public Scanner(IO.TextReader input)
        {
            this.result = new Collections.List<GStringObject>();
            this.Scan(input);
        }

        public Collections.IList<GStringObject> Tokens
        {
            get { return this.result; }
        }

        #region ArithmiticConstants

        // Constants to represent arithmitic tokens. This could
        // be alternatively written as an enum.
        public static readonly LogicalSymbol Add = new LogicalSymbol("Add","+");
        public static readonly LogicalSymbol Sub = new LogicalSymbol("Sub", "-");
        public static readonly LogicalSymbol Mul = new LogicalSymbol("Mul", "*");
        public static readonly LogicalSymbol Div = new LogicalSymbol("Div", "/");
        public static readonly LogicalSymbol Mod = new LogicalSymbol("Mod", "%");
        public static readonly LogicalSymbol Neg = new LogicalSymbol("SubNeg", "-");
        public static readonly object FullStop = new object();
        public static readonly object Equal = new object();
        public static readonly object Comma = new object();
        public static readonly object Colon = new object();
        public static readonly object SemiColon = new object();
        public static readonly object Left_Bracket = new object();
        public static readonly object Right_Bracket = new object();
        public static readonly object Left_Braces = new object();
        public static readonly object Right_Braces = new object();
        public static readonly object Left_SquareBracket = new object();
        public static readonly object Right_SquareBracket = new object();
        public static readonly LogicalSymbol LessThanAndEqual = new LogicalSymbol("LessThanAndEqual","<=");
        public static readonly LogicalSymbol MoreThanAndEqual = new LogicalSymbol("MoreThanAndEqual", ">=");
        public static readonly LogicalSymbol LessThan = new LogicalSymbol("LessThan", "<");
        public static readonly LogicalSymbol MoreThan = new LogicalSymbol("MoreThan", ">");
        public static readonly LogicalSymbol And = new LogicalSymbol("And", "&");
        public static readonly LogicalSymbol Or = new LogicalSymbol("Or", "|");
        public static readonly LogicalSymbol AndAlso = new LogicalSymbol("AndAlso", "&&");
        public static readonly LogicalSymbol OrElse = new LogicalSymbol("OrElse", "||");
        public static readonly LogicalSymbol LogicalEqual = new LogicalSymbol("LogicalEqual", "==");
        public static readonly LogicalSymbol LogicalNot = new LogicalSymbol("LogicalNot", "!");
        public static readonly LogicalSymbol LogicalNotEqual = new LogicalSymbol("LogicalNotEqual", "!=");
        #endregion

        private void Scan(IO.TextReader input)
        {
            bool isVarWithNum = false;
            while (input.Peek() != -1)
            {
                char ch = (char)input.Peek();
                // Scan individual tokens.
                if (char.IsWhiteSpace(ch))
                {
                    if (ch == Convert.ToChar(10))
                        line++;
                    // Eat the current char and skip ahead!
                    input.Read();
                }
                else if (char.IsLetter(ch) || ch == '_')
                {
                    // Keyword or identifier.
                    Text.StringBuilder accum = new Text.StringBuilder();

                    while (char.IsLetter(ch) || ch == '_')
                    {
                        accum.Append(ch);
                        input.Read();

                        if (input.Peek() == -1)
                        {
                            break;
                        }
                        else
                        {
                            ch = (char)input.Peek();
                        }
                    }

                    // Name var with num.
                    if (char.IsDigit(ch))
                        isVarWithNum = true;

                    this.result.Add(new GStringObject { Object = accum.ToString(), Line = line });
                }
                else if (ch == '"')
                {
                    // String literal.
                    Text.StringBuilder accum = new Text.StringBuilder();

                    // skip the '"'.
                    input.Read(); 

                    if (input.Peek() == -1)
                    {
                        throw new Error("unterminated string literal [At line: " + line + "]");
                    }

                    while ((ch = (char)input.Peek()) != '"')
                    {
                        accum.Append(ch);
                        input.Read();

                        if (input.Peek() == -1)
                        {
                            throw new System.Exception("unterminated string literal [At line: " + line + "]");
                        }
                    }

                    // Skip the terminating ".
                    input.Read();
                    this.result.Add(new GStringObject { Object = accum, Line = line });
                }
                else if (char.IsDigit(ch))
                {
                    // Numeric literal.
                    Text.StringBuilder accum = new Text.StringBuilder();
                    bool isContainDot = false;
                    bool isFloat = false;
                    while (char.IsDigit(ch) || ch == '.')
                    {
                        accum.Append(ch);
                        input.Read();
                       
                        if (ch == '.')
                            isContainDot = true;
                        else if (isContainDot)
                            isFloat = true;

                        if (input.Peek() == -1)
                            break;
                        else
                            ch = (char)input.Peek();
                    }
                    
                    if (isFloat)
                    {
                        if (accum.ToString().Split('.')[1].Length > 7)
                            this.result.Add(new GStringObject { Object = double.Parse(accum.ToString()), Line = line });
                        else
                            this.result.Add(new GStringObject { Object = float.Parse(accum.ToString()), Line = line });
                    }
                    else if(isContainDot)
                    {
                        this.result.Add(new GStringObject { Object = int.Parse(accum.ToString().Replace(".", "")), Line = line });
                        this.result.Add(new GStringObject { Object = Scanner.FullStop, Line = line });
                    }
                    else
                        this.result.Add(new GStringObject { Object = int.Parse(accum.ToString()), Line = line });

                    // Check negative number.
                    if (this.result[this.result.Count - 2].Object is LogicalSymbol)
                    {
                        if (
                            (this.result[this.result.Count - 2].Object as LogicalSymbol).Symbol.Equals("-")
                            && (this.result[this.result.Count - 3].Object == Scanner.Left_Braces || this.result[this.result.Count - 3].Object == Scanner.Left_Bracket
                            || this.result[this.result.Count - 3].Object == Scanner.Left_SquareBracket
                            || this.result[this.result.Count - 3].Object == Scanner.Comma)
                            )
                        {
                            var currentDecVal = this.result[this.result.Count - 1].Object.ToString();                            
                            if (currentDecVal.Contains("."))
                            {
                                // TODO Float or double, implement later.
                            }
                            else
                                this.result[this.result.Count - 1].Object = (int)this.result[this.result.Count - 1].Object *(- 1);
                            this.result.RemoveAt(this.result.Count - 2);
                        }
                    }

                    // Name var with num.
                    if ((int)this.result[this.result.Count - 1].Object >= 0 && this.result[this.result.Count - 2].Object is string
                        && !NameVarWithNumExcept.Contains(this.result[this.result.Count - 2].Object.ToString()) && isVarWithNum)
                    {
                        this.result[this.result.Count - 2].Object = this.result[this.result.Count - 2].Object.ToString() + this.result[this.result.Count - 1].Object.ToString();
                        this.result.RemoveAt(this.result.Count - 1);
                        isVarWithNum = false;
                    }
                }
                else switch (ch)
                    {
                        case '+':
                            input.Read();
                            this.result.Add(new GStringObject { Object = Scanner.Add, Line = line });
                            break;

                        case '-':
                            input.Read();
                            this.result.Add(new GStringObject { Object = Scanner.Sub, Line = line });
                            break;

                        case '*':
                            input.Read();
                            char starcheck = (char)input.Peek();
                            if (starcheck == '*')
                            {
                                input.ReadLine();
                                line++;
                            }
                            else if (starcheck == '{')
                            {
                                bool commnt_flag = true;
                                while (input.Peek() != -1 && commnt_flag)
                                {
                                    char comment_ch = (char)input.Peek();
                                    input.Read();
                                    switch (comment_ch)
                                    {
                                        case '}':                                            
                                            char end_comment_chk = (char)input.Peek();
                                            if (end_comment_chk == '*')
                                            {
                                                // Give while main read next char.
                                                input.Read();
                                                commnt_flag = false;
                                                break;
                                            }
                                            break;
                                        case '\n':
                                            line++;
                                            break;
                                    }
                                }
                            }
                            else
                                this.result.Add(new GStringObject { Object = Scanner.Mul, Line = line });
                            break;
                        case '/':
                            input.Read();
                            this.result.Add(new GStringObject { Object = Scanner.Div, Line = line });
                            break;
                        case '%':
                            input.Read();
                            this.result.Add(new GStringObject { Object = Scanner.Mod, Line = line });
                            break;
                        case '=':
                            input.Read();
                            char eqcheck = (char)input.Peek();
                            if (eqcheck == '=')
                            {
                                input.Read();
                                this.result.Add(new GStringObject { Object = Scanner.LogicalEqual, Line = line });
                            }
                            else
                                this.result.Add(new GStringObject { Object = Scanner.Equal, Line = line });
                            break;
                        case '&':
                            input.Read();
                            char andalsocheck = (char)input.Peek();
                            if (andalsocheck == '&')
                            {
                                input.Read();
                                this.result.Add(new GStringObject { Object = Scanner.AndAlso, Line = line });
                            }
                            else
                                this.result.Add(new GStringObject { Object = Scanner.And, Line = line });
                            break;
                        case '|':
                            input.Read();
                            char orelsecheck = (char)input.Peek();
                            if (orelsecheck == '|')
                            {
                                input.Read();
                                this.result.Add(new GStringObject { Object = Scanner.OrElse, Line = line });
                            }
                            else
                                this.result.Add(new GStringObject { Object = Scanner.Or, Line = line });
                            break;
                        case '!':
                            input.Read();
                            char neqcheck = (char)input.Peek();
                            if (neqcheck == '=')
                            {
                                input.Read();
                                this.result.Add(new GStringObject { Object = Scanner.LogicalNotEqual, Line = line });
                            }
                            else
                                this.result.Add(new GStringObject { Object = Scanner.LogicalNot, Line = line });
                            break;
                        case '.':
                            input.Read();
                            this.result.Add(new GStringObject { Object = Scanner.FullStop, Line = line });
                            break;
                        case ',':
                            input.Read();
                            this.result.Add(new GStringObject { Object = Scanner.Comma, Line = line });
                            break;
                        case ':':
                            input.Read();
                            this.result.Add(new GStringObject { Object = Scanner.Colon, Line = line });
                            break;
                        case '{':
                            input.Read();
                            this.result.Add(new GStringObject { Object = Scanner.Left_Braces, Line = line });
                            break;
                        case '}':
                            input.Read();
                            this.result.Add(new GStringObject { Object = Scanner.Right_Braces, Line = line });
                            break;
                        case '[':
                            input.Read();
                            this.result.Add(new GStringObject { Object = Scanner.Left_SquareBracket, Line = line });
                            break;
                        case ']':
                            input.Read();
                            this.result.Add(new GStringObject { Object = Scanner.Right_SquareBracket, Line = line });
                            break;
                        case '(':
                            input.Read();
                            this.result.Add(new GStringObject { Object = Scanner.Left_Bracket, Line = line });
                            break;
                        case ')':
                            input.Read();
                            this.result.Add(new GStringObject { Object = Scanner.Right_Bracket, Line = line });
                            break;
                        case '>':
                            input.Read();
                            char morethanEqCheck = (char)input.Peek();
                            if (morethanEqCheck == '=')
                            {
                                input.Read();
                                this.result.Add(new GStringObject { Object = Scanner.MoreThanAndEqual, Line = line });
                            }
                            else
                                this.result.Add(new GStringObject { Object = Scanner.MoreThan, Line = line });
                            break;
                        case '<':
                            input.Read();
                            char gteqcheck = (char)input.Peek();
                            if (gteqcheck == '=')
                            {
                                input.Read();
                                this.result.Add(new GStringObject { Object = Scanner.LessThanAndEqual, Line = line });
                            }
                            else
                                this.result.Add(new GStringObject { Object = Scanner.LessThan, Line = line });
                            break;
                        default:
                            throw new Error("scanner encountered unrecognized character '" + ch + "' [At line: " + line + "]");
                    }
            }
        }

        public void PreventObjectNotReferenceCommand()
        {
            this.result.Add(new GStringObject { Object = "Display",Line = line});
            this.result.Add(new GStringObject { Object = Scanner.Colon, Line = line });
            this.result.Add(new GStringObject { Object = new Text.StringBuilder(), Line = line });
            this.result.Add(new GStringObject { Object = Scanner.FullStop, Line = line });
        }
    }
}
