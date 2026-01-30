using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Linq.Eval
{
    /// <summary>
    /// Provides extension methods for converting string queries to delegates using dynamic compilation.
    /// </summary>
    public static class DelegateQuery
    {
        /// <summary>
        /// Internal cache for compiled scripts to improve performance when caching is enabled.
        /// </summary>
        internal static ConcurrentDictionary<int, object> ScriptCache { get; } = new ConcurrentDictionary<int, object>();

        /// <summary>
        /// Lazy-loaded script options configured with all non-dynamic assemblies and common namespaces.
        /// </summary>
        internal static readonly Lazy<ScriptOptions> scriptOptions = new Lazy<ScriptOptions>(() =>
        {
            var scriptOptions = ScriptOptions.Default;

            var asms = AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic);
            foreach (Assembly asm in asms)
            {
                scriptOptions = scriptOptions.AddReferences(asm);
            }

            scriptOptions = scriptOptions.AddImports("System");
            scriptOptions = scriptOptions.AddImports("System.Linq");
            return scriptOptions;
        });
        
        /// <summary>
        /// Converts a string query to a delegate of type T using C# scripting.
        /// </summary>
        /// <typeparam name="T">The delegate type to convert to (e.g., Func&lt;Student, bool&gt;).</typeparam>
        /// <param name="query">The string query to evaluate (e.g., "x => x.Age > 18").</param>
        /// <param name="cache">Whether to cache the compiled script for reuse. Default is false.</param>
        /// <returns>A compiled delegate of type T.</returns>
        /// <exception cref="ArgumentNullException">Thrown when query is null or empty.</exception>
        /// <exception cref="CompilationErrorException">Thrown when the query contains syntax errors.</exception>
        /// <example>
        /// <code>
        /// var filter = await "x => x.Age > 18".ToDelegate&lt;Func&lt;Student, bool&gt;&gt;();
        /// var results = students.Where(filter);
        /// </code>
        /// </example>
        public static async Task<T> ToDelegate<T>(this string query, bool? cache = false)
        {
            T script;
            if (cache.HasValue && cache.Value)
            {
                script = (T)ScriptCache.GetOrAdd(query.GetHashCode(), await CSharpScript.EvaluateAsync<T>(query, scriptOptions.Value));
            }
            else
                script = (T)await CSharpScript.EvaluateAsync<T>(query, scriptOptions.Value);
            return script;
        }

    }

}