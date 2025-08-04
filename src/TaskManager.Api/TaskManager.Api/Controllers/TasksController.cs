using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Services;
using TaskManager.Shared.DTOs;

namespace TaskManager.Api.Controllers;


[ApiController]
[Route("api/[controller]")] 
public class TasksController : ControllerBase
{
    // Service care comunică cu Scheduler-ul prin gRPC
    private readonly ITaskApiService _taskService;
    private readonly ILogger<TasksController> _logger;


    public TasksController(ITaskApiService taskService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

  
    [HttpPost("process")]
    public async Task<IActionResult> ProcessTaskRequest([FromBody] TaskRequest request)
    {
        try
        {
            // Logare pentru debugging și monitoring
            _logger.LogInformation("Processing task request: {Action}", request.Action);

       
            var result = request.Action?.ToLower() switch
            {
                // CRUD OPERATIONS (Sincrone prin gRPC)
                "create" => await _taskService.CreateTaskAsync(request.CreateRequest!),      // gRPC: CreateTask
                "delete" => await _taskService.DeleteTaskAsync(request.TaskId!.Value),      // gRPC: DeleteTask
                "update" => await _taskService.UpdateTaskAsync(request.TaskId!.Value, request.UpdateRequest!),  
                "getall" => await _taskService.GetAllTasksAsync(),                          // gRPC: GetAllTasks
                "getrunning" => await _taskService.GetRunningTasksAsync(),                  // gRPC: GetRunningTasks
                
               
                "schedule" => await _taskService.ScheduleTaskAsync(request.ScheduleRequest!),   // RabbitMQ: task.schedule
                "unschedule" => await _taskService.UnscheduleTaskAsync(request.TaskId!.Value),  // RabbitMQ: task.unschedule
                
               
                _ => (object)new { success = false, message = "Unknown action" }
            };

           
            return Ok(result);
        }
        catch (Exception ex)
        {
      
            _logger.LogError(ex, "Error processing task request");
            

            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }
}


public class TaskRequest
{
  
   
    public string? Action { get; set; }
    
 
    /// ID-ul task-ului (obligatoriu pentru: delete, unschedule)
 
    public Guid? TaskId { get; set; }
    

  
    public CreateTaskRequest? CreateRequest { get; set; }
    

    public ScheduleTaskRequest? ScheduleRequest { get; set; }
    

    public CreateTaskRequest? UpdateRequest { get; set; }
}
