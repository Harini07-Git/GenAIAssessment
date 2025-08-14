using System;

namespace PaymentGateway.Models
{
    public class PaymentRequest
    {
        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string CardNumber { get; set; }
        public string CardHolderName { get; set; }
        public string ExpiryMonth { get; set; }
        public string ExpiryYear { get; set; }
        public string Cvv { get; set; }
    }

    public class PaymentResponse
    {
        public string TransactionId { get; set; }
        public string Status { get; set; }
        public DateTime Timestamp { get; set; }
        public string AuthorizationCode { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class RefundRequest
    {
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; }
    }

    public class RefundResponse
    {
        public string RefundId { get; set; }
        public string Status { get; set; }
        public DateTime Timestamp { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class PaymentStatusResponse
    {
        public string TransactionId { get; set; }
        public string Status { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string ErrorMessage { get; set; }
    }
}
