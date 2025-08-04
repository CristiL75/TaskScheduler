using TaskManager.Shared.DTOs;

namespace TaskManager.Api.Services;



public interface ITaskApiService
{

    /// Creează task nou (gRPC sync)

    Task<object> CreateTaskAsync(CreateTaskRequest request);
    
  
    
    /// Șterge task (gRPC sync)
 
    Task<object> DeleteTaskAsync(Guid taskId);

    Task<object> UpdateTaskAsync(Guid taskId, CreateTaskRequest request);
    

    Task<object> GetAllTasksAsync();
    

    Task<object> GetRunningTasksAsync();
    
    
    /// Programează task (RabbitMQ async) → IsRunning = true

    Task<object> ScheduleTaskAsync(ScheduleTaskRequest request);
    

    /// Oprește task (RabbitMQ async) → IsRunning = false

    Task<object> UnscheduleTaskAsync(Guid taskId);
}
