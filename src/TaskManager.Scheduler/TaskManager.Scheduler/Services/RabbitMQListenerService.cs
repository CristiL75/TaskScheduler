using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using TaskManager.Shared.Interfaces;
using TaskManager.Shared.Services;

namespace TaskManager.Scheduler.Services;

public class RabbitMQListenerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMQListenerService> _logger;
    private readonly IRabbitMQService _rabbitMQService;
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMQListenerService(
        IServiceProvider serviceProvider,
        ILogger<RabbitMQListenerService> logger,
        IRabbitMQService rabbitMQService)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _rabbitMQService = rabbitMQService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await InitializeRabbitMQAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Păstrăm serviciul alive
                await Task.Delay(1000, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task InitializeRabbitMQAsync()
    {
        try
        {
            _connection = await _rabbitMQService.GetConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            // Declarăm queue-ul pentru mesajele de schedule
            var scheduleQueue = await _channel.QueueDeclareAsync("task.schedule.queue", true, false, false);
            
            // Bind la exchange-ul de schedule
            await _channel.QueueBindAsync(scheduleQueue.QueueName, "task.schedule", "schedule");
            await _channel.QueueBindAsync(scheduleQueue.QueueName, "task.schedule", "unschedule");

            // Configurăm consumer-ul
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += OnMessageReceivedAsync;

            await _channel.BasicConsumeAsync(scheduleQueue.QueueName, true, consumer);

            _logger.LogInformation("RabbitMQ listener started. Listening for schedule/unschedule messages.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ listener");
        }
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs e)
    {
        try
        {
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var messageData = JsonSerializer.Deserialize<JsonElement>(message);

            var taskIdStr = messageData.GetProperty("TaskId").GetString();
            var action = messageData.GetProperty("Action").GetString();

            if (!Guid.TryParse(taskIdStr, out var taskId))
            {
                _logger.LogWarning("Invalid TaskId format: {TaskId}", taskIdStr);
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var taskRepository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();

            switch (action?.ToLower())
            {
                case "schedule":
                    await HandleScheduleTaskAsync(taskRepository, taskId);
                    break;
                case "unschedule":
                    await HandleUnscheduleTaskAsync(taskRepository, taskId);
                    break;
                default:
                    _logger.LogWarning("Unknown action: {Action}", action);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing RabbitMQ message");
        }
    }

    private async Task HandleScheduleTaskAsync(ITaskRepository taskRepository, Guid taskId)
    {
        _logger.LogInformation("Scheduling task: {TaskId}", taskId);
        
        var task = await taskRepository.GetTaskByIdAsync(taskId);
        if (task != null)
        {
            await taskRepository.SetTaskRunningAsync(taskId, true);
            
            
            await _rabbitMQService.PublishTaskNotificationAsync(taskId, task.Name, "scheduled");
            
            _logger.LogInformation("Task {TaskId} scheduled successfully", taskId);
        }
        else
        {
            _logger.LogWarning("Task {TaskId} not found for scheduling", taskId);
        }
    }

    private async Task HandleUnscheduleTaskAsync(ITaskRepository taskRepository, Guid taskId)
    {
        _logger.LogInformation("Unscheduling task: {TaskId}", taskId);
        
        var task = await taskRepository.GetTaskByIdAsync(taskId);
        if (task != null)
        {
            await taskRepository.SetTaskRunningAsync(taskId, false);
            
            // Trimitem notificare prin fanout
            await _rabbitMQService.PublishTaskNotificationAsync(taskId, task.Name, "unscheduled");
            
            _logger.LogInformation("Task {TaskId} unscheduled successfully", taskId);
        }
        else
        {
            _logger.LogWarning("Task {TaskId} not found for unscheduling", taskId);
        }
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
