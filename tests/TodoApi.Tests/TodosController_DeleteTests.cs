using Microsoft.AspNetCore.Mvc;
using TodoApi.Controllers;
using TodoApi.Models;
using TodoApi.Repositories;
using Xunit;

namespace TodoApi.Tests;

/// <summary>
/// Tests for the DELETE /api/todos/{id} endpoint (TODOAPP-13).
/// Uses the real <see cref="InMemoryTodoRepository"/> rather than a mock.
/// </summary>
public class TodosController_DeleteTests
{
    private static TodosController NewController(ITodoRepository repo) => new(repo);

    [Fact]
    public void Delete_ExistingTodo_ReturnsNoContent_AndRemovesIt()
    {
        var repo = new InMemoryTodoRepository();
        var created = repo.Add(new Todo { Title = "Delete me" });
        var controller = NewController(repo);

        var result = controller.Delete(created.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.Null(repo.GetById(created.Id));
    }

    [Fact]
    public void Delete_MissingId_ReturnsNotFound()
    {
        var repo = new InMemoryTodoRepository();
        var controller = NewController(repo);

        var result = controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);
    }
}
