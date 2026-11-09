---
description: Create a database migration for PostgreSQL 18 with tests
---

Create a new EF Core migration for PostgreSQL 18.

I will give you: the tables or columns to add or change, the business rules and the requirement IDs.

Rules:

1. Never edit a migration that is already merged into `preview`; create a new one.
2. Money uses `numeric` with a currency code; timestamps use `timestamptz` (UTC).
3. Add primary keys, foreign keys, and unique, check and not-null constraints wherever the rule allows.
4. Anything EF Core cannot model (exclusion constraints, functions, views, triggers, grants) goes in `migrationBuilder.Sql(...)`, together with a matching `Down` step.
5. Add or update tests in `tests/db` (or the integration tests) that run against a real PostgreSQL 18, never an in-memory provider.
6. Explain the indexes you add and the queries they serve.

Show me the generated SQL so I can review it before I commit.
