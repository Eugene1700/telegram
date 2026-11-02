using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bot.Commands.Abstract.Attributes;

namespace Bot.Commands.Core;

/// <summary>Discovers types only in loaded assemblies that depend on command contracts.</summary>
public static class CommandTypeDiscovery
{
    public static IEnumerable<Type> GetTypes() => GetTypes(AppDomain.CurrentDomain.GetAssemblies());

    public static IEnumerable<Type> GetTypes(IEnumerable<Assembly> assemblies)
    {
        var loaded = assemblies.ToArray();
        var relevant = new HashSet<string> { typeof(CommandAttribute).Assembly.GetName().Name };
        var references = loaded.Select(a => (Assembly: a, References: a.GetReferencedAssemblies())).ToArray();
        bool changed;
        do
        {
            changed = false;
            foreach (var entry in references)
                if (entry.References.Any(r => relevant.Contains(r.Name)))
                    changed |= relevant.Add(entry.Assembly.GetName().Name);
        } while (changed);

        // Do not hide loader errors in command assemblies: those require fixing.
        return loaded.Where(a => relevant.Contains(a.GetName().Name)).SelectMany(a => a.GetTypes());
    }
}
