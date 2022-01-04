using System;
using System.Linq;

namespace Telegram.Commands.Core.Services
{
    internal static class ReflectionHelper
    {
        public static Type GetTypeInterface(this Type type, Type interfaceType)
        {
            return type
                .GetInterfaces()
                .SingleOrDefault(i => i.IsGenericType &&
                                      i.GetGenericTypeDefinition() == interfaceType);
        }
    }
}