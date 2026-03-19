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
        private readonly IMessageBus _messageBus;
        private readonly OzonDbContext _dbContext;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var request = await _messageBus.ConsumeAsync<HistoryRequest>(
                        QueueNames.OzonHistoryRequest, stoppingToken);

                    if (request != null)
                    {
                        #region get history
                        var query = _dbContext.PriceHistories
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
                        #endregion

                        #region grid VM
                        var groupedHistory = history.GroupBy(x => x.OzonTask.Url);

                        var responseVM = new HistoryResponse();
                        foreach (var priceItems in groupedHistory)
                        {
                            var task = priceItems.First().OzonTask;

                            var productItem = new ProductHistoryDto();
                            productItem.Status = task.Status;
                            productItem.ProductName = task.Name;
                            productItem.PriceHistory = priceItems
                                .Select(x => new PricePointDto { Price = x.Price, Date = x.CheckedAt })
                                .OrderBy(x => x.Date)
                                .ToList();

                            responseVM.Products.Add(productItem);
                        }
                        #endregion

                        await _messageBus.PublishAsync(
                            responseVM,
                            QueueNames.OzonHistoryResponse,
                            stoppingToken);
                    }

                    await Task.Delay(100, stoppingToken);
                }
                catch (Exception ex)
                {
                    //logger
                    await Task.Delay(1000, stoppingToken);                    
                }
            }
        }
    }
}
