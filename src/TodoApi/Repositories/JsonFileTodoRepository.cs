using System.Text.Json;
using TodoApi.Models;

namespace TodoApi.Repositories;

/// <summary>
/// JSON-file implementation of <see cref="ITodoRepository"/> (Milestone 2, TODOAPP-17).
/// Loads all todos from a JSON file on construction and rewrites the file after every
/// mutation, so data survives app restarts. Thread-safe via a single coarse lock; the
/// file path is injectable so tests can point at a temp file.
/// See design_doc/02-milestone-2-persistence.md.
/// </summary>
public class JsonFileTodoRepository : ITodoRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
    };

    private readonly string _filePath;
    private readonly object _lock = new();
    private readonly List<Todo> _todos;
    private int _nextId;

    /// <summary>
    /// Creates the repository backed by <paramref name="filePath"/>. The file (and its
    /// directory) need not exist yet; a missing/empty/corrupt file starts an empty store.
    /// </summary>
    public JsonFileTodoRepository(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        _todos = Load(_filePath);
        _nextId = _todos.Count == 0 ? 0 : _todos.Max(t => t.Id);
    }

    public IEnumerable<Todo> GetAll()
    {
        lock (_lock)
        {
            // Return copies ordered by Id so callers can't mutate stored state.
            return _todos.OrderBy(t => t.Id).Select(Copy).ToList();
        }
    }

    public Todo? GetById(int id)
    {
        lock (_lock)
        {
            var todo = _todos.FirstOrDefault(t => t.Id == id);
            return todo is null ? null : Copy(todo);
        }
    }

    public Todo Add(Todo todo)
    {
        lock (_lock)
        {
            // Id and CreatedAt are server-controlled; any client-supplied values are ignored.
            var created = new Todo
            {
                Id = ++_nextId,
                Title = todo.Title,
                IsComplete = todo.IsComplete,
                CreatedAt = DateTime.UtcNow,
            };

            _todos.Add(created);
            Save();
            return Copy(created);
        }
    }

    public bool Update(int id, Todo todo)
    {
        lock (_lock)
        {
            var existing = _todos.FirstOrDefault(t => t.Id == id);
            if (existing is null)
            {
                return false;
            }

            // Replace editable fields only; Id and CreatedAt are immutable.
            existing.Title = todo.Title;
            existing.IsComplete = todo.IsComplete;
            Save();
            return true;
        }
    }

    public bool Delete(int id)
    {
        lock (_lock)
        {
            var existing = _todos.FirstOrDefault(t => t.Id == id);
            if (existing is null)
            {
                return false;
            }

            _todos.Remove(existing);
            Save();
            return true;
        }
    }

    /// <summary>
    /// Reads and deserializes the store. Returns an empty list when the file is missing,
    /// empty, or corrupt (defensive — see design doc §6). Caller holds no lock yet
    /// (construction only).
    /// </summary>
    private static List<Todo> Load(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return new List<Todo>();
            }

            var json = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<Todo>();
            }

            var todos = JsonSerializer.Deserialize<List<Todo>>(json, SerializerOptions);
            return todos ?? new List<Todo>();
        }
        catch (JsonException)
        {
            // Corrupt file: start empty; the next mutation rewrites valid JSON.
            return new List<Todo>();
        }
    }

    /// <summary>Persists the full list to disk. Caller must hold <see cref="_lock"/>.</summary>
    private void Save()
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(_todos, SerializerOptions);
        File.WriteAllText(_filePath, json);
    }

    private static Todo Copy(Todo todo) => new()
    {
        Id = todo.Id,
        Title = todo.Title,
        IsComplete = todo.IsComplete,
        CreatedAt = todo.CreatedAt,
    };
}
