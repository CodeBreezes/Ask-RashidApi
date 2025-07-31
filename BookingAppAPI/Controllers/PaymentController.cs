using BookingAppAPI.DB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

[Route("api/[controller]")]
[ApiController]
public class PaymentController : ControllerBase
{
    private readonly StripeSettings _stripeSettings;

    public PaymentController(IOptions<StripeSettings> stripeOptions)
    {
        _stripeSettings = stripeOptions.Value;
        StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
    }
    [HttpPost("create-payment-intent")]
    public IActionResult CreatePaymentIntent([FromBody] PaymentRequest request)
    {
        if (request == null || request.Amount <= 0)
        {
            return BadRequest("Invalid payment request.");
        }

        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(request.Amount * 100), // Stripe uses smallest currency unit
            Currency = "aed",
            PaymentMethodTypes = new List<string> { "card" },
            Description = $"Booking payment for {request.PhoneNumber}",
            Metadata = new Dictionary<string, string>
        {
            { "customerName", request.CustomerName ?? "Guest" },
           { "bookingId", request.BookingId.ToString() }

        }
        };

        var service = new PaymentIntentService();
        var intent = service.Create(options);

        return Ok(new
        {
            clientSecret = intent.ClientSecret
        });
    }

}
