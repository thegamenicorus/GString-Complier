using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GStringCompiler.Extension
{
    public static class TypeExtension
    {
        public static Type ToReferenceType(this Type source)
        {
            if (!source.Name.Last().Equals('&'))
                return Type.GetType(source.FullName + "&");
            return source;
        }
    }
}
