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
    public static class ExpressionQuery
    {
        static CSharpParseOptions ParseOptions = CSharpParseOptions.Default.WithKind(SourceCodeKind.Script);

        public static Expression<T> ToExpression<T>(this string query)
        {
            var ts = TypeConvert<T>();
            var syntax = ToSyntax(query);
            var pars = ExtractParameter(syntax, ts.Pars);
            var body = Dispatcher(syntax.Body, pars);

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

        static Expression? Dispatcher(CSharpSyntaxNode body, ParameterExpression[] pars = null)
        {
            switch (body.GetType().Name)
            {
                case nameof(LiteralExpressionSyntax):
                    return ConcatBody(body as LiteralExpressionSyntax);
                case nameof(MemberAccessExpressionSyntax):
                    return ConcatBody(body as MemberAccessExpressionSyntax, pars);
                case nameof(BinaryExpressionSyntax):
                    return ConcatBody(body as BinaryExpressionSyntax, pars);
                case nameof(PostfixUnaryExpressionSyntax):
                    return ConcatBody(body as PostfixUnaryExpressionSyntax, pars);
                case nameof(PrefixUnaryExpressionSyntax):
                    return ConcatBody(body as PrefixUnaryExpressionSyntax, pars);
                case nameof(ParenthesizedExpressionSyntax):
                    return ConcatBody(body as ParenthesizedExpressionSyntax, pars);
                case nameof(ElementAccessExpressionSyntax):
                    return ConcatBody(body as ElementAccessExpressionSyntax, pars);
                case nameof(AssignmentExpressionSyntax):
                    return ConcatBody(body as AssignmentExpressionSyntax, pars);
                case nameof(ConditionalAccessExpressionSyntax):
                    return ConcatBody(body as ConditionalAccessExpressionSyntax, pars);
                case nameof(IdentifierNameSyntax):
                    return ConcatBody(body as IdentifierNameSyntax, pars);
                case nameof(ConditionalExpressionSyntax):
                    return ConcatBody(body as ConditionalExpressionSyntax, pars);
                case nameof(MemberBindingExpressionSyntax):
                    return ConcatBody(body as MemberBindingExpressionSyntax, pars);
                case nameof(InvocationExpressionSyntax):
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
                    return Expression.Constant(body.Token.Text);
                case SyntaxKind.CharacterLiteralToken:
                    return Expression.Constant(body.Token.Text.ToCharArray()[0], typeof(char));
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
        static Expression? ConcatBody(MemberAccessExpressionSyntax body, ParameterExpression[] pars = null)
            => Expression.Property(Dispatcher(body.Expression, pars), body.Name.Identifier.Text);
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
                    return Expression.Equal(Dispatcher(body.Left, pars), Dispatcher(body.Right, pars));
                case SyntaxKind.NotEqualsExpression:
                    return Expression.NotEqual(Dispatcher(body.Left, pars), Dispatcher(body.Right, pars));
                case SyntaxKind.LessThanExpression:
                    return Expression.LessThan(Dispatcher(body.Left, pars), Dispatcher(body.Right, pars));
                case SyntaxKind.LessThanLessThanEqualsToken:
                    return Expression.LessThanOrEqual(Dispatcher(body.Left, pars), Dispatcher(body.Right, pars));
                case SyntaxKind.GreaterThanExpression:
                    return Expression.GreaterThan(Dispatcher(body.Left, pars), Dispatcher(body.Right, pars));
                case SyntaxKind.GreaterThanOrEqualExpression:
                    return Expression.GreaterThanOrEqual(Dispatcher(body.Left, pars), Dispatcher(body.Right, pars));
                case SyntaxKind.CoalesceExpression:
                    return Expression.Coalesce(Dispatcher(body.Left, pars), Dispatcher(body.Right, pars));
                default:
                    break;
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
                default:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }
        static Expression? ConcatBody(ParenthesizedExpressionSyntax body, ParameterExpression[] pars = null)
            => Dispatcher(body.Expression, pars);
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
        static Expression? ConcatBody(ConditionalAccessExpressionSyntax body, ParameterExpression[] pars = null)
        {
            var id = Dispatcher(body.Expression, pars);
            var notNull = Dispatcher(body.WhenNotNull, pars);
            return Expression.Condition(Expression.Equal(id, Expression.Constant(null, id.Type)), notNull, Expression.Constant(null, notNull.Type));
        }
        static Expression? ConcatBody(IdentifierNameSyntax body, ParameterExpression[] pars = null)
            => pars.First(x => x.Name == body.Identifier.Text);
        static Expression? ConcatBody(ConditionalExpressionSyntax body, ParameterExpression[] pars = null)
            => Expression.Condition(Dispatcher(body.Condition, pars), Dispatcher(body.WhenTrue, pars), Dispatcher(body.WhenFalse, pars));
        static Expression? ConcatBody(MemberBindingExpressionSyntax body, ParameterExpression[] pars = null) //TODO
        {
            switch (body.Parent.GetType().Name)
            {
                //    //case nameof(SimpleLambdaExpressionSyntax):
                //    //    return Expression.Property((Dispatcher());
                //    //case nameof(ParenthesizedExpressionSyntax):
                //    //    return Expression.Property((Dispatcher(().Expression, pars) as MemberExpression).Member,);
                //    //case nameof(ConditionalAccessExpressionSyntax):
                //    //    return Expression.Property((Dispatcher((body.Parent as ConditionalAccessExpressionSyntax).Expression, pars));
                //    case nameof(IdentifierNameSyntax):
                case nameof(ConditionalAccessExpressionSyntax):
                    var pp = Dispatcher((body.Parent as ConditionalAccessExpressionSyntax).Expression, pars);
                    return Expression.Property(pp, body.Name.Identifier.Text);
                default:
                    return Expression.Property(Dispatcher(body.Parent as ExpressionSyntax, pars), body.Name.Identifier.Text);
            }

            //throw new NotImplementedException();
        }
        static Expression? ConcatBody(InvocationExpressionSyntax body, ParameterExpression[] pars = null)
        {
            //return Expression.MemberBind();
            throw new NotImplementedException();
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
    }
}
