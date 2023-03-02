using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Telegram.Commands.Core.Tests.Core;

public static class ScopedInjectionExtensions
{
    public static void Inject(this IServiceScope scope, object obj)
    {
        var fields = obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
            .Where(x => x.GetCustomAttribute<ScopedInjectionAttribute>() != null)
            .ToArray();

        foreach (var fieldInfo in fields)
        {
            var value = scope.ServiceProvider.GetRequiredService(fieldInfo.FieldType);
            fieldInfo.SetValue(obj, value);
        }
    }
}