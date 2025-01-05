using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GStringCompiler
{
    public class TypeIdentifier
    {
        private static int lineOfStmt;
        private static Method metKey = null;
        private static CodeGen codeGen = new CodeGen();

        public static void LoadMethod(Method method)
        {
            metKey = method;
        }

        public static Type FindTypeOfVariable(string Ident, int line)
        {
            lineOfStmt = line;
            DeclareLocalVar declared = Parser.Getinstance().mainDeclaredLocalVar[metKey].SingleOrDefault(i => i.Ident == Ident);

            if (declared == null)
            {
                if (Ident == "true" || Ident == "false") { return typeof(bool); }

                // Method params.
                var inMethod = metKey.MethodParams.SingleOrDefault(i => i.ParamName.Equals(Ident));
                if (inMethod != null)
                    return metKey.MethodParams[inMethod.Order].ParameterType;
                else
                    // TODO: Need impl for class global.
                    throw new Error("variable: " + Ident + " is not declared [At line: " + line + "]");
            }
            else if (declared.Expr != null)
                return TypeOfExpr(declared.Expr);
            else if (declared.FixedType == typeof(System.Text.StringBuilder))
                return typeof(string);
            return declared.FixedType;
        }

        public static bool IsGStringCollection(string Ident, int line)
        {
            lineOfStmt = line;
            DeclareLocalVar declared = Parser.Getinstance().mainDeclaredLocalVar[metKey].SingleOrDefault(i => i.Ident == Ident);
            if (declared == null)
            {
                var inMethod = metKey.MethodParams.SingleOrDefault(i => i.ParamName.Equals(Ident));
                if (inMethod != null)
                    // May need to change in future.
                    return metKey.MethodParams[inMethod.Order].ParameterType.IsArray;
                else
                    throw new Error("variable: " + Ident + " is not declared [At line: " + line + "]");
            }
            else if (declared.Expr != null)
            {
                if (declared.Expr is GStringCollection)
                    return true;
            }
            return false;
        }

        public static bool IsArray(string Ident)
        {
            DeclareLocalVar declared = Parser.Getinstance().mainDeclaredLocalVar[metKey].SingleOrDefault(i => i.Ident == Ident);
            if (declared == null)
            {
                // Method params.
                var inMethod = metKey.MethodParams.SingleOrDefault(i => i.ParamName.Equals(Ident));
                if (inMethod != null)
                    return metKey.MethodParams[inMethod.Order].ParameterType.IsArray;
                else
                    // TODO: Need impl for class global.
                    throw new Error("variable: " + Ident + " is not declared");
            }
            else if (declared.Expr != null)
            {
                if (declared.Expr is LocalArray)
                    return true;
            }
            return false;
        }

        public static bool IsArray(string Ident, int line)
        {
            lineOfStmt = line;
            DeclareLocalVar declared = Parser.Getinstance().mainDeclaredLocalVar[metKey].SingleOrDefault(i => i.Ident == Ident);
            if (declared == null)
            {
                // Method params.
                var inMethod = metKey.MethodParams.SingleOrDefault(i => i.ParamName.Equals(Ident));
                if (inMethod != null)
                    return metKey.MethodParams[inMethod.Order].ParameterType.IsArray;
                else
                    throw new Error("variable: " + Ident + " is not declared [At line: " + line + "]");
            }
            else if (declared.Expr != null)
            {
                if (declared.Expr is LocalArray)
                    return true;
            }
            return false;
        }

        public static bool IsDeclared(string Ident)
        {
            DeclareLocalVar declared = Parser.Getinstance().mainDeclaredLocalVar[metKey].SingleOrDefault(i => i.Ident == Ident);
            if (declared == null)
            {
                // Method params.
                var inMethod = metKey.MethodParams.SingleOrDefault(i => i.ParamName.Equals(Ident));
                if (inMethod != null)
                    return true;
                else
                    return false;
            }
            return true;
        }

        public static LocalArray GetArrayExpr(string Ident)
        {
            DeclareLocalVar declared = Parser.Getinstance().mainDeclaredLocalVar[metKey].SingleOrDefault(i => i.Ident == Ident);
            if (declared == null)
                throw new Error("variable: " + Ident + " is not declared");
            else if (declared.Expr != null)
            {
                if (declared.Expr is LocalArray)
                    return (declared.Expr as LocalArray);
            }
            return null;
        }

        public static MethodParam GetDetailOfParam(string ident)
        {
            var inMethod = metKey.MethodParams.SingleOrDefault(i => i.ParamName.Equals(ident));
            return inMethod != null ? inMethod : null;
        }

        // Change in this don't forgot to change in CodeGen.
        public static System.Type TypeOfExpr(Expr expr)
        {
            if (expr is StringLiteral)
            {
                return typeof(string);
            }
            else if (expr is IntLiteral)
            {
                return typeof(int);
            }
            else if (expr is FloatLiteral)
            {
                return typeof(float);
            }
            else if (expr is DoubleLiteral)
            {
                return typeof(double);
            }
            else if (expr is BooleanLiteral)
            {
                return typeof(bool);
            }
            else if (expr is Variable)
            {
                Variable var = (Variable)expr;
                return FindTypeOfVariable(var.Ident, lineOfStmt);
            }
            else if (expr is LocalArray)
            {
                LocalArray loc = expr as LocalArray;
                return loc.Type;
            }
            else if (expr is SequenceExpr)
            {
                var sexpr = expr as SequenceExpr;

                if (sexpr.Value == null)
                    return null;

                bool isContainString = sexpr.Value.ShuntingYardArtmValue.Where(i => i is StringLiteral).Count() > 0;
                if (isContainString)
                {
                    return typeof(string);
                }
                else return GetExprTypeFromTree(sexpr.Value);
            }
            else if (expr is OptStrSequenceExpr)
            {
                OptStrSequenceExpr sexpr = expr as OptStrSequenceExpr;
                bool isContainString = sexpr.Value.ShuntingYardArtmValue.Where(i => i is StringLiteral).Count() > 0;
                if (isContainString)
                {
                    return typeof(string);
                }
                else return GetExprTypeFromTree(sexpr.Value);
            }
            else if (expr is CallMethod)
            {
                var cm = (expr as CallMethod);
                if (cm.ReturnType != null)
                    return cm.ReturnType;
                return null;
            }
            else if (expr is ReadValue || expr is Display)
                return typeof(string);
            else
            {
                throw new Error("GString compiler don't know how to generate " + expr.GetType().Name);
            }
        }

        public static System.Type GetExprTypeFromTree(TreeNode tree)
        {
            System.Type result = null;
            if (tree.ShuntingYardArtmValue != null && tree.ShuntingYardArtmValue.Where(c => c is LogicalEqualSymbol || c is LogicalNotEqualSymbol
                || c is LogicalNotSymbol || c is MoreThanAndEqualSymbol || c is MoreThanSymbol || c is LessThanAndEqualSymbol || c is LessThanSymbol)
                .Count() > 0)
            {
                result = typeof(bool);
            }
            else if (tree.Op != null)
            {
                result = GetExprTypeFromTree(tree.Left);
                if (result == null)
                    result = GetExprTypeFromTree(tree.Right);
            }
            // For cut string sequence.
            else if (tree.Left != null && tree.Right != null)
            {
                if (result == null)
                    result = GetExprTypeFromTree(tree.Right);
            }
            else
                result = TypeOfExpr(tree.Value);

            return result;
        }
    }
}
