using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using Telegram.Commands.Core.Exceptions;

namespace Telegram.Commands.Core.Services
{
    public static class CallbackDataSerializer
    {
        public static string Serialize(this object obj)
        {
            var properties = from p in obj.GetType().GetProperties()
                where p.GetValue(obj, null) != null
                select p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(obj, null)?.ToString());

            return string.Join("&", properties.ToArray());
        }

        public static T Deserialize<T>(this string input) where T : new()
        {
            var obj = new T();
            var properties = typeof(T).GetProperties();
            var dictParams = input.ToDict();
            foreach (var property in properties)
            {
                var valueAsString = dictParams[property.Name];
                var typeConverter = TypeDescriptor.GetConverter(property.PropertyType);
                var value = typeConverter.ConvertFromString(valueAsString);

                if (value == null)
                    continue;

                property.SetValue(obj, value, null);
            }

            return obj;
        }

        private static IDictionary<string, string> ToDict(this string input)
        {
            var paramStr = input.Split('?');
            if (paramStr.Length > 2)
                throw new TelegramCommandsInternalException("Incorrect format");
            var r = paramStr[0].Split('&').Select(ToKeyValuePair);
            return new Dictionary<string, string>(r);
        }

        private static KeyValuePair<string, string> ToKeyValuePair(string input)
        {
            var keyValue = input.Split('=');
            return new KeyValuePair<string, string>(keyValue[0], keyValue[1]);
        }
    }
}