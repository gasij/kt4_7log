using System;
using System.IO;
using Serilog;

namespace logandtrac.Logging;

public static class LoggerConfig
{
    private static string logDirectory = "Logs";

    public static ILogger Configure()
    {
        // Создаем директорию для логов, если её нет
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        // Настраиваем логгер
        var logger = new LoggerConfiguration()
            .MinimumLevel.Debug() // Логируем все уровни от Debug и выше
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
            .WriteTo.File(
                path: Path.Combine(logDirectory, "taskmanager_.log"),
                rollingInterval: RollingInterval.Day, // Новый файл каждый день
                retainedFileCountLimit: 7, // Храним логи за 7 дней
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information // В файл пишем от Information
            )
            .CreateLogger();

        // Логируем старт приложения
        logger.Information("=== ЗАПУСК TASK MANAGER ===");
        logger.Debug("Версия: 1.0.0");
        logger.Debug("OS: {OS}", Environment.OSVersion);
        logger.Debug(".NET Version: {Version}", Environment.Version);
        logger.Information("Директория логов: {LogDirectory}", Path.GetFullPath(logDirectory));

        return logger;
    }
}