// Program.cs
using System;
using logandtrac.Services;
using logandtrac.Logging;
using Serilog;

namespace logandtrac;

class Program
{
    static void Main(string[] args)
    {
        // Настраиваем структурированное логирование
        StructuredLogger.Configure();
        var logger = Log.Logger;
        
        try
        {
            logger.Information("=== TASK MANAGER v2.0 STARTED ===");
            
            var taskManager = new TaskManagerService(logger);
            
            Console.WriteLine("╔════════════════════════════════╗");
            Console.WriteLine("║     TASK MANAGER v2.0          ║");
            Console.WriteLine("║  Структурированное логирование ║");
            Console.WriteLine("╚════════════════════════════════╝");
            ShowHelp();
            
            bool isRunning = true;
            
            while (isRunning)
            {
                Console.Write("\n> ");
                string? input = Console.ReadLine()?.Trim();
                
                if (string.IsNullOrEmpty(input))
                {
                    continue;
                }
                
                string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string command = parts[0].ToLower();
                
                switch (command)
                {
                    case "add":
                        AddTask(taskManager, parts);
                        break;
                        
                    case "remove":
                        RemoveTask(taskManager, parts);
                        break;
                        
                    case "list":
                        taskManager.ListTasks();
                        break;
                        
                    case "complete":
                        CompleteTask(taskManager, parts);
                        break;
                        
                    case "search":
                        SearchTasks(taskManager, parts);
                        break;
                        
                    case "stats":
                        taskManager.ShowStats();
                        break;
                        
                    case "help":
                        ShowHelp();
                        break;
                        
                    case "exit":
                        isRunning = false;
                        logger.Information("Application shutdown initiated by user");
                        Console.WriteLine("\nДо свидания!");
                        Console.WriteLine($"Структурированные логи сохранены в Logs/");
                        break;
                        
                    default:
                        logger.Warning("Unknown command: {Command}", command);
                        Console.WriteLine($"Неизвестная команда: {command}");
                        Console.WriteLine("Введите 'help' для списка команд.");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            logger.Fatal(ex, "Critical application error");
            Console.WriteLine($"Критическая ошибка: {ex.Message}");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    static void AddTask(TaskManagerService taskManager, string[] parts)
    {
        if (parts.Length < 2)
        {
            Console.Write("Введите название задачи: ");
            string? title = Console.ReadLine();
            
            if (!string.IsNullOrEmpty(title))
            {
                Console.Write("Введите приоритет (High/Medium/Low) [Medium]: ");
                string? priority = Console.ReadLine();
                if (string.IsNullOrEmpty(priority)) priority = "Medium";
                
                taskManager.AddTask(title, priority);
            }
        }
        else
        {
            string title = string.Join(" ", parts.Skip(1));
            taskManager.AddTask(title, "Medium");
        }
    }

    static void RemoveTask(TaskManagerService taskManager, string[] parts)
    {
        if (parts.Length < 2)
        {
            Console.Write("Введите название задачи для удаления: ");
            string? title = Console.ReadLine();
            
            if (!string.IsNullOrEmpty(title))
            {
                taskManager.RemoveTask(title);
            }
        }
        else
        {
            string title = string.Join(" ", parts.Skip(1));
            taskManager.RemoveTask(title);
        }
    }

    static void CompleteTask(TaskManagerService taskManager, string[] parts)
    {
        if (parts.Length < 2)
        {
            Console.Write("Введите название задачи для отметки выполнения: ");
            string? title = Console.ReadLine();
            
            if (!string.IsNullOrEmpty(title))
            {
                taskManager.CompleteTask(title);
            }
        }
        else
        {
            string title = string.Join(" ", parts.Skip(1));
            taskManager.CompleteTask(title);
        }
    }

    static void SearchTasks(TaskManagerService taskManager, string[] parts)
    {
        if (parts.Length < 2)
        {
            Console.Write("Введите поисковый запрос: ");
            string? query = Console.ReadLine();
            
            if (!string.IsNullOrEmpty(query))
            {
                taskManager.SearchTasks(query);
            }
        }
        else
        {
            string query = string.Join(" ", parts.Skip(1));
            taskManager.SearchTasks(query);
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine("\nДоступные команды:");
        Console.WriteLine("  add [название]        - Добавить новую задачу");
        Console.WriteLine("  remove [название]     - Удалить задачу");
        Console.WriteLine("  list                   - Показать список задач");
        Console.WriteLine("  complete [название]    - Отметить задачу как выполненную");
        Console.WriteLine("  search [запрос]        - Поиск задач");
        Console.WriteLine("  stats                  - Показать статистику");
        Console.WriteLine("  help                    - Показать это сообщение");
        Console.WriteLine("  exit                    - Выход из программы");
        Console.WriteLine("\nПримеры:");
        Console.WriteLine("  > add Купить продукты");
        Console.WriteLine("  > complete Купить продукты");
        Console.WriteLine("  > search продукты");
    }
}