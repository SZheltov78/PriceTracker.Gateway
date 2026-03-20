using PriceTracker.Ozon.Worker.Data;
using PriceTracker.Ozon.Worker.Entities;
using PriceTracker.Shared.Constants;
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
    public class TaskCreationWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public TaskCreationWorker(IServiceScopeFactory scopeFactory)
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

                        var request = await messageBus.ConsumeAsync<CreateTask>(
                            QueueNames.CreateTasks,
                            stoppingToken);

                        if (request != null)
                        {
                            var task = new OzonTask
                            {
                                Id = Guid.NewGuid(),
                                Name = request.ProductName,
                                Url = request.Url,
                                ThresholdPrice = request.ThresholdPrice,
                                LastParseDate = DateTime.MinValue,
                                ParseToDate = DateTime.UtcNow.AddDays(10),
                                Email = request.Email,
                                CreatedAt = DateTime.UtcNow,
                                Status = TStatus.Active
                            };

                            await dbContext.OzonTasks.AddAsync(task);
                            await dbContext.SaveChangesAsync();
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
    }
}
