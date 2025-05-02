using System;

namespace UnisonMQ.Client
{
    [Serializable]
    public class UnisonMqClientException : Exception
    {
        public UnisonMqClientException() { }
        public UnisonMqClientException(string? message) : base(message) { }
        public UnisonMqClientException(string? message, Exception? inner) : base(message, inner) { }
    }
}