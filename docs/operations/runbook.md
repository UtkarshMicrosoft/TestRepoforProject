# Runbook

Owner: OPS track (OPS-C09). Fill in each section as the platform grows.

## Local setup

See the [README](../../README.md), sections 1 to 3.

## Environments and URLs

| Environment | Source branch | How to start | URL |
|---|---|---|---|
| Local | any | `podman compose up --build` | http://localhost:3000 |
| Preview | `preview` | To do | |
| Release | `main` (tags) | To do | |

## Deployment and rollback

To do.

## Data reset and reseeding

To do.

## Backup and restore

To do (DB-C08). Restore drill reports go in this folder.

## Common failures and fixes

| Symptom | Fix |
|---|---|
| `npm` fails with "running scripts is disabled" | Use `npm.cmd` / `npx.cmd` in PowerShell |
| Database container exits at start | PostgreSQL 18 needs the volume at `/var/lib/postgresql` (already set in `compose.yaml`). Check `podman compose logs db` |
| `podman compose` says it cannot connect | Start the Podman machine in Podman Desktop |
| Variables in `compose.yaml` are empty | Create `.env`: `Copy-Item .env.example .env` |
