using Microsoft.AspNetCore.Mvc;
using TodoApi.Controllers;
using TodoApi.Models;
using TodoApi.Repositories;
using Xunit;

namespace TodoApi.Tests;

/// <summary>
/// Unit tests for the PUT /api/todos/{id} endpoint (TODOAPP-12).
/// Uses the real <see cref="InMemoryTodoRepository"/>.
/// </summary>
public class TodosController_UpdateTests
{
    private static (TodosController controller, InMemoryTodoRepository repo) NewController()
    {
        var repo = new InMemoryTodoRepository();
        return (new TodosController(repo), repo);
    }

    [Fact]
    public void Update_ExistingTodo_Returns204_AndStoresNewValues()
    {
        var (controller, repo) = NewController();
        var created = repo.Add(new Todo { Title = "Old" });

        var result = controller.Update(created.Id, new Todo { Title = "New", IsComplete = true });

        Assert.IsType<NoContentResult>(result);
        var stored = repo.GetById(created.Id)!;
        Assert.Equal("New", stored.Title);
        Assert.True(stored.IsComplete);
    }

    [Fact]
    public void Update_MissingId_Returns404()
    {
        var (controller, _) = NewController();

        var result = controller.Update(123, new Todo { Title = "Anything" });

        Assert.IsType<NotFoundResult>(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_EmptyOrWhitespaceTitle_Returns400(string title)
    {
        var (controller, repo) = NewController();
        var created = repo.Add(new Todo { Title = "Old" });

        var result = controller.Update(created.Id, new Todo { Title = title });

        Assert.IsType<BadRequestObjectResult>(result);
    }
}
