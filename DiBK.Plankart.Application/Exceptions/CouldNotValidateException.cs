using System;

namespace DiBK.Plankart.Application.Exceptions
{
    public class CouldNotValidateException : Exception
    {
        public CouldNotValidateException()
        {
        }

        public CouldNotValidateException(string message) : base(message)
        {
        }

        public CouldNotValidateException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
