using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GStringCompiler
{
    class QueueTreeController
    {
        public static Stmt AssignSequence(System.Collections.Generic.List<Stmt> seqlist, ref int index, IList<GStringObject> tokens)
        {
            if ((index < tokens.Count && tokens[index].Object == Scanner.FullStop) || tokens[index].Object.Equals("End"))
            {
                index++;
                if (index < tokens.Count &&
                    !tokens[index].Object.Equals("end"))
                {
                    Sequence sequence = new Sequence();
                    sequence.First = AssignSequence(seqlist, 0);
                    sequence.Second = Parser.Getinstance().ParseStmt();
                    return sequence;
                }
            }
            return null;
        }

        public static Stmt AssignSequence(System.Collections.Generic.List<Stmt> seqlist)
        {
            return AssignSequence(seqlist, 0);
        }

        private static Stmt AssignSequence(System.Collections.Generic.List<Stmt> seqlist, int index)
        {
            if (index == seqlist.Count - 1)
                return seqlist[index];
            Sequence sequence = new Sequence();
            sequence.First = seqlist[index];
            sequence.Second = AssignSequence(seqlist, index + 1);
            return sequence;
        }
    }
}
