using System;

namespace TeslaApi
{
    public class TeslaApiException : Exception
    {
        public TeslaApiException(string message)
            : base(message)
        {
        }
    }
}