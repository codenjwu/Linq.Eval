using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Concurrent;
using System.Linq;

namespace Linq.Eval
{
    public static class DelegateQuery
    {
        internal static ConcurrentDictionary<int, object> ScriptCache { get; } = new ConcurrentDictionary<int, object>();
        public static async Task<IEnumerable<T>> Where<T>(this IEnumerable<T> source, string query, bool? cache = false, params string[] pars)
        {
            Func<T, bool> script;
            if (cache.HasValue && cache.Value)
            {
                script = ScriptCache.GetOrAdd(query.GetHashCode(), await CSharpScript.EvaluateAsync<Func<T, bool>>(query)) as Func<T, bool>;
            }
            else 
                script = await CSharpScript.EvaluateAsync<Func<T, bool>>(query) as Func<T, bool>;

            return source.Where(script);
        }
        
        public static async Task<IEnumerable<TResult>> Select<TSource, TResult>(this IEnumerable<TSource> source, string query, bool? useIndex = false, bool? cache = false)
        {
            if (useIndex == null || !useIndex.Value)
            {
                Func<TSource, TResult> script;
                if (cache.HasValue && cache.Value)
                {
                    script = ScriptCache.GetOrAdd(query.GetHashCode(), await CSharpScript.EvaluateAsync<Func<TSource, TResult>>(query)) as Func<TSource, TResult>;
                }
                else
                    script = await CSharpScript.EvaluateAsync<Func<TSource, TResult>>(query) as Func<TSource, TResult>;

                return source.Select(script);
            }
            else
            {
                Func<TSource, int, TResult> script;
                if (cache.HasValue && cache.Value)
                {
                    script = ScriptCache.GetOrAdd(query.GetHashCode(), await CSharpScript.EvaluateAsync<Func<TSource, int, TResult>>(query)) as Func<TSource, int, TResult>;
                }
                else
                    script = await CSharpScript.EvaluateAsync<Func<TSource, int, TResult>>(query) as Func<TSource, int, TResult>;
                return source.Select(script);
            }
        }

        public static async Task<IEnumerable<TSource>> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, string keySelector, IComparer<TKey>? comparer = null, bool? cache = false)
        {
            Func<TSource, TKey> script;
            if (cache.HasValue && cache.Value)
                script = ScriptCache.GetOrAdd(keySelector.GetHashCode(), await CSharpScript.EvaluateAsync<Func<TSource, TKey>>(keySelector)) as Func<TSource, TKey>;
            else
                script = await CSharpScript.EvaluateAsync<Func<TSource, TKey>>(keySelector) as Func<TSource, TKey>;
                //var script = await CSharpScript.EvaluateAsync<Func<TSource, TKey>>(query) as Func<TSource, TKey>;
            return comparer == null ? source.OrderBy(script) : source.OrderBy(script, comparer);
        }

        public static async Task<IEnumerable<TSource>> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, string keySelector, IComparer<TKey>? comparer = null, bool? cache = false)
        {
            Func<TSource, TKey> script;
            if (cache.HasValue && cache.Value)
                script = ScriptCache.GetOrAdd(keySelector.GetHashCode(), await CSharpScript.EvaluateAsync<Func<TSource, TKey>>(keySelector)) as Func<TSource, TKey>; 
            else
                script = await CSharpScript.EvaluateAsync<Func<TSource, TKey>>(keySelector) as Func<TSource, TKey>;
            return comparer == null ? source.ThenBy(script) : source.ThenBy(script, comparer);
        }
        public static async Task<TSource> Aggregate<TSource>(this IEnumerable<TSource> source, string func, bool? cache = false)
        {
            Func<TSource, TSource, TSource> script;
            if (cache.HasValue && cache.Value)
                script = ScriptCache.GetOrAdd(func.GetHashCode(), await CSharpScript.EvaluateAsync<Func<TSource, TSource, TSource>>(func)) as Func<TSource, TSource, TSource>;
            else
                script = await CSharpScript.EvaluateAsync<Func<TSource, TSource, TSource>>(func) as Func<TSource, TSource, TSource>;
            return source.Aggregate(script);
        }


        public static async Task<bool> All<TSource>(this IEnumerable<TSource> source, string predicate, bool? cache = false)
        {
            Func<TSource, bool> script;
            if (cache.HasValue && cache.Value)
                script = ScriptCache.GetOrAdd(predicate.GetHashCode(), await CSharpScript.EvaluateAsync<Func<TSource, bool>>(predicate)) as Func<TSource, bool>;
            else
                script = await CSharpScript.EvaluateAsync<Func<TSource, bool>>(predicate) as Func<TSource, bool>;
            return source.All(script);
        }

        public static async Task<bool> Any<TSource>(this IEnumerable<TSource> source, string predicate, bool? cache = false)
        {
            Func<TSource, bool> script;
            if (cache.HasValue && cache.Value)
                script = ScriptCache.GetOrAdd(predicate.GetHashCode(), await CSharpScript.EvaluateAsync<Func<TSource, bool>>(predicate)) as Func<TSource, bool>;
            else
                script = await CSharpScript.EvaluateAsync<Func<TSource, bool>>(predicate) as Func<TSource, bool>;
            return source.Any(script);
        }

    }

}