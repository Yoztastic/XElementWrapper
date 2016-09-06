using System;

namespace Syp.Extensions
{
    [Serializable]
    public class UnknownEnumException : Exception
    {
        public UnknownEnumException(string message) : base(message)
        {
        }
    }
}
