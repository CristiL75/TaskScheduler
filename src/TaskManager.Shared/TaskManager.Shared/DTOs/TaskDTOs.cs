namespace TaskManager.Shared.DTOs;

public class CreateTaskRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class TaskResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public bool IsRunning { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ScheduleTaskRequest
{
    public Guid TaskId { get; set; }
    public DateTime? ScheduleTime { get; set; }
}

public class TaskNotification
{
    public Guid TaskId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // "scheduled" sau "unscheduled"
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
