using PriceTracker.Ozon.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<HistoryRequestWorker>();

var host = builder.Build();
host.Run();
