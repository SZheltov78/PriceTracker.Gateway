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
        private readonly IOzonParserApi _ozonParserApi;
        private readonly OzonDbContext _dbContext;
        private readonly IMessageBus _messageBus;

        public Worker(
            IOzonParserApi ozonParserApi, 
            OzonDbContext ozonDbContext,
            IMessageBus messageBus
            )
        {           
            _ozonParserApi = ozonParserApi;
            _dbContext = ozonDbContext;
            _messageBus = messageBus;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var tasks = await _dbContext.OzonTasks
                    .Where(x => x.LastParseDate > DateTime.Now.AddHours(-1))
                    .Where(x => x.ParseToDate <= DateTime.Now)
                    .Where(x => x.Status == TStatus.Active)
                    .Take(5)
                    .ToListAsync();
                
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = 5,
                    CancellationToken = stoppingToken
                };

                await Parallel.ForEachAsync(tasks, parallelOptions, async (task, token) =>
                {
                    try
                    {
                        var responseData = await _ozonParserApi.GetPriceAsync(task.Url, token);

                        if (responseData != null)
                        {
                            task.LastParseDate = DateTime.Now;

                            var history = new PriceHistory
                            {
                                Id = Guid.NewGuid(),
                                OzonTaskId = task.Id,
                                Price = responseData.Price,
                                CheckedAt = DateTime.UtcNow
                            };

                            await _dbContext.PriceHistories.AddAsync(history, token);
                                                       
                            if (responseData.Price <= task.ThresholdPrice)
                            {
                                await _messageBus.PublishAsync(                                        
                                    new NotificationEvent                                        
                                    {       
                                        ProductName = task.Name,                                            
                                        Price = responseData.Price,                                            
                                        Email = task.Email                                        
                                    },
                                    QueueNames.NotificationTasks,
                                    token
                                );
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //logger
                    }
                });

                await _dbContext.SaveChangesAsync(stoppingToken);


                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);                
            }
        }
    }
}
