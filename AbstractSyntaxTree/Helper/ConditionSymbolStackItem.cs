using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GStringCompiler//.AbstractSyntaxTree.Helper
{
    class ConditionSymbolStackItem
    {
        public LogicalSymbolExpr Symbol { get; set; }
        public bool IsWalked { get; set; }
    }
}
