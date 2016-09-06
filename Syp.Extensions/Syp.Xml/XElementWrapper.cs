using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Syp.Extensions;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Syp.Xml
{
    public class XElementWrapper 
    {
        private readonly XNamespace _xmlNamespace;
        private readonly Action<string> _explain;

        public static XElementWrapper Wrap(XElement element, XNamespace xmlNamespace, Action<string> explain = null)
        {
            return new XElementWrapper(element, xmlNamespace, explain);
        }

        private XElementWrapper(XElement element, XNamespace xmlNamespace, Action<string> explain)
        {
            _xmlNamespace = xmlNamespace;
            _explain = explain;
            Element = element;
        }

        public virtual ValueWrapper As
        {
            get
            {
                _explain?.Invoke(Path(Element).Select(n => $"<{n}>").ToString(" -> ") + " = " + Value);
                return new ValueWrapper(this);
            }
        }

        public XElementsWrapper Elements
        {
            get
            {
                Check("Value");
                return new XElementsWrapper(Element, _xmlNamespace, _explain);
            }
        }

        public XElementWrapper this[string elementName]
        {
            get
            {
                Check(elementName);
                var swsElement = Element.Element(_xmlNamespace + elementName);
                if (swsElement != null)
                    return new XElementWrapper(swsElement, _xmlNamespace, _explain);
                return new EmptyXElementWrapper(Element, elementName, _xmlNamespace, _explain);
            }
        }

        public XElementWrapper this[XNamespace ns, string elementName]
        {
            get
            {
                Check(elementName);
                var element = Element.Element(ns + elementName);
                return element != null ? new XElementWrapper(element, ns, _explain) : new EmptyXElementWrapper(Element, elementName, ns, _explain);
            }
        }

        public bool Exists => Element != null;

        #region privates

        public XElement Element { get; }

        private string Value
        {
            get
            {
                Check("Value");
                return Element.Value;
            }
        }

        private void Check(string elementName)
        {
            if (!Exists)
                throw new XElementWrapperException($"'{elementName}' not found on null Element");
        }

        private static IEnumerable<string> Path(XElement element)
        {
            return element.AncestorsAndSelf().Select(Location).Reverse().ToList();
        }

        private static string Location(XElement x)
        {
            var xmlLineInfo = x as IXmlLineInfo;
            return x.Name.LocalName + (xmlLineInfo != null && xmlLineInfo.HasLineInfo() ? ":" + xmlLineInfo.LineNumber + ":" + xmlLineInfo.LinePosition : "");
        }

        #endregion

        #region Fluent DSL

        public class ValueWrapperBase
        {
            protected readonly XElementWrapper ElementWrapper;

            protected ValueWrapperBase(XElementWrapper elementWrapper)
            {
                ElementWrapper = elementWrapper;
            }

            public bool Bool => As<bool>();

            public double Double => As<double>();

            public int Int => As<int>();

            public DateTime DateTime => ElementWrapper == null || !ElementWrapper.Exists ? default(DateTime) : As<DateTime>();

            public T Value<T>(Func<XElementWrapper, T> custom)
            {
                return custom(ElementWrapper);
            }

            public decimal Decimal => As<decimal>();

            public virtual string String => ElementWrapper.Value;

            public T Enum<T>() where T : struct
            {
                return ElementWrapper == null || ElementWrapper.Value.IsNullOrWhiteSpace() ? default(T) : ElementWrapper.Value.ParseEnum<T>();
            }

            private T As<T>() where T : struct, IConvertible
            {
                T value;
                try
                {
                    value = ElementWrapper?.Value.As<T>() ?? default(T);
                }
                catch (Exception e)
                {
                    throw UnParsable<T>(e);
                }
                return value;
            }

            protected XElementWrapperException UnParsable<T>(Exception e) where T : struct
            {
                var type = typeof(T).Name;
                return new XElementWrapperException(
                    $"Value [{ElementWrapper.Value}] ElementPath '{Path(ElementWrapper.Element).ToString(" -> ")}' not could not parse as '{type}'", e);
            }
        }

        public class ValueWrapper : ValueWrapperBase
        {

            public ValueWrapper(XElementWrapper elementWrapper) : base(elementWrapper)
            {
            }

            public virtual StrictValueWrapper Strict => new StrictValueWrapper(ElementWrapper);

            public NullableValueWrapper Nullable => new NullableValueWrapper(ElementWrapper);
        }

        public class StrictValueWrapper : ValueWrapperBase
        {

            internal StrictValueWrapper(XElementWrapper elementWrapper)
                : base(elementWrapper)
            {
                Check();
            }

            private void Check()
            {
                if (!ElementWrapper.Exists)
                    throw new XElementWrapperException("Value could not be determined on null Element");
                if (ElementWrapper.Value.IsNullOrWhiteSpace())
                {
                    throw new XElementWrapperException($"Value is Empty for {Path(ElementWrapper.Element).ToString(".")}");
                }
            }
        }

        public class NullableValueWrapper : ValueWrapperBase
        {

            public NullableValueWrapper(XElementWrapper elementWrapper)
                : base(elementWrapper)
            {
            }

            public new decimal? Decimal => AsNullable<decimal>();

            public new double? Double => AsNullable<double>();

            public new int? Int => AsNullable<int>();

            public new bool? Bool => AsNullable<bool>();

            public new DateTime? DateTime => ElementWrapper == null || ElementWrapper.Value.IsNullOrWhiteSpace() ? null : AsNullable<DateTime>();

            public override string String => ElementWrapper?.Value;

            public new T? Enum<T>() where T : struct
            {
                if (ElementWrapper == null || ElementWrapper.Value.IsNullOrWhiteSpace())
                    return null;
                T t;
                return ElementWrapper.Value.TryParseEnum(out t) ? t : (T?)null; // you may want this to throw if not valid 
            }

            private T? AsNullable<T>() where T : struct, IConvertible
            {
                try
                {
                    return ElementWrapper != null && ElementWrapper.Exists ?
                        ElementWrapper.Value.AsNullable<T>() : null;
                }
                catch (Exception e)
                {
                    throw UnParsable<T>(e);
                }
            }
        }

        #endregion

        private class EmptyXElementWrapper : XElementWrapper
        {
            private readonly XElement _parent;
            private readonly string _missingElement;

            public EmptyXElementWrapper(XElement parent, string missingElement, XNamespace ns, Action<string> explain)
                : base(null, ns, explain)
            {
                _parent = parent;
                _missingElement = missingElement;
            }

            #region Overrides of XElementWrapper

            public override ValueWrapper As
            {
                get
                {
                    if (Element != null) _explain?.Invoke(Path(Element).Select(n => $"<{n}>").ToString(" -> ") + " is Empty");
                    return new EmptyValueWrapper(this);
                }
            }

            private class EmptyValueWrapper : ValueWrapper
            {
                private readonly EmptyXElementWrapper _emptyXElementWrapper;

                public EmptyValueWrapper(EmptyXElementWrapper emptyXElementWrapper) : base(null)
                {
                    _emptyXElementWrapper = emptyXElementWrapper;
                }

                public override StrictValueWrapper Strict
                {
                    get
                    {
                        throw new XElementWrapperException(
                            $"Element '{Path(_emptyXElementWrapper._parent).Select(x => $"<{x}>").ToString(" -> ")}' has no Element named '{_emptyXElementWrapper._missingElement}'");
                    }
                }

            }

            #endregion
        }
        public class XElementsWrapper
        {
            private readonly XElement _element;
            private readonly XNamespace _xmlNamespace;
            private readonly Action<string> _explain;

            public XElementsWrapper(XElement element, XNamespace xmlNamespace, Action<string> explain)
            {
                _element = element;
                _xmlNamespace = xmlNamespace;
                _explain = explain;
            }

            public IEnumerable<XElementWrapper> this[string elementName]
            {
                get
                {
                    return GetElements(_element, elementName).Select(element => Wrap(element, _xmlNamespace, _explain));
                }
            }

            private IEnumerable<XElement> GetElements(XContainer source, string elementName)
            {
                return source.Elements(_xmlNamespace + elementName);
            }
        }

    }
}