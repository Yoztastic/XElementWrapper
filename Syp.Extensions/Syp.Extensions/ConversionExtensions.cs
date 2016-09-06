using System;
using System.Collections.Generic;
using System.Reflection;

namespace Syp.Extensions
{
    public static class ConversionExtensions
    {
        private static readonly Dictionary<Type, Method> Dictionary = new Dictionary<Type, Method>();

        public static T As<T>(this string value) where T : struct, IConvertible
        {
            if (value.IsNullOrWhiteSpace())
                return default(T);
            var parsable = typeof(T).GetOrCache(GetMethod<T>, Dictionary).Cast<T>();
            return parsable.Parse(value);
        }

        public static T? AsNullable<T>(this string value) where T : struct, IConvertible
        {
            if (value.IsNullOrWhiteSpace())
                return default(T?);
            var parsable = typeof(T).GetOrCache(GetMethod<T>, Dictionary).Cast<T>();
            return parsable.Parse(value);
        }

        private static Method<T> GetMethod<T>(Type t) where T : struct, IConvertible
        {
            return new Method<T>(t.GetMethod("Parse", new[] { typeof(string) }));
        }

        private class Method<T> : Method where T : struct, IConvertible
        {
            public Method(MethodInfo methodInfo) : base(methodInfo)
            {
            }

            public T Parse(string input)
            {
                return (T)MethodInfo.Invoke(null, new object[] { input });
            }
        }

        private abstract class Method
        {
            protected readonly MethodInfo MethodInfo;

            protected Method(MethodInfo methodInfo)
            {
                MethodInfo = methodInfo;
            }

            public Method<T> Cast<T>() where T : struct, IConvertible
            {
                return (Method<T>)this;
            }
        }

    }
}