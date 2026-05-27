// Models/Task.cs
using System;
using System.Text.Json;

namespace logandtrac.Models;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsCompleted { get; set; }
    public string? Description { get; set; }
    public string Priority { get; set; } = "Medium";

    public TaskItem(string title)
    {
        Id = new Random().Next(1000, 9999);
        Title = title;
        CreatedAt = DateTime.Now;
        IsCompleted = false;
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
    }

    public override string ToString()
    {
        string status = IsCompleted ? "✓" : "○";
        return $"[{Id}] {Title} - {status} ({Priority}) [{CreatedAt:dd.MM.yyyy HH:mm}]";
    }
}