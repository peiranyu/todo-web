using Microsoft.AspNetCore.Mvc;
using TodoApi.Repositories;

namespace TodoApi.Controllers;

/// <summary>
/// REST endpoints for todos. See design_doc/01-milestone-1-rest-api.md (§2 API contract).
/// </summary>
[Route("api/todos")]
[ApiController]
public class TodosController : ControllerBase
{
    private readonly ITodoRepository _repository;

    public TodosController(ITodoRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Deletes the todo with the given id (TODOAPP-13).
    /// Returns 204 No Content on success, or 404 Not Found if no such todo exists.
    /// </summary>
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        if (!_repository.Delete(id))
        {
            return NotFound();
        }

        return NoContent();
    }
}
