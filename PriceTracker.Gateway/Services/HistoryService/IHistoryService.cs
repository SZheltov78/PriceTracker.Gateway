using PriceTracker.Shared.DTO;

namespace PriceTracker.Gateway.Services.HistoryService
{
    public interface IHistoryService
    {
        Task<HistoryResponse> GetHistoryAsync(int Take, CancellationToken cancellationToken = default);
    }
}
