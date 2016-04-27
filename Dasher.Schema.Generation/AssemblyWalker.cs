using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Dasher.Schema.Generation
{
    public class AssemblyWalker
    {
        private readonly List<string> ExcludedPrefixes = new List<string> { "System", "Microsoft" };
        private readonly List<string> ExcludedAssemblies = new List<string> { "Dasher", "Dasher.Schema" };
        private readonly List<string> IncludedPrefixes = new List<string>();        
        private readonly List<string> IncludedAssemblies = new List<string>();
        private HashSet<string> _usedAssemblies;

        public AssemblyWalker(string includedDependencies, string excludedDependencies)
        {
            AddPrefixesAndAssemblies(includedDependencies, IncludedPrefixes, IncludedAssemblies);
            AddPrefixesAndAssemblies(excludedDependencies, ExcludedPrefixes, ExcludedAssemblies);
        }

        public DasherAssemblyInfo GetDasherAssemblyInfo(Assembly assembly)
        {
            _usedAssemblies = new HashSet<string>();
            var dir = Path.GetDirectoryName(assembly.Location);
            var result = new DasherAssemblyInfo();
            GetMessages(assembly, dir, result);
            return result;
        }

        private void AddPrefixesAndAssemblies(string dependencies, List<string> prefixes, List<string> assemblies)
        {
            if (string.IsNullOrWhiteSpace(dependencies)) return;
            if (prefixes == null) throw new ArgumentNullException(nameof(prefixes));
            if (assemblies == null) throw new ArgumentNullException(nameof(assemblies));
            var list = dependencies.Split(',').Where(s => !string.IsNullOrWhiteSpace(s));
            foreach (var s in list)
            {
                if (s.EndsWith("*") && s.Length > 1)
                {
                    prefixes.Add(s.Remove(s.Length - 1, 1));
                }
                else assemblies.Add(s);
            }
        }

        private void GetMessages(Assembly assembly, string dir, DasherAssemblyInfo result, int depth = 0)
        {
            if (depth > 2) return; //just in case of circular references. Too large number slows down the whole operation
            foreach (var t in assembly.GetTypes())
            {
                var attributes = t.GetCustomAttributes(typeof(DasherMessageAttribute), false).Cast<DasherMessageAttribute>().ToList();
                if (attributes.Any(a => a.Usage == MessageDirection.SendOnly || a.Usage == MessageDirection.SendReceive))
                    result.SendMessageTypes.Add(t);
                if (attributes.Any(a => a.Usage == MessageDirection.ReceiveOnly || a.Usage == MessageDirection.SendReceive))
                    result.ReceiveMessageTypes.Add(t);
            }
            var referencedAssemblyNames = GetFilteredReferencedAssemblyNames(assembly.GetReferencedAssemblies());
            foreach (var refAssemblyName in referencedAssemblyNames)
            {
                if (_usedAssemblies.Contains(refAssemblyName.Name)) continue;
                var refAssembly = TryLoadAssembly(dir, refAssemblyName);
                if (refAssembly == null)
                {
                    Debug.WriteLine($"Cannot load assembly {refAssemblyName.Name}");
                    continue;
                }
                GetMessages(refAssembly, dir, result, depth + 1);
            }
        }

        private IList<AssemblyName> GetFilteredReferencedAssemblyNames(AssemblyName[] referencedAssemblies)
        {
            return referencedAssemblies.Where(a =>
                !ExcludedPrefixes.Any(e => a.Name.StartsWith(e, StringComparison.OrdinalIgnoreCase))
            &&  !ExcludedAssemblies.Any(e => a.Name.Equals(e, StringComparison.OrdinalIgnoreCase))
            && ((IncludedPrefixes.Any() && IncludedPrefixes.Any(e => a.Name.StartsWith(e, StringComparison.OrdinalIgnoreCase)))
            || (IncludedAssemblies.Any() && IncludedAssemblies.Any(e => a.Name.Equals(e, StringComparison.OrdinalIgnoreCase)))
            || (!IncludedPrefixes.Any() && !IncludedAssemblies.Any())
            )).ToList();
        }

        private Assembly TryLoadAssembly(string dir, AssemblyName refAssemblyName)
        {
            Assembly refAssembly;
            try
            {
                refAssembly = Assembly.Load(refAssemblyName);
                Debug.WriteLine($"Loaded by Load {refAssemblyName.Name}");
            }
            catch
            {
                var assemblyFile = Path.Combine(dir, refAssemblyName.Name + ".dll");
                if (!File.Exists(assemblyFile)) return null;
                try
                {
                    refAssembly = Assembly.LoadFrom(assemblyFile);
                    Debug.WriteLine($"Loaded by LoadFrom {assemblyFile}");
                }
                catch
                {
                    return null;
                }
            }
            _usedAssemblies.Add(refAssemblyName.Name);
            return refAssembly;
        }
    }
}