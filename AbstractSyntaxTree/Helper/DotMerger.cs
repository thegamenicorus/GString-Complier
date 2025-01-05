using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GStringCompiler
{
    class DotMerger
    {
        public static string MergeDot(ref int index, IList<GStringObject> tokens)
        {
            return MergeDotInner(ref index, tokens, new object[] { Scanner.Right_Braces });
        }

        public static string MergeDot(ref int index, IList<GStringObject> tokens, object stopper)
        {
            return MergeDotInner(ref index, tokens, new object[] { stopper });
        }

        public static string MergeDot(ref int index, IList<GStringObject> tokens, object[] stopper)
        {
            return MergeDotInner(ref index, tokens, stopper);
        }

        private static string MergeDotInner(ref int index, IList<GStringObject> tokens, object[] stopper)
        {
            string result = "";
            do
            {
                index++;
                if (tokens[index].Object == Scanner.FullStop)
                    result += ".";
                else if (tokens[index].Object is string)
                    result += tokens[index].Object;
            } while (!stopper.Contains(tokens[index].Object) && tokens[index].Object != Scanner.Right_Braces);
            return result;
        }
    }
}
