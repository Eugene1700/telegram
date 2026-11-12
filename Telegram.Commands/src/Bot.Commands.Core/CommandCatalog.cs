using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Core.Models;

namespace Bot.Commands.Core;

// Cache registered metadata, not user input or a particular bot's routing decisions.
public sealed class CommandCatalog
{
    private readonly Assembly[] _configuredAssemblies;
    private readonly object _sync = new();
    private static int _assemblyVersion;
    private int _cachedAssemblyVersion;
    private ILookup<string, FullCommandDescriptor> _commands;
    private ILookup<EventType, FullCommandDescriptor> _events;

    static CommandCatalog()
    {
        AppDomain.CurrentDomain.AssemblyLoad += (_, _) => Interlocked.Increment(ref _assemblyVersion);
    }

    public CommandCatalog(IEnumerable<Assembly> assemblies = null)
        => _configuredAssemblies = assemblies?.ToArray();

    public Type[] GetCommandTypes()
    {
        lock (_sync)
        {
            EnsureInitialized();
            return _commands.SelectMany(group => group).Concat(_events.SelectMany(group => group))
                .Select(command => command.Type).ToArray();
        }
    }

    internal FullCommandDescriptor[] FindCommands(string query)
    {
        if (string.IsNullOrEmpty(query)) return Array.Empty<FullCommandDescriptor>();
        var name = BotCommandExtensions.ExtractCommand(query);
        lock (_sync)
        {
            EnsureInitialized();
            return _commands[name].ToArray();
        }
    }

    internal FullCommandDescriptor[] FindCommands(Type commandType)
    {
        if (commandType == null) throw new ArgumentNullException(nameof(commandType));
        lock (_sync)
        {
            EnsureInitialized();
            return _commands.SelectMany(group => group)
                .Where(command => commandType.IsAbstract
                    ? commandType.IsAssignableFrom(command.Type)
                    : command.Type == commandType)
                .ToArray();
        }
    }

    internal FullCommandDescriptor[] FindEvents(EventType eventType)
    {
        lock (_sync)
        {
            EnsureInitialized();
            return _events[eventType].ToArray();
        }
    }

    private void EnsureInitialized()
    {
        var assemblyVersion = Volatile.Read(ref _assemblyVersion);
        if (_commands != null && (_configuredAssemblies != null || _cachedAssemblyVersion == assemblyVersion))
            return;

        var assemblies = _configuredAssemblies ?? AppDomain.CurrentDomain.GetAssemblies();
        var descriptors = Describe(CommandTypeDiscovery.GetTypes(assemblies)).ToArray();
        _commands = descriptors.Where(d => d.Descriptor is not EventAttribute)
            .ToLookup(d => d.Descriptor.Name, StringComparer.Ordinal);
        _events = descriptors.Where(d => d.Descriptor is EventAttribute)
            .ToLookup(d => ((EventAttribute)d.Descriptor).Type);
        _cachedAssemblyVersion = assemblyVersion;
    }

    private static IEnumerable<FullCommandDescriptor> Describe(IEnumerable<Type> types)
        => types.Where(t => t.IsClass && !t.IsAbstract && !t.ContainsGenericParameters)
            .Select(t => new FullCommandDescriptor(t)).Where(d => d.Descriptor != null);
}
