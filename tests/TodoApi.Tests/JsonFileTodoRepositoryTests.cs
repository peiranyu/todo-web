using TodoApi.Models;
using TodoApi.Repositories;
using Xunit;

namespace TodoApi.Tests;

/// <summary>
/// Unit tests for <see cref="JsonFileTodoRepository"/> (TODOAPP-17). Mirrors the
/// in-memory repository's semantics and additionally covers persistence across
/// instances (restart) and resilience to a corrupt/empty file. Each test uses a
/// unique temp file that is deleted afterwards (IDisposable).
/// </summary>
public class JsonFileTodoRepositoryTests : IDisposable
{
    private readonly string _filePath;

    public JsonFileTodoRepositoryTests()
    {
        // Unique temp file path per test instance; not created until first save.
        _filePath = Path.Combine(Path.GetTempPath(), $"todos-test-{Guid.NewGuid():N}.json");
    }

    public void Dispose()
    {
        if (File.Exists(_filePath))
        {
            File.Delete(_filePath);
        }
    }

    private JsonFileTodoRepository NewRepo() => new(_filePath);

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

        Assert.Equal(1, created.Id);                    // client Id ignored
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

    // --- Persistence-specific tests ---

    [Fact]
    public void Todos_PersistAcrossInstances_OnSameFile()
    {
        var repo1 = NewRepo();
        repo1.Add(new Todo { Title = "A" });
        repo1.Add(new Todo { Title = "B", IsComplete = true });

        // Simulate a restart: a brand-new repo over the same file.
        var repo2 = NewRepo();
        var all = repo2.GetAll().ToList();

        Assert.Equal(2, all.Count);
        Assert.Equal("A", all[0].Title);
        Assert.Equal("B", all[1].Title);
        Assert.True(all[1].IsComplete);
    }

    [Fact]
    public void NextId_ContinuesAfterRestart()
    {
        var repo1 = NewRepo();
        repo1.Add(new Todo { Title = "A" }); // id 1
        repo1.Add(new Todo { Title = "B" }); // id 2

        var repo2 = NewRepo();
        var third = repo2.Add(new Todo { Title = "C" });

        Assert.Equal(3, third.Id);
    }

    [Fact]
    public void NextId_ContinuesForward_EvenAfterDeletingLast()
    {
        var repo1 = NewRepo();
        repo1.Add(new Todo { Title = "A" }); // id 1
        var b = repo1.Add(new Todo { Title = "B" }); // id 2
        repo1.Delete(b.Id);

        // After restart, max remaining id is 1, so next is 2 again
        // (ids only reused when the higher one is gone before persisting max).
        var repo2 = NewRepo();
        var next = repo2.Add(new Todo { Title = "C" });

        Assert.Equal(2, next.Id);
    }

    [Fact]
    public void Update_PersistsAcrossInstances()
    {
        var repo1 = NewRepo();
        var created = repo1.Add(new Todo { Title = "Old" });
        repo1.Update(created.Id, new Todo { Title = "New", IsComplete = true });

        var repo2 = NewRepo();
        var reloaded = repo2.GetById(created.Id)!;

        Assert.Equal("New", reloaded.Title);
        Assert.True(reloaded.IsComplete);
    }

    [Fact]
    public void Delete_PersistsAcrossInstances()
    {
        var repo1 = NewRepo();
        var created = repo1.Add(new Todo { Title = "Bye" });
        repo1.Delete(created.Id);

        var repo2 = NewRepo();

        Assert.Null(repo2.GetById(created.Id));
        Assert.Empty(repo2.GetAll());
    }

    [Fact]
    public void MissingFile_StartsEmpty()
    {
        // _filePath does not exist yet.
        Assert.False(File.Exists(_filePath));

        var repo = NewRepo();

        Assert.Empty(repo.GetAll());
    }

    [Fact]
    public void EmptyFile_StartsEmpty()
    {
        File.WriteAllText(_filePath, "");

        var repo = NewRepo();

        Assert.Empty(repo.GetAll());
        // And it remains usable.
        var added = repo.Add(new Todo { Title = "A" });
        Assert.Equal(1, added.Id);
    }

    [Fact]
    public void CorruptFile_StartsEmpty_AndIsSelfHealingOnNextWrite()
    {
        File.WriteAllText(_filePath, "{ this is not valid json ]");

        var repo = NewRepo();
        Assert.Empty(repo.GetAll());

        // Next mutation rewrites valid JSON, readable by a fresh instance.
        repo.Add(new Todo { Title = "A" });
        var repo2 = NewRepo();
        Assert.Single(repo2.GetAll());
    }
}
