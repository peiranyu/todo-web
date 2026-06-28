// Frontend for the Todo API (Milestone 3).
// Plain vanilla JS — talks to the same-origin REST API at /api/todos with fetch().
// See design_doc/03-milestone-3-frontend.md.

const API = "/api/todos";

const form = document.getElementById("add-form");
const input = document.getElementById("new-title");
const list = document.getElementById("todo-list");
const emptyMsg = document.getElementById("empty");
const errorMsg = document.getElementById("error");

// --- API helpers --------------------------------------------------------

async function getTodos() {
  const res = await fetch(API);
  if (!res.ok) throw new Error(`Failed to load todos (${res.status})`);
  return res.json();
}

async function createTodo(title) {
  const res = await fetch(API, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ title }),
  });
  if (!res.ok) throw new Error(`Failed to add todo (${res.status})`);
}

async function updateTodo(todo, isComplete) {
  // PUT is a full replace: send the existing title plus the new isComplete.
  const res = await fetch(`${API}/${todo.id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ title: todo.title, isComplete }),
  });
  if (!res.ok) throw new Error(`Failed to update todo (${res.status})`);
}

async function deleteTodo(id) {
  const res = await fetch(`${API}/${id}`, { method: "DELETE" });
  if (!res.ok) throw new Error(`Failed to delete todo (${res.status})`);
}

// --- Rendering ----------------------------------------------------------

function showError(message) {
  errorMsg.textContent = message;
  errorMsg.hidden = false;
}

function clearError() {
  errorMsg.hidden = true;
}

function renderTodo(todo) {
  const li = document.createElement("li");
  li.className = "todo-item" + (todo.isComplete ? " complete" : "");

  const checkbox = document.createElement("input");
  checkbox.type = "checkbox";
  checkbox.checked = todo.isComplete;
  checkbox.addEventListener("change", async () => {
    try {
      clearError();
      await updateTodo(todo, checkbox.checked);
      await refresh();
    } catch (err) {
      showError(err.message);
      await refresh();
    }
  });

  const title = document.createElement("span");
  title.className = "title";
  title.textContent = todo.title;

  const del = document.createElement("button");
  del.type = "button";
  del.className = "delete";
  del.textContent = "Delete";
  del.addEventListener("click", async () => {
    try {
      clearError();
      await deleteTodo(todo.id);
      await refresh();
    } catch (err) {
      showError(err.message);
    }
  });

  li.append(checkbox, title, del);
  return li;
}

function render(todos) {
  list.innerHTML = "";
  emptyMsg.hidden = todos.length > 0;
  for (const todo of todos) {
    list.appendChild(renderTodo(todo));
  }
}

// --- Flow ---------------------------------------------------------------

async function refresh() {
  try {
    const todos = await getTodos();
    render(todos);
  } catch (err) {
    showError(err.message);
  }
}

form.addEventListener("submit", async (e) => {
  e.preventDefault();
  const title = input.value.trim();
  if (!title) return; // ignore empty titles
  try {
    clearError();
    await createTodo(title);
    input.value = "";
    input.focus();
    await refresh();
  } catch (err) {
    showError(err.message);
  }
});

// Initial load.
refresh();
