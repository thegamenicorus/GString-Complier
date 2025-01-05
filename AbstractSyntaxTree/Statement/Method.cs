using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GStringCompiler.Extension;

namespace GStringCompiler
{
    public enum AccessLevel { Public, Protected, Private };

    public class MethodParam
    {
        public Type ParameterType { get; set; }
        public string ParamName { get; set; }
        public bool IsPassByRef { get; set; }
        public bool IsMethodStatic { get; set; }
        public int Order { get; set; }
    }

    public class Method
    {
        public AccessLevel AccessLevel { get; set; }
        public bool IsStatic { get; set; }
        public Type ReturnType { get; set; }
        public List<MethodParam> MethodParams { get; set; }
        public Stmt Body { get; set; }
        private bool isModified;
        public bool IsEntryMethod { get; set; }
        public string Name { get; set; }
        public Method()
        {

        }

        public static Method EntryMethod(ref int index, IList<GStringObject> tokens)
        {
            Method method = new Method
            {
                AccessLevel = AccessLevel.Private,
                IsStatic = true,
                ReturnType = typeof(void),
                MethodParams = new List<MethodParam>(),
                IsEntryMethod = true,
                Name = "main"
            };
            Parser.Getinstance().CurrentSynMethod = method;
            Parser.Getinstance().mainDeclaredLocalVar[method] = new List<DeclareLocalVar>();
            TypeIdentifier.LoadMethod(method);

            method.MethodParams.Add(new MethodParam { ParameterType = typeof(string[]), ParamName = "args", Order = 0, IsMethodStatic = method.IsStatic });

            index++;
            if (tokens[index].Object == Scanner.Comma)
            {
                index++;
                Optimizer(ref index, tokens, method);
            }
            if (tokens[index].Object != Scanner.FullStop)
                throw new Error("require .[Full Stop] after :[Colon] [At line: " + (tokens[index].Line) + "]");

            index++;

            method.Body = Parser.Getinstance().ParseStmt();

            if (!tokens[index].Object.Equals("End") || index == tokens.Count)
                throw new Error("unterminated 'Method' body, require 'End method.' [At line: " + tokens[index].Line + "]");

            index++;
            if (!tokens[index].Object.Equals("method") || index == tokens.Count)
                throw new Error("unterminated 'Method' body, require 'End method.' [At line: " + tokens[index].Line + "]");

            index++;
            if (tokens[index].Object != Scanner.FullStop || index == tokens.Count)
                throw new Error("require .[Full stop] [At line: " + tokens[index].Line + "]");

            return method;
        }

        public static Method VirMethod(ref int index, IList<GStringObject> tokens)
        {
            Method method = new Method
            {
                MethodParams = new List<MethodParam>(),
            };
            Parser.Getinstance().CurrentSynMethod = method;
            Parser.Getinstance().mainDeclaredLocalVar[method] = new List<DeclareLocalVar>();
            TypeIdentifier.LoadMethod(method);

            index++;
            if (tokens[index].Object != Scanner.Colon)
                throw new Error("require :[Colon] after Method [At line: " + tokens[index].Line + "]");

            index++;
            if (tokens[index].Object != Scanner.Left_Braces)
                throw new Error("require {[Left Braces] after :[Colon] [At line: " + tokens[index].Line + "]");

            index++;
            if (tokens[index].Object is string)
            {
                string ident = (string)tokens[index].Object;
                if (ident.IsReserved())
                    throw new Error("'" + ident + "' is reserved keyword [At line: " + tokens[index].Line + "]");
                method.Name = ident;
            }
            else
                throw new Error("invalid method name [At line: " + tokens[index].Line + "]");

            index++;
            if (tokens[index].Object != Scanner.Right_Braces)
                throw new Error("require }[Right Braces] after method name [At line: " + tokens[index].Line + "]");

            index++;
            if (tokens[index].Object == Scanner.Comma)
            {
                index++;
                Optimizer(ref index, tokens, method);
            }
            if (tokens[index].Object != Scanner.FullStop)
                throw new Error("require .[Full Stop] after :[Colon] [At line: " + (tokens[index].Line) + "]");

            index++;
            method.Body = Parser.Getinstance().ParseStmt();

            if (!tokens[index].Object.Equals("End") || index == tokens.Count)
                throw new Error("unterminated 'Method' body, require 'End method.' [At line: " + tokens[index].Line + "]");

            index++;
            if (!tokens[index].Object.Equals("method") || index == tokens.Count)
                throw new Error("unterminated 'Method' body, require 'End method.' [At line: " + tokens[index].Line + "]");

            index++;
            if (tokens[index].Object != Scanner.FullStop || index == tokens.Count)
                throw new Error("require .[Full stop] [At line: " + tokens[index].Line + "]");

            return method;
        }

        private static void Optimizer(ref int index, IList<GStringObject> tokens, Method method)
        {
            switch (tokens[index].Object.ToString())
            {
                case "modifiers":
                    Modifier(ref index, tokens, method);
                    break;
                case "parameters":
                    Parameter(ref index, tokens, method);
                    break;
                case "implement":
                    break;
                case "return":
                    index++;
                    if (tokens[index].Object.Equals("type"))
                    {
                        ReturnTypeGetter(ref index, tokens, method);
                    }
                    else
                        throw new Error("require 'type' after 'return' [At line: " + tokens[index].Line + "]");
                    break;
                default:
                    throw new Error("invalid method declaration [At line: " + (tokens[index].Line - 1) + "]");
            }
        }

        private static void ReturnTypeGetter(ref int index, IList<GStringObject> tokens, Method method)
        {
            index++;
            if (tokens[index].Object != Scanner.Colon)
                throw new Error("require :[Colon] after modifiers [At line: " + tokens[index].Line + "]");

            index++;
            if (tokens[index].Object != Scanner.Left_Braces)
                throw new Error("require {[Left Braces] after :[Colon] [At line: " + tokens[index].Line + "]");

            index++;
            try
            {
                method.ReturnType = GSType.GetTypeByTypeText((string)tokens[index].Object, CollectionType.None, true);
            }
            catch
            {
                throw new Error("invalid return type define [At line: " + tokens[index].Line + "]");
            }

            if (method.ReturnType == null)
                throw new Error("return type of method is not identified [At line: " + tokens[index].Line + "]");

            index++;
            if (tokens[index].Object != Scanner.Right_Braces)
                throw new Error("require }[Right Braces] after class name [At line: " + tokens[index].Line + "]");

            index++;
            if (tokens[index].Object == Scanner.Comma)
            {
                index++;
                Optimizer(ref index, tokens, method);
            }
        }

        private static void Modifier(ref int index, IList<GStringObject> tokens, Method method)
        {
            if (method.isModified)
                throw new Error("cannot do multiple method modifiers [At line: " + (tokens[index].Line - 1) + "]");

            index++;
            if (tokens[index].Object != Scanner.Colon)
                throw new Error("require :[Colon] after modifiers [At line: " + tokens[index].Line + "]");

            index++;
            if (tokens[index].Object != Scanner.Left_Braces)
                throw new Error("require {[Left Braces] after :[Colon] [At line: " + tokens[index].Line + "]");

            index++;
            List<string> modStr = new List<string>();
            do
            {
                if (tokens[index].Object == Scanner.Comma) { }
                else if (tokens[index].Object is string) modStr.Add(tokens[index].Object.ToString());
                else throw new Error("invalid method modifiers declaration [At line: " + (tokens[index].Line) + "]");
                index++;
            } while (tokens[index].Object != Scanner.Right_Braces);

            if (modStr.Count > 2) throw new Error("invalid method modifiers declaration [At line: " + (tokens[index].Line) + "]");

            foreach (var mode in modStr)
                switch (mode.ToLower())
                {
                    case "public":
                        method.AccessLevel = AccessLevel.Public;
                        break;
                    case "private":
                        method.AccessLevel = AccessLevel.Private;
                        break;
                    case "protected":
                        method.AccessLevel = AccessLevel.Protected;
                        break;
                    case "static":
                        method.IsStatic = true;
                        break;
                    default:
                        throw new Error("invalid method modifiers declaration [At line: " + (tokens[index].Line) + "]");
                }
            method.isModified = true;

            index++;
            if (tokens[index].Object == Scanner.Comma)
            {
                index++;
                Optimizer(ref index, tokens, method);
            }
        }

        private static void Parameter(ref int index, IList<GStringObject> tokens, Method method)
        {
            bool isPassByRefernce = false;
            index++;
            if (tokens[index].Object != Scanner.Colon)
                throw new Error("require :[Colon] after parameters [At line: " + tokens[index].Line + "]");

            index++;
            if (tokens[index].Object != Scanner.Left_Braces)
                throw new Error("require {[Left Braces] after :[Colon] [At line: " + tokens[index].Line + "]");

            index++;

            do
            {
                if (tokens[index].Object == Scanner.Comma) { }
                else if (tokens[index].Object == Scanner.Left_Bracket)
                {
                    do
                    {
                        index++;
                        if (tokens[index].Object == Scanner.Comma) { }
                        else if (tokens[index].Object.Equals("pass"))
                        {
                            index++;
                            if (!tokens[index].Object.Equals("by"))
                                throw new Error("require 'by' after 'pass' [At line: " + (tokens[index].Line) + "]");

                            index++;
                            if (tokens[index].Object.Equals("reference"))
                                isPassByRefernce = true;
                            else if (tokens[index].Object.Equals("value"))
                                isPassByRefernce = false;
                            else
                                throw new Error("require 'reference' or 'value' after 'by' [At line: " + (tokens[index].Line) + "]");
                        }
                        else if (tokens[index].Object is string)
                        {
                            // Need Implement, assume a as string.
                            ParamInside(ref index, tokens, method, isPassByRefernce);
                            isPassByRefernce = false;
                        }
                    } while (tokens[index].Object != Scanner.Right_Bracket);
                }
                else if (tokens[index].Object is string)
                {
                    // Need Implement, assume a as string.
                    ParamInside(ref index, tokens, method, isPassByRefernce);
                    isPassByRefernce = false;
                }
                else if (tokens[index + 1].Object == Scanner.Right_Braces) { }
                //else if (methodParams.Count == 0) { }
                else throw new Error("invalid method parameter declaration [At line: " + (tokens[index].Line) + "]");
                index++;
            } while (tokens[index].Object != Scanner.Right_Braces);

            index++;
            if (tokens[index].Object == Scanner.Comma)
            {
                index++;
                Optimizer(ref index, tokens, method);
            }
        }

        private static void ParamInside(ref int index, IList<GStringObject> tokens, Method method, bool isReference)
        {
            Type ParameterType;

            string ident = (string)tokens[index].Object;
            if (ident.IsReserved())
                throw new Error("'" + ident + "' is reserved keyword [At line: " + tokens[index].Line + "]");

            string ParamName = ident;

            index++;
            if (!tokens[index].Object.Equals("as"))
            {
                throw new Error("require 'as' after parameter name [At line: " + (tokens[index - 1].Line) + "]");
            }

            index++;
            var type = GSType.GetTypeByTypeText((string)tokens[index].Object, CollectionType.None, true, isReference);
            ParameterType = type;

            method.MethodParams.Add(new MethodParam
            {
                IsPassByRef = isReference,
                ParameterType = ParameterType,
                ParamName = ParamName,
                IsMethodStatic = method.IsStatic,
                Order = method.MethodParams.Count()
            });
        }
    }
}
