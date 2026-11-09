# Copilot instructions for this repository

Keep this file current: update it whenever the architecture, conventions or commands change (AI-02).

## What we are building

Building Operations Platform: a web app for the day-to-day operations of residential buildings.

- **Roles:** Administrator, Building Manager, Resident (Tenant or Owner), Technician.
- **Modules:** M1 identity and access, M2 buildings and 2D floor map, M3 residents and leases, M4 maintenance work orders, M5 meters, billing and payments, M6 announcements and notifications, M7 audit, dashboards and reporting.
- **Demo data** must include Aurora Residence, unit 4B on floor 4, and the personas Ana (Resident), Mihai (Building Manager), Ioana (Technician) and Radu (Administrator).

## Architecture

- **Modular monolith.**
- **`src/web`** (Next.js 16, React 19, TypeScript) talks **only** to the API over HTTPS/JSON. Never access the database from the web app, including server components and server functions.
- **`src/api`** (ASP.NET Core, .NET 10):
  - Feature code goes in `Modules/<Module>`: Buildings, Residents, Leases, Maintenance, Billing, Notifications, Audit.
  - Cross-cutting code goes in `Shared/`: Auth, Validation, Caching, Persistence, Telemetry.
- **Business rules** (state machines, billing allocation, lease overlap, authorization relationships) live in the API and domain, never only in the interface.
- **Database:** PostgreSQL 18 through EF Core 10 and the Npgsql provider. Constraints, functions, views and triggers are added as SQL inside migrations.
- **Cache:** Valkey through HybridCache.

## Commands

| Task | Command |
|---|---|
| Build | `dotnet build BuildingOps.slnx` |
| .NET tests | `dotnet test BuildingOps.slnx` |
| Run API | `dotnet run --project src/api` |
| Web (in `src/web`) | `npm ci`, `npm run dev`, `npm run lint`, `npm run build` |
| Database and cache | `podman compose up -d db cache` |
| Whole stack | `podman compose up --build` |

## Conventions

- English for interface text, code, comments, docs, commits and pull requests.
- **API style:**
  - Routes under `/api/v1/`, with plural, kebab-case nouns, for example `/api/v1/work-orders`.
  - Server-side pagination (`page`, `pageSize`), filtering and sorting (`sort=-dueAt`).
  - Errors are RFC 9457 ProblemDetails.
  - `If-Match` with ETags for updates, and `Idempotency-Key` for payments.
- **Money:** C# `decimal` and PostgreSQL `numeric`, always with an ISO currency code (default RON). Never `float` or `double`.
- **Time:** store UTC (`timestamptz`), display in the user's time zone (default Europe/Bucharest), and use ISO 8601 in the API.
- **History:** records with history are deactivated or archived, never hard-deleted.
- **Commits:** Conventional Commits with a work item, for example `feat: add unit list endpoint (#42)`.
- **Branches:** `<type>/<alias>/<item>-<description>`.

## Security rules

- Every API operation enforces object-level authorization: the role **and** the relationship to the resource.
- Access tokens live only in HttpOnly, Secure, SameSite cookies, never in `localStorage` or `sessionStorage`.
- Internal work notes are never returned by resident-facing endpoints.
- No secrets in code or Git. Use `.env` (git-ignored), .NET user secrets or the pipeline secret store.
- Synthetic data only. Validate uploads (type and size).

## Allowed technologies (ARC-10)

- **Allowed:** Microsoft products, open source under an OSI-approved licence, and the approved exceptions MongoDB Community Server and Dragonfly.
- **Do not suggest:**
  - Redis
  - Terraform or OpenTofu
  - Docker Desktop
  - MediatR 13+ or AutoMapper 15+
  - FluentAssertions 8+
  - ImageSharp, QuestPDF, Duende IdentityServer or NBomber
  - Commercial UI component suites
- **Before adding any package,** check that it exists, is the intended package, is maintained and has an allowed licence (AI-06).

## Definition of Done

- Acceptance criteria met; major features have a spec in `docs/specs/`.
- Tests added and passing, and they fail when the behaviour is broken.
- CI green; no new Critical or High vulnerabilities.
- Interface handles loading, empty, error and success states, and is keyboard accessible.
- Docs, ADRs and this file updated when architecture or conventions change.
- Pull request approved by another person; AI assistance disclosed.
