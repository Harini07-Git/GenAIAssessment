using System;

namespace PaymentGateway.Exceptions
{
    public class PaymentProcessingException : Exception
    {
        public PaymentProcessingException(string message) : base(message)
        {
        }

        public PaymentProcessingException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }

    public class RefundProcessingException : Exception
    {
        public RefundProcessingException(string message) : base(message)
        {
        }

        public RefundProcessingException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }

    public class ApiCommunicationException : Exception
    {
        public ApiCommunicationException(string message) : base(message)
        {
        }

        public ApiCommunicationException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
