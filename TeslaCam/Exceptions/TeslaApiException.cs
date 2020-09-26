using System;

namespace TeslaCam.Exceptions
{
    public class TeslaApiException : Exception
    {
        public TeslaApiException(string message)
            : base(message)
        {
        }
    }
}