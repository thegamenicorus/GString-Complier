using System.Collections.Generic;
using System;
using System.Text;
using System.Linq;
using GStringCompiler.Extension;

namespace GStringCompiler
{
    // var <ident> = <expr>
    public class DeclareLocalVar : Stmt
    {
        public string Ident;
        public Expr Expr;
        public Type FixedType;


        public static Stmt Synthesize(ref int index, IList<GStringObject> tokens)
        {
            List<Stmt> stmtList = new List<Stmt>();
            DeclareLocalVar declareLocal = new DeclareLocalVar();
            stmtList.Add(declareLocal);
            index++;
            if (index < tokens.Count && tokens[index].Object is string)
            {
                string ident = (string)tokens[index].Object;
                if (ident.IsReserved())
                    throw new Error("'" + ident + "' is reserved keyword [At line: " + tokens[index].Line + "]");
                declareLocal.Ident = ident;
            }
            else
            {
                throw new Error("require variable name after 'Declare' [At line: " + tokens[index].Line + "]");
            }

            index++;


            if (index != tokens.Count)
            {
                if (tokens[index].Object.Equals("as"))
                {
                    index++;
                    if (tokens[index].Object.Equals("array"))
                    {
                        index++;
                        if (!tokens[index].Object.Equals("of") || index == tokens.Count)
                            throw new Error("require 'of' after 'array' [At line: " + tokens[index].Line + "]");

                        index++;
                        declareLocal.Expr = new LocalArray() { Ident = declareLocal.Ident };
                        InstanceFixedType(ref declareLocal, tokens[index], ref index, CollectionType.Array);
                        (declareLocal.Expr as LocalArray).Type = declareLocal.FixedType;

                        var res = UnsortedFinder.Find(
                        new List<FinderObject>{
                        new FinderObject{Keyword="size",ExpectedType=typeof(int)}
                        }
                        , ref index, tokens, new List<string>());

                        if (res[0] != null)
                            (declareLocal.Expr as LocalArray).Size = (List<object>)res[0];
                    }
                    else if (tokens[index].Object.Equals("list"))
                    {
                        index++;
                        if (!tokens[index].Object.Equals("of") || index == tokens.Count)
                            throw new Error("require 'of' after 'list' [At line: " + tokens[index].Line + "]");

                        index++;
                        InstanceFixedType(ref declareLocal, tokens[index], ref index, CollectionType.List);
                    }
                    else
                    {
                        InstanceFixedType(ref declareLocal, tokens[index], ref index, CollectionType.None);
                    }

                    index++;
                    if (tokens[index].Object == Scanner.FullStop)
                    {
                        if (declareLocal.Expr == null)
                            declareLocal.Expr = Parser.Getinstance().ParseExpr(declareLocal.FixedType);
                    }
                    else if (tokens[index].Object == Scanner.Comma)
                    {
                        index++;
                        if (!tokens[index].Object.Equals("value") || index == tokens.Count)
                            throw new Error("require statement after ,[Comma] [At line: " + tokens[index].Line + "]");

                        index++;
                        if (tokens[index].Object != Scanner.Colon || index == tokens.Count)
                            throw new Error("require :[Colon] after 'value' [At line: " + tokens[index].Line + "]");

                        index++;
                        if (tokens[index].Object != Scanner.Left_Braces || index == tokens.Count)
                            throw new Error("require {[Left braces] after :[Colon] [At line: " + tokens[index].Line + "]");

                        index++;

                        ValueSynthesizer.DeclareLocalVarValueSynthesize(ref declareLocal, ref stmtList, ref index, tokens);
                    }
                    else
                    {
                        if (tokens[index].Object.Equals("value") || index == tokens.Count)
                            throw new Error("require ,[Comma] after variable type [At line: " + tokens[index].Line + "]");
                        else
                            throw new Error("require 'as' or ,[Comma] after variable name [At line: " + tokens[index].Line + "]");
                    }

                }
                else if (tokens[index].Object == Scanner.Comma)
                {
                    index++;
                    if (!tokens[index].Object.Equals("value") || index == tokens.Count)
                        throw new Error("require statement after ,[Comma] [At line: " + tokens[index].Line + "]");

                    index++;
                    if (tokens[index].Object != Scanner.Colon || index == tokens.Count)
                        throw new Error("require :[Colon] after 'value' [At line: " + tokens[index].Line + "]");

                    index++;
                    if (tokens[index].Object != Scanner.Left_Braces || index == tokens.Count)
                        throw new Error("require {[Left braces] after :[Colon] [At line: " + tokens[index].Line + "]");

                    index++;

                    ValueSynthesizer.DeclareLocalVarValueSynthesize(ref declareLocal, ref stmtList, ref index, tokens);
                }
                else if (tokens[index].Object == Scanner.FullStop || index == tokens.Count)
                    throw new Error("cannot declare variable without define variable type [At line: " + tokens[index].Line + "]");
                else
                {
                    if (tokens[index].Object.Equals("value") || index == tokens.Count)
                        throw new Error("require ,[Comma] after variable name [At line: " + tokens[index].Line + "]");
                    else
                        throw new Error("require 'as' or ,[Comma] after variable name [At line: " + tokens[index].Line + "]");
                }
            }
            else
            {
                throw new Error("require .[Full stop] or ,[Comma] after 'Declare ident' [At line: " + tokens[index].Line + "]");
            }

            // Keep declared localVar.
            if (Parser.Getinstance().mainDeclaredLocalVar[Parser.Getinstance().CurrentSynMethod].SingleOrDefault(i => i.Ident == declareLocal.Ident) == null)
                Parser.Getinstance().mainDeclaredLocalVar[Parser.Getinstance().CurrentSynMethod].Add(declareLocal);

            if (index < tokens.Count - 1 && !tokens[index].Object.Equals("end"))
                return QueueTreeController.AssignSequence(stmtList, ref index, tokens);
            else
                return QueueTreeController.AssignSequence(stmtList);
        }

        private static void InstanceFixedType(ref DeclareLocalVar declareLocal, GStringObject GSobj, ref int index, CollectionType collectionType)
        {
            declareLocal.FixedType = GSType.GetTypeByTypeText(GSobj.Object.ToString(), collectionType);
            if (declareLocal.FixedType == null)
                throw new Error("Unknown type: '" + GSobj.Object + "'[At line: " + GSobj.Line + "]");
        }
    }
}
