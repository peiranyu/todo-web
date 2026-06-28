# Milestone 1 Design — Todo REST API (in-memory)

**Epic:** TODOAPP-7
**Goal:** A working todo REST API in .NET 9 with full CRUD and in-memory storage.

This document is the **Design** phase output for Milestone 1. It defines the data
model, the API contract, and the storage design before implementation.

---

## 1. Data model

A single entity, `Todo`:

| Field | Type | Notes |
|-------|------|-------|
| `Id` | `int` | Auto-incrementing, server-assigned. Unique per todo. |
| `Title` | `string` | The task text, e.g. "Buy milk". Required, non-empty. |
| `IsComplete` | `bool` | Whether the task is done. Defaults to `false`. |
| `CreatedAt` | `DateTime` | When the todo was created (UTC). Server-assigned. |

**Decisions**
- `Id` is `int` (auto-increment) — simple and readable for a single-store app.
- Model kept lean for M1; `Description`, `DueDate`, `Priority`, etc. can be added in later milestones.

```csharp
public class Todo
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsComplete { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

## 2. API contract

Base route: `/api/todos`. JSON request/response bodies.

| Verb | Path | Purpose | Success | Errors |
|------|------|---------|---------|--------|
| `POST` | `/api/todos` | Create a todo | `201 Created` + the created todo | `400` if Title missing/empty |
| `GET` | `/api/todos` | List all todos | `200 OK` + array | — |
| `GET` | `/api/todos/{id}` | Get one todo | `200 OK` + todo | `404` if not found |
| `PUT` | `/api/todos/{id}` | Update a todo (full replace) | `204 No Content` | `400` invalid; `404` if not found |
| `DELETE` | `/api/todos/{id}` | Delete a todo | `204 No Content` | `404` if not found |

### Request/response shape

**Decision:** Reuse the `Todo` model directly for request and response bodies
(no separate DTOs in M1).

**Consequence — server-controlled fields:** On `POST`, the server **ignores** any
`Id` and `CreatedAt` sent by the client and assigns them itself (`Id` = next number,
`CreatedAt` = current UTC time). This keeps creation correct even though the client
sends a full `Todo` object.

> Future improvement: introduce request DTOs (e.g. `CreateTodoRequest { Title }`)
> so clients can't send server-controlled fields at all. Deferred for simplicity.

### Examples

**Create** — `POST /api/todos`
```json
// request
{ "title": "Buy milk" }
// response 201
{ "id": 1, "title": "Buy milk", "isComplete": false, "createdAt": "2026-06-28T07:30:00Z" }
```

**Update** — `PUT /api/todos/1` (full replace)
```json
// request
{ "title": "Buy oat milk", "isComplete": true }
// response 204 (no body)
```

**Update style decision:** `PUT` (full replace) — client sends the full set of
editable fields; server replaces them. `PATCH` (partial update) deferred.

---

## 3. Storage design

Endpoints do **not** access storage directly. They depend on a repository
**interface**, so the storage implementation can be swapped (in-memory now, a
database in Milestone 2) without changing controller code.

```csharp
public interface ITodoRepository
{
    IEnumerable<Todo> GetAll();
    Todo? GetById(int id);
    Todo Add(Todo todo);      // assigns Id + CreatedAt, returns the stored todo
    bool Update(int id, Todo todo);  // returns false if not found
    bool Delete(int id);             // returns false if not found
}
```

**M1 implementation:** `InMemoryTodoRepository`
- Stores todos in memory (e.g. a `Dictionary<int, Todo>`).
- Maintains a counter for the next `Id`.
- Registered in DI as a **singleton** so data persists for the app's lifetime
  (until restart — that's the M1 limitation that M2 fixes).

**Why an interface:** this is the key design move. In M2 we add an
`EfTodoRepository` (database-backed) and change one line of DI registration —
the controller stays identical. Teaches dependency inversion.

---

## 4. Out of scope for M1

- Persistence across restarts (→ Milestone 2)
- Frontend UI (→ Milestone 3)
- Authentication, paging, filtering (→ Milestone 4)
- Request DTOs and `PATCH` (possible future improvements)

---

## 5. Ticket mapping

| Ticket | Covered by |
|--------|-----------|
| TODOAPP-8 | §1 Data model |
| TODOAPP-9 | §3 Storage design |
| TODOAPP-10 | `POST` in §2 |
| TODOAPP-11 | `GET` list + by id in §2 |
| TODOAPP-12 | `PUT` in §2 |
| TODOAPP-13 | `DELETE` in §2 |
