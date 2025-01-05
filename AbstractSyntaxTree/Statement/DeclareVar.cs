using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GStringCompiler
{
    // var <ident> = <expr>
    public class DeclareVar : Stmt
    {
        public string Ident;
        public Expr Expr;
        public Type FixedType;



        public static DeclareVar Synthesize(ref int index, IList<object> tokens)
        {
            DeclareVar declare = new DeclareVar();
            index++;
            if (index < tokens.Count && tokens[index] is string)
            {
                declare.Ident = (string)tokens[index];
            }
            else
            {
                throw new System.Exception("Compiel error : expected variable name after 'Declare'");
            }

            index++; 
            

            if (index != tokens.Count) 
            {
                if (tokens[index].Equals("as"))
                {
                    index++;
                    if (tokens[index].Equals("integer"))
                        declare.FixedType = typeof(int);
                    else if(tokens[index].Equals("string"))
                        declare.FixedType = typeof(StringBuilder);
                    //and etc --------------------------------

                    index++;
                    if (tokens[index] == Scanner.FullStop)
                        declare.Expr = Parser.Getinstance().ParseExpr(declare.FixedType);
                    else if (tokens[index] == Scanner.Comma)
                    {
                        index++;
                        if (tokens[index].Equals("value"))
                        {
                            index++;
                            if(tokens[index] == Scanner.Equal)
                            {
                                index++;
                                if(declare.FixedType == tokens[index].GetType())
                                    declare.Expr = Parser.Getinstance().ParseExpr();
                                else
                                    throw new System.Exception("Compile error : type of value is not match");
                            }
                            else
                                throw new System.Exception("Compile error : need =[Equal] after 'value'");
                        }
                        else
                            throw new System.Exception("Compile error : need statement after ,[Comma]");
                    }
                }
                else if (tokens[index] == Scanner.Comma)
                {
                    index++;
                    if (tokens[index].Equals("value"))
                    {
                        index++;
                        if (tokens[index] == Scanner.Equal)
                        {
                            index++;
                                declare.Expr = Parser.Getinstance().ParseExpr();
                        }
                        else
                            throw new System.Exception("Compile error : need =[Equal] after 'value'");
                    }
                    else
                        throw new System.Exception("Compile error : need statement after ,[Comma]");
                }
            }
            else
            {
                throw new System.Exception("Compile error : expected .[Full stop] or ,[Comma] after 'Declare ident'");
            }

            //index++;

            //declare.Expr = Parser.Getinstance().ParseExpr();
            return declare;
        }
    }
}
