using System.Collections.Concurrent;
using TaskManager.Shared.Interfaces;
using TaskManager.Shared.Models;

namespace TaskManager.Scheduler.Services;



public class InMemoryTaskRepository : ITaskRepository
{

    private readonly ConcurrentDictionary<Guid, TaskItem> _tasks = new();

    public async Task<TaskItem> CreateTaskAsync(string name, string description)
    {
 
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),                    
            Name = name,                            
            Description = description,             
            CreatedAt = DateTime.UtcNow,           
            Status = TaskItemStatus.Created       
  
        };

        // Adaugă task-ul în dictionary (thread-safe)
        _tasks.TryAdd(task.Id, task);
        return await Task.FromResult(task);
    }


    public async Task<bool> DeleteTaskAsync(Guid id)
    {
        // TryRemove este thread-safe și returnează true/false
        var result = _tasks.TryRemove(id, out _);
        return await Task.FromResult(result);
    }


    public async Task<IEnumerable<TaskItem>> GetAllTasksAsync()
    {

        return await Task.FromResult(_tasks.Values.ToList());
    }


    public async Task<IEnumerable<TaskItem>> GetRunningTasksAsync()
    {

        var runningTasks = _tasks.Values.Where(t => t.IsRunning).ToList();
        return await Task.FromResult(runningTasks);
    }


    public async Task<TaskItem?> GetTaskByIdAsync(Guid id)
    {
        // TryGetValue returnează task-ul sau null dacă nu există
        _tasks.TryGetValue(id, out var task);
        return await Task.FromResult(task);
    }


    public async Task<bool> UpdateTaskAsync(TaskItem task)
    {
        // Verifică dacă task-ul există și îl înlocuiește
        if (_tasks.ContainsKey(task.Id))
        {
            _tasks[task.Id] = task;  // Overwrite complet
            return await Task.FromResult(true);
        }
        return await Task.FromResult(false);
    }


    public async Task<bool> SetTaskRunningAsync(Guid id, bool isRunning)
    {
        // Găsește task-ul în dictionary
        if (_tasks.TryGetValue(id, out var task))
        {
            // SCHIMBĂ FLAG-UL IsRunning (false → true sau true → false)
            task.IsRunning = isRunning;
            
            // ACTUALIZEAZĂ STATUS-UL în funcție de flag
            task.Status = isRunning ? TaskItemStatus.Running : TaskItemStatus.Scheduled;
            
            // SETEAZĂ TIMESTAMP-UL când task-ul începe să ruleze
            if (isRunning && task.ScheduledAt == null)
            {
                task.ScheduledAt = DateTime.UtcNow;
            }
            
            return await Task.FromResult(true);
        }
        return await Task.FromResult(false);  // Task nu există
    }
}
