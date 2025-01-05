using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GStringCompiler
{

    public class FinderObject
    {
        public string Keyword;
        public Type ExpectedType;
    }

    public class UnsortedFinder
    {
        public static object[] Find(List<FinderObject> searchObj, ref int index, IList<GStringObject> tokens, List<string> avoidStmt)
        {
            int current = index;
            int expectedLine = tokens[current].Line;
            object[] result = new object[searchObj.Count];
            int[] refIndex = new int[searchObj.Count];

            while (current < tokens.Count && searchObj.Count() != 0 && tokens[current].Object != Scanner.FullStop)
            {
                for (int i = 0; i < searchObj.Count; i++)
                {
                    if (avoidStmt.Contains(tokens[current].Object.ToString()))
                        return result;
                    else if (searchObj[i].Keyword.Equals(tokens[current].Object))
                    {
                        current++;
                        if (tokens[current].Object != Scanner.Colon)
                            throw new Error("require :[Colon] after '" + tokens[current - 1].Object + "' [At line: " + tokens[current].Line + "]");

                        current++;
                        if (tokens[current].Object != Scanner.Left_Braces)
                        {
                            if (searchObj[i].ExpectedType != tokens[current].Object.GetType() || tokens[current].Object == Scanner.FullStop)
                                throw new Error("require type:'" + searchObj[i].ExpectedType.ToString() + "' for value of '" + searchObj[i].Keyword + "'  [At line: " + tokens[current].Line + "]");
                            else
                            {
                                refIndex[i] = current;
                                result[i] = tokens[current].Object;
                                index = refIndex.Max();
                                searchObj.RemoveAt(i);
                            }
                            break;
                        }
                        else
                        {
                            current++;
                            List<object> res = new List<object>();
                            while (tokens[current].Object != Scanner.Right_Braces && current <= tokens.Count)
                            {
                                // Keep val will gen at GenArrayExpr in CodeGen.cs.
                                if (OrderController.IsSeqExpr(current, tokens, Scanner.Comma))
                                {
                                    res.Add(OrderController.GetSequenceOfOperation(ref current, tokens, Scanner.Comma, null));
                                    current--;
                                }
                                else if (searchObj[i].ExpectedType == tokens[current].Object.GetType())
                                    res.Add(new IntLiteral { Value = (int)tokens[current].Object });
                                else if (typeof(string) == tokens[current].Object.GetType())
                                    res.Add(new Variable { Ident = tokens[current].Object.ToString() });

                                // Skip comma.
                                else if (tokens[current].Object == Scanner.Comma) { }
                                else
                                    throw new Error("require type:'" + searchObj[i].ExpectedType.ToString() + "' for value of '" + searchObj[i].Keyword + "'  [At line: " + tokens[current].Line + "]");

                                if ((tokens[current].Object == Scanner.Comma && tokens[current + 1].Object == Scanner.Comma)
                                    || (tokens[current].Object == Scanner.Comma && tokens[current + 1].Object == Scanner.Right_Braces))
                                    res.Add(Type.EmptyTypes);

                                current++;
                            }
                            if (tokens[current].Object != Scanner.Right_Braces)
                                throw new Error("require }[Right Brace] [At line: " + tokens[current].Line + "]");

                            result[i] = res;
                            index = current;
                        }
                        return result;
                    }
                }
                current++;
            }
            return result;
        }
    }
}
