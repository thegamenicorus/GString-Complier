using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GStringCompiler
{
    class BracketReadOperation
    {

        public static Stmt Synthesize(ref int index, IList<GStringObject> tokens, Stmt parser)
        {
            List<Stmt> stmt = null;
            index++;
            if (tokens[index].Object.Equals("Read"))
            {
                index++;

                if (tokens[index].Object.Equals("input"))
                    stmt = ReadValue.Synthesize(ref index, tokens, parser);
                else
                    throw new Error("require 'input' after 'Read' [At line: " + tokens[index].Line + "]");
            }

            if (stmt != null)
                if (tokens[index].Object == Scanner.Right_Braces)
                {
                    index++;
                    return QueueTreeController.AssignSequence(stmt, ref index, tokens);
                }
                else
                    throw new Error("require }[Right bracket] [At line: " + tokens[index].Line + "]");
            else
                throw new Error("unidentified error has occur [At line: " + tokens[index].Line + "]");
        }
    }
}
