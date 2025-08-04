namespace TaskManager.Shared.Models;

public class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ScheduledAt { get; set; }
    public bool IsRunning { get; set; } = false;
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Created;
}

public enum TaskItemStatus
{
    Created,
    Scheduled,
    Running,
    Completed,
    Failed,
    Cancelled
}
