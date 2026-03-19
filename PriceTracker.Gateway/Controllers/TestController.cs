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


    [HttpGet("ozon-history-request-publish")]
    public async Task<IActionResult> TestOzonHistoryRequest()
    {
        try
        {
            var task = new HistoryRequest
            {
                Email = "test6@test.ru",
                Take = 10
            };

            await _messageBus.PublishAsync(
                task,
                QueueNames.OzonHistoryRequest,
                HttpContext.RequestAborted
                );

            return Ok(new
            {
                success = true                
            });
        }
        catch (Exception ex)
        {            
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    [HttpGet("ozon-history-request-consume")]
    public async Task<IActionResult> TestOzonHistoryResponse()
    {
        try
        {
            var result = await _messageBus.ConsumeAsync<HistoryRequest>(QueueNames.OzonHistoryRequest, HttpContext.RequestAborted);
            
            if (result == null) return Content("null");
            
            return Ok(new
            {
                result.Email,
                result.Take
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

}