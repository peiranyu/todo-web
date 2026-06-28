using Microsoft.AspNetCore.Mvc;
using TodoApi.Controllers;
using TodoApi.Models;
using TodoApi.Repositories;
using Xunit;

namespace TodoApi.Tests;

/// <summary>
/// Tests for the GET endpoints on <see cref="TodosController"/> (TODOAPP-11),
/// exercised against the real <see cref="InMemoryTodoRepository"/>.
/// </summary>
public class TodosController_GetTests
{
    private static TodosController NewController(ITodoRepository repo) => new(repo);

    [Fact]
    public void GetAll_ReturnsAllTodos()
    {
        var repo = new InMemoryTodoRepository();
        repo.Add(new Todo { Title = "A" });
        repo.Add(new Todo { Title = "B" });
        var controller = NewController(repo);

        var result = controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var todos = Assert.IsAssignableFrom<IEnumerable<Todo>>(ok.Value);
        Assert.Equal(2, todos.Count());
    }

    [Fact]
    public void GetById_ReturnsTodo_WhenExists()
    {
        var repo = new InMemoryTodoRepository();
        var created = repo.Add(new Todo { Title = "Find me" });
        var controller = NewController(repo);

        var result = controller.GetById(created.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var todo = Assert.IsType<Todo>(ok.Value);
        Assert.Equal(created.Id, todo.Id);
        Assert.Equal("Find me", todo.Title);
    }

    [Fact]
    public void GetById_ReturnsNotFound_WhenMissing()
    {
        var repo = new InMemoryTodoRepository();
        var controller = NewController(repo);

        var result = controller.GetById(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }
}
