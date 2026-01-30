using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Linq.Eval
{
    /// <summary>
    /// Provides extension methods for converting string queries to LINQ Expression Trees.
    /// This enables dynamic query construction and compilation with full type safety.
    /// </summary>
    public static class ExpressionQuery
    {
        static CSharpParseOptions ParseOptions = CSharpParseOptions.Default.WithKind(SourceCodeKind.Script);

        /// <summary>
        /// Converts a string query into a strongly-typed LINQ Expression.
        /// </summary>
        /// <typeparam name="T">The delegate type of the expression (e.g., Func&lt;Student, bool&gt;).</typeparam>
        /// <param name="query">The string query to parse (e.g., "x => x.Age > 18").</param>
        /// <returns>An Expression&lt;T&gt; that can be compiled or used in LINQ providers.</returns>
        /// <exception cref="Exception">Thrown when the syntax is not supported.</exception>
        /// <exception cref="NotImplementedException">Thrown when encountering unsupported syntax constructs.</exception>
        /// <example>
        /// <code>
        /// var expr = "x => x.Age > 18".ToExpression&lt;Func&lt;Student, bool&gt;&gt;();
        /// var compiled = expr.Compile();
        /// var results = students.Where(compiled);
        /// </code>
        /// </example>
        public static Expression<T> ToExpression<T>(this string query)
        {
            var ts = TypeConvert<T>();
            var syntax = ToSyntax(query);
            var pars = ExtractParameter(syntax, ts.Pars);
            var body = Dispatcher(syntax.Body, pars);
            
            // Ensure body type matches the return type of the delegate
            if (body.Type != ts.Return && body.Type == typeof(bool?) && ts.Return == typeof(bool))
            {
                // Convert bool? to bool for predicate functions
                body = Expression.Convert(body, typeof(bool));
            }

            return Expression.Lambda<T>(body, pars);
        }

        static LambdaExpressionSyntax ToSyntax(string query) => SyntaxFactory.ParseExpression(query, options: ParseOptions) as LambdaExpressionSyntax ?? throw new Exception("Does not support this syntax");

        static ParameterExpression[] ExtractParameter(LambdaExpressionSyntax expr, Type[] pars)
        {
            switch (expr.GetType().Name)
            {
                case nameof(SimpleLambdaExpressionSyntax):
                    return GetParaFromLambda(expr as SimpleLambdaExpressionSyntax, pars);
                case nameof(ParenthesizedLambdaExpressionSyntax):
                    return GetParaFromLambda(expr as ParenthesizedLambdaExpressionSyntax, pars);
                default:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        static ParameterExpression[] GetParaFromLambda(SimpleLambdaExpressionSyntax lamb, Type[] pars = null)
            => pars == null ? new ParameterExpression[0] : new ParameterExpression[] { Expression.Parameter(pars[0], lamb.Parameter.Identifier.Text) };

        static ParameterExpression[] GetParaFromLambda(ParenthesizedLambdaExpressionSyntax lamb, Type[] pars = null)
            => pars == null || !pars.Any() ? new ParameterExpression[0] : ConvertToParameterExpression(lamb.ParameterList, pars);

        static ParameterExpression ConvertToParameterExpression(ParameterSyntax par, Type type = null)
            => Expression.Parameter(type, par.Identifier.ValueText);

        static ParameterExpression[] ConvertToParameterExpression(ParameterListSyntax pars, params Type[] type)
            => pars.Parameters.Select((x, i) => Expression.Parameter(type[i], x.Identifier.ValueText)).ToArray();

        static Expression? Dispatcher(CSharpSyntaxNode body, ParameterExpression[] pars = null, bool isMethodCall = false, ArgumentListSyntax args = null, Expression? member = null)
        {
            switch (body.GetType().Name)
            {
                case nameof(LiteralExpressionSyntax):
                    return ConcatBody(body as LiteralExpressionSyntax);
                case nameof(MemberAccessExpressionSyntax):
                    return ConcatBody(body as MemberAccessExpressionSyntax, pars, isMethodCall, args);
                case nameof(BinaryExpressionSyntax):
                    return ConcatBody(body as BinaryExpressionSyntax, pars);
                case nameof(PostfixUnaryExpressionSyntax):
                    return ConcatBody(body as PostfixUnaryExpressionSyntax, pars);
                case nameof(PrefixUnaryExpressionSyntax):
                    return ConcatBody(body as PrefixUnaryExpressionSyntax, pars);
                case nameof(ParenthesizedExpressionSyntax):
                    return ConcatBody(body as ParenthesizedExpressionSyntax, pars, isMethodCall, args, member);
                case nameof(ElementAccessExpressionSyntax):
                    return ConcatBody(body as ElementAccessExpressionSyntax, pars);
                case nameof(AssignmentExpressionSyntax):
                    return ConcatBody(body as AssignmentExpressionSyntax, pars);
                case nameof(ConditionalAccessExpressionSyntax):
                    return ConcatBody(body as ConditionalAccessExpressionSyntax, pars, member: member);
                case nameof(IdentifierNameSyntax):
                    return ConcatBody(body as IdentifierNameSyntax, pars);
                case nameof(ConditionalExpressionSyntax):
                    return ConcatBody(body as ConditionalExpressionSyntax, pars);
                case nameof(MemberBindingExpressionSyntax):
                    return ConcatBody(body as MemberBindingExpressionSyntax, pars, member);
                case nameof(InvocationExpressionSyntax):
                    throw new NotImplementedException();
                case nameof(CastExpressionSyntax):
                    return ConcatBody(body as CastExpressionSyntax, pars);
                case nameof(ElementBindingExpressionSyntax):
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }

            throw new NotImplementedException();
        }

        static Expression? ConcatBody(LiteralExpressionSyntax body)
        {
            switch (body.Kind())
            {
                case SyntaxKind.TrueLiteralExpression:
                    return Expression.Constant(true);
                case SyntaxKind.FalseLiteralExpression:
                    return Expression.Constant(false);
                case SyntaxKind.StringLiteralExpression:
                    return Expression.Constant(body.Token.ValueText);
                case SyntaxKind.CharacterLiteralToken:
                    return Expression.Constant(body.Token.ValueText.ToCharArray()[0], typeof(char));
                case SyntaxKind.NumericLiteralExpression:
                    return Expression.Constant(body.Token.Value, body.Token.Value.GetType());
                case SyntaxKind.NullLiteralExpression:
                    return Expression.Constant(null);
                //case SyntaxKind.ArgListExpression:
                //    return Expression.Constant(null);
                default:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }
        static Expression? ConcatBody(MemberAccessExpressionSyntax body, ParameterExpression[] pars = null, bool isMethod = false, ArgumentListSyntax args = null)
         => !isMethod ? Expression.Property(Dispatcher(body.Expression, pars), body.Name.Identifier.Text) : GetMethodExpression(body, pars, isMethod, args);
        static Expression? GetMethodExpression(MemberAccessExpressionSyntax body, ParameterExpression[] pars = null, bool isMethod = false, ArgumentListSyntax args = null)
        {
            throw new NotImplementedException();
        }
        static Expression? ConcatBody(BinaryExpressionSyntax body, ParameterExpression[] pars = null)
        {
            switch (body.Kind())
            {
                case SyntaxKind.AddExpression:
                    var left = Dispatcher(body.Left, pars);
                    var right = Dispatcher(body.Right, pars);
                    if (left.Type == typeof(string) && right.Type == typeof(string))
                    {
                        var concatMethod = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) });
                        var addExpr = Expression.Add(left, right, concatMethod);
                        return addExpr;
                    }
                    return Expression.Add(Dispatcher(body.Left, pars), Dispatcher(body.Right, pars));
                case SyntaxKind.SubtractExpression:
                    return Expression.Subtract(Dispatcher(body.Left, pars), Dispatcher(body.Right, pars));
                case SyntaxKind.MultiplyExpression:
                    return Expression.Multiply(Dispatcher(body.Left, pars), Dispatcher(body.Right, pars));
                case SyntaxKind.DivideExpression:
                    return Expression.Divide(Dispatcher(body.Left, pars), Dispatcher(body.Right, pars));
                case SyntaxKind.ModuloExpression:
                    return Expression.Modulo(Dispatcher(body.Left, pars), Dispatcher(body.Right, pars));
                case SyntaxKind.LogicalOrExpression:
                    return Expression.Or(Dispatcher(body.Left, pars), Dispatcher(body.Right, pars));
                case SyntaxKind.LogicalAndExpression:
                    return Expression.And(Dispatcher(body.Left, pars), Dispatcher(body.Right, pars));
                case SyntaxKind.LogicalNotExpression:
                    return Expression.Not(Dispatcher(body.Left, pars));
                case SyntaxKind.ExclusiveOrExpression:
                    return Expression.ExclusiveOr(Dispatcher(body.Left, pars), Dispatcher(body.Right, pars));
                case SyntaxKind.EqualsExpression:
                    var eqLeft = Dispatcher(body.Left, pars);
                    var eqRight = Dispatcher(body.Right, pars);
                    NormalizeNullableTypes(ref eqLeft, ref eqRight);
                    return Expression.Equal(eqLeft, eqRight, liftToNull: ShouldLiftToNull(eqLeft, eqRight), method: null);
                case SyntaxKind.NotEqualsExpression:
                    var neLeft = Dispatcher(body.Left, pars);
                    var neRight = Dispatcher(body.Right, pars);
                    NormalizeNullableTypes(ref neLeft, ref neRight);
                    return Expression.NotEqual(neLeft, neRight, liftToNull: ShouldLiftToNull(neLeft, neRight), method: null);
                case SyntaxKind.LessThanExpression:
                    var ltLeft = Dispatcher(body.Left, pars);
                    var ltRight = Dispatcher(body.Right, pars);
                    NormalizeNullableTypes(ref ltLeft, ref ltRight);
                    return Expression.LessThan(ltLeft, ltRight, liftToNull: ShouldLiftToNull(ltLeft, ltRight), method: null);
                case SyntaxKind.LessThanOrEqualExpression:
                    var leLeft = Dispatcher(body.Left, pars);
                    var leRight = Dispatcher(body.Right, pars);
                    NormalizeNullableTypes(ref leLeft, ref leRight);
                    return Expression.LessThanOrEqual(leLeft, leRight, liftToNull: ShouldLiftToNull(leLeft, leRight), method: null);
                case SyntaxKind.GreaterThanExpression:
                    var gtLeft = Dispatcher(body.Left, pars);
                    var gtRight = Dispatcher(body.Right, pars);
                    NormalizeNullableTypes(ref gtLeft, ref gtRight);
                    return Expression.GreaterThan(gtLeft, gtRight, liftToNull: ShouldLiftToNull(gtLeft, gtRight), method: null);
                case SyntaxKind.GreaterThanOrEqualExpression:
                    var geLeft = Dispatcher(body.Left, pars);
                    var geRight = Dispatcher(body.Right, pars);
                    NormalizeNullableTypes(ref geLeft, ref geRight);
                    return Expression.GreaterThanOrEqual(geLeft, geRight, liftToNull: ShouldLiftToNull(geLeft, geRight), method: null);
                case SyntaxKind.CoalesceExpression:
                    var left_coalesce = Dispatcher(body.Left, pars);
                    var right_coalesce = Dispatcher(body.Right, pars);
                    
                    // Ensure left operand is nullable
                    if (left_coalesce.Type.IsValueType && Nullable.GetUnderlyingType(left_coalesce.Type) == null)
                    {
                        var nullableType = typeof(Nullable<>).MakeGenericType(left_coalesce.Type);
                        left_coalesce = Expression.Convert(left_coalesce, nullableType);
                    }
                    
                    // Ensure right operand matches the underlying type of left or is nullable
                    var leftUnderlyingType = Nullable.GetUnderlyingType(left_coalesce.Type) ?? left_coalesce.Type;
                    if (right_coalesce.Type != left_coalesce.Type && right_coalesce.Type != leftUnderlyingType)
                    {
                        if (Nullable.GetUnderlyingType(left_coalesce.Type) != null && right_coalesce.Type.IsValueType && Nullable.GetUnderlyingType(right_coalesce.Type) == null)
                        {
                            // Right is non-nullable value type, ensure it matches the underlying type
                            right_coalesce = Expression.Convert(right_coalesce, leftUnderlyingType);
                        }
                    }
                    
                    return Expression.Coalesce(left_coalesce, right_coalesce);
                case SyntaxKind.AsExpression:
                    //Expression.Convert();
                    throw new NotImplementedException();// TODO deep type
                    switch (body.Right.Kind())
                    {
                        case SyntaxKind.IdentifierName:
                            //var r = (body.Right as IdentifierNameSyntax).Identifier.Text;
                            //var ty = pars.FirstOrDefault(x => x.Type.Name == r)?.Type;
                            //return ty != null 
                            //    ? Expression.Convert(Dispatcher(body.Left, pars), ty) 
                            //    : throw new NotImplementedException();
                            throw new NotImplementedException();
                        case SyntaxKind.PredefinedType:
                            return Aliases.TryGetValue((body.Right as PredefinedTypeSyntax).Keyword.Text, out Type tp)
                                ? Expression.Convert(Dispatcher(body.Left, pars), tp)
                                : throw new NotImplementedException();
                        default:
                            throw new NotImplementedException();
                    }
                    throw new NotImplementedException();
                //return Expression.Coalesce(Dispatcher(body.Left, pars), Dispatcher(body.Right, pars));
                default:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }
        static Expression? ConcatBody(PostfixUnaryExpressionSyntax body, ParameterExpression[] pars = null)
        {
            switch (body.Kind())
            {
                case SyntaxKind.PostIncrementExpression:
                    return Expression.PostIncrementAssign(Dispatcher(body.Operand, pars));
                case SyntaxKind.PostDecrementExpression:
                    return Expression.PostDecrementAssign(Dispatcher(body.Operand, pars));
                default:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }
        static Expression? ConcatBody(PrefixUnaryExpressionSyntax body, ParameterExpression[] pars = null)
        {
            switch (body.Kind())
            {
                case SyntaxKind.LogicalNotExpression:
                    return Expression.Not(Dispatcher(body.Operand, pars));
                case SyntaxKind.UnaryMinusExpression:
                    var operand = Dispatcher(body.Operand, pars);
                    // Handle special case: -2147483648 is parsed as -(2147483648u)
                    // Need to convert uint to int before negating
                    if (operand.Type == typeof(uint))
                    {
                        operand = Expression.Convert(operand, typeof(long));
                        return Expression.Convert(Expression.Negate(operand), typeof(int));
                    }
                    return Expression.Negate(operand);
                case SyntaxKind.UnaryPlusExpression:
                    return Expression.UnaryPlus(Dispatcher(body.Operand, pars));
                default:
                    throw new NotImplementedException();
            }
        }
        static Expression? ConcatBody(ParenthesizedExpressionSyntax body, ParameterExpression[] pars = null, bool isMethodCall = false, ArgumentListSyntax args = null, Expression? member = null)
            => Dispatcher(body.Expression, pars, isMethodCall, args, member);
        static Expression? ConcatBody(ElementAccessExpressionSyntax body, ParameterExpression[] pars = null)
            => Expression.ArrayAccess(Dispatcher(body.Expression, pars), Dispatcher(body.ArgumentList.Arguments[0].Expression, pars));
        static Expression? ConcatBody(AssignmentExpressionSyntax body, ParameterExpression[] pars = null)
        {
            switch (body.Kind())
            {
                case SyntaxKind.SimpleAssignmentExpression:
                    return Expression.Assign(Dispatcher(body.Left, pars), Dispatcher(body.Right, pars));
                case SyntaxKind.AddAssignmentExpression:
                    var left = Dispatcher(body.Left, pars);
                    var right = Dispatcher(body.Right, pars);
                    if (left.Type == typeof(string) && right.Type == typeof(string))
                    {
                        var concatMethod = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) });
                        var addExpr = Expression.AddAssign(left, right, concatMethod);
                        return addExpr;
                    }
                    return Expression.AddAssign(Dispatcher(body.Left, pars), Dispatcher(body.Right, pars));
                case SyntaxKind.SubtractAssignmentExpression:
                    return Expression.SubtractAssign(Dispatcher(body.Left, pars), Dispatcher(body.Right, pars));
                case SyntaxKind.MultiplyAssignmentExpression:
                    return Expression.MultiplyAssign(Dispatcher(body.Left, pars), Dispatcher(body.Right, pars));
                case SyntaxKind.DivideAssignmentExpression:
                    return Expression.DivideAssign(Dispatcher(body.Left, pars), Dispatcher(body.Right, pars));
                case SyntaxKind.ModuloAssignmentExpression:
                    return Expression.ModuloAssign(Dispatcher(body.Left, pars), Dispatcher(body.Right, pars));
                case SyntaxKind.AndAssignmentExpression:
                    return Expression.AndAssign(Dispatcher(body.Left, pars), Dispatcher(body.Right, pars));
                case SyntaxKind.OrAssignmentExpression:
                    return Expression.OrAssign(Dispatcher(body.Left, pars), Dispatcher(body.Right, pars));
                case SyntaxKind.ExclusiveOrExpression:
                    return Expression.ExclusiveOrAssign(Dispatcher(body.Left, pars), Dispatcher(body.Right, pars));
                default:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }
        static Expression? ConcatBody(ConditionalAccessExpressionSyntax body, ParameterExpression[] pars = null, Expression? member = null)
        {
            //switch (body.WhenNotNull.Kind())
            //{
            //    case SyntaxKind.MemberBindingExpression:
            //        if (member == null)
            //            member = Dispatcher(body.Expression, pars);
            //        else
            //            member = Expression.Property(member, (body.WhenNotNull as MemberBindingExpressionSyntax).Name.Identifier.Text);
            //        break;
            //    case SyntaxKind.ConditionalAccessExpression:
            //        if(member == null)
            //            member = Dispatcher(body.Expression, pars);
            //        else
            //            member = Expression.Property(member, Dispatcher());
            //        break;
            //    default:
            //        throw new NotImplementedException();
            //}
            switch (body.Expression.Kind())
            {
                case SyntaxKind.IdentifierName:
                case SyntaxKind.SimpleMemberAccessExpression:
                    member = Dispatcher(body.Expression, pars);
                    break;
                case SyntaxKind.MemberBindingExpression:
                    if (member == null)
                        member = Dispatcher(body.Expression, pars);
                    else
                        member = Expression.Property(member, (body.Expression as MemberBindingExpressionSyntax).Name.Identifier.Text);
                    break;
                //case SyntaxKind.ConditionalAccessExpression:
                //    member = Expression.Property(member, ((body.Expression as ConditionalAccessExpressionSyntax).Expression as MemberBindingExpressionSyntax).Name.Identifier.Text);
                //    break;
                default:
                    throw new NotImplementedException();
            }

            //else
            //member = Expression.Property(member, (body.Expression as MemberBindingExpressionSyntax).Name.Identifier.Text);
            Expression? notNull;
            if (body.WhenNotNull is MemberBindingExpressionSyntax memberbinding)
                notNull = Expression.Property(member, memberbinding.Name.Identifier.Text);
            else if (body.WhenNotNull is MemberAccessExpressionSyntax memberaccess && memberaccess.Expression is ElementBindingExpressionSyntax elementbinding)
            {
                // Handle array access followed by property access: x.Students?[0].Age
                var arrayAccess = Expression.ArrayAccess(member, Dispatcher(elementbinding.ArgumentList.Arguments[0].Expression, pars));
                notNull = Expression.Property(arrayAccess, memberaccess.Name.Identifier.Text);
            }
            else
                notNull = Dispatcher(body.WhenNotNull, pars, member: member);

            //var t = Expression.Equal(id, Expression.Constant(null, id.Type));
            //var rt =  Expression.Condition(Expression.Equal(id, Expression.Constant(null, id.Type)), notNull, Expression.Constant(null, notNull.Type));
            if (!notNull.Type.IsClass && !notNull.Type.Name.StartsWith("Nullable", StringComparison.OrdinalIgnoreCase))
            {
                if (ToNullable.TryGetValue(notNull.Type, out Type ty_null))
                    return Expression.Condition(Expression.Equal(member, Expression.Constant(null, member.Type)), Expression.Constant(null, ty_null), Expression.Convert(notNull, ty_null));
                else
                    throw new NotImplementedException();
            }

            else
                return Expression.Condition(Expression.Equal(member, Expression.Constant(null, member.Type)), Expression.Constant(null, notNull.Type), notNull);
        }
        static Expression? ConcatBody(IdentifierNameSyntax body, ParameterExpression[] pars = null)
            => pars.First(x => x.Name == body.Identifier.Text);
        static Expression? ConcatBody(ConditionalExpressionSyntax body, ParameterExpression[] pars = null)
        {
            var condition = Dispatcher(body.Condition, pars);
            var whenTrue = Dispatcher(body.WhenTrue, pars);
            var whenFalse = Dispatcher(body.WhenFalse, pars);
            
            // Ensure condition is bool, not bool?
            if (condition.Type == typeof(bool?))
            {
                condition = Expression.Convert(condition, typeof(bool));
            }
            
            return Expression.Condition(condition, whenTrue, whenFalse);

            //var condition = Dispatcher(body.Condition, pars);
            //var whentrue = Dispatcher(body.WhenTrue, pars);
            //var whenfalse = Dispatcher(body.WhenFalse, pars);
            //if (whenfalse.Type != whentrue.Type)
            //{
            //    if (!whenfalse.Type.IsClass && whenfalse.Type.Name.StartsWith("Nullable", StringComparison.OrdinalIgnoreCase))
            //    {
            //        whenfalse = Expression.Convert(whenfalse, NullableToAliases.TryGetValue(whenfalse.Type, out Type conv) ? conv : throw new NotImplementedException());
            //    }
            //    else if (!whentrue.Type.IsClass && whentrue.Type.Name.StartsWith("Nullable", StringComparison.OrdinalIgnoreCase))
            //    {
            //        whentrue = Expression.Convert(whentrue, NullableToAliases.TryGetValue(whentrue.Type, out Type conv) ? conv : throw new NotImplementedException());
            //    }
            //    else
            //    {
            //        throw new NotImplementedException();
            //    }
            //}
            //return Expression.Condition(condition, whentrue, whenfalse);
        }
        static Expression? ConcatBody(MemberBindingExpressionSyntax body, ParameterExpression[] pars = null, Expression? Member = null) //TODO
        {
            throw new NotImplementedException();
        }
        static Expression? ConcatBody(CastExpressionSyntax body, ParameterExpression[] pars = null)
        {
            switch (body.Type.Kind())
            {
                case SyntaxKind.IdentifierName: // TODO deep type
                    //var r = (body.Type as IdentifierNameSyntax).Identifier.Text;
                    //var ty = pars.FirstOrDefault(x => x.Type.Name == r)?.Type;
                    //return ty != null
                    //     ? Expression.Convert(Dispatcher(body.Expression, pars), ty)
                    //    : throw new NotImplementedException();
                    throw new NotImplementedException();
                case SyntaxKind.PredefinedType:
                    return Aliases.TryGetValue((body.Type as PredefinedTypeSyntax).Keyword.Text, out Type tp)
                        ? Expression.Convert(Dispatcher(body.Expression, pars), tp)
                        : throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }

            throw new NotImplementedException();
        }
        static Expression? ConcatBody(InvocationExpressionSyntax body, ParameterExpression[] pars = null)
        {
            //return ConcatBody(body.Expression as MemberAccessExpressionSyntax, pars, true, body.ArgumentList);
            //throw new NotImplementedException();
            return Dispatcher(body.Expression, pars, true, body.ArgumentList);
        }
        static (Type[] Pars, Type? Return) TypeConvert<T>()
        {
            var t = typeof(T);
            var pars = t.GetGenericArguments();
            if (t.Name.StartsWith("Func"))
            {
                return (pars.Take(pars.Length - 1).ToArray(), pars.Last());
            }
            else if (t.Name.StartsWith("Action"))
            {
                return (pars, null);
            }
            else
            {
                throw new Exception("does not support this type");
            }
        }

        static readonly Dictionary<string, Type> Aliases = new Dictionary<string, Type>()
        {
           { "byte" ,typeof(byte) },
           { "sbyte" ,typeof(sbyte)  }   ,
           { "short" ,typeof(short)  }   ,
           { "ushort" ,typeof(ushort)  }   ,
           { "int" ,typeof(int)  }   ,
           { "uint" ,typeof(uint)  }   ,
           { "long" ,typeof(long)   }   ,
           {  "ulong" ,typeof(ulong)    }   ,
           { "float" ,typeof(float)    }   ,
           {  "double" ,typeof(double)   }   ,
           {  "decimal" ,typeof(decimal)   }   ,
           {  "object" ,typeof(object)      }   ,
           { "bool" ,typeof(bool)     }   ,
           { "char" ,typeof(char)     }   ,
           { "string" ,typeof(string)    }   ,
           { "void" ,typeof(void)       }   ,
           {"byte?" ,typeof(Nullable<byte>)     }   ,
           { "sbyte?" ,typeof(Nullable<sbyte>)    }   ,
           {"short?" ,typeof(Nullable<short>)    }   ,
           {"ushort?" ,typeof(Nullable<ushort>)    }   ,
           {"int?" ,typeof(Nullable<int>)     }   ,
           {"uint?" ,typeof(Nullable<uint>)     }   ,
           {"long?" ,typeof(Nullable<long>)    }   ,
           {"ulong?" ,typeof(Nullable<ulong>)    }   ,
           {"float?" ,typeof(Nullable<float>)   }   ,
           {"double?" ,typeof(Nullable<double>)  }   ,
           { "decimal?" ,typeof(Nullable<decimal>)    }   ,
           {"bool?" ,typeof(Nullable<bool>)   }   ,
           {"char?", typeof(Nullable<char>) }
        };
        static readonly Dictionary<Type, Type> ToNullable = new Dictionary<Type, Type>
        {
            {typeof(byte) ,typeof(Nullable<byte>)     }   ,
           {typeof(sbyte) ,typeof(Nullable<sbyte>)    }   ,
           {typeof(short) ,typeof(Nullable<short>)    }   ,
           {typeof(ushort) ,typeof(Nullable<ushort>)    }   ,
           {typeof(int) ,typeof(Nullable<int>)     }   ,
           {typeof(uint) ,typeof(Nullable<uint>)     }   ,
           {typeof(long) ,typeof(Nullable<long>)    }   ,
           {typeof(ulong) ,typeof(Nullable<ulong>)    }   ,
           {typeof(float) ,typeof(Nullable<float>)   }   ,
           {typeof(double) ,typeof(Nullable<double>)  }   ,
           {typeof(decimal) ,typeof(Nullable<decimal>)    }   ,
           {typeof(bool),typeof(Nullable<bool>)   }   ,
           {typeof(char), typeof(Nullable<char>) }
        };
        static readonly Dictionary<Type, Type> NullableToNonNullable = new Dictionary<Type, Type>
        {
           {typeof(Nullable<byte>)   ,typeof(byte)  }   ,
           {typeof(Nullable<sbyte>)  ,typeof(sbyte)    }   ,
           {typeof(Nullable<short>)  ,typeof(short)   }   ,
           {typeof(Nullable<ushort>) ,typeof(ushort)    }   ,
           {typeof(Nullable<int>)    ,typeof(int)    }   ,
           {typeof(Nullable<uint>)   ,typeof(uint)   }   ,
           {typeof(Nullable<long>)   ,typeof(long)   }   ,
           {typeof(Nullable<ulong>)  ,typeof(ulong)   }   ,
           {typeof(Nullable<float>)  ,typeof(float)   }   ,
           {typeof(Nullable<double>) ,typeof(double)  }   ,
           {typeof(Nullable<decimal>),typeof(decimal)   }   ,
           {typeof(Nullable<bool>)   ,typeof(bool)   }   ,
           {typeof(Nullable<char>)   ,typeof(char)    }
        };

        /// <summary>
        /// Normalizes types for comparison operations by converting non-nullable to nullable when one side is nullable.
        /// This ensures both operands have compatible types for comparison.
        /// </summary>
        private static void NormalizeNullableTypes(ref Expression left, ref Expression right)
        {
            if (left?.Type == null || right?.Type == null)
                return;
                
            var leftUnderlyingType = Nullable.GetUnderlyingType(left.Type);
            var rightUnderlyingType = Nullable.GetUnderlyingType(right.Type);
            
            // If one is nullable and the other is the underlying type, convert to nullable
            if (leftUnderlyingType != null && rightUnderlyingType == null && leftUnderlyingType == right.Type)
            {
                // Right is the underlying type of left's nullable, convert right to nullable
                right = Expression.Convert(right, left.Type);
            }
            else if (rightUnderlyingType != null && leftUnderlyingType == null && rightUnderlyingType == left.Type)
            {
                // Left is the underlying type of right's nullable, convert left to nullable
                left = Expression.Convert(left, right.Type);
            }
        }

        /// <summary>
        /// Determines whether to lift null semantics for comparison operations.
        /// Returns true if either operand is nullable to handle null comparisons correctly.
        /// </summary>
        private static bool ShouldLiftToNull(Expression left, Expression right)
        {
            var leftType = left?.Type;
            var rightType = right?.Type;
            
            if (leftType == null || rightType == null)
                return false;
                
            // Check if either type is nullable
            bool leftIsNullable = Nullable.GetUnderlyingType(leftType) != null;
            bool rightIsNullable = Nullable.GetUnderlyingType(rightType) != null;
            
            return leftIsNullable || rightIsNullable;
        }
    }
}
