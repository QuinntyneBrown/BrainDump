# BrainDump

A personal knowledge-management tool that stores a hierarchical tree of sections and facts, designed to mirror a canonical Markdown document that can always be reconstructed from the application state.

## Overview

BrainDump provides a structured editor for capturing and organizing declarative knowledge. Facts live inside named sections, sections nest arbitrarily deep, and sibling ordering is preserved deterministically. The UI embeds the Monaco editor for editing fact text, making it feel like a developer-grade notebook rather than a generic note-taking app.

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Angular 21, Angular Material, Monaco Editor |
| Backend | .NET 9, Clean Architecture, MediatR v12 |
| Database | Azure SQL Database (TDE, Entra ID auth, TLS 1.2+) |
| Auth | OAuth 2.0 Authorization Code + PKCE (Microsoft Entra ID) |

## Repository Layout

```
brain-dump/
├── backend/
│   ├── src/
│   │   ├── BrainDump.Domain/
│   │   ├── BrainDump.Application/
│   │   ├── BrainDump.Infrastructure/
│   │   └── BrainDump.Api/
│   ├── tests/
│   │   ├── BrainDump.UnitTests/
│   │   └── BrainDump.IntegrationTests/
│   └── BrainDump.sln
├── frontend/           # Angular workspace
├── docs/
│   ├── specs/          # L1 and L2 requirements
│   └── user-interface-designs/
└── README.md
```

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) with Docker Compose v2 — the canonical local-development path runs every dependency in containers (L1-012).
- For host-side iteration on backend or frontend code (alternative to Compose), you'll additionally want:
  - [.NET 9 SDK](https://dotnet.microsoft.com/download)
  - [Node.js 20+](https://nodejs.org/) and npm 10+
  - [Angular CLI 21](https://angular.dev/tools/cli) (`npm install -g @angular/cli`)
- A Microsoft Entra ID (Azure AD) tenant — only required for **deployed** environments. Local development uses an in-app dev sign-in that is disabled outside `Development` (see [L1-014](docs/specs/L1.md)).

## Getting Started

### Quick start (Docker Compose)

```bash
# 1. Copy the env template — picks up the default SQL Server SA password.
cp .env.example .env

# 2. Bring everything up (sqlserver → api → web).
docker compose up

# 3. Open the app and sign in with the development credentials below.
#    http://localhost:4200
```

| Service | Host port | Container port |
|---|---|---|
| Angular dev server (`web`) | 4200 | 4200 |
| API (`api`) | 5153 | 8080 |
| SQL Server (`sqlserver`) | 1433 | 1433 |

**Dev credentials** (configured in `appsettings.Development.json` under `Jwt:LocalAuth`):

- Email: `user@braindump.dev`
- Password: `Password1!`

These credentials drive a local PKCE sign-in flow (`POST /api/auth/authorize` + `POST /api/auth/token`) that is **only** registered when `Jwt:UseLocalAuth=true`. The application refuses to start with that flag enabled in any non-`Development` environment (see [L2-032](docs/specs/L2.md)).

The `eng/scripts/start.bat` and `eng/scripts/stop.bat` helpers are thin wrappers around `docker compose up` / `docker compose down` for click-to-run on Windows.

To inspect logs, follow with `docker compose logs -f api` (or `web`, `sqlserver`). To wipe the SQL Server volume and reset to an empty database, use `docker compose down -v`.

### Host development (alternative)

If you'd rather run the backend or frontend on the host (faster startup, easier debugger attach), keep the SQL Server container up and run the rest natively:

```bash
# Just the database
docker compose up -d sqlserver

# Backend on the host (will connect to localhost:1433)
dotnet build backend/BrainDump.sln
dotnet test  backend/BrainDump.sln
dotnet run --project backend/src/BrainDump.Api

# Frontend on the host
cd frontend
npm install
npm start        # ng serve — http://localhost:4200
```

`appsettings.Development.json` already targets `Server=localhost,1433` with the same SA password as `.env.example`, so host-run and Compose-run share the same database without further configuration.

### Configuration

`appsettings.Development.json` ships with everything needed for a local run, including the dev sign-in keys. Production requires (and only accepts) the following:

- `ConnectionStrings:DefaultConnection` — Azure SQL connection string (passwordless / Entra ID, no password). See [L2-011](docs/specs/L2.md).
- `Jwt:Authority`, `Jwt:Audience` — your Entra ID tenant + app registration. `Jwt:UseLocalAuth` must be `false` (the default).

## API Reference

| Method | Path | Description |
|---|---|---|
| `GET` | `/api/tree` | Returns all sections and facts in one response |
| `POST` | `/api/sections` | Create a section |
| `PUT` | `/api/sections/{id}` | Update a section |
| `DELETE` | `/api/sections/{id}` | Delete a section (cascades to descendants and facts) |
| `POST` | `/api/facts` | Create a fact |
| `PUT` | `/api/facts/{id}` | Update a fact |
| `DELETE` | `/api/facts/{id}` | Delete a fact |
| `POST` | `/api/reorder` | Batch-update sibling positions |

All endpoints require a valid JWT bearer token issued by the configured identity provider.

## Architecture Notes

The backend follows Clean Architecture dependency rules:

```
Api → Application ← Domain
Api → Infrastructure → Application
```

`BrainDump.Domain` has no project references. `BrainDump.Application` references only Domain. `BrainDump.Infrastructure` wires up EF Core / Dapper and Azure SQL. `BrainDump.Api` is the executable host.

MediatR is pinned to v12.x (Apache 2.0 license). v13+ requires a commercial license and must not be introduced.

## Performance Targets

| Operation | p95 budget |
|---|---|
| Full-tree read (sections + facts) | < 100 ms at the database |
| Full-tree endpoint response | < 300 ms end-to-end |
| Any single-row write | < 50 ms |
| First query after idle (no cold start) | < 500 ms |

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md).

## Security

See [SECURITY.md](SECURITY.md) for the vulnerability disclosure policy.

## License

[MIT](LICENSE)
