// Logging/LoggerExtensions.cs
using Serilog;
using logandtrac.Models;

namespace logandtrac.Logging;

public static class LoggerExtensions
{
    public static void LogOperationStart(this ILogger logger, string operation, object? data = null)
    {
        if (data != null)
        {
            logger.Debug("▶ Начало операции {Operation} {@OperationData}", operation, data);
        }
        else
        {
            logger.Debug("▶ Начало операции {Operation}", operation);
        }
    }

    public static void LogOperationEnd(this ILogger logger, string operation, string result = "", object? data = null)
    {
        if (data != null)
        {
            logger.Debug("◀ Конец операции {Operation} | Результат: {Result} {@OperationData}", 
                operation, result, data);
        }
        else
        {
            logger.Debug("◀ Конец операции {Operation} | Результат: {Result}", operation, result);
        }
    }

    public static void LogTaskCreated(this ILogger logger, TaskItem task, int totalTasks)
    {
        logger.Information("TASK_CREATED: {TaskTitle} (ID: {TaskId}) | Total: {TotalTasks}", 
            task.Title, task.Id, totalTasks);
        
        StructuredLogger.LogMetric("tasks.created", 1, new Dictionary<string, object>
        {
            ["taskId"] = task.Id,
            ["priority"] = task.Priority
        });
    }

    public static void LogTaskDeleted(this ILogger logger, string taskTitle, int taskId, int totalTasks)
    {
        logger.Information("TASK_DELETED: {TaskTitle} (ID: {TaskId}) | Remaining: {TotalTasks}", 
            taskTitle, taskId, totalTasks);
        
        StructuredLogger.LogMetric("tasks.deleted", 1, new Dictionary<string, object>
        {
            ["taskId"] = taskId
        });
    }

    public static void LogTaskCompleted(this ILogger logger, TaskItem task)
    {
        logger.Information("TASK_COMPLETED: {TaskTitle} (ID: {TaskId})", task.Title, task.Id);
        
        StructuredLogger.LogMetric("tasks.completed", 1, new Dictionary<string, object>
        {
            ["taskId"] = task.Id,
            ["completionTime"] = DateTime.Now
        });
    }

    public static void LogTasksListed(this ILogger logger, int count)
    {
        logger.Information("TASKS_LISTED: Count = {TaskCount}", count);
        
        StructuredLogger.LogMetric("tasks.listed", count);
    }

    public static void LogErrorWithContext(this ILogger logger, Exception ex, string operation, object context)
    {
        logger.Error(ex, "ERROR in {Operation} | Context: {@ErrorContext}", operation, context);
        
        StructuredLogger.LogStructured("Operation_Error", new
        {
            Operation = operation,
            Error = ex.Message,
            StackTrace = ex.StackTrace,
            Context = context,
            Timestamp = DateTime.Now
        });
    }
}