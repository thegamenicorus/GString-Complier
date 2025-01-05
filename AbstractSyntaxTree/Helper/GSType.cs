using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace GStringCompiler
{
    class GSType
    {
        public static Type GetTypeByTypeText(string typeText, CollectionType colType = CollectionType.None, bool isReturnTypeOfMethod = false, bool isPassByReference = false)
        {
            // Default type.
            var type = GetDefaultType(typeText, colType, isReturnTypeOfMethod, isPassByReference);

            if (type != null)
                return type;

            return null;

        }

        private static Type GetDefaultType(string typeText, CollectionType colType = CollectionType.None, bool isReturnTypeOfMethod = false, bool isPassByReference = false)
        {
            string typeFullName = string.Empty;

            switch (typeText)
            {
                case "bool":
                    typeFullName = "System.Boolean"; break;
                case "short":
                    typeFullName = "System.Int16"; break;
                case "int":
                    typeFullName = "System.Int32"; break;
                case "long":
                    typeFullName = "System.Int64"; break;
                case "string":
                    if (isReturnTypeOfMethod == true)
                        typeFullName = "System.String";
                    else if (colType == CollectionType.None)
                        return typeof(StringBuilder);
                    else
                        typeFullName = "System.String"; break;

                case "float":
                    typeFullName = "System.Single"; break;
                case "double":
                    typeFullName = "System.Double"; break;
                case "void":
                    return typeof(void);
            }

            if (isPassByReference)
            {
                typeFullName += "&";
            }

            return Type.GetType(typeFullName);
        }

        public static MethodInfo GetMethodFromRefType(Type refType, string ident, Type[] args, bool isStatic, int line = 0)
        {
            try
            {
                MethodInfo result = refType.GetMethod(ident, args);
                if (result == null)
                {
                    result = refType.GetMethod(ident, args.ToList().Select(i => typeof(object)).ToArray());
                    if (result == null)
                    {
                        if (refType.BaseType != null)
                        {
                            return GetMethodFromRefType(refType.BaseType, ident, args, isStatic, line);
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                if (line != 0)
                {
                    throw new Error(ex.Message + " [At line: " + line + "]");
                }
                else
                {
                    throw new Error(ex.Message);
                }
            }
        }

        public static bool ConvertStaticTypeChecker(Type type)
        {
            return (type == typeof(bool) || type == typeof(short) || type == typeof(int) || type == typeof(long) ||
               type == typeof(float) || type == typeof(double));
        }

        public static bool IsValueType(Type type)
        {
            return type == typeof(string) || type == typeof(bool) || type == typeof(short) || type == typeof(int) || type == typeof(long) ||
               type == typeof(float) || type == typeof(double);
        }
    }
}
