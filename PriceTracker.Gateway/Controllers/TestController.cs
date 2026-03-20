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


    [HttpGet("ozon-add-mock-task")]
    public async Task<IActionResult> TestOzonAddTask(
    [FromQuery] string? url = null,
    [FromQuery] string? name = null,
    [FromQuery] string? email = null,
    [FromQuery] decimal? thresholdPrice = null,
    [FromQuery] int daysToParse = 7)
    {
        try
        {
            // Значения по умолчанию, если параметры не переданы
            var task = new Ozon
            {
                Url = url ?? "https://www.ozon.ru/product/example-smartphone-123456789",
                Name = name ?? "Тестовый смартфон",
                Email = email ?? "test@example.com",
                ThresholdPrice = thresholdPrice ?? 10000m,
                ParseToDate = DateTime.UtcNow.AddDays(daysToParse)
            };

            // Публикуем задачу в очередь для воркера
            await _messageBus.PublishAsync(
                task,
                QueueNames.OzonCteateTasks, // Предполагаю, что такая очередь есть
                HttpContext.RequestAborted);

            return Ok(new
            {
                success = true,
                message = "Mock task added to queue",
                task = new
                {
                    task.Url,
                    task.Name,
                    task.Email,
                    task.ThresholdPrice,
                    ParseToDate = task.ParseToDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    Queue = QueueNames.OzonCteateTasks
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                error = ex.Message
            });
        }
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