using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;
using TodoApi.Repositories;

namespace TodoApi.Controllers;

/// <summary>
/// REST endpoints for todos. See design_doc/01-milestone-1-rest-api.md (§2 API contract).
/// Depends on <see cref="ITodoRepository"/> so storage can be swapped without changing the controller.
/// </summary>
[ApiController]
[Route("api/todos")]
public class TodosController : ControllerBase
{
    private readonly ITodoRepository _repository;

    public TodosController(ITodoRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Creates a todo (TODOAPP-10). The server assigns Id and CreatedAt; any values
    /// supplied on those fields by the client are ignored (see design §2).
    /// </summary>
    /// <returns>201 Created with the stored todo, or 400 if Title is missing/empty.</returns>
    [HttpPost]
    public IActionResult Create([FromBody] Todo todo)
    {
        if (string.IsNullOrWhiteSpace(todo.Title))
        {
            return BadRequest("Title is required.");
        }

        var created = _repository.Add(todo);

        // Other endpoints (e.g. GET by id, TODOAPP-11) are being built in parallel,
        // so we return the location URL directly rather than CreatedAtAction, which
        // would depend on a GetById action that does not exist yet.
        return Created($"/api/todos/{created.Id}", created);
    }
}
