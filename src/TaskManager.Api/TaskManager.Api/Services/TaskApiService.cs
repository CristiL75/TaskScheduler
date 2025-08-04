using Grpc.Net.Client;
using TaskManager.Shared.DTOs;
using TaskManager.Shared.Grpc;
using TaskManager.Shared.Services;
using System.Net.Http;

namespace TaskManager.Api.Services;

/// <summary>
/// Implementarea concretă a ITaskApiService
/// Orchestrează comunicarea între API și Scheduler prin:
/// - gRPC pentru operațiuni CRUD (sincrone)
/// - RabbitMQ pentru operațiuni de scheduling (asincrone)
/// </summary>
public class TaskApiService : ITaskApiService
{
    private readonly GrpcChannel _grpcChannel;           // Canal pentru comunicarea gRPC
    private readonly TaskService.TaskServiceClient _grpcClient;  // Client gRPC generat din .proto
    private readonly IRabbitMQService _rabbitMQService;  // Service pentru mesageria RabbitMQ
    private readonly ILogger<TaskApiService> _logger;

    /// <summary>
    /// Constructor - configurează conexiunile gRPC și RabbitMQ
    /// </summary>
    public TaskApiService(IConfiguration configuration, IRabbitMQService rabbitMQService, ILogger<TaskApiService> logger)
    {
        // Obține URL-ul Scheduler-ului din configurație (default: localhost:5001)
        var schedulerUrl = configuration.GetConnectionString("SchedulerGrpc") ?? "http://localhost:5001";
        
        // FIX pentru gRPC pe localhost - activează HTTP/2 unencrypted
        // Necesar pentru comunicarea gRPC între microservicii locale
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        
        // Creează canalul gRPC către Scheduler
        _grpcChannel = GrpcChannel.ForAddress(schedulerUrl);
        
        // Inițializează clientul gRPC generat din task_service.proto
        _grpcClient = new TaskService.TaskServiceClient(_grpcChannel);
        _rabbitMQService = rabbitMQService;
        _logger = logger;
    }

    /// <summary>
    /// CREATE OPERATION - Creează task nou prin gRPC
    /// Flow: API Request → gRPC Call → Scheduler → Repository → Response
    /// </summary>
    public async Task<object> CreateTaskAsync(CreateTaskRequest request)
    {
        try
        {
            _logger.LogInformation("Creating task via gRPC: {TaskName}", request.Name);

            // Convertește DTO-ul API în Request gRPC (mapare de modele)
            var grpcRequest = new CreateTaskGrpcRequest
            {
                Name = request.Name,
                Description = request.Description
            };

            // Apel gRPC sincron către Scheduler
            var response = await _grpcClient.CreateTaskAsync(grpcRequest);

            // Convertește Response gRPC înapoi în DTO pentru API
            return new
            {
                success = true,
                task = new TaskResponse
                {
                    Id = Guid.Parse(response.Id),
                    Name = response.Name,
                    Description = response.Description,
                    CreatedAt = DateTime.Parse(response.CreatedAt),
                    ScheduledAt = !string.IsNullOrEmpty(response.ScheduledAt) ? DateTime.Parse(response.ScheduledAt) : null,
                    IsRunning = response.IsRunning,      // Default: false
                    Status = response.Status             // Default: "Created"
                }
            };
        }
        catch (Exception ex)
        {
            // Error handling - log și response standardizat
            _logger.LogError(ex, "Error creating task");
            return new { success = false, message = "Failed to create task" };
        }
    }

    /// <summary>
    /// DELETE OPERATION - Șterge task prin gRPC
    /// </summary>
    public async Task<object> DeleteTaskAsync(Guid taskId)
    {
        try
        {
            _logger.LogInformation("Deleting task via gRPC: {TaskId}", taskId);

            // Prepare gRPC request cu ID-ul task-ului
            var grpcRequest = new DeleteTaskGrpcRequest
            {
                Id = taskId.ToString()
            };

            // Apel gRPC către Scheduler pentru ștergere
            var response = await _grpcClient.DeleteTaskAsync(grpcRequest);

            return new
            {
                success = response.Success,
                message = response.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task");
            return new { success = false, message = "Failed to delete task" };
        }
    }

   
    public async Task<object> UpdateTaskAsync(Guid taskId, CreateTaskRequest request)
    {
        try
        {
            _logger.LogInformation("Updating task via gRPC: {TaskId}", taskId);

            // Prepare gRPC request pentru update
            var grpcRequest = new UpdateTaskGrpcRequest
            {
                Id = taskId.ToString(),
                Name = request.Name,
                Description = request.Description
            };

            // Apel gRPC către Scheduler pentru actualizare
            var response = await _grpcClient.UpdateTaskAsync(grpcRequest);

            if (response.Success)
            {
                // Trimite notificare prin RabbitMQ (fanout pentru toate sistemele)
                await _rabbitMQService.PublishUpdateTaskAsync(taskId, request.Name, request.Description);
                
                _logger.LogInformation("Task updated and notification sent: {TaskId}", taskId);
            }

            return new
            {
                success = response.Success,
                message = response.Message,
                taskId = taskId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task");
            return new { success = false, message = "Failed to update task" };
        }
    }


    public async Task<object> GetAllTasksAsync()
    {
        try
        {
            _logger.LogInformation("Getting all tasks via gRPC");

            var grpcRequest = new GetAllTasksGrpcRequest();
            var response = await _grpcClient.GetAllTasksAsync(grpcRequest);

            // Mapează fiecare task din gRPC în TaskResponse pentru API
            var tasks = response.Tasks.Select(t => new TaskResponse
            {
                Id = Guid.Parse(t.Id),
                Name = t.Name,
                Description = t.Description,
                CreatedAt = DateTime.Parse(t.CreatedAt),
                ScheduledAt = !string.IsNullOrEmpty(t.ScheduledAt) ? DateTime.Parse(t.ScheduledAt) : null,
                IsRunning = t.IsRunning,
                Status = t.Status
            }).ToList();

            return new
            {
                success = true,
                tasks = tasks
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all tasks");
            return new { success = false, message = "Failed to get tasks" };
        }
    }

     public async Task<object> GetRunningTasksAsync()
    {
        try
        {
            _logger.LogInformation("Getting running tasks via gRPC");

            var grpcRequest = new GetRunningTasksGrpcRequest();
            var response = await _grpcClient.GetRunningTasksAsync(grpcRequest);

            // Scheduler-ul returnează doar task-urile cu IsRunning = true
            var tasks = response.Tasks.Select(t => new TaskResponse
            {
                Id = Guid.Parse(t.Id),
                Name = t.Name,
                Description = t.Description,
                CreatedAt = DateTime.Parse(t.CreatedAt),
                ScheduledAt = !string.IsNullOrEmpty(t.ScheduledAt) ? DateTime.Parse(t.ScheduledAt) : null,
                IsRunning = t.IsRunning,    // Toate vor fi true
                Status = t.Status
            }).ToList();

            return new
            {
                success = true,
                tasks = tasks
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting running tasks");
            return new { success = false, message = "Failed to get running tasks" };
        }
    }


    public async Task<object> ScheduleTaskAsync(ScheduleTaskRequest request)
    {
        try
        {
            _logger.LogInformation("Scheduling task via RabbitMQ: {TaskId}", request.TaskId);

            await _rabbitMQService.PublishScheduleTaskAsync(request.TaskId, request.ScheduleTime);

            // Returnează imediat confirmarea (mesajul e în queue, nu e procesat încă)
            return new
            {
                success = true,
                message = "Task scheduled successfully",
                taskId = request.TaskId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling task");
            return new { success = false, message = "Failed to schedule task" };
        }
    }

  
    public async Task<object> UnscheduleTaskAsync(Guid taskId)
    {
        try
        {
            _logger.LogInformation("Unscheduling task via RabbitMQ: {TaskId}", taskId);

         
            await _rabbitMQService.PublishUnscheduleTaskAsync(taskId);

            // Returnează imediat confirmarea (procesarea e asincronă)
            return new
            {
                success = true,
                message = "Task unscheduled successfully",
                taskId = taskId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unscheduling task");
            return new { success = false, message = "Failed to unschedule task" };
        }
    }

     public void Dispose()
    {
        _grpcChannel?.Dispose();
    }
}
