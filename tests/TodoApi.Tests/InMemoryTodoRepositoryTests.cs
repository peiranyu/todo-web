using TodoApi.Models;
using TodoApi.Repositories;
using Xunit;

namespace TodoApi.Tests;

/// <summary>
/// Unit tests for <see cref="InMemoryTodoRepository"/> (TODOAPP-9).
/// Covers id assignment, server-controlled fields, and not-found behaviour.
/// </summary>
public class InMemoryTodoRepositoryTests
{
    private static InMemoryTodoRepository NewRepo() => new();

    [Fact]
    public void Add_AssignsIncrementingIds_StartingAtOne()
    {
        var repo = NewRepo();

        var first = repo.Add(new Todo { Title = "A" });
        var second = repo.Add(new Todo { Title = "B" });

        Assert.Equal(1, first.Id);
        Assert.Equal(2, second.Id);
    }

    [Fact]
    public void Add_SetsCreatedAt_AndIgnoresClientSuppliedIdAndCreatedAt()
    {
        var repo = NewRepo();
        var clientDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var created = repo.Add(new Todo { Id = 999, Title = "X", CreatedAt = clientDate });

        Assert.Equal(1, created.Id);                 // client Id ignored
        Assert.NotEqual(clientDate, created.CreatedAt); // client CreatedAt ignored
        Assert.True(created.CreatedAt > clientDate);
    }

    [Fact]
    public void Add_DefaultsIsCompleteToFalse()
    {
        var repo = NewRepo();

        var created = repo.Add(new Todo { Title = "X" });

        Assert.False(created.IsComplete);
    }

    [Fact]
    public void GetAll_ReturnsAllAddedTodos_OrderedById()
    {
        var repo = NewRepo();
        repo.Add(new Todo { Title = "A" });
        repo.Add(new Todo { Title = "B" });

        var all = repo.GetAll().ToList();

        Assert.Equal(2, all.Count);
        Assert.Equal(new[] { 1, 2 }, all.Select(t => t.Id).ToArray());
    }

    [Fact]
    public void GetAll_OnEmptyRepo_ReturnsEmpty()
    {
        var repo = NewRepo();

        Assert.Empty(repo.GetAll());
    }

    [Fact]
    public void GetById_ReturnsTodo_WhenExists()
    {
        var repo = NewRepo();
        var created = repo.Add(new Todo { Title = "Find me" });

        var found = repo.GetById(created.Id);

        Assert.NotNull(found);
        Assert.Equal("Find me", found!.Title);
    }

    [Fact]
    public void GetById_ReturnsNull_WhenNotFound()
    {
        var repo = NewRepo();

        Assert.Null(repo.GetById(123));
    }

    [Fact]
    public void Update_ChangesEditableFields_AndReturnsTrue()
    {
        var repo = NewRepo();
        var created = repo.Add(new Todo { Title = "Old" });

        var ok = repo.Update(created.Id, new Todo { Title = "New", IsComplete = true });

        Assert.True(ok);
        var updated = repo.GetById(created.Id)!;
        Assert.Equal("New", updated.Title);
        Assert.True(updated.IsComplete);
    }

    [Fact]
    public void Update_DoesNotChangeIdOrCreatedAt()
    {
        var repo = NewRepo();
        var created = repo.Add(new Todo { Title = "Old" });
        var originalCreatedAt = created.CreatedAt;

        repo.Update(created.Id, new Todo { Id = 555, Title = "New", CreatedAt = DateTime.UnixEpoch });

        var updated = repo.GetById(created.Id)!;
        Assert.Equal(created.Id, updated.Id);
        Assert.Equal(originalCreatedAt, updated.CreatedAt);
    }

    [Fact]
    public void Update_ReturnsFalse_WhenNotFound()
    {
        var repo = NewRepo();

        Assert.False(repo.Update(123, new Todo { Title = "x" }));
    }

    [Fact]
    public void Delete_RemovesTodo_AndReturnsTrue()
    {
        var repo = NewRepo();
        var created = repo.Add(new Todo { Title = "Bye" });

        var ok = repo.Delete(created.Id);

        Assert.True(ok);
        Assert.Null(repo.GetById(created.Id));
    }

    [Fact]
    public void Delete_ReturnsFalse_WhenNotFound()
    {
        var repo = NewRepo();

        Assert.False(repo.Delete(123));
    }
}
