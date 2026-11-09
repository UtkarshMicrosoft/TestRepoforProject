# Building Operations Platform

> Working name. Replace it with your team's product name.

A web platform for the day-to-day operations of residential buildings: units and a live 2D floor map, residents and leases, maintenance work orders, utility meters and monthly charges, announcements and notifications, dashboards and an audit trail.

DevAcademy 2026–2027 capstone project.

## Tech stack

| Part | Technology | Folder |
|---|---|---|
| Web app | Next.js 16 (App Router), React 19, TypeScript, Tailwind CSS | `src/web` |
| API | ASP.NET Core on .NET 10 (modular monolith) | `src/api` |
| Database | PostgreSQL 18 (see `docs/adr/0001-use-postgresql-as-database.md`) | `src/db` |
| Cache | Valkey 8 | `compose.yaml` |
| Containers | Podman + Compose (Docker Desktop is not allowed) | `compose.yaml`, `infra/` |
| CI | GitHub Actions | `.github/workflows/` |

## 1. Install the tools (once per laptop)

Open **PowerShell as administrator**:

```powershell
wsl --update
wsl --install --no-distribution
# Restart the PC, then:
winget install -e --id Git.Git
winget install -e --id Microsoft.VisualStudioCode
winget install -e --id Microsoft.DotNet.SDK.10
winget install -e --id OpenJS.NodeJS.LTS
winget install -e --id RedHat.Podman-Desktop
winget install -e --id GitHub.cli
```

Then:

1. Open **Podman Desktop** and click **Start onboarding**. Let it install Podman and Compose and create the Podman machine.
2. Close and reopen PowerShell (normal, not administrator) and tell Git who you are, using your Microsoft email:

   ```powershell
   git config --global user.name  "Your Name"
   git config --global user.email "alias@microsoft.com"
   ```

> **Windows tip:** in PowerShell, type `npm.cmd` and `npx.cmd` instead of `npm` and `npx`. Plain `npm` can fail with *"running scripts is disabled on this system"*; the `.cmd` versions avoid that.

## 2. Get the code (once)

```powershell
cd $HOME
mkdir dev -Force
cd dev
git clone <repository URL>
cd <repository folder>
git switch preview
Copy-Item .env.example .env
dotnet restore BuildingOps.slnx
cd src/web
npm.cmd ci
cd ../..
code .
```

When VS Code asks *"Do you want to install the recommended extensions?"*, click **Install**.

## 3. Run it locally

| What | Command (from the repository folder) | Check |
|---|---|---|
| Database + cache | `podman compose up -d db cache` | `podman compose ps` shows both running |
| API | `dotnet run --project src/api` | Open the URL printed in the terminal + `/health` |
| Web app | `cd src/web` then `npm.cmd run dev` | http://localhost:3000 |
| Whole stack in containers | `podman compose up --build` | http://localhost:3000 and http://localhost:8080/health |
| Stop containers | `podman compose down` | Add `-v` to also delete the database data |

Tests and checks (the same ones CI runs):

```powershell
dotnet build BuildingOps.slnx
dotnet test BuildingOps.slnx
cd src/web
npm.cmd run lint
npm.cmd run build
cd ../..
```

## 4. How we work with Git

- `preview` is the integration trunk and the **default branch**. `main` is the release line and receives one release pull request from `preview` every week.
- Never commit directly to `preview` or `main`. Branch protection blocks it anyway.

**Every piece of work:**

```powershell
git switch preview
git pull
git switch -c feature/<alias>/<item>-<short-description>
# ... change code, run the checks above ...
git add -A
git commit -m "feat: short description (#<item>)"
git push -u origin feature/<alias>/<item>-<short-description>
```

Then on GitHub:

1. Open a pull request **into `preview`** and fill in the template.
2. Wait for the checks to turn green and for at least one teammate to approve.
3. Click **Squash and merge**. The branch is deleted automatically.
4. Everyone updates: `git switch preview` then `git pull`.

**Keep your branch up to date** while you work:

```powershell
git switch preview
git pull
git switch <your-branch>
git merge preview
```

**Rules (Section 11 of the requirements):**

- Branch names: `<type>/<alias>/<item>-<description>`. Types: `feature`, `fix`, `refactor`, `test`, `docs`, `chore`, `spike`.
- Commit messages: Conventional Commits with a work item, for example `feat: add unit list endpoint (#42)`. Types: `feat`, `fix`, `refactor`, `test`, `docs`, `chore`, `ci`, `perf`.
- Branch → `preview`: **squash merge**. `preview` → `main` (weekly release): **merge commit**.
- Merge into `preview` at least once a week. Branches should live 3 working days or less.
- Keep pull requests under about 400 changed lines. Review at least one pull request per week, including one outside your track.

## 5. Repository structure

```text
/
├── .github/
│   ├── workflows/ci.yml           # CI on every pull request
│   ├── ISSUE_TEMPLATE/            # user story, task, bug
│   ├── pull_request_template.md
│   ├── copilot-instructions.md    # repository AI context (AI-02)
│   ├── prompts/                   # team prompt library (AI-09)
│   └── dependabot.yml
├── AGENTS.md                      # instructions for AI coding agents (AI-02)
├── CODEOWNERS
├── src/
│   ├── web/                       # Next.js app
│   ├── api/                       # ASP.NET Core API (modular monolith)
│   │   ├── Modules/               # Buildings, Residents, Leases, Maintenance, Billing, Notifications, Audit
│   │   └── Shared/                # Auth, Validation, Caching, Persistence, Telemetry
│   ├── worker/                    # optional separate background worker
│   └── db/                        # migrations, procedures, views, triggers, seed
├── tests/
│   ├── Api.UnitTests/
│   ├── Api.IntegrationTests/
│   ├── db/
│   ├── web/
│   └── e2e/                       # Playwright
├── infra/                         # Dockerfiles, Bicep
├── docs/                          # ADRs, architecture, specs, operations, AI journals
├── compose.yaml
├── global.json                    # pins the .NET 10 SDK
├── BuildingOps.slnx
├── CHANGELOG.md
└── README.md
```

## 6. Documentation

See [`docs/README.md`](docs/README.md).
