using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GStringCompiler
{
    public interface Ornament
    {
    }

    public class VariableIndex : Ornament
    {
        public List<object> Index { get; set; }
        public VariableIndex() { Index = new List<object>(); }
    }
}
