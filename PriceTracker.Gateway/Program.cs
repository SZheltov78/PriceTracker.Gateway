using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using PriceTracker.Gateway.Data;
using PriceTracker.Gateway.Services.HistoryService;
using PriceTracker.Shared.Infrastructure.MessageBus;
using PriceTracker.Shared.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<GatewayDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<IMessageBus, RabbitMqBus>();
builder.Services.AddScoped<IHistoryService, HistoryService>();

var app = builder.Build();

app.MapControllers();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapFallbackToFile("index.html");

app.Run();

