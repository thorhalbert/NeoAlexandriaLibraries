using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NeoCliFunctions
{
    public static class GetVerbs
    {
        // Find all the verbs in this assembly
        public static Type[] GetVerbInAssembly(List<Type> typesL)
        {
            typesL.AddRange(Assembly.GetExecutingAssembly().GetTypes()
              .Where(t => t.GetCustomAttribute<VerbAttribute>() != null).ToList());

            return typesL.ToArray();
        }
    }
}
