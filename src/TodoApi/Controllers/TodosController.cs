using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;
using TodoApi.Repositories;

namespace TodoApi.Controllers;

/// <summary>
/// REST endpoints for todos under <c>/api/todos</c>.
/// See design_doc/01-milestone-1-rest-api.md (§2 API contract).
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
}
