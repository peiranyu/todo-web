namespace TodoApi.Models;

/// <summary>
/// A single todo item. See design_doc/01-milestone-1-rest-api.md (§1 Data model).
/// </summary>
public class Todo
{
    /// <summary>Unique identifier. Auto-incrementing, assigned by the server.</summary>
    public int Id { get; set; }

    /// <summary>The task text, e.g. "Buy milk". Required, non-empty.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Whether the task is done. Defaults to false.</summary>
    public bool IsComplete { get; set; }

    /// <summary>When the todo was created (UTC). Assigned by the server.</summary>
    public DateTime CreatedAt { get; set; }
}
