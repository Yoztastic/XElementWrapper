using System;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Syp.Xml
{
    public class XElementWrapperException : Exception
    {
        public XElementWrapperException(string format) : base (format)
        {
        }

        public XElementWrapperException(string format, Exception exception) : base(format,exception)
        {
        }
    }
}
