# ADR-0001: Use PostgreSQL 18 as the platform database

| Field | Value |
|---|---|
| Status | Proposed (change to **Accepted** after team review) |
| Date | 2026-11-08 |
| Author | Utkarsh Gupta (DB track) |
| Deciders | [Add team member names] |
| Requirements | DB-C01, ARC-07, ARC-10; supports DB-C02 to DB-C09, M3-04, M3-05, M5-07, M6-05, M7-02, PX-04, PX-05 |

## 1. Context

We are building the Building Operations Platform: an ASP.NET Core (.NET 10) API and a React 19 / Next.js web app. Only the API talks to the database (ARC-04). We must choose one database engine, and decide where it runs in each environment.

**What our data looks like**

- **Highly connected.**
  - Buildings have floors, and floors have units and common areas.
  - Persons are linked to units through ownership, leases and household membership.
  - Work orders belong to a unit or a common area. Each has a reporter, a technician, a status history, comments and attachments.
  - Billing runs produce statements, and statements have lines and payments.
  - Audit records can reference any entity.
  - Almost every screen (floor map, unit detail, dashboards) reads across several of these entities at once.
- **Hard business rules that must hold even when requests run at the same time:**
  - Two Active leases for the same unit must never overlap (M3-04), including when two activations race.
  - Money is exact: decimal types only, and allocations add up to the last cent (M5-07, PX-05).
  - A notification is written in the same transaction as the change that triggers it, or through an outbox (M6-05).
  - Audit records are append-only (M7-02), and issued statements are immutable (M5-06).
  - All timestamps are stored in UTC (PX-04).
- **Small volume.** The seed holds at least 2 buildings, 60 units, 100 persons, 12 months of readings and statements, and 300 work orders (PX-08). Performance depends on good indexes (DB-C05), not on scaling out.
- **Program constraints.**
  - Only Microsoft products, OSI-licensed open source, or the approved exceptions are allowed (ARC-10).
  - Local containers run on Podman or Rancher Desktop.
  - CI must apply migrations to a disposable database on every pull request (DB-C04, OPS-C03).
  - The DB track also requires programmable objects (DB-C06), a backup and restore drill (DB-C08) and least-privilege access (DB-C09).
  - Hosting is optional. Each participant has €150 of Azure credit per month.

## 2. Decision drivers

1. **Integrity and consistency.** Enforce the rules above in the database, safely under concurrent requests.
2. **Fit with our query patterns.** Joins and aggregations across many entities, and date-range questions such as "Who lived in unit 4B on 1 March?" (M3-05).
3. **Programmable objects.** Functions or procedures, views and triggers (DB-C06).
4. **Team skills and tooling.** C#, EF Core 10 and migrations, GitHub Copilot.
5. **Local and CI friendliness.** A light container that starts quickly on Podman and on GitHub Actions runners.
6. **Licence.** Must satisfy ARC-10.
7. **Operations and cost.** Simple backup and restore, TLS and least-privilege roles, and optional Azure hosting within budget.

## 3. Options considered

1. **PostgreSQL 18**: open-source relational database.
2. **SQL Server (Linux container) or Azure SQL Database**: Microsoft relational database.
3. **MongoDB Community Server or DocumentDB**: document databases. MongoDB is an approved source-available exception; DocumentDB is MIT-licensed and MongoDB-compatible.
4. **Azure Cosmos DB for NoSQL**: Microsoft's managed document database.

## 4. Comparison

Legend: ✅ strong fit · ⚠️ possible with extra work · ❌ poor fit

| Driver | PostgreSQL 18 | SQL Server / Azure SQL | MongoDB / DocumentDB | Cosmos DB for NoSQL |
|---|---|---|---|---|
| Lease overlap (M3-04), safe when requests race | ✅ Declarative exclusion constraint: the database itself rejects the second of two racing activations | ⚠️ No exclusion constraints. Needs a SERIALIZABLE transaction or explicit range locks, written and tested by hand | ⚠️ No range uniqueness. Needs a multi-document transaction plus a per-unit lock or version document | ⚠️ Same as MongoDB, and transactions are limited to one partition key |
| Exact money | ✅ `numeric` | ✅ `decimal` | ✅ `Decimal128` | ❌ No native decimal type; amounts would be stored as integer cents or strings |
| Joins, foreign keys, dashboards | ✅ | ✅ | ⚠️ `$lookup` only, no foreign keys | ❌ No joins across documents |
| Date ranges (occupancy history) | ✅ Native range types with GiST indexes | ⚠️ Start and end columns compared by hand | ⚠️ Compared by hand | ⚠️ Compared by hand |
| Programmable objects (DB-C06) | ✅ Functions, procedures, views, triggers | ✅ Same, plus temporal tables | ⚠️ Aggregation pipelines, views, change streams | ⚠️ JavaScript procedures and triggers limited to one partition; change feed |
| Multi-row transactions | ✅ Full ACID, all isolation levels | ✅ Full ACID | ⚠️ Require a replica set, also on laptops and in CI | ⚠️ Only within one partition key |
| EF Core 10 and migrations | ✅ Npgsql provider 10.x targets .NET 10 | ✅ Microsoft provider | ⚠️ No EF Core migrations; needs a schema-version field plus scripts | ⚠️ EF Core Cosmos provider; no migrations |
| Containers on Podman and in CI | ✅ Small official image for x64 and ARM64 | ⚠️ x64-only image that needs at least 2 GB of RAM | ✅ Official image, plus replica-set setup for transactions | ⚠️ The emulator differs from the cloud service |
| Licence (ARC-10) | ✅ PostgreSQL Licence (OSI-approved) | ✅ Microsoft product, but the Developer edition is for non-production use only and Express has size limits | ⚠️ MongoDB: SSPL exception with conditions. ✅ DocumentDB: MIT | ✅ Microsoft product |
| Azure hosting (optional) | ✅ Azure Database for PostgreSQL Flexible Server supports version 18 | ✅ Azure SQL Database | ✅ Azure DocumentDB | ✅ Native |
| Backup and restore drill (DB-C08) | ✅ `pg_dump` / `pg_restore` | ✅ `BACKUP` / `RESTORE` | ✅ `mongodump` / `mongorestore` | ⚠️ Managed backups; harder to drill into a fresh environment |

**Team skills.** [Add each member's experience with PostgreSQL, SQL Server and NoSQL.] Everyone works in C# and EF Core. EF Core code is almost identical for PostgreSQL and SQL Server, so the engine choice mostly affects the raw SQL owned by the DB track.

## 5. Decision

We will use **PostgreSQL 18** as the single database for all modules (M1 to M7). Only the API accesses it, through **EF Core 10** with the **Npgsql** provider.

- **Schema changes are EF Core migrations.** EF Core cannot model the lease exclusion constraint, functions, views, triggers or database roles. We add those as raw SQL inside migrations, so every change stays versioned and goes through CI.
- **We pin major version 18 everywhere.** PostgreSQL 19 is planned for general availability on 19 November 2026. We will not upgrade during the project.

**Where the database runs**

| Environment | Database |
|---|---|
| Local | `docker.io/library/postgres:18` container on Podman, started by `compose.yaml`, with data in a named volume |
| CI (every pull request) | A fresh PostgreSQL 18 service container on the GitHub Actions runner. Migrations and seed are applied, and the container is discarded after the run |
| Preview and Release (default, not hosted) | A PostgreSQL 18 container started by `compose.yaml` together with the published API and web images. It is seeded and backed up with `pg_dump` |
| Preview and Release (only if the hosting ADR chooses Azure) | Azure Database for PostgreSQL Flexible Server, version 18, smallest Burstable tier, in one participant's subscription. TLS required, the `btree_gist` extension allow-listed, budget alerts at 50 %, 80 % and 100 % |

## 6. How PostgreSQL meets the key rules

Table and column names below are illustrative. The final names come from the data model (DB-C02).

**Lease overlap (M3-04, DB-C03).** The database enforces the rule, so it holds even when two requests race:

```sql
CREATE EXTENSION IF NOT EXISTS btree_gist;

ALTER TABLE lease
  ADD CONSTRAINT ex_lease_active_no_overlap
  EXCLUDE USING gist (
    unit_id WITH =,
    daterange(start_date, end_date, '[]') WITH &&
  )
  WHERE (status = 'Active');
```

If a second, overlapping lease is activated, PostgreSQL raises error `23P01` (exclusion violation). The API maps that error to a ProblemDetails validation response (HTTP 409 or 422). An integration test fires two activations in parallel and checks that exactly one succeeds.

**Exact money (M5-07, PX-05).**
- Amounts are stored as `numeric(12,2)`, with a `char(3)` currency code (default RON).
- Unit prices and meter readings get more decimal places, for example `numeric(12,4)`.
- All of these map to C# `decimal`.
- We never use `real`, `double precision` or PostgreSQL's `money` type.

**UTC time (PX-04).** All timestamps use `timestamptz`. Npgsql accepts only UTC `DateTime` values for this type, which catches time-zone mistakes early.

**Atomic changes (M6-05, DB-C07).**
- The business change, its status-history row, the audit record and the outbox message are written in one transaction.
- The default isolation level is READ COMMITTED.
- Stronger locking (`SELECT … FOR UPDATE` or SERIALIZABLE) is used only where the transactions note justifies it, for example for billing approve/issue and payment application.
- Payment idempotency uses a unique constraint on the idempotency key.

**Append-only audit (M7-02).** The application role is granted only `INSERT` and `SELECT` on the audit table. `UPDATE` and `DELETE` are never granted.

**Programmable objects (DB-C06).** Planned candidates, each covered by automated tests:
- A billing-preview function.
- An occupancy-on-date function.
- An outstanding-balances view.
- A work-order status-history trigger.

**Secure access (DB-C09).**
- **Roles.** There are three:
  - `bop_migrator` owns the schema and is used only by the migration step.
  - `bop_app` is used by the API. It can read and write data but cannot change the schema, and it is never a superuser or owner.
  - `bop_readonly` is used for backups and reporting.
- **Passwords** come from GitHub secrets, a local `.env` file that is never committed, or Azure Key Vault if we host.
- **Connections** use `SSL Mode=Require` when hosted.

**Backup and restore (DB-C08).**
- We take a `pg_dump --format=custom` backup before every weekly release. If hosted on Azure, the service also takes automatic backups.
- The restore drill uses `pg_restore` into a fresh container. It is verified with row counts and an application smoke test.

## 7. Consequences

**Positive**

- The most important business rules are enforced by the database itself, not only by application code.
- One engine and one major version everywhere: laptop, CI, Preview, Release and optional Azure.
- Light, fast containers let every CI run use a fresh database.
- The database stack is fully open source, so there are no licence questions under ARC-10.

**Negative, and how we handle it**

| Risk | Mitigation |
|---|---|
| PostgreSQL's SQL dialect and PL/pgSQL are new to some of us | Small worked examples in the team prompt library (AI-09). Every AI-generated SQL statement is reviewed and tested |
| EF Core does not know about raw-SQL objects (exclusion constraint, functions, view, trigger) | Keep them in migrations. Integration tests run against a real PostgreSQL 18, never an in-memory provider |
| Npgsql is community-maintained, not a Microsoft package | It is mature, open source (PostgreSQL Licence) and supports EF Core 10. Versions are pinned and watched by Dependabot |
| PostgreSQL-specific features make switching engines harder | Accepted: we will not switch engines during the project |
| EF Core's default naming creates quoted PascalCase names | Agree on one naming convention in the migration rules (DB-C04) |
| Azure cost, if we host | Smallest tier, stop resources when unused, budget alerts, delete everything after Demo Day |

## 8. Why not the other options

- **SQL Server or Azure SQL Database.** Our closest alternative: a Microsoft-supported EF Core provider, mature tooling and temporal tables. We rejected it for three reasons:
  - The lease-overlap rule would need hand-written locking logic instead of a declarative constraint.
  - The Linux container image is x64-only and heavier on laptops and CI runners.
  - Edition licensing (Developer is for non-production use only, and Express has limits) adds a decision for every environment.

  We would revisit this if the team's T-SQL experience turns out to be much stronger than its PostgreSQL experience.
- **MongoDB or DocumentDB.** The requirements describe this domain as relational at heart. Lease overlap, exact billing and cross-entity dashboards would all depend on application code, with no foreign keys. Multi-document transactions need a replica set, even locally and in CI, and EF Core migrations would not apply.
- **Azure Cosmos DB for NoSQL.** Transactions and stored procedures are limited to one partition key, there are no joins across documents and there is no native decimal type. Our access patterns (by unit, by building, by person, by technician, by period) cannot share a single partition key without heavy duplication.

## 9. How we will know this decision is right

This decision is confirmed when:

- CI applies every migration to a fresh PostgreSQL 18 database on every pull request.
- The parallel lease-activation test passes.
- The billing tests reconcile to the last cent.
- The first restore drill succeeds.

Revisit it if any of these cannot be achieved, or if key queries stay slow on the seed data after indexing.

## 10. References

- DevAcademy 2026–2027 Capstone Project Requirements v1.4: DB-C01 to DB-C09, ARC-07, ARC-10, Section 7.4 and Appendix C.
- Supported versions of PostgreSQL in Azure Database for PostgreSQL: https://learn.microsoft.com/en-us/azure/postgresql/configure-maintain/concepts-supported-versions
- Npgsql EF Core provider 10.0 release notes: https://www.npgsql.org/efcore/release-notes/10.0.html
- PostgreSQL 19 RC 1 announcement (release schedule): https://www.postgresql.org/about/news/postgresql-19-rc-1-released-3409/
- PostgreSQL documentation: exclusion constraints and range types.

---

*AI assistance (AI-07): drafted with Microsoft Copilot and reviewed by [author name].*
