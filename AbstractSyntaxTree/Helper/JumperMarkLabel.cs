using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GStringCompiler
{
    public class JumperMarkLabel
    {
        private int loop = 0;
        private int condition = 0;
        private static JumperMarkLabel jml;
        public string MarkLabelLoop { get { return "LoopMarkLabel" + (loop++); } }

        public static JumperMarkLabel GetInstance()
        {
            if (jml == null)
                jml = new JumperMarkLabel();
            return jml;
        }
    }
}
