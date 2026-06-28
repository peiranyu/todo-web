# Design Docs

This folder holds **all design-related documentation** for the todo web app.

These docs cover the *what* and *how* of the project — the roadmap, data models,
API contracts, storage design, and architectural decisions — separate from the
source code in [`../src`](../src).

## Process

Design docs follow the same lightweight SDLC as the code: we write/update a design
doc during the **Design** phase of each feature or milestone, before implementation,
and changes go through a pull request like any other change.

## Index

| Doc | Description | Status |
|-----|-------------|--------|
| [00-project-roadmap.md](00-project-roadmap.md) | Big-picture roadmap — the 4 milestones | ✅ Done |
| [01-milestone-1-rest-api.md](01-milestone-1-rest-api.md) | Milestone 1 design — Todo model, API contract, in-memory storage | ✅ Done |
| `02-milestone-2-persistence.md` | Milestone 2 design — database & persistence | 🔜 Future |
| `03-milestone-3-frontend.md` | Milestone 3 design — frontend UI | 🔜 Future |
| `04-milestone-4-quality.md` | Milestone 4 design — auth, deployment, CI/CD | 🔜 Future |

> Per-milestone design docs are written when we reach that milestone's Design phase,
> not all up front — details change as we learn.
