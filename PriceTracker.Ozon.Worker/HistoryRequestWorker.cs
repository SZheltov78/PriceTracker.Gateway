using Microsoft.EntityFrameworkCore;
using PriceTracker.Ozon.Worker.Data;
using PriceTracker.Shared.Contracts;
using PriceTracker.Shared.DTO;
using PriceTracker.Shared.Infrastructure.MessageBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceTracker.Ozon.Worker
{
    public class HistoryRequestWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;        

        public HistoryRequestWorker(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;            
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {           

            while (!stoppingToken.IsCancellationRequested)
            {                
                try
                {                    
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
                        var dbContext = scope.ServiceProvider.GetRequiredService<OzonDbContext>();
                      
                        var request = await messageBus.ConsumeAsync<HistoryRequest>(
                            QueueNames.OzonHistoryRequest,
                            stoppingToken);

                        if (request != null)
                        {
                            await ProcessRequestAsync(request, dbContext, messageBus, stoppingToken);
                            continue;
                        }
                    }
                    
                    await Task.Delay(1000, stoppingToken);
                }
                catch (OperationCanceledException oe)
                {
                    //logger
                    break;
                }
                catch (Exception ex)
                {
                    //logger
                    await Task.Delay(1000, stoppingToken);
                }
            }
            
        }

        private async Task ProcessRequestAsync(
            HistoryRequest request,
            OzonDbContext dbContext,
            IMessageBus messageBus,
            CancellationToken stoppingToken)
        {
            try
            {                
                var query = dbContext.PriceHistories
                    .Include(x => x.OzonTask)
                    .AsQueryable();

                if (request.Email != null)
                {
                    query = query.Where(x => x.OzonTask.Email == request.Email);
                }

                var history = await query
                    .OrderByDescending(x => x.CheckedAt)
                    .Take(request.Take)
                    .ToListAsync(stoppingToken);
                                
                var groupedHistory = history.GroupBy(x => x.OzonTask.Url);

                var responseVM = new HistoryResponse();
                responseVM.CorrelationId = request.CorrelationId;

                foreach (var priceItems in groupedHistory)
                {
                    var task = priceItems.First().OzonTask;

                    var productItem = new ProductHistoryDto
                    {
                        Status = task.Status,
                        ProductName = task.Name,
                        PriceHistory = priceItems
                            .Select(x => new PricePointDto
                            {
                                Price = x.Price,
                                Date = x.CheckedAt
                            })
                            .OrderBy(x => x.Date)
                            .ToList()
                    };

                    responseVM.Products.Add(productItem);
                }
                
                await messageBus.PublishAsync(
                    responseVM,
                    QueueNames.OzonHistoryResponse,
                    stoppingToken);
                
            }
            catch (Exception ex)
            {
                //logger
                throw; 
            }
        }
    }
}