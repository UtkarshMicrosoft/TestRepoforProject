# Instructions for AI coding agents

Read [`.github/copilot-instructions.md`](.github/copilot-instructions.md) first. It describes the architecture, conventions, commands, security rules and Definition of Done.

## Before you change code

- Work on a branch named `<type>/<alias>/<item>-<description>` created from the latest `preview`.
- Keep the change small (about 400 changed lines or fewer). Split larger work into several pull requests.
- Do not add a NuGet or npm package without naming it and its licence. Only licences allowed by ARC-10.

## Commands

| Task | Command |
|---|---|
| Build the API and tests | `dotnet build BuildingOps.slnx` |
| Run .NET tests | `dotnet test BuildingOps.slnx` |
| Run the API | `dotnet run --project src/api` |
| Web app (from `src/web`) | `npm ci`, `npm run dev`, `npm run lint`, `npm run build` |
| Database and cache | `podman compose up -d db cache` |

## Never

- Commit secrets, `.env` files or real personal data.
- Access the database from the web app.
- Store tokens in `localStorage` or `sessionStorage`.
- Return internal work notes from resident-facing endpoints.
- Edit a database migration that is already merged into `preview`; add a new one instead.
