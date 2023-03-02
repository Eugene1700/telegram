using Microsoft.Extensions.DependencyInjection;

namespace Telegram.Commands.Core.Tests.Core;

public abstract class TestsBase
{
    public void SetUp()
    {
        var serviceCollection = new ServiceCollection();
        RegisterServices(serviceCollection);
        ServiceProvider = serviceCollection.BuildServiceProvider();

        var scopeFactory = ServiceProvider.GetRequiredService<IServiceScopeFactory>();
        CurrentScope = scopeFactory.CreateScope();
        CurrentScope.Inject(this);
    }

    public IServiceScope CurrentScope { get; private set; }

    protected abstract void RegisterServices(ServiceCollection serviceCollection);

    public IServiceProvider ServiceProvider { get; private set; }
}