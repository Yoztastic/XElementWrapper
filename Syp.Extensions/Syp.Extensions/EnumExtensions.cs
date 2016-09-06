using System;
using System.Collections.Generic;

namespace Syp.Extensions
{
    public static class EnumExtensions
    {
        public static T ParseEnum<T>(this string value) where T : struct
        {
            ThrowIfNotEnum<T>();
            T t;
            return TryParseEnum(value, out t) ? t : Throw<T>(value);
        }

        public static bool TryParseEnum<T>(this string value, out T t) where T : struct
        {
            t = default(T);
            if (!typeof(T).IsEnum) return false;
            return Enum.TryParse(value, false, out t) || Enum.TryParse(value, true, out t);
        }

        private static T Throw<T>(string name) where T : struct
        {
            var enumType = typeof(T);
            const string messageFmt = "[{0}] is not a known member of Enum<{1}> known names are [{2}]";
            throw new UnknownEnumException(string.Format(messageFmt, name.IsNullOrWhiteSpace() ? "missing or empty" : name, enumType.Name, Names<T>().ToString(",")));
        }

        private static IEnumerable<string> Names<T>() where T : struct
        {
            ThrowIfNotEnum<T>();
            return typeof(T).GetEnumNames();
        }

        private static void ThrowIfNotEnum<T>() where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException("this utility is only for Enum types");
        }
    }
}