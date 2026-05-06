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

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) and npm 10+
- [Angular CLI 21](https://angular.dev/tools/cli) (`npm install -g @angular/cli`)
- SQL Server (LocalDB, Docker, or Azure SQL) for integration tests
- A Microsoft Entra ID (Azure AD) tenant for authentication

## Getting Started

### Backend

```bash
# Restore and build
dotnet build backend/BrainDump.sln

# Run all tests
dotnet test backend/BrainDump.sln

# Run the API (configure appsettings.Development.json first)
dotnet run --project backend/src/BrainDump.Api
```

### Frontend

```bash
cd frontend
npm install
npm start        # ng serve — http://localhost:4200
npm test         # ng test
npm run build    # production build
```

### Configuration

Copy `backend/src/BrainDump.Api/appsettings.Development.json.example` (once provided) and supply:

- `ConnectionStrings:BrainDump` — your SQL Server / Azure SQL connection string
- `AzureAd:TenantId`, `AzureAd:ClientId` — your Entra ID app registration details

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
