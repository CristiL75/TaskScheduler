using TaskManager.Shared.Models;

namespace TaskManager.Shared.Interfaces;

public interface ITaskRepository
{
    Task<TaskItem> CreateTaskAsync(string name, string description);
    Task<bool> DeleteTaskAsync(Guid id);
    Task<IEnumerable<TaskItem>> GetAllTasksAsync();
    Task<IEnumerable<TaskItem>> GetRunningTasksAsync();
    Task<TaskItem?> GetTaskByIdAsync(Guid id);
    Task<bool> UpdateTaskAsync(TaskItem task);
    Task<bool> SetTaskRunningAsync(Guid id, bool isRunning);
}
