using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GStringCompiler
{
    public class DisplayObjectArray:GStringCollection
    {
        public DisplayObjectArray()
        {
            this.Ident = "Display_Object_Array";
            this.Type = typeof(object);            
        }
    }
}
