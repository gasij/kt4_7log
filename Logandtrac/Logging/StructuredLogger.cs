// Logging/StructuredLogger.cs
using System;
using System.Collections.Generic;
using System.IO;
using Serilog;
using logandtrac.Models;

namespace logandtrac.Logging;

public static class StructuredLogger
{
    private static ILogger _logger = Log.Logger;

    public static void Configure()
    {
        // Создаем директорию для логов
        string logDirectory = "Logs";
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        // Настраиваем структурированное логирование (без WithThreadId, так как может не быть)
        _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("Application", "TaskManager")
            .Enrich.WithProperty("Version", "2.0.0")
            
            // Консольный вывод
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
            
            // Файловый вывод в JSON формате
            .WriteTo.File(
                path: Path.Combine(logDirectory, "structured-log-.json"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                formatter: new Serilog.Formatting.Compact.CompactJsonFormatter()
            )
            
            // Читаемые логи
            .WriteTo.File(
                path: Path.Combine(logDirectory, "readable-log-.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}"
            )
            
            .CreateLogger();

        Log.Logger = _logger;
        
        LogStructured("Application_Started", new
        {
            StartupTime = DateTime.Now,
            OS = Environment.OSVersion.ToString(),
            DotNetVersion = Environment.Version.ToString(),
            User = Environment.UserName,
            WorkingDirectory = Environment.CurrentDirectory
        });
    }

    // Структурированное логирование операций с задачами
    public static void LogTaskOperation(string operation, TaskItem task, string result = "success", int totalTasks = 0)
    {
        var logData = new
        {
            Operation = operation,
            TaskId = task.Id,
            TaskTitle = task.Title,
            IsCompleted = task.IsCompleted,
            Priority = task.Priority,
            Result = result,
            Timestamp = DateTime.Now,
            TotalTasks = totalTasks
        };

        _logger.Information("TaskOperation: {Operation} | Task: {TaskTitle} (ID: {TaskId}) | Result: {Result}", 
            operation, task.Title, task.Id, result);
        
        // Дополнительно логируем как структурированные данные
        _logger.Information("StructuredTaskOperation {@TaskData}", logData);
    }

    public static void LogWarning(string message, object? data = null)
    {
        if (data != null)
        {
            _logger.Warning(message + " {@AdditionalData}", data);
        }
        else
        {
            _logger.Warning(message);
        }
    }

    public static void LogError(string message, Exception? ex = null, object? data = null)
    {
        if (ex != null && data != null)
        {
            _logger.Error(ex, message + " {@AdditionalData}", data);
        }
        else if (ex != null)
        {
            _logger.Error(ex, message);
        }
        else if (data != null)
        {
            _logger.Error(message + " {@AdditionalData}", data);
        }
        else
        {
            _logger.Error(message);
        }
    }

    public static void LogStructured(string eventType, object data)
    {
        _logger.Information("Event: {EventType} {@EventData}", eventType, data);
    }

    public static void LogMetric(string metricName, double value, Dictionary<string, object>? tags = null)
    {
        var metricData = new Dictionary<string, object>
        {
            ["Metric"] = metricName,
            ["Value"] = value,
            ["Timestamp"] = DateTime.Now
        };

        if (tags != null)
        {
            foreach (var tag in tags)
            {
                metricData[tag.Key] = tag.Value;
            }
        }

        _logger.Information("Metric: {MetricName} = {MetricValue} {@MetricTags}", 
            metricName, value, metricData);
    }

    public static void LogInformation(string message, params object[] propertyValues)
    {
        _logger.Information(message, propertyValues);
    }
}