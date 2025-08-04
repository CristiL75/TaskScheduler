using Grpc.Core;
using TaskManager.Shared.Grpc;
using TaskManager.Shared.Interfaces;

namespace TaskManager.Scheduler.Services;

public class TaskGrpcService : TaskService.TaskServiceBase
{
    private readonly ITaskRepository _taskRepository;
    private readonly ILogger<TaskGrpcService> _logger;

    public TaskGrpcService(ITaskRepository taskRepository, ILogger<TaskGrpcService> logger)
    {
        _taskRepository = taskRepository;
        _logger = logger;
    }

    public override async Task<TaskGrpcResponse> CreateTask(CreateTaskGrpcRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Creating task: {TaskName}", request.Name);
        
        var task = await _taskRepository.CreateTaskAsync(request.Name, request.Description);
        
        return new TaskGrpcResponse
        {
            Id = task.Id.ToString(),
            Name = task.Name,
            Description = task.Description,
            CreatedAt = task.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
            ScheduledAt = task.ScheduledAt?.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ") ?? "",
            IsRunning = task.IsRunning,
            Status = task.Status.ToString()
        };
    }

    public override async Task<DeleteTaskGrpcResponse> DeleteTask(DeleteTaskGrpcRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Deleting task: {TaskId}", request.Id);
        
        if (!Guid.TryParse(request.Id, out var taskId))
        {
            return new DeleteTaskGrpcResponse
            {
                Success = false,
                Message = "Invalid task ID format"
            };
        }

        var success = await _taskRepository.DeleteTaskAsync(taskId);
        
        return new DeleteTaskGrpcResponse
        {
            Success = success,
            Message = success ? "Task deleted successfully" : "Task not found"
        };
    }

    public override async Task<UpdateTaskGrpcResponse> UpdateTask(UpdateTaskGrpcRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Updating task: {TaskId}", request.Id);
        
        if (!Guid.TryParse(request.Id, out var taskId))
        {
            return new UpdateTaskGrpcResponse
            {
                Success = false,
                Message = "Invalid task ID format"
            };
        }

        // Găsește task-ul existent
        var existingTask = await _taskRepository.GetTaskByIdAsync(taskId);
        if (existingTask == null)
        {
            return new UpdateTaskGrpcResponse
            {
                Success = false,
                Message = "Task not found"
            };
        }

        // Actualizează câmpurile
        existingTask.Name = request.Name;
        existingTask.Description = request.Description;

        // Salvează în repository
        var success = await _taskRepository.UpdateTaskAsync(existingTask);
        
        return new UpdateTaskGrpcResponse
        {
            Success = success,
            Message = success ? "Task updated successfully" : "Failed to update task"
        };
    }

    public override async Task<GetAllTasksGrpcResponse> GetAllTasks(GetAllTasksGrpcRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Getting all tasks");
        
        var tasks = await _taskRepository.GetAllTasksAsync();
        var response = new GetAllTasksGrpcResponse();
        
        foreach (var task in tasks)
        {
            response.Tasks.Add(new TaskGrpcResponse
            {
                Id = task.Id.ToString(),
                Name = task.Name,
                Description = task.Description,
                CreatedAt = task.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                ScheduledAt = task.ScheduledAt?.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ") ?? "",
                IsRunning = task.IsRunning,
                Status = task.Status.ToString()
            });
        }
        
        return response;
    }

    public override async Task<GetRunningTasksGrpcResponse> GetRunningTasks(GetRunningTasksGrpcRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Getting running tasks");
        
        var tasks = await _taskRepository.GetRunningTasksAsync();
        var response = new GetRunningTasksGrpcResponse();
        
        foreach (var task in tasks)
        {
            response.Tasks.Add(new TaskGrpcResponse
            {
                Id = task.Id.ToString(),
                Name = task.Name,
                Description = task.Description,
                CreatedAt = task.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                ScheduledAt = task.ScheduledAt?.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ") ?? "",
                IsRunning = task.IsRunning,
                Status = task.Status.ToString()
            });
        }
        
        return response;
    }
}
