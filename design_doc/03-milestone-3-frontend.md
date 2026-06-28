# Milestone 3 Design — Frontend UI

**Epic:** TODOAPP-15
**Tasks:** TODOAPP-18 (design), TODOAPP-19 (implementation)
**Goal:** A simple frontend UI — plain HTML + CSS + JavaScript (no framework, no
build tools) — served by the API itself, letting the user add, list,
toggle-complete, and delete todos by calling the existing REST API with `fetch()`.

This document is the **Design** phase output for Milestone 3. It defines the page
layout, the fetch-to-endpoint mapping, the file structure, and the one Program.cs
change before implementation.

---

## 1. Approach & constraints

- **No framework, no build step.** Plain `index.html`, `css/styles.css`, `js/app.js`.
  Keeps the project simple and dependency-free; the browser loads the files directly.
- **Served by the API.** The same .NET app serves both the JSON API (`/api/todos`)
  and the static frontend (from `wwwroot/`). Because the page is served from the
  same origin as the API, `fetch()` calls use relative URLs (`/api/todos`) with no
  CORS configuration needed.
- **Do not change the API.** The REST contract from Milestone 1 is reused as-is.

---

## 2. Page layout

A single page (`index.html`):

```
+------------------------------------------+
|              My Todos (h1)               |
|                                          |
|  [ text input: "What needs doing?" ] [Add]|
|                                          |
|  <ul id="todo-list">                     |
|   [x] Buy milk                  [Delete] |   <- checkbox toggles complete
|   [ ] Walk the dog              [Delete] |
|   [x] ~~Pay rent~~ (strikethrough)[Delete]|
|  </ul>                                   |
+------------------------------------------+
```

Elements:
- An `<h1>` title.
- An add row: a text `<input>` + an "Add" `<button>`.
- A `<ul id="todo-list">` container. Each todo renders as an `<li>` with:
  - a **checkbox** (checked = `isComplete`), which toggles completion;
  - the **title** text (shown with strikethrough when complete);
  - a **Delete** button.
- Completed todos are shown visually distinct (strikethrough + muted colour).

---

## 3. Fetch calls mapped to endpoints

All requests use the same-origin relative path `/api/todos`. JSON is camelCase
(`id`, `title`, `isComplete`, `createdAt`), matching the API.

| User action | fetch call | Endpoint | Notes |
|-------------|-----------|----------|-------|
| Page load / refresh list | `fetch('/api/todos')` | `GET /api/todos` → 200 + array | Render each todo as an `<li>`. |
| Add a todo | `fetch('/api/todos', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ title }) })` | `POST /api/todos` → 201 | Skip if title is empty/whitespace. After success, clear input and re-render. |
| Toggle complete | `fetch('/api/todos/' + id, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ title, isComplete: newValue }) })` | `PUT /api/todos/{id}` → 204 | Full replace: must send the existing `title` plus the new `isComplete`. |
| Delete a todo | `fetch('/api/todos/' + id, { method: 'DELETE' })` | `DELETE /api/todos/{id}` → 204 | After success, re-render. |

**Re-render strategy:** for simplicity, after any mutating action (add / toggle /
delete) we re-fetch the full list and re-render. This keeps the UI in sync with the
server with minimal client-side state. (A future optimisation could update the DOM
in place, but it is unnecessary at this scale.)

**Empty-title handling:** the Add handler trims the input and does nothing if it is
empty, so the API's 400 path is avoided from the UI.

---

## 4. File structure under `wwwroot/`

ASP.NET Core serves static files from the `wwwroot` folder by default.

```
src/TodoApi/
  wwwroot/
    index.html        # page markup: title, add row, <ul id="todo-list">
    css/
      styles.css      # simple, clean styling
    js/
      app.js          # fetch logic: load, add, toggle, delete, render
```

- `index.html` links `css/styles.css` and `js/app.js` with relative paths.
- `app.js` runs on load (e.g. on `DOMContentLoaded`), fetches the list, and wires up
  the Add button + input.

---

## 5. Program.cs change (serve static files)

ASP.NET Core does not serve static files by default. Two middleware calls enable it:

- `app.UseDefaultFiles();` — rewrites a request for `/` to `/index.html`.
- `app.UseStaticFiles();` — serves files from `wwwroot`.

**Ordering matters:** `UseDefaultFiles()` must run **before** `UseStaticFiles()`, and
both must run early in the pipeline (before `UseHttpsRedirection` / `MapControllers`).

These two lines are added together immediately after `var app = builder.Build();`
and before `app.UseHttpsRedirection();`:

```csharp
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// ... existing pipeline (UseHttpsRedirection, UseAuthorization, MapControllers) ...
```

This is the **only** change to the backend. No controller, model, or repository code
changes; the API contract is untouched.

> Note: another developer is editing Program.cs in parallel for Milestone 2 storage.
> This change is intentionally kept to these two lines to minimise merge conflicts.

---

## 6. Out of scope for M3

- Inline editing of a todo's title (only toggle-complete + delete in the UI).
- Client-side routing, filtering, sorting, or paging.
- Build tooling, bundlers, TypeScript, or a frontend framework.
- Authentication (→ Milestone 4).

---

## 7. Ticket mapping

| Ticket | Covered by |
|--------|-----------|
| TODOAPP-18 | This design doc (§2–§5) |
| TODOAPP-19 | `wwwroot/index.html`, `wwwroot/css/styles.css`, `wwwroot/js/app.js`, Program.cs static-files change |
