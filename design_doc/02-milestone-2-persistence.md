# Milestone 2 Design — JSON file persistence

**Epic:** TODOAPP-14
**Tasks:** TODOAPP-16 (design), TODOAPP-17 (implementation)
**Goal:** Todos survive an app restart by persisting to a JSON file on disk.

This document is the **Design** phase output for Milestone 2. It defines how a new
file-backed repository persists todos, while keeping the M1 `ITodoRepository`
contract (and therefore the controllers) unchanged.

---

## 1. Approach

The key M1 design move was depending on the `ITodoRepository` **interface** rather
than a concrete store. M2 cashes that in: we add a second implementation,
`JsonFileTodoRepository`, that reads and writes a JSON file, and swap **one line**
of DI registration. Controllers and the `Todo` model are untouched.

**Decision: a JSON file, not a database.** A single-file JSON store is the simplest
thing that gives us durability across restarts. A real database (EF Core) is
deferred — it is overkill for a single-user practice app and adds migration and
hosting complexity. The interface means we can still move to a DB later with a
one-line swap.

---

## 2. File location

- Path: `App_Data/todos.json`, resolved **relative to the app content root**
  (`IHostEnvironment.ContentRootPath`), so it does not depend on the current
  working directory.
- The `App_Data/` directory is **created on demand** if it does not exist.
- The path is **injectable**: `JsonFileTodoRepository` takes the absolute file
  path as a constructor argument (defaulting to the `App_Data/todos.json` location
  computed in `Program.cs`). This lets tests point each instance at a unique temp
  file.
- `App_Data/` (runtime data) is **git-ignored** — it is generated, not source.

---

## 3. Serialization

- `System.Text.Json` (built in, no extra package).
- The whole store is one JSON array of `Todo` objects:
  ```json
  [
    { "Id": 1, "Title": "Buy milk", "IsComplete": false, "CreatedAt": "2026-06-28T07:30:00Z" }
  ]
  ```
- Written with `WriteIndented = true` for readability/debuggability.
- The full list is rewritten on every mutation (Add/Update/Delete). For the small
  data volume of this app this is simpler and safer than incremental writes.

---

## 4. Restoring the next Id on load

On construction the repository reads the file and loads all todos into memory.
The next id is computed as **`max(existing Id) + 1`**, or `1` when the file is
empty / has no todos. This continues the incrementing-id sequence correctly across
restarts (e.g. if ids 1–3 exist, the next Add assigns 4) and matches the
in-memory repository's "ids start at 1, monotonically increasing" semantics.

> Note: ids are **not** reused after deletion — the counter only ever moves
> forward, exactly like the in-memory store.

---

## 5. Thread-safety

The app registers the repository as a **singleton**, so concurrent requests share
one instance. All read-modify-write operations are guarded by a single private
`lock`:

- `Add` / `Update` / `Delete`: take the lock, mutate the in-memory list, write the
  file, release.
- `GetAll` / `GetById`: take the lock and return **copies** (snapshots) so callers
  can't mutate stored state and don't observe partial writes.

Holding one coarse lock is sufficient and simple for this workload; finer-grained
locking is unnecessary.

---

## 6. Error handling

On load the repository is defensive and **starts empty** rather than crashing when:

- the file is **missing** (first run),
- the file is **empty** (zero bytes / whitespace),
- the file is **corrupt** (not valid JSON / not deserializable).

In all three cases the in-memory list is empty and the next id is `1`. The next
successful mutation rewrites the file with valid JSON, so a corrupt file is
self-healing on the next write. (We intentionally do not throw on a corrupt file:
losing unreadable data is preferable to a boot loop for this practice app; a
production system would log/quarantine instead.)

---

## 7. Same semantics as in-memory

`JsonFileTodoRepository` mirrors `InMemoryTodoRepository` exactly:

- **Add** assigns the next `Id` and sets `CreatedAt = DateTime.UtcNow`, **ignoring**
  any client-supplied `Id`/`CreatedAt`; returns the stored todo.
- **Update** replaces `Title` + `IsComplete` only; `Id`/`CreatedAt` are immutable;
  returns `false` when the id is missing.
- **Delete** returns `false` when the id is missing.
- **GetAll** returns todos ordered by `Id`.

---

## 8. Program.cs swap (one line)

```csharp
// before
builder.Services.AddSingleton<ITodoRepository, InMemoryTodoRepository>();
// after
builder.Services.AddSingleton<ITodoRepository>(sp =>
    new JsonFileTodoRepository(
        Path.Combine(sp.GetRequiredService<IHostEnvironment>().ContentRootPath,
                     "App_Data", "todos.json")));
```

Only this single registration line changes; the rest of `Program.cs` is left alone
(the frontend work edits `Program.cs` in parallel — keeping the diff to one line
avoids merge conflicts).

---

## 9. Out of scope for M2

- A real database / EF Core (interface makes this a future one-line swap).
- Concurrency across multiple processes / machines (single-process file lock only).
- Backups, migrations, schema versioning.

---

## 10. Ticket mapping

| Ticket | Covered by |
|--------|-----------|
| TODOAPP-16 | This design doc (§1–§9) |
| TODOAPP-17 | `JsonFileTodoRepository` + `Program.cs` swap + tests |
