using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;
using TodoApi.Repositories;

namespace TodoApi.Controllers;

/// <summary>
/// REST endpoints for todos under <c>/api/todos</c>.
/// See design_doc/01-milestone-1-rest-api.md (§2 API contract).
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

    /// <summary>Lists all todos. Returns 200 OK with an array (possibly empty).</summary>
    [HttpGet]
    public ActionResult<IEnumerable<Todo>> GetAll()
    {
        return Ok(_repository.GetAll());
    }

    /// <summary>Gets a single todo by id. Returns 200 OK, or 404 if not found.</summary>
    [HttpGet("{id}", Name = "GetById")]
    public ActionResult<Todo> GetById(int id)
    {
        var todo = _repository.GetById(id);
        if (todo is null)
        {
            return NotFound();
        }

        return Ok(todo);
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
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates a todo (full replace of editable fields). TODOAPP-12.
    /// Title is required; Id and CreatedAt are server-controlled and ignored.
    /// </summary>
    /// <returns>204 on success, 400 if Title is missing/empty, 404 if not found.</returns>
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Todo todo)
    {
        if (string.IsNullOrWhiteSpace(todo.Title))
        {
            return BadRequest("Title is required.");
        }

        if (!_repository.Update(id, todo))
        {
            return NotFound();
        }

        return NoContent();
    }
}
