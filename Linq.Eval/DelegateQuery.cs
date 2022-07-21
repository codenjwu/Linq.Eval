using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Linq.Eval
{
    public static class DelegateQuery
    {
        internal static ConcurrentDictionary<int, object> ScriptCache { get; } = new ConcurrentDictionary<int, object>();

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