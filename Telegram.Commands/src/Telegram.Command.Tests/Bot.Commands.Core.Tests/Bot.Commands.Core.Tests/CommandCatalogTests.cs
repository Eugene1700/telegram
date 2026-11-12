using System.Reflection;
using System.Reflection.Emit;
using Bot.Commands.Abstract.Attributes;
using Moq;

namespace Bot.Commands.Core.Tests;

public class CommandCatalogTests
{
    [Test]
    public void ReusesMetadataAcrossArgumentsAndKeepsEventsSeparate()
    {
        var assembly = CommandsAssembly(typeof(CachedCommand), typeof(CachedEvent));
        var catalog = new CommandCatalog(new[] { assembly.Object });
        var first = catalog.FindCommands("/catalog_test first").Single();
        Assert.That(catalog.FindCommands("/catalog_test second").Single(), Is.SameAs(first));
        Assert.That(catalog.FindCommands("/CATALOG_TEST"), Is.Empty);
        Assert.That(catalog.FindCommands("/catalog_event"), Is.Empty);
        var eventDescriptor = catalog.FindEvents(EventType.ChatMemberLeft).Single();
        Assert.That(catalog.FindEvents(EventType.ChatMemberLeft).Single(), Is.SameAs(eventDescriptor));
        assembly.Verify(a => a.GetTypes(), Times.Once);
    }

    [Test]
    public void ExplicitAssemblyListIsCopiedAndCachedOnce()
    {
        var first = CommandsAssembly(typeof(CachedCommand));
        var second = CommandsAssembly(typeof(OtherCommand));
        var loaded = new[] { first.Object };
        var catalog = new CommandCatalog(loaded);
        Assert.That(catalog.FindCommands("/catalog_other"), Is.Empty);
        Assert.That(catalog.FindCommands("/catalog_test"), Has.Length.EqualTo(1));
        loaded[0] = second.Object;
        AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("CatalogExplicit_" + Guid.NewGuid()), AssemblyBuilderAccess.Run);
        Assert.That(catalog.FindCommands("/catalog_other"), Is.Empty);
        Assert.That(catalog.FindCommands("/catalog_test"), Has.Length.EqualTo(1));
        first.Verify(a => a.GetTypes(), Times.Once);
        second.Verify(a => a.GetTypes(), Times.Never);
    }

    [Test]
    public void EmptyAssemblyListDoesNotFallBackToCurrentDomain()
    {
        Assert.That(new CommandCatalog(Array.Empty<Assembly>()).GetCommandTypes(), Is.Empty);
    }

    [Test]
    public void DefaultCatalogRefreshesAfterAssemblyLoad()
    {
        var catalog = new CommandCatalog();
        catalog.FindCommands("/catalog_test");
        var beforeLoad = catalog.FindCommands("/catalog_test").Single();
        Assert.That(catalog.FindCommands("/catalog_test").Single(), Is.SameAs(beforeLoad));
        // A real AssemblyLoad event, without adding commands or special handling of emitted types.
        AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("CatalogLoad_" + Guid.NewGuid()), AssemblyBuilderAccess.Run);
        var afterLoad = catalog.FindCommands("/catalog_test").Single();
        Assert.That(afterLoad.Type, Is.EqualTo(beforeLoad.Type));
        Assert.That(afterLoad, Is.Not.SameAs(beforeLoad));
    }

    private static Mock<Assembly> CommandsAssembly(params Type[] types)
    {
        var assembly = new Mock<Assembly>();
        assembly.Setup(a => a.GetName()).Returns(new AssemblyName("Catalog_" + Guid.NewGuid()));
        assembly.Setup(a => a.GetReferencedAssemblies()).Returns(new[] { typeof(CommandAttribute).Assembly.GetName() });
        assembly.Setup(a => a.GetTypes()).Returns(types);
        return assembly;
    }

    [Command(Name = "catalog_test")]
    public sealed class CachedCommand { }
    [Command(Name = "catalog_other")]
    public sealed class OtherCommand { }
    [Event(Name = "catalog_event", Type = EventType.ChatMemberLeft)]
    public sealed class CachedEvent { }
}
