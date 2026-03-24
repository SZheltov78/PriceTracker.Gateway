using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using PriceTracker.Shared.Contracts;
using PriceTracker.Shared.DTO;
using PriceTracker.Shared.Infrastructure.MessageBus;

namespace PriceTracker.Gateway.Services.HistoryService
{
    public class HistoryService : IHistoryService
    {
        private readonly IMessageBus _messageBus;
        public HistoryService(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        public async Task<HistoryResponse> GetHistoryAsync(int take, CancellationToken cancellationToken = default)
        {
            var request = new HistoryRequest
            {                
                Take = take
            };
            
            var response = await _messageBus.CallAsync<HistoryRequest, HistoryResponse>(
                request,
                requestQueue: QueueNames.OzonHistoryRequest,
                responseQueue: QueueNames.OzonHistoryResponse,
                ct: cancellationToken);

            return response;
        }

        
    }
}
