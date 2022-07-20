using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Linq.Eval
{
    public static class ExpressionQuery
    {
        public static Dictionary<string, string> JointOperators = new Dictionary<string, string>
        {
            {"&&",nameof(Expression.AndAlso) },
            {"||",nameof(Expression.OrElse) },
            {"!",nameof(Expression.Not) },
            //{"&",nameof(Expression.And) }, 
            //{"|",nameof(Expression.Or) },
        };

        public static Dictionary<string, string> LogicOperators = new Dictionary<string, string>
        {
            {">=",nameof(Expression.GreaterThanOrEqual) },
            {"==",nameof(Expression.Equal) },
            {"!=",nameof(Expression.NotEqual) },
            {"<=",nameof(Expression.LessThanOrEqual) },
            {">",nameof(Expression.GreaterThan) },
            {"<",nameof(Expression.LessThan) },
            {"!",nameof(Expression.Not) },
            //{"??",nameof(Expression.IfThenElse) },
            //{"?:",nameof(Expression.IfThenElse) },
            //{"?.",nameof(Expression.IfThenElse) },

        };

        public static Dictionary<string, string> BinaryOperators = new Dictionary<string, string>
        {
            {"+",nameof(Expression.Add) },
            {"-",nameof(Expression.Subtract) },
            {"*",nameof(Expression.Multiply) },
            {"/",nameof(Expression.Divide) },
            //{"%",nameof(Expression.OnesComplement) },
            {"=",nameof(Expression.Assign) },
            {"+=",nameof(Expression.AddAssign) },
            {"-=",nameof(Expression.SubtractAssign) },
            {"*=",nameof(Expression.MultiplyAssign) },
            {"/=",nameof(Expression.DivideAssign) },
            {"++",nameof(Expression.PostIncrementAssign) },
            {"--",nameof(Expression.PostDecrementAssign) },
        };

        public static Dictionary<string, Expression> ConstKeyWords = new Dictionary<string, Expression>
        {
            { "true",Expression.Constant(true)},
            { "false",Expression.Constant(false)},
        };

        static Expression JoinOperation(string joint, params Expression[] pars)
        {
            switch (joint)
            {
                case nameof(Expression.AndAlso):
                    return Expression.AndAlso(pars[0], pars[1]);
                case nameof(Expression.OrElse):
                    return Expression.OrElse(pars[0], pars[1]);
                case nameof(Expression.Not):
                    return Expression.Not(pars[0]);
                default:
                    throw new Exception("does not support");
            }
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

        public static Expression<T> ToExpression<T>(this string query)
        {
            var left = query.Substring(0, query.IndexOf("=>")).Trim();
            var ts = TypeConvert<T>();
            var argExprs = left.Trim('(').Trim(')').Split(",").Select((x, i) => Expression.Parameter(ts.Pars[i], x.Trim())).ToArray() ?? new ParameterExpression[0];

            var right = query.Substring(query.IndexOf("=>") + 2, query.Length - query.IndexOf("=>") - 2).Trim();
            var rightStrs = ExtractString(ref right).Select(x => x.Substring(2, x.Length - 4)).ToArray();

            return Expression.Lambda<T>(ObjectToExpression(JsonConvert.DeserializeObject<object[]>(Convert(right)), rightStrs, argExprs), argExprs);
        }

        static string[] ExtractString(ref string str)
        {
            var reg = new Regex(@"(\\"".+?\\"")", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            var matches = reg.Matches(str).Select(m => m.Groups[1].Value).ToArray();
            for (int i = 0; i < matches.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(matches[i])) throw new Exception("could not be empty");
                var regex = new Regex(Regex.Escape(matches[i]));
                str = regex.Replace(str, $"{{{i}}}", 1);
            }
            return matches;
        }

        static string Convert(string ts)
        {
            ts = "[\"" + ts   //TODO 
                .Replace("(", "[\"")
                .Replace(")", "\"]")
                .Replace("&&", "\",\"&&\",\"")
                .Replace("||", "\",\"||\",\"")
                .Replace("!", "\",\"!\",\"")
                + "\"]";
            var q = new Stack<char>();
            char[] special = new char[] { '[', ']', ',' };
            char[] tmp = new char[2];
            for (int i = 0; i < ts.Length; i++)
            {
                if (special.Contains(ts[i]))
                {
                    if (tmp.Last() == '"')
                    {
                        while (q.TryPeek(out char res) && (res == ' ' || res == '"'))
                        {
                            q.Pop();
                        }
                    }
                    Array.Clear(tmp, 0, 2);
                    tmp[0] = ts[i];
                }
                else if (ts[i] == '"' && special.Contains(tmp.First()))
                {
                    tmp[1] = ts[i];
                }
                else if (ts[i] != ' ')
                {
                    Array.Clear(tmp, 0, 2);
                }
                q.Push(ts[i]);
            }
            return new string(q.ToArray().Reverse().ToArray()).Replace(",,", ",").Replace("[,", "[");
        }

        static string[] BinarySplit(string query)
        {
            var pattern = @"(\+\+)|(--)|(\+=)|(-=)|(\*=)|(/=)|(\+)|(-)|(\*)|(/)|(%)|(=)";

            return Regex.Split(query, pattern, RegexOptions.IgnorePatternWhitespace).Select(x => x?.Trim()).ToArray();
        }

        //static readonly Regex _isNumericRegex =
        //    new Regex("^(" +
        //                /*Hex*/ @"0x[0-9a-f]+" + "|" +
        //                /*Bin*/ @"0b[01]+" + "|" +
        //                /*Oct*/ @"0[0-7]*" + "|" +
        //                /*Dec*/ @"((?!0)|[-+]|(?=0+\.))(\d*\.)?\d+(e\d+)?" +
        //                ")$");
        static Expression? ConstOrPropExpr(string query, Type tp = null, string[] constStr = null, params ParameterExpression[] pars)
        {
            if (ConstKeyWords.TryGetValue(query, out Expression pr)) // true , false
            {
                return pr;
            }
            else
            {
                if (query.StartsWith('{') && query.EndsWith('}') && int.TryParse(query.Trim('{').Trim('}'), out int index)) //  {1} string
                {
                    return Expression.Constant(constStr[index], typeof(string));
                }
                return !decimal.TryParse(query, out decimal d) ? PropertyExpr(query, pars) : Expression.Constant(System.Convert.ChangeType(query, tp), tp);
            }
        }

        static Expression ConditionExpr(string query)
        {
            throw new NotImplementedException();
        }

        
        static (string? left, string? oper, string? right) JointSplit(string query)
        {
            
            var pattern = @"(>=)|(<=)|(!=)|(==)|(>)|(<)";
            var k = Regex.Split(query, pattern);
            return (k.First(), k.Skip(1).Take(1).FirstOrDefault(), k.Skip(2).Take(1).FirstOrDefault());
        }

        static Expression StrToExpr(string expr, string[] constValues, params ParameterExpression[] pars) //TODO 1, method 2, complex expression 
        {
            // support formats:
            //x.Name == \\\"worker\\\"
            //x.Age < 50
            //!x.IsFavorite
            //!!!x.IsFavorite
            // ??
            // ?.
            // ? :

            expr = expr.Trim();
            if (expr.StartsWith("!"))
            {
                var ind = expr.LastIndexOf('!');
                var torf = expr.Substring(0, ind + 1).Count(x => x == '!') % 2 == 0 ? true : false;
                var res = expr.Substring(ind, expr.Length - ind - 1);
                return torf ? ConditionExpr(res) : Expression.Not(ConditionExpr(res));
            }
            else
            {
                var splited_str = JointSplit(expr);

                if (string.IsNullOrWhiteSpace(splited_str.right))
                {
                    if (string.IsNullOrWhiteSpace(splited_str.left)) throw new Exception("must have a left expression");
                    return LeftToExpr(splited_str.left, constValues, pars);
                }
                else if (string.IsNullOrWhiteSpace(splited_str.left))
                {
                    throw new Exception("must have a left expression");
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(splited_str.oper) || string.IsNullOrWhiteSpace(splited_str.left)) throw new Exception("must have a operator or left expression");
                    var left = LeftToExpr(splited_str.left, constValues, pars);
                    var right = RightToExpr(splited_str.right, constValues, left.Type, pars);
                    return OperationToExpr(splited_str.oper, left, right);
                }
            }
        }
        static Expression OperationToExpr(string oper, Expression left, Expression right)
        {
            switch (oper)
            {
                case ">=":
                    return Expression.GreaterThanOrEqual(left, right);
                case "==":
                    return Expression.Equal(left, right);
                case "<=":
                    return Expression.LessThanOrEqual(left, right);
                case ">":
                    return Expression.GreaterThan(left, right);
                case "<":
                    return Expression.LessThan(left, right);
                default:
                    throw new Exception("does not support this operator");
            }
        }

        static Expression? ConcatExpr(string[] strs, string[] constStr = null, Type? type = null, params ParameterExpression[] pars)
        {

            Expression expr = null;
            string oper = string.Empty;
            for (int i = 0; i < strs.Length; i++)
            {
                if (i == 0)
                    expr = ConstOrPropExpr(strs[i], type, constStr, pars);
                else
                {
                    if (BinaryOperators.TryGetValue(strs[i], out string val))
                    {
                        oper = val;
                    }
                    else
                    {
                        switch (oper)
                        {
                            case nameof(Expression.Add):
                                if (expr.Type == typeof(string)) return Expression.Add(expr, ConstOrPropExpr(strs[i], expr.Type, constStr, pars), typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) }));
                                expr = Expression.Add(expr, ConstOrPropExpr(strs[i], expr.Type, constStr, pars));
                                break;
                            case nameof(Expression.Subtract):
                                expr = Expression.Subtract(expr, ConstOrPropExpr(strs[i], expr.Type, constStr, pars));
                                break;
                            case nameof(Expression.Multiply):
                                expr = Expression.Multiply(expr, ConstOrPropExpr(strs[i], expr.Type, constStr, pars));
                                break;
                            case nameof(Expression.Divide):
                                expr = Expression.Divide(expr, ConstOrPropExpr(strs[i], expr.Type, constStr, pars));
                                break;
                            case nameof(Expression.Assign):
                                expr = Expression.Assign(expr, ConstOrPropExpr(strs[i], expr.Type, constStr, pars));
                                break;
                            case nameof(Expression.AddAssign):
                                expr = Expression.AddAssign(expr, ConstOrPropExpr(strs[i], expr.Type, constStr, pars));
                                break;
                            case nameof(Expression.SubtractAssign):
                                expr = Expression.SubtractAssign(expr, ConstOrPropExpr(strs[i], expr.Type, constStr, pars));
                                break;
                            case nameof(Expression.MultiplyAssign):
                                expr = Expression.MultiplyAssign(expr, ConstOrPropExpr(strs[i], expr.Type, constStr, pars));
                                break;
                            case nameof(Expression.DivideAssign):
                                expr = Expression.DivideAssign(expr, ConstOrPropExpr(strs[i], expr.Type, constStr, pars));
                                break;
                            case nameof(Expression.PostIncrementAssign):
                                expr = Expression.PostIncrementAssign(expr);
                                break;
                            case nameof(Expression.PostDecrementAssign):
                                expr = Expression.PostDecrementAssign(expr);
                                break;
                            default:
                                throw new Exception("does not support");
                        }
                    }
                }
            }
            return expr;
        }
        static Expression LeftToExpr(string left, string[] constValues, params ParameterExpression[] pars)
        {
            var expr_strs = BinarySplit(left);
            return ConcatExpr(expr_strs, constValues, null, pars);

        }
        static Expression PropertyExpr(string left, params ParameterExpression[] pars)
        {
            var fields = left.Split('.').Select(x => x?.Trim()).ToArray();
            if (fields.Length <= 1) throw new Exception("does not support object comparision");
            ParameterExpression prop = pars.First(x => x.Name == fields[0]);
            Expression member = Expression.Property(prop, fields[1]);
            for (int i = 2; i < fields.Length; i++)
            {
                member = Expression.Property(member, fields[i]);
            }
            return member;

        }
        static Expression RightToExpr(string left, string[] constValues, Type? type, params ParameterExpression[] pars)
        {
            var expr_strs = BinarySplit(left);
            if (expr_strs.Length <= 1) return ConcatExpr(expr_strs, constValues, type, pars);
            return ConcatExpr(expr_strs, constValues, null, pars);
        }
        static Expression ConcatJoint(object[] que, Type returnType = null)
        {
            Expression query = null;
            string oper = string.Empty;
            foreach (var e in que)
            {
                var expr = e as Expression;
                if (expr != null)
                {
                    if (query == null && oper != nameof(Expression.Not))
                    {
                        query = expr;
                    }
                    else
                    {
                        switch (oper)
                        {
                            case nameof(Expression.AndAlso):
                                query = Expression.AndAlso(query, expr);
                                break;
                            case nameof(Expression.OrElse):
                                query = Expression.OrElse(query, expr);
                                break;
                            case nameof(Expression.Not):
                                query = Expression.Not(expr);
                                break;
                            case nameof(Expression.Add):
                                query = Expression.Add(query, expr);
                                break;
                            case nameof(Expression.Subtract):
                                query = Expression.Subtract(query, expr);
                                break;
                            case nameof(Expression.Multiply):
                                query = Expression.Multiply(query, expr);
                                break;
                            case nameof(Expression.Divide):
                                query = Expression.Divide(query, expr);
                                break;
                            case nameof(Expression.AddAssign):
                                query = Expression.AddAssign(query, expr);
                                break;
                            case nameof(Expression.SubtractAssign):
                                query = Expression.SubtractAssign(query, expr);
                                break;
                            case nameof(Expression.MultiplyAssign):
                                query = Expression.MultiplyAssign(query, expr);
                                break;
                            case nameof(Expression.DivideAssign):
                                query = Expression.DivideAssign(query, expr);
                                break;
                            default:
                                break;
                        }
                    }
                }
                else
                {
                    oper = e.ToString();
                }
            }
            return query;
        }
        static Expression JArrayToExpression(JArray jar, string[] constValues, params ParameterExpression[] pars)
        {
            var query = new object[jar.Count];
            for (int i = 0; i < jar.Count; i++)
            {
                if (jar[i] as JArray != null)
                {
                    query[i] = JArrayToExpression(jar[i] as JArray, constValues, pars);
                }
                else if ((jar[i] as JValue) != null)
                {
                    
                    if (JointOperators.TryGetValue(jar[i].Value<string>(), out string keyword))
                    {
                        query[i] = keyword;
                    }
                    else
                    {
                        var v = jar[i].Value<string>().Trim();
                        query[i] = StrToExpr(v, constValues, pars);
                    }
                }
                else
                    throw new Exception("can not convert");
            }
            return ConcatJoint(query);
        }
        static Expression ObjectToExpression(object[] jars, string[] constValues, params ParameterExpression[] pars)
        {
            var query = new object[jars.Count()];
            for (int i = 0; i < jars.Count(); i++)
            {
                var jar = jars[i] as JArray;
                if (jar == null)
                {
                    var a = jars[i].ToString();
                    if (JointOperators.TryGetValue(a, out string keyword))
                        query[i] = keyword;
                    else
                        query[i] = StrToExpr(a, constValues, pars);
                }
                else
                {
                    query[i] = JArrayToExpression(jar, constValues, pars);
                }
            }
            return ConcatJoint(query);
        }

    }
}
