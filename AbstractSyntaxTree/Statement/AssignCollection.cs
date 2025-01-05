using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GStringCompiler
{
    public class AssignCollection : Stmt
    {
        public CollectionType CollectionType { get; set; }
        public Type TypeOfExpr { get; set; }
        public string Ident { get; set; }
        public int[] Index { get; set; }
        public object Value { get; set; }
    }
}
