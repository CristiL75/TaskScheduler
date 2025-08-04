using TaskManager.Scheduler.Services;
using TaskManager.Shared.Interfaces;
using TaskManager.Shared.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurăm serviciile
builder.Services.AddSingleton<ITaskRepository, InMemoryTaskRepository>();
builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();

// Adăugăm serviciile background
builder.Services.AddHostedService<RabbitMQListenerService>();

// Configurăm gRPC
builder.Services.AddGrpc();

// Configurăm Kestrel pentru gRPC HTTP/2 
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5001, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
});

var app = builder.Build();

// Configurăm gRPC endpoint
app.MapGrpcService<TaskGrpcService>();
app.MapGet("/", () => "TaskManager Scheduler Service - Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();
