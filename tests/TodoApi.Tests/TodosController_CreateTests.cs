using Microsoft.AspNetCore.Mvc;
using TodoApi.Controllers;
using TodoApi.Models;
using TodoApi.Repositories;
using Xunit;

namespace TodoApi.Tests;

/// <summary>
/// Tests for the POST /api/todos endpoint (TODOAPP-10). Uses the real
/// <see cref="InMemoryTodoRepository"/> so the controller and storage are exercised together.
/// </summary>
public class TodosController_CreateTests
{
    private static TodosController NewController() => new(new InMemoryTodoRepository());

    [Fact]
    public void Create_WithValidTitle_Returns201_AndAssignsId()
    {
        var controller = NewController();

        var result = controller.Create(new Todo { Title = "Buy milk" });

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, created.StatusCode);
        Assert.Equal(nameof(TodosController.GetById), created.ActionName);

        var todo = Assert.IsType<Todo>(created.Value);
        Assert.True(todo.Id > 0);
        Assert.Equal("Buy milk", todo.Title);
        Assert.Equal(todo.Id, created.RouteValues!["id"]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithMissingOrWhitespaceTitle_Returns400(string? title)
    {
        var controller = NewController();

        var result = controller.Create(new Todo { Title = title! });

        Assert.IsType<BadRequestObjectResult>(result);
    }
}
