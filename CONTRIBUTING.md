# Contributing to BrainDump

Thank you for taking the time to contribute. The following guidelines help keep the codebase consistent and the review process efficient.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [How to Report a Bug](#how-to-report-a-bug)
- [How to Request a Feature](#how-to-request-a-feature)
- [Development Workflow](#development-workflow)
- [Pull Request Checklist](#pull-request-checklist)
- [Architecture Constraints](#architecture-constraints)
- [Commit Style](#commit-style)

## Code of Conduct

This project follows the [Contributor Covenant Code of Conduct](CODE_OF_CONDUCT.md). By participating you agree to abide by its terms.

## Getting Started

1. Fork the repository and clone your fork.
2. Follow the setup steps in [README.md](README.md#getting-started).
3. Create a feature branch from `main`: `git checkout -b feat/my-change`.

## How to Report a Bug

Open an issue using the **Bug Report** template. Include:

- A clear description of the unexpected behavior
- Steps to reproduce it
- Expected vs. actual behavior
- Environment details (.NET version, Node version, browser, OS)

Do **not** include secrets, connection strings, or personal access tokens in issues.

## How to Request a Feature

Open an issue using the **Feature Request** template. Describe the problem you are trying to solve, not just the solution you have in mind. If the feature touches the API contract or database schema, link to or draft the relevant L1/L2 requirement update.

## Development Workflow

### Backend

- All business logic lives in `BrainDump.Application`. Keep `BrainDump.Api` thin (controllers delegate to MediatR, nothing else).
- Every new command or query gets a corresponding unit test in `BrainDump.UnitTests`.
- Integration tests in `BrainDump.IntegrationTests` must run against a real SQL Server instance; do not use in-memory providers.
- MediatR must remain at v12.x. Do not upgrade to v13+.

### Frontend

- Components use Angular Material. Do not introduce a second component library.
- Fact text editing must go through the Monaco editor instance; do not use plain `<textarea>` elements for that purpose.
- All layouts must remain usable from 360 px through 1920 px (no horizontal scroll, no clipped controls).

### Database

- Schema changes must be delivered as EF Core migrations checked into `BrainDump.Infrastructure`.
- Never add SQL authentication logins for application access; always use managed identity.

## Pull Request Checklist

Before marking a PR ready for review, confirm:

- [ ] `dotnet build backend/BrainDump.sln` succeeds with zero warnings in CI mode
- [ ] `dotnet test backend/BrainDump.sln` passes (unit and integration)
- [ ] `ng build` produces a clean production build
- [ ] New endpoints are covered by at least one integration test
- [ ] New Angular components render without console errors at 360 px and 1440 px viewports
- [ ] No client secret or credential appears in any committed file
- [ ] The PR description explains *why* the change is being made, not just *what* changed

## Architecture Constraints

| Constraint | Rule |
|---|---|
| MediatR version | Must satisfy `[12.0.0, 13.0.0)` |
| Project dependencies | `Domain` → nothing; `Application` → `Domain` only |
| Auth | JWT bearer validated against Entra ID; no SQL logins for app access |
| DB encryption | TDE on (default for Azure SQL); TLS 1.2+ enforced on all connections |

## Commit Style

Use [Conventional Commits](https://www.conventionalcommits.org/):

```
feat(api): add sibling reorder endpoint
fix(frontend): prevent horizontal scroll at 360px viewport
docs: update API reference table in README
chore(deps): pin MediatR to 12.4.1
```

Scope is optional but encouraged for larger areas (`api`, `frontend`, `db`, `auth`).
