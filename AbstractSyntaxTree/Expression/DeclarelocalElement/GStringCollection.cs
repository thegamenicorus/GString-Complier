using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GStringCompiler
{
    public class GStringCollection : VarDecExpr
    {
        public string Ident { get; set; }
        public List<object> Size { get; set; }
        public List<object> Element { get; set; }
        public Type Type { get; set; }
    }
}
