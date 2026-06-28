using System.Collections.Concurrent;
using TodoApi.Models;

namespace TodoApi.Repositories;

/// <summary>
/// In-memory implementation of <see cref="ITodoRepository"/>. Stores todos in a
/// thread-safe dictionary keyed by id, with a monotonically increasing id counter.
/// Registered as a singleton so data persists for the app's lifetime (until restart).
/// This is the Milestone 1 limitation that Milestone 2 (a database) will fix.
/// </summary>
public class InMemoryTodoRepository : ITodoRepository
{
    private readonly ConcurrentDictionary<int, Todo> _todos = new();
    private int _nextId = 0;
    private readonly object _idLock = new();

    public IEnumerable<Todo> GetAll() => _todos.Values.OrderBy(t => t.Id).ToList();

    public Todo? GetById(int id) => _todos.TryGetValue(id, out var todo) ? todo : null;

    public Todo Add(Todo todo)
    {
        int id;
        lock (_idLock)
        {
            id = ++_nextId;
        }

        var created = new Todo
        {
            Id = id,
            Title = todo.Title,
            IsComplete = todo.IsComplete,
            CreatedAt = DateTime.UtcNow,
        };

        _todos[id] = created;
        return created;
    }

    public bool Update(int id, Todo todo)
    {
        if (!_todos.TryGetValue(id, out var existing))
        {
            return false;
        }

        // Replace editable fields only; Id and CreatedAt are server-controlled.
        existing.Title = todo.Title;
        existing.IsComplete = todo.IsComplete;
        return true;
    }

    public bool Delete(int id) => _todos.TryRemove(id, out _);
}
