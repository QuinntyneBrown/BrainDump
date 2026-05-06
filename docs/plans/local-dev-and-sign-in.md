# Plan: Implement L1-012 — L1-014

Implementation plan for the three local-development requirements added to the spec:

- **L1-012** Containerized Local Development Environment
- **L1-013** Microsoft SQL Server in Docker for Local Development
- **L1-014** Local Sign-In for Development

## Current state

| Concern | State |
|---|---|
| Local sign-in (L1-014) | **Mostly built.** `AuthController` exposes PKCE-style `POST /api/auth/authorize` + `POST /api/auth/token`. The JWT bearer pipeline in `Program.cs` switches between local symmetric-key validation and production OIDC based on `Jwt:UseLocalAuth`. Frontend `AuthService` already drives the two-step flow. |
| SQL Server in Docker (L1-013) | **Not built.** Local dev currently runs Sqlite (via the `Database:Provider` switch). No Compose file exists. |
| Docker Compose (L1-012) | **Not built.** No `docker-compose.yml` at repo root. No Dockerfiles for `api` or `web`. |
| Production-safety guard (L2-032) | **Missing.** `Jwt:UseLocalAuth=true` will silently work outside Development; spec requires startup to fail. |

## Spec/code gap to resolve first

L2-031 specifies `POST /api/auth/login` returning a token directly. The existing implementation uses PKCE-style two-step (`/authorize` + `/token`). Pick one before implementing:

- **(A) Update the spec to match the code.** Rewrite L2-031 to describe the existing two-step flow. More consistent with L1-011 (production also uses PKCE).
- **(B) Update the code to match the spec.** Replace the two endpoints with a single `/api/auth/login`. Simpler, but breaks the existing frontend `AuthService` and diverges from L1-011's PKCE intent.

**Recommendation: (A).** The existing flow already works end-to-end and aligns with L1-011.

## Phases

### Phase 1 — Production-safety guard (L2-032)

Smallest change, closes a real gap, do first.

- In `backend/src/BrainDump.Api/Program.cs`, after reading `Jwt:UseLocalAuth`, throw at startup if it is `true` and `builder.Environment.IsDevelopment()` is false.
- Add an explicit `Jwt:UseLocalAuth: false` to `appsettings.json` for clarity (currently absent, resolves to false).
- Add an integration test asserting host construction throws under `ASPNETCORE_ENVIRONMENT=Production` with `UseLocalAuth=true`.

**Acceptance:** L2-032 #1, #2, #3 pass.

### Phase 2 — SQL Server in Docker (L1-013, L2-029, L2-030)

- Add `sqlserver` service to the (yet-to-be-created) root `docker-compose.yml`:
  - Image `mcr.microsoft.com/mssql/server:2022-latest`
  - Env `ACCEPT_EULA=Y`, `MSSQL_SA_PASSWORD` from env with documented dev default
  - Port `1433:1433`
  - Named volume `mssql-data:/var/opt/mssql`
  - Healthcheck polling `sqlcmd -Q "SELECT 1"`
- Update `backend/src/BrainDump.Api/appsettings.Development.json`:
  - `Database:Provider` from `Sqlite` to `SqlServer`
  - `ConnectionStrings:DefaultConnection` referencing the Compose service name: `Server=sqlserver,1433;Database=BrainDump;User Id=sa;Password=...;TrustServerCertificate=true`
- Keep the Sqlite branch in `DependencyInjection.cs` as an opt-in fallback for developers who prefer file-based dev.
- Confirm L2-030: production manifests still use Entra ID; SQL credentials live only in the Compose file and developer-local config.

**Acceptance:** L2-029 #1–#5, L2-030 #1–#2 pass.

### Phase 3 — Compose for api + web (L1-012, L2-027, L2-028)

- Add `backend/src/BrainDump.Api/Dockerfile`: multi-stage `dotnet/sdk` build → `dotnet/aspnet` runtime.
- Add `frontend/Dockerfile.dev`: runs `npx ng serve --host 0.0.0.0`; source mounted as a volume for hot reload.
- Add root `docker-compose.yml` with three services on a shared user-defined network:
  - `sqlserver` (from Phase 2)
  - `api` — depends on `sqlserver` healthy; connection string references `sqlserver` by service name
  - `web` — depends on `api`
- Add `/healthz` to the API: `MapHealthChecks("/healthz")` + `AddHealthChecks().AddDbContextCheck<AppDbContext>()` so L2-028 #2 has a real target.
- Validate with `docker compose config` and a manual `docker compose up` smoke test against L2-028 #1–#4.

**Risk note:** Angular dev server in Docker is fiddly on Windows — file watching across the host/container boundary is the usual snag. If painful, fall back to Compose with `sqlserver` + `api` only and keep `web` on the host. This would partially miss L2-027 #2 and L2-028 #3 — confirm with stakeholders before accepting that trade.

**Acceptance:** L2-027 #1–#4, L2-028 #1–#4 pass.

### Phase 4 — Acceptance tests (ATDD)

For each new L2, add tests under `backend/tests/BrainDump.IntegrationTests` (or a new project for Compose-level checks). Each test file leads with a `// Traces to: L2-XXX` header.

| Test target | L2 | What it asserts |
|---|---|---|
| Compose file structure | L2-027 | Parse `docker-compose.yml`; assert `sqlserver`, `api`, `web` services with expected images/ports |
| Container boot | L2-029 #5 | Connection test against the running SQL Server container |
| Local sign-in happy path | L2-031 #1, #4 | `WebApplicationFactory` POST valid creds → 200 + token; token works on `/api/tree` |
| Local sign-in failures | L2-031 #2, #3 | Wrong creds → 401; malformed body → 400 |
| Endpoint absence when disabled | L2-031 #5 | With `UseLocalAuth=false`, POST to login → 404 |
| Production startup safety | L2-032 #3 | Host construction throws when `UseLocalAuth=true` outside Development |

### Phase 5 — Docs

- Update `README.md` quick-start: `docker compose up`, browse to `http://localhost:4200`, dev creds `user@braindump.dev` / `Password1!`.
- Decide whether `eng/scripts/start.bat` should be retired or rewritten as a thin wrapper around `docker compose up`.

## Order of work and rationale

1. **Phase 1 first** — single-file change, no infra, closes a production-safety gap.
2. **Settle the L2-031 A/B question** before Phase 4, since it changes the test surface.
3. **Phase 2 before Phase 3** — confirm SQL Server container works in isolation before layering api/web on top.
4. **Phase 3** is the highest-risk phase (see Windows file-watching note above).
5. **Phase 4 + 5** can interleave with Phases 1–3 as each phase lands, rather than batched at the end.

## Out of scope for this plan

- EF Core migrations (still using `EnsureCreatedAsync` per the bootstrap comment in `Program.cs`).
- Production Compose / Kubernetes manifests — those continue to track L1-004 (Azure SQL) and L2-011 (Entra ID).
- Multi-user local accounts — local sign-in remains a single configured dev user.
