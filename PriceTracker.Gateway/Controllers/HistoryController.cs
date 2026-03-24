using Microsoft.AspNetCore.Mvc;
using PriceTracker.Gateway.Services.HistoryService;

namespace PriceTracker.Gateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HistoryController : ControllerBase
    {
        IHistoryService _historyService;

        public HistoryController(IHistoryService historyService)
        {
            _historyService = historyService;
        }

        [HttpGet("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var result = await _historyService.GetHistoryAsync(10, HttpContext.RequestAborted);
            return Ok(result);
        }
    }
}
