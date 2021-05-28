using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Interstellar
{
    internal static class ExpressionExtensions
    {
        public static MemberInfo GetMember<T, TProperty>(this Expression<Func<T, TProperty>> expression)
        {
            if (expression.Body.RemoveUnary() is not MemberExpression memberExp)
            {
                return null;
            }

            Expression currentExpr = memberExp.Expression;

            // Unwind the expression to get the root object that the expression acts upon.
            while (true)
            {
                currentExpr = currentExpr.RemoveUnary();

                if (currentExpr != null && currentExpr.NodeType == ExpressionType.MemberAccess)
                {
                    currentExpr = ((MemberExpression)currentExpr).Expression;
                }
                else
                {
                    break;
                }
            }

            if (currentExpr == null || currentExpr.NodeType != ExpressionType.Parameter)
            {
                return null; // We don't care if we're not acting upon the model instance.
            }

            return memberExp.Member;
        }

        private static Expression RemoveUnary(this Expression toUnwrap) =>
            toUnwrap is UnaryExpression ue ? ue.Operand : toUnwrap;

        //public static bool IsNumericValue(this object value) =>
        //    value is not null && ((Type.GetTypeCode(value.GetType())) switch
        //    {
        //        TypeCode.Byte or TypeCode.SByte or TypeCode.UInt16 or TypeCode.UInt32 or
        //        TypeCode.UInt64 or TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 or
        //        TypeCode.Decimal or TypeCode.Double or TypeCode.Single => true,
        //        _ => false,
        //    });
    }
}
