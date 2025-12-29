using Microsoft.AspNetCore.Mvc;
using Net.payOS;
using Net.payOS.Types;

[Route("api/[controller]")]
[ApiController]
public class PayOSController : ControllerBase
{
    [HttpPost("create-payment-link")]
    public async Task<IActionResult> CreatePaymentLink()
    {
        try
        {
            var clientId = "3ff229b0-8608-4e88-a265-0d944313328a";
            var apiKey = "6903b61d-324a-415d-b214-ad30698d0258";
            var checksumKey = "566297ee4ca09f0e79e04723667277fa25ad455a7b86bb58792a792a0077095b";
            var payOS = new PayOS(clientId, apiKey, checksumKey);
            var request = HttpContext.Request;
            var domain = $"{request.Scheme}://{request.Host}/";


            var paymentLinkRequest = new PaymentData(
                orderCode: (int)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % int.MaxValue) + new Random().Next(100, 999),
                amount: 39000,
                description: "Thanh toán VIP",
                items: [new("VIP Subscription", 1, 39000)],
                returnUrl: domain + "VietQR?success=true",
                cancelUrl: domain + "VietQR?cancel=true"
            );
            var response = await payOS.createPaymentLink(paymentLinkRequest);

            return Ok(response);
        }
        catch (Exception ex)
        {
            // Return the error message to the client
            return StatusCode(500, ex.Message + (ex.InnerException != null ? " | " + ex.InnerException.Message : ""));
        }
    }
}