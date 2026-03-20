using Microsoft.AspNetCore.Mvc;
using PriceTracker.Shared.Contracts;
using PriceTracker.Shared.DTO;
using PriceTracker.Shared.Infrastructure.MessageBus;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{

    private readonly IMessageBus _messageBus;
    public TestController(IMessageBus messageBus)
    {
        _messageBus = messageBus;            
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { message = "Gateway is alive", timestamp = DateTime.UtcNow });
    }
    

    [HttpGet("ozon-history-request-call")]
    public async Task<IActionResult> TestOzonHistoryCall(
    [FromQuery] string? email = null,
    [FromQuery] int take = 100)
    {
        try
        {
            var request = new HistoryRequest
            {
                Email = email,
                Take = take
            };

            //с ожиданием ответа
            var response = await _messageBus.CallAsync<HistoryRequest, HistoryResponse>(
                request,
                requestQueue: QueueNames.OzonHistoryRequest,
                responseQueue: QueueNames.OzonHistoryResponse,
                ct: HttpContext.RequestAborted);

            return Ok(response);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(504, new { error = "Gateway timeout - worker did not respond within 30 seconds" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

}