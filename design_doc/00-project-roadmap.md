# Project Roadmap

The todo web app is built in **4 milestones**, each a working, demonstrable increment.
After every milestone the app runs and does more than before — no half-built states.

```
M1: Todo REST API        →  M2: Persistent storage  →  M3: Frontend UI  →  M4: Quality & polish
   (in-memory)               (real database)            (the web app)        (auth, deploy, CI)
```

## Milestones

| # | Milestone | What you get | Key learning |
|---|-----------|--------------|--------------|
| **M1** | **Todo REST API** *(in-memory)* | Backend API — create/list/get/update/delete todos, data held in memory | Web API basics, controllers, routing, HTTP verbs, status codes, testing |
| **M2** | **Persistent storage** | Same API, but todos survive restarts — backed by a real database (SQLite + EF Core) | Databases, ORM, migrations, data access |
| **M3** | **Frontend UI** | A web page that talks to the API — add/complete/delete todos in a browser | HTTP from a UI, JSON, basic frontend, wiring front + back |
| **M4** | **Quality & polish** | Production-ish concerns: user accounts/auth, validation hardening, error handling, deployment, CI | Auth, security basics, CI/CD, deployment |

## Why this order

- **M1 first (in-memory):** learn the API shape without database complexity. Fastest path to "it works."
- **M2 next:** swap the in-memory store for a real DB — shows *why* the storage interface was designed to be swappable (an M1 decision).
- **M3:** build the UI only once the API is stable and persistent.
- **M4:** harden last — auth, deployment, CI — once the core works end to end.

## Status & tracking

- Each milestone = a **Jira Epic**.
- **M1** Epic: **TODOAPP-7** — tickets TODOAPP-8 (model), TODOAPP-9 (in-memory storage),
  TODOAPP-10 (create), TODOAPP-11 (list & get), TODOAPP-12 (update), TODOAPP-13 (delete).
  (TODOAPP-6 = project scaffold.)
- M2–M4 Epics are created when we reach them.

## Caveat

This is a **plan, not a commitment** — a learning project. We can stop at any milestone,
reorder, or expand. Many people stop after M3 (a fully working app) and treat M4 as optional.

## Tech stack

- **.NET 9** Web API (controller-based)
- Repo: `peiranyu/todo-web`, source under `src/TodoApi`
- Database (from M2): SQLite + EF Core
