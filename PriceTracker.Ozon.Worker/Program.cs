using Microsoft.EntityFrameworkCore;
using PriceTracker.Ozon.Worker;
using PriceTracker.Ozon.Worker.Data;
using PriceTracker.Ozon.Worker.Services;
using PriceTracker.Shared.Infrastructure.Http;
using PriceTracker.Shared.Infrastructure.MessageBus;
using PriceTracker.Shared.Messaging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<OzonDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<IHttpApiClient, HttpApiClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddTransient<IOzonParserApi, OzonParserApi>();


builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddTransient<IMessageBus, RabbitMqBus>();

// Фоновые службы
builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<HistoryRequestWorker>();

var host = builder.Build();
host.Run();