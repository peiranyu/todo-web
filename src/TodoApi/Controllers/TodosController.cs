using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;
using TodoApi.Repositories;

namespace TodoApi.Controllers;

/// <summary>
/// REST endpoints for todos. See design_doc/01-milestone-1-rest-api.md (§2 API contract).
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
