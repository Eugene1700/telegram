using System.Reflection;
using Bot.Commands.Abstract.Attributes;
using Moq;

namespace Bot.Commands.Core.Tests;

public class CommandTypeDiscoveryTests
{
    [Test]
    public void UnrelatedBrokenAssemblyIsNotInspected()
    {
        var unrelated = Assembly("Legacy.Orm");
        unrelated.Setup(a => a.GetTypes()).Throws(new ReflectionTypeLoadException(Array.Empty<Type>(), Array.Empty<Exception>()));
        var commands = Assembly("Application.Commands", typeof(CommandAttribute).Assembly.GetName());
        commands.Setup(a => a.GetTypes()).Returns(new[] { typeof(CommandTypeDiscoveryTests) });

        var types = CommandTypeDiscovery.GetTypes(new[] { unrelated.Object, commands.Object }).ToArray();

        Assert.That(types, Does.Contain(typeof(CommandTypeDiscoveryTests)));
        unrelated.Verify(a => a.GetTypes(), Times.Never);
    }

    [Test]
    public void TransitiveCommandDependenciesAreIncluded()
    {
        var parent = Assembly("Shared.Commands", typeof(CommandAttribute).Assembly.GetName());
        parent.Setup(a => a.GetTypes()).Returns(Array.Empty<Type>());
        var child = Assembly("Application.Commands", parent.Object.GetName());
        child.Setup(a => a.GetTypes()).Returns(new[] { typeof(CommandTypeDiscoveryTests) });
        Assert.That(CommandTypeDiscovery.GetTypes(new[] { child.Object, parent.Object }),
            Does.Contain(typeof(CommandTypeDiscoveryTests)));
    }

    [Test]
    public void BrokenCommandAssemblyStillReportsLoaderError()
    {
        var commands = Assembly("Broken.Commands", typeof(CommandAttribute).Assembly.GetName());
        commands.Setup(a => a.GetTypes()).Throws(new ReflectionTypeLoadException(Array.Empty<Type>(), Array.Empty<Exception>()));
        Assert.Throws<ReflectionTypeLoadException>(() => CommandTypeDiscovery.GetTypes(new[] { commands.Object }).ToArray());
    }

    private static Mock<Assembly> Assembly(string name, params AssemblyName[] references)
    {
        var assembly = new Mock<Assembly>();
        assembly.Setup(a => a.GetName()).Returns(new AssemblyName(name));
        assembly.Setup(a => a.GetReferencedAssemblies()).Returns(references);
        return assembly;
    }
}
