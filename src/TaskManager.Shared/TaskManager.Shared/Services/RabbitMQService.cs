using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace TaskManager.Shared.Services;

public interface IRabbitMQService
{
    Task PublishScheduleTaskAsync(Guid taskId, DateTime? scheduleTime = null);
    Task PublishUnscheduleTaskAsync(Guid taskId);
    Task PublishUpdateTaskAsync(Guid taskId, string taskName, string description);
    Task PublishTaskNotificationAsync(Guid taskId, string taskName, string action);
    Task<IConnection> GetConnectionAsync();
}

public class RabbitMQService : IRabbitMQService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly string _scheduleExchange = "task.schedule";
    private readonly string _notificationExchange = "task.notifications";

    public RabbitMQService(string connectionString = "amqp://localhost")
    {
        var factory = new ConnectionFactory() { Uri = new Uri(connectionString) };
        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

        
        _channel.ExchangeDeclareAsync(_scheduleExchange, ExchangeType.Direct).GetAwaiter().GetResult();
        _channel.ExchangeDeclareAsync(_notificationExchange, ExchangeType.Fanout).GetAwaiter().GetResult();
    }

    public async Task PublishScheduleTaskAsync(Guid taskId, DateTime? scheduleTime = null)
    {
        var message = new
        {
            TaskId = taskId,
            Action = "schedule",
            ScheduleTime = scheduleTime,
            Timestamp = DateTime.UtcNow
        };

        await PublishMessageAsync(_scheduleExchange, "schedule", message);
    }

    public async Task PublishUnscheduleTaskAsync(Guid taskId)
    {
        var message = new
        {
            TaskId = taskId,
            Action = "unschedule",
            Timestamp = DateTime.UtcNow
        };

        await PublishMessageAsync(_scheduleExchange, "unschedule", message);
    }

    public async Task PublishUpdateTaskAsync(Guid taskId, string taskName, string description)
    {
        var message = new
        {
            TaskId = taskId,
            TaskName = taskName,
            Description = description,
            Action = "update",
            Timestamp = DateTime.UtcNow
        };

    
        await PublishMessageAsync(_notificationExchange, "", message);
    }

    public async Task PublishTaskNotificationAsync(Guid taskId, string taskName, string action)
    {
        var message = new
        {
            TaskId = taskId,
            TaskName = taskName,
            Action = action,
            Timestamp = DateTime.UtcNow
        };

        await PublishMessageAsync(_notificationExchange, "", message); 
    }

    private async Task PublishMessageAsync(string exchange, string routingKey, object message)
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        await _channel.BasicPublishAsync(
            exchange: exchange,
            routingKey: routingKey,
            body: body
        );
    }

    public async Task<IConnection> GetConnectionAsync()
    {
        return await Task.FromResult(_connection);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
