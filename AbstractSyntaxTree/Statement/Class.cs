using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GStringCompiler
{
    public class Project
    {
        public List<Class> Classes { get; set; }
        public Project() { Classes = new List<Class>(); }
    }

    public class BuiltMethodAndParam
    {
        public System.Reflection.Emit.MethodBuilder BuiltMethod { get; set; }
        public Type[] Parameters { get; set; }
        public Method ReferenceGSMethodModel { get; set; }
    }

    public class Class
    {
        public AccessLevel ClassAccessLevel {get;set;}
        public bool IsStatic { get; set; }
        public List<Method> Methods { get; set; }
        public List<Field> Fields { get; set; }
        public string Namespace { get; set; }
        public string Name { get; set; }
        private bool isModified;
        // private bool isInherited;
        // private bool isImpled;
        public List<BuiltMethodAndParam> MethodBuilderKeeper { get; set; }
        public System.Reflection.Emit.TypeBuilder ClsBuilder { get; set; }

        public Class()
        {
            Methods = new List<Method>();
            Fields = new List<Field>();
            MethodBuilderKeeper = new List<BuiltMethodAndParam>();
            ClassAccessLevel = AccessLevel.Private;
        }

        public static Class SynthesizeClass(ref int index, IList<GStringObject> tokens, string ns)
        {
            Class c = new Class();
            Parser.Getinstance().CurrentSynClass = c;
            Parser.Getinstance().CurrentSynMethod = null;
            c.Namespace = ns;

            index++;
            if (tokens[index].Object != Scanner.Colon)
                throw new Error("require :[Colon] after Class [At line: " + tokens[index].Line + "]");

            index++;
            if (tokens[index].Object != Scanner.Left_Braces)
                throw new Error("require {[Left Braces] after :[Colon] [At line: " + tokens[index].Line + "]");

            index++;
            if (tokens[index].Object is string)
                c.Name = tokens[index].Object.ToString();
            else
                throw new Error("invalid class name [At line: " + tokens[index].Line + "]");

            index++;
            if (tokens[index].Object != Scanner.Right_Braces)
                throw new Error("require }[Right Braces] after class name [At line: " + tokens[index].Line + "]");

            index++;
            if (tokens[index].Object == Scanner.Comma)
            {
                index++;
                ClassOptimizer(ref index, tokens, c);
            }
            if (tokens[index].Object != Scanner.FullStop)
                throw new Error("require .[Full Stop] after :[Colon] [At line: " + (tokens[index].Line - 1) + "]");

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
            if (!tokens[index].Object.Equals("End") || index == tokens.Count)
                throw new Error("unterminated 'Class' body, require 'End class.' [At line: " + tokens[index].Line + "]");

            index++;
            if (!tokens[index].Object.Equals("class") || index == tokens.Count)
                throw new Error("unterminated 'Class' body, require 'End class.' [At line: " + tokens[index].Line + "]");

            index++;
            if (tokens[index].Object != Scanner.FullStop || index == tokens.Count)
                throw new Error("require .[Full stop] [At line: " + tokens[index].Line + "]");

            Parser.Getinstance().CurrentSynClass = null;
            return c;
        }

        public static void ClassOptimizer(ref int index,IList<GStringObject> tokens,Class c)
        {
            switch (tokens[index].Object.ToString())
            {
                case "modifiers":
                    ClassModifier(ref index, tokens, c);
                    break;
                case "inherite":
                    break;
                case "implement":
                    break;
                default:
                    throw new Error("invalid class declaration [At line: " + (tokens[index].Line - 1) + "]");
            }
        }

        private static void ClassModifier(ref int index, IList<GStringObject> tokens, Class c)
        {
            // For private,protected,public.
            //bool P_Mode = false; 
            // For static.
            //bool S_Mode = false;
            if(c.isModified)
                throw new Error("cannot do multiple class modifiers [At line: " + (tokens[index].Line - 1) + "]");

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
                else throw new Error("invalid class modifiers declaration [At line: " + (tokens[index].Line) + "]");
                index++;
            } while (tokens[index].Object != Scanner.Right_Braces);

            if (modStr.Count > 2) throw new Error("invalid class modifiers declaration [At line: " + (tokens[index].Line) + "]");

            foreach(var mode in modStr)
                switch (mode.ToLower())
                { 
                    case "public":
                        c.ClassAccessLevel = AccessLevel.Public;
                        break;
                    case "private":
                        c.ClassAccessLevel = AccessLevel.Private;
                        break;
                    case "protected":
                        c.ClassAccessLevel = AccessLevel.Protected;
                        break;
                    case "static":
                        c.IsStatic = true;
                        break;
                    default:
                        throw new Error("invalid class modifiers declaration [At line: " + (tokens[index].Line) + "]");
                }         
            c.isModified = true;

            index++;
            if (tokens[index].Object == Scanner.Comma)
            {
                index++;
                ClassOptimizer(ref index, tokens, c);
            }
        }
    }
}
