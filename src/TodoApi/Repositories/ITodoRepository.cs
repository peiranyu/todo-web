using TodoApi.Models;

namespace TodoApi.Repositories;

/// <summary>
/// Storage abstraction for todos. Controllers depend on this interface, not on a
/// concrete store, so the implementation can be swapped (in-memory now, a database
/// in Milestone 2) without changing controller code.
/// See design_doc/01-milestone-1-rest-api.md (§3 Storage design).
/// </summary>
public interface ITodoRepository
{
    /// <summary>Returns all todos.</summary>
    IEnumerable<Todo> GetAll();

    /// <summary>Returns the todo with the given id, or null if not found.</summary>
    Todo? GetById(int id);

    /// <summary>
    /// Adds a new todo. Assigns the Id and CreatedAt (server-controlled);
    /// any values supplied on those fields are ignored. Returns the stored todo.
    /// </summary>
    Todo Add(Todo todo);

    /// <summary>
    /// Replaces the editable fields (Title, IsComplete) of an existing todo.
    /// Returns false if no todo with the given id exists.
    /// </summary>
    bool Update(int id, Todo todo);

    /// <summary>Deletes the todo with the given id. Returns false if not found.</summary>
    bool Delete(int id);
}
