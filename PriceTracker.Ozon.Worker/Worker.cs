using Microsoft.EntityFrameworkCore;
using PriceTracker.Ozon.Worker.Data;
using PriceTracker.Ozon.Worker.Entities;
using PriceTracker.Ozon.Worker.Services;
using PriceTracker.Shared.Constants;
using PriceTracker.Shared.Contracts;
using PriceTracker.Shared.Infrastructure.Http;
using PriceTracker.Shared.Infrastructure.MessageBus;
using System.Threading.Tasks;

namespace PriceTracker.Ozon.Worker
{
    public class Worker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;        

        public Worker(IServiceScopeFactory scopeFactory )            
        {
            _scopeFactory = scopeFactory;            
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {                    
                    var tasks = await GetTasksAsync(stoppingToken);
                                       
                    if (tasks.Count == 0)
                    {                        
                        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                        continue;
                    }
                    
                    await ProcessTasksAsync(tasks, stoppingToken);
                    
                    await Task.Delay(100, stoppingToken);
                }
                catch (OperationCanceledException oe)
                {
                    //logger
                    break;
                }
                catch (Exception ex)
                {
                    //logger
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }            
        }

        private async Task<List<OzonTask>> GetTasksAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OzonDbContext>();

            return await dbContext.OzonTasks
                .Where(x => x.LastParseDate > DateTime.UtcNow.AddHours(-1))
                .Where(x => x.ParseToDate <= DateTime.UtcNow)
                .Where(x => x.Status == TStatus.Active)
                .OrderBy(x => x.LastParseDate)
                .Take(5)
                .ToListAsync(stoppingToken);
        }

        private async Task ProcessTasksAsync(List<OzonTask> tasks, CancellationToken stoppingToken)
        {
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = 5,
                CancellationToken = stoppingToken
            };

            await Parallel.ForEachAsync(tasks, parallelOptions, async (task, token) =>
            {               
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<OzonDbContext>();
                var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
                var ozonParserApi = scope.ServiceProvider.GetRequiredService<IOzonParserApi>();

                try
                {                    
                    var freshTask = await dbContext.OzonTasks
                        .FirstOrDefaultAsync(t => t.Id == task.Id, token);

                    if (freshTask == null)
                    {
                        //logger
                        return;
                    }

                    var responseData = await ozonParserApi.GetPriceAsync(freshTask.Url, token);

                    if (responseData != null)
                    {
                        freshTask.LastParseDate = DateTime.UtcNow;

                        var history = new PriceHistory
                        {
                            Id = Guid.NewGuid(),
                            OzonTaskId = freshTask.Id,
                            Price = responseData.Price,
                            CheckedAt = DateTime.UtcNow
                        };

                        await dbContext.PriceHistories.AddAsync(history, token);

                        if (responseData.Price <= freshTask.ThresholdPrice)
                        {
                            await messageBus.PublishAsync(
                                new NotificationEvent
                                {
                                    ProductName = freshTask.Name,
                                    Price = responseData.Price,
                                    Email = freshTask.Email
                                },
                                QueueNames.NotificationTasks,
                                token
                            );
                        }

                        await dbContext.SaveChangesAsync(token);
                    }
                }
                catch (Exception ex)
                {
                    //logger
                }
            });
        }
    }
}