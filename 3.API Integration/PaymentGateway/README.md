# Payment Gateway Integration Service

This project implements a .NET 7 payment gateway integration service that handles payment processing, refunds, and status checks through a third-party payment API.

## Features

- Async payment processing with retry logic
- Secure refund processing
- Payment status checking
- Robust error handling
- Comprehensive logging
- Unit test coverage

## Prerequisites

- .NET 7 SDK
- Visual Studio 2022 or VS Code
- NuGet package manager

## Required NuGet Packages

```xml
<PackageReference Include="Microsoft.Extensions.Http" Version="7.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration" Version="7.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="7.0.0" />
<PackageReference Include="Polly" Version="7.2.4" />
```

For testing:
```xml
<PackageReference Include="xunit" Version="2.4.2" />
<PackageReference Include="Moq" Version="4.18.4" />
```

## Configuration

Configure the payment gateway settings in `appsettings.json`:

```json
{
  "PaymentGateway": {
    "BaseUrl": "https://api.payment-gateway.com/",
    "ApiKey": "your-api-key-here"
  }
}
```

## Usage

1. Register the service in your DI container:

```csharp
services.AddHttpClient();
services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();
```

2. Inject and use the service:

```csharp
public class PaymentController
{
    private readonly IPaymentGatewayService _paymentService;

    public PaymentController(IPaymentGatewayService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<IActionResult> ProcessPayment(PaymentRequest request)
    {
        try
        {
            var result = await _paymentService.ProcessPaymentAsync(request);
            return Ok(result);
        }
        catch (PaymentProcessingException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
```

## Error Handling

The service throws specific exceptions:

- `PaymentProcessingException`: For payment processing failures
- `RefundProcessingException`: For refund processing failures
- `ApiCommunicationException`: For network or API communication issues

## Testing

Run the unit tests using:

```bash
dotnet test
```

## Logging

The service uses structured logging via ILogger. Configure logging as needed in your application's logging configuration.

## Security

- Sensitive data is masked in logs
- HTTPS is enforced for API communication
- API keys are stored in configuration
