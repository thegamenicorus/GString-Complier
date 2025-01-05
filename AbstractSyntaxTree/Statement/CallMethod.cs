using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GStringCompiler.Extension;

namespace GStringCompiler
{
    public class CallMethodArgs
    {
        public Expr ArgsExpr { get; set; }
        public bool IsPassByRef { get; set; }
        public Type ArgsType { get; set; }
    }

    public class CallMethod : Stmt, Expr, Ornament
    {
        public List<CallMethodArgs> MethodArgs { get; set; }
        public bool IsVirtCall { get; set; }
        public bool IsStatic { get; set; }
        public string Ident;
        public Type RefType { get; set; }
        public Type ReturnType { get; set; }
        // Store next call method.
        public CallMethod NextCallMethod { get; set; }
        public System.Reflection.MethodInfo PromptCallMethod = null;

        public static Stmt Synthesize(ref int index, IList<GStringObject> tokens, bool isVirCall = false, bool isExpr = false, Type refType = null)
        {
            List<Stmt> stmtList = new List<Stmt>();

            CallMethod callMethod = new CallMethod
            {
                MethodArgs = new List<CallMethodArgs>(),
                IsVirtCall = isVirCall,
                RefType = refType,
            };
            stmtList.Add(callMethod);

            index++;
            if (tokens[index].Object != Scanner.Colon)
                throw new Error("require :[Colon] after 'Call method' [At line: " + tokens[index].Line + "]");

            index++;
            if (tokens[index].Object != Scanner.Left_Braces)
                throw new Error("require {[Left braces] after :[Colon] [At line: " + tokens[index].Line + "]");


            string ident = DotMerger.MergeDot(ref index, tokens, new object[] { Scanner.Comma });

            if (string.IsNullOrEmpty(ident))
                throw new Error("require method name after {[Left braces]  [At line: " + tokens[index].Line + "]");
            else
            {
                callMethod.Ident = ident;
            }

            if (tokens[index].Object == Scanner.Comma)
            {
                index++;
                Optimizer(ref index, tokens, callMethod);
            }

            // Not define in internal, can check method exist and their return type.
            if (callMethod.RefType != null && callMethod.ReturnType == null && callMethod.MethodArgs.Where(x => x.ArgsType == null).Count() == 0)
            {
                callMethod.PromptCallMethod =
                    GSType.GetMethodFromRefType(callMethod.RefType, callMethod.Ident, callMethod.MethodArgs.Select(x => x.ArgsType).ToArray(), callMethod.IsStatic, tokens[index].Line);

                if (callMethod.PromptCallMethod == null)
                {
                    var param = string.Join(",", Enumerable.Range(0, callMethod.MethodArgs.Count()).Select(i => "(" + callMethod.MethodArgs[i].ArgsType.Name + ")").ToArray());
                    throw new Error("Method: '" + callMethod.Ident + "' with types: '" + param + "' in class: '" + callMethod.RefType.Name + "' is undefined");
                }

                callMethod.ReturnType = callMethod.PromptCallMethod.ReturnType;
            }

            if (tokens[index].Object == Scanner.Right_Bracket) { return callMethod; }

            if (tokens[index].Object == Scanner.Comma) { }
            else if (tokens[index].Object == Scanner.Right_Braces) { index++; }
            else
            {
                throw new Error("require }[Right braces] after method name [At line: " + (tokens[index].Line) + "]");
            }

            if (!isExpr)
            {
                if (tokens[index].Object != Scanner.FullStop)
                    throw new Error("require .[Full Stop] after :[Colon] [At line: " + (tokens[index].Line) + "]");
            }
            else if (tokens[index].Object == Scanner.Comma)
            {
                if (tokens[index + 1].Object.Equals("call"))
                {
                    // Keyword: Call.
                    index++;
                    // Keyword: method.
                    index++;
                    if (tokens[index].Object.Equals("method"))
                        callMethod.NextCallMethod = (CallMethod)CallMethod.Synthesize(ref index, tokens, true, true, callMethod.ReturnType);
                    else
                        throw new Error("'call " + tokens[index].Object + "' is undefined [At line: " + tokens[index].Line + "]");
                }
            }

            return callMethod;
        }

        private static void Optimizer(ref int index, IList<GStringObject> tokens, CallMethod callMethod)
        {
            switch (tokens[index].Object.ToString())
            {
                case "arguments":
                    Arguments(ref index, tokens, callMethod);
                    break;
                case "call":
                    index++;
                    if (tokens[index].Object.Equals("method"))
                        callMethod.NextCallMethod = (CallMethod)CallMethod.Synthesize(ref index, tokens, true, true, callMethod.ReturnType);
                    else
                        throw new Error("'call " + tokens[index].Object + "' is undefined [At line: " + tokens[index].Line + "]");
                    break;
                default:
                    break;
            }
        }

        private static void Arguments(ref int index, IList<GStringObject> tokens, CallMethod callMethod)
        {
            bool isPassByRefernce = false;
            index++;
            if (tokens[index].Object != Scanner.Colon)
                throw new Error("require :[Colon] after modifiers [At line: " + tokens[index].Line + "]");

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
                        else if (tokens[index].Object == Scanner.Right_Braces) { }
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

                            index++;
                        }
                        else
                        {
                            // Need Implement, assume a as string.
                            ArgsInside(ref index, tokens, callMethod, isPassByRefernce);
                            isPassByRefernce = false;
                        }
                    } while (tokens[index].Object != Scanner.Right_Braces);
                }
                else
                {
                    // Need Implement, assume a as string.
                    ArgsInside(ref index, tokens, callMethod, isPassByRefernce);
                    if (callMethod.MethodArgs.Last().ArgsExpr == null)
                        callMethod.MethodArgs.Remove(callMethod.MethodArgs.Last());
                    isPassByRefernce = false;
                }
                index++;
            } while (tokens[index].Object != Scanner.Right_Braces);

            // Not define in internal, can check method exist and their return type.
            if (callMethod.RefType != null && callMethod.MethodArgs.Where(x => x.ArgsType == null).Count() == 0)
            {

                callMethod.PromptCallMethod = GSType.GetMethodFromRefType(callMethod.RefType, callMethod.Ident, callMethod.MethodArgs.Select(x => x.ArgsType).ToArray(), callMethod.IsStatic);
                if (callMethod.PromptCallMethod == null)
                {
                    var param = string.Join(",", Enumerable.Range(0, callMethod.MethodArgs.Count()).Select(i => "(" + callMethod.MethodArgs[i].ArgsType.Name + ")").ToArray());
                    throw new Error("Method: '" + callMethod.Ident + "' with types: '" + param + "' in class: '" + callMethod.RefType.Name + "' is undefined");
                }
                callMethod.ReturnType = callMethod.PromptCallMethod.ReturnType;
            }

            index++;
            if (tokens[index].Object == Scanner.Comma)
            {
                index++;
                Optimizer(ref index, tokens, callMethod);
            }
            else if (tokens[index].Object == Scanner.FullStop)
            {
                index--;
            }
        }

        private static void ArgsInside(ref int index, IList<GStringObject> tokens, CallMethod callMethod, bool isPassByRefernce)
        {

            CallMethodArgs cm = new CallMethodArgs
            {
                IsPassByRef = isPassByRefernce,
            };

            callMethod.MethodArgs.Add(cm);

            if (OrderController.IsSeqExpr(index, tokens, new object[] { Scanner.Comma }))
            {
                index--;
                // If args is seq but not use ( and ).
                if (tokens[index].Object != Scanner.Left_Bracket)
                {
                    index++;
                }
                var argsExpr = OrderController.GetSequenceOfOperation(ref index, tokens, new object[] { Scanner.Comma }, null);
                if (argsExpr.Value != null)
                {
                    cm.ArgsExpr = argsExpr;
                    cm.ArgsType = TypeIdentifier.TypeOfExpr(cm.ArgsExpr);
                }
                index--;
            }
            else if (tokens[index].Object is LogicalSymbol)
            {
                LogicalSymbol ls = tokens[index].Object as LogicalSymbol;
                if (ls.Symbol == "!")
                {
                    index++;
                    var expr = OrderController.GetSequenceOfOperation(ref index, tokens, new object[] { Scanner.Comma }, null);
                    cm.ArgsExpr = new NotSymbolSequenceExpr { Value = expr.Value };
                    cm.ArgsType = TypeIdentifier.TypeOfExpr(cm.ArgsExpr);
                }
                else if (ls.Symbol == "-")
                {
                    var expr = OrderController.GetSequenceOfOperation(ref index, tokens, new object[] { Scanner.Comma }, null);
                    if (tokens[index].Object != Scanner.Right_Braces || index == tokens.Count)
                        throw new Error("require }[Right braces] after'" + tokens[index - 1].Object + "' [At line: " + tokens[index].Line + "]");
                    index++;
                }
            }
            else if (tokens[index].Object == Scanner.Left_Braces && tokens[index + 1].Object == Scanner.Right_Braces)
            {
                callMethod.MethodArgs.Remove(cm);
            }
            else
            {
                int line = tokens[index].Line;
                string ident = tokens[index].Object.ToString();
                cm.ArgsExpr = Parser.Getinstance().ParseExpr();
                cm.ArgsType = TypeIdentifier.TypeOfExpr(cm.ArgsExpr);
                if (callMethod.IsVirtCall)
                    index--;
                if (isPassByRefernce)
                {
                    cm.ArgsType = cm.ArgsType.ToReferenceType();
                }
            }
        }
    }
}
