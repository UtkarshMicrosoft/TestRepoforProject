# Database (PostgreSQL 18)

Owner: DB track. Decision: [`docs/adr/0001-use-postgresql-as-database.md`](../../docs/adr/0001-use-postgresql-as-database.md).

## Planned layout

The DB track finalises this layout together with the migration naming and review rules (DB-C04).

| Folder | Contents | Requirement |
|---|---|---|
| `migrations/` | Versioned schema migrations | DB-C04 |
| `procedures/` | Functions and stored procedures, for example the billing preview | DB-C06 |
| `views/` | Views, for example outstanding balances | DB-C06 |
| `triggers/` | Triggers, for example work-order status history | DB-C06 |
| `seed/` | Synthetic, reproducible seed data: Aurora Residence, unit 4B, Ana, Mihai, Ioana, Radu | PX-08 |

## Rules

- Never edit a migration that is already merged into `preview`; add a new one.
- Money: `numeric` with a currency code. Time: `timestamptz` (UTC).
- Synthetic data only. Never commit database dumps.

## Local database

```powershell
podman compose up -d db
```

Connect with host `localhost`, port `5432`, and the database, user and password from your `.env` file.
