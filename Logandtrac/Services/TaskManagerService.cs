// Services/TaskManagerService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using logandtrac.Models;
using logandtrac.Logging;
using Serilog;

namespace logandtrac.Services;

public class TaskManagerService
{
    private List<TaskItem> tasks = new List<TaskItem>();
    private readonly ILogger _logger;

    public TaskManagerService(ILogger logger)
    {
        _logger = logger.ForContext<TaskManagerService>();
    }

    public void AddTask(string title, string priority = "Medium")
    {
        var operationData = new { Title = title, Priority = priority };
        _logger.Debug("▶ Начало операции AddTask {@OperationData}", operationData);

        if (string.IsNullOrWhiteSpace(title))
        {
            _logger.Warning("ADD_TASK_FAILED: Empty title");
            Console.WriteLine("Ошибка: Название задачи не может быть пустым!");
            _logger.Debug("◀ Конец операции AddTask | Результат: Отменено - пустое название");
            
            StructuredLogger.LogWarning("Task creation failed - empty title", new { AttemptedTime = DateTime.Now });
            return;
        }

        if (tasks.Any(t => t.Title.Equals(title, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.Warning("ADD_TASK_FAILED: Duplicate title {Title}", title);
            Console.WriteLine("Ошибка: Задача с таким названием уже существует!");
            _logger.Debug("◀ Конец операции AddTask | Результат: Отменено - дубликат");
            
            StructuredLogger.LogWarning("Task creation failed - duplicate", new { Title = title, Time = DateTime.Now });
            return;
        }

        var task = new TaskItem(title) { Priority = priority };
        tasks.Add(task);
        
        _logger.Information("TASK_CREATED: {TaskTitle} (ID: {TaskId}) | Total: {TotalTasks}", 
            task.Title, task.Id, tasks.Count);
        _logger.Debug("◀ Конец операции AddTask | Результат: Успешно | TaskId: {TaskId}", task.Id);
        
        Console.WriteLine($"✓ Задача \"{title}\" успешно добавлена! (ID: {task.Id}, Приоритет: {priority})");
        
        StructuredLogger.LogTaskOperation("CREATE", task, "success", tasks.Count);
        StructuredLogger.LogMetric("tasks.created", 1, new Dictionary<string, object>
        {
            ["taskId"] = task.Id,
            ["priority"] = task.Priority
        });
    }

    public void RemoveTask(string title)
    {
        var operationData = new { Title = title };
        _logger.Debug("▶ Начало операции RemoveTask {@OperationData}", operationData);

        var task = tasks.FirstOrDefault(t => t.Title.Equals(title, StringComparison.OrdinalIgnoreCase));

        if (task == null)
        {
            _logger.Error("REMOVE_TASK_FAILED: Task not found {Title}", title);
            Console.WriteLine($"Ошибка: Задача \"{title}\" не найдена!");
            _logger.Debug("◀ Конец операции RemoveTask | Результат: Ошибка - задача не найдена");
            
            StructuredLogger.LogError("Task not found for deletion", null, new { Title = title, Time = DateTime.Now });
            return;
        }

        int taskId = task.Id;
        tasks.Remove(task);
        
        _logger.Information("TASK_DELETED: {TaskTitle} (ID: {TaskId}) | Remaining: {TotalTasks}", 
            title, taskId, tasks.Count);
        _logger.Debug("◀ Конец операции RemoveTask | Результат: Успешно");
        
        Console.WriteLine($"✓ Задача \"{title}\" удалена!");
        
        StructuredLogger.LogTaskOperation("DELETE", task, "success", tasks.Count);
        StructuredLogger.LogMetric("tasks.deleted", 1, new Dictionary<string, object>
        {
            ["taskId"] = taskId
        });
    }

    public void ListTasks()
    {
        _logger.Debug("▶ Начало операции ListTasks | CurrentCount: {Count}", tasks.Count);

        Console.WriteLine("\n=== СПИСОК ЗАДАЧ ===");
        Console.WriteLine($"Всего задач: {tasks.Count}");
        Console.WriteLine("─────────────────────");
        
        if (tasks.Count == 0)
        {
            _logger.Information("LIST_TASKS: Empty list");
            Console.WriteLine("Список задач пуст.");
        }
        else
        {
            var groupedTasks = tasks.GroupBy(t => t.Priority);
            
            foreach (var group in groupedTasks)
            {
                Console.WriteLine($"\n[{group.Key} приоритет]:");
                var taskList = group.ToList();
                for (int i = 0; i < taskList.Count; i++)
                {
                    Console.WriteLine($"  {i + 1}. {taskList[i]}");
                }
            }
            
            _logger.Information("TASKS_LISTED: Count = {TaskCount}", tasks.Count);
            
            // Логируем статистику по приоритетам
            foreach (var group in groupedTasks)
            {
                _logger.Debug("Priority group {Priority}: {Count} tasks", group.Key, group.Count());
            }
            
            StructuredLogger.LogMetric("tasks.listed", tasks.Count);
        }
        
        _logger.Debug("◀ Конец операции ListTasks | TotalTasks: {TotalTasks}", tasks.Count);
        Console.WriteLine("─────────────────────");
    }

    public void CompleteTask(string title)
    {
        var operationData = new { Title = title };
        _logger.Debug("▶ Начало операции CompleteTask {@OperationData}", operationData);
        
        var task = tasks.FirstOrDefault(t => t.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
        
        if (task == null)
        {
            _logger.Error("COMPLETE_TASK_FAILED: Task not found {Title}", title);
            Console.WriteLine($"Ошибка: Задача \"{title}\" не найдена!");
            _logger.Debug("◀ Конец операции CompleteTask | Результат: Ошибка - задача не найдена");
            
            StructuredLogger.LogError("Task not found for completion", null, new { Title = title });
            return;
        }
        
        if (task.IsCompleted)
        {
            _logger.Warning("COMPLETE_TASK_FAILED: Already completed {Title}", title);
            Console.WriteLine($"Задача \"{title}\" уже отмечена как выполненная!");
            _logger.Debug("◀ Конец операции CompleteTask | Результат: Уже выполнена");
            return;
        }
        
        task.IsCompleted = true;
        
        _logger.Information("TASK_COMPLETED: {TaskTitle} (ID: {TaskId})", task.Title, task.Id);
        _logger.Debug("◀ Конец операции CompleteTask | Результат: Успешно | TaskId: {TaskId}", task.Id);
        
        Console.WriteLine($"✓ Задача \"{title}\" отмечена как выполненная!");
        
        StructuredLogger.LogTaskOperation("COMPLETE", task, "success", tasks.Count);
        StructuredLogger.LogMetric("tasks.completed", 1, new Dictionary<string, object>
        {
            ["taskId"] = task.Id,
            ["completionTime"] = DateTime.Now
        });
    }

    public void ShowStats()
    {
        _logger.Debug("▶ Начало операции ShowStats");
        
        int completed = tasks.Count(t => t.IsCompleted);
        int active = tasks.Count - completed;
        int percentComplete = tasks.Count > 0 ? (completed * 100 / tasks.Count) : 0;
        
        var priorityStats = tasks.GroupBy(t => t.Priority)
            .Select(g => new { Priority = g.Key, Count = g.Count(), Completed = g.Count(t => t.IsCompleted) })
            .ToList();
        
        Console.WriteLine($"\n=== СТАТИСТИКА ===");
        Console.WriteLine($"Всего задач: {tasks.Count}");
        Console.WriteLine($"Активных: {active}");
        Console.WriteLine($"Завершённых: {completed}");
        Console.WriteLine($"Процент выполнения: {percentComplete}%");
        
        Console.WriteLine($"\nПо приоритетам:");
        foreach (var stat in priorityStats)
        {
            double priorityPercent = stat.Count > 0 ? (stat.Completed * 100.0 / stat.Count) : 0;
            Console.WriteLine($"  {stat.Priority}: {stat.Count} задач ({stat.Completed} завершено, {priorityPercent:F1}%)");
        }
        
        // Структурированное логирование статистики
        var statsData = new
        {
            TotalTasks = tasks.Count,
            ActiveTasks = active,
            CompletedTasks = completed,
            CompletionRate = percentComplete,
            PriorityDistribution = priorityStats,
            Timestamp = DateTime.Now
        };
        
        _logger.Information("STATS: {@Stats}", statsData);
        
        StructuredLogger.LogStructured("Statistics_Calculated", statsData);
        
        // Логируем метрики
        StructuredLogger.LogMetric("tasks.total", tasks.Count);
        StructuredLogger.LogMetric("tasks.active", active);
        StructuredLogger.LogMetric("tasks.completed", completed);
        StructuredLogger.LogMetric("tasks.completion_rate", percentComplete);
        
        _logger.Debug("◀ Конец операции ShowStats");
    }

    public void SearchTasks(string query)
    {
        _logger.Debug("▶ Начало операции SearchTasks | Query: {Query}", query);
        
        var results = tasks.Where(t => 
            t.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            (t.Description?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
        ).ToList();
        
        Console.WriteLine($"\n=== РЕЗУЛЬТАТЫ ПОИСКА: \"{query}\" ===");
        
        if (results.Count == 0)
        {
            Console.WriteLine("Ничего не найдено.");
            _logger.Information("SEARCH: No results for query {Query}", query);
        }
        else
        {
            foreach (var task in results)
            {
                Console.WriteLine($"  {task}");
            }
            _logger.Information("SEARCH: Found {Count} tasks for query {Query}", results.Count, query);
            StructuredLogger.LogMetric("search.results", results.Count, new Dictionary<string, object>
            {
                ["query"] = query
            });
        }
        
        _logger.Debug("◀ Конец операции SearchTasks | ResultsFound: {Count}", results.Count);
    }
}