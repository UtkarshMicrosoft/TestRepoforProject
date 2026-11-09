# Architecture

Planned content (ARC-06, DB-C02). Keep diagrams as code with Mermaid so they render on GitHub.

- [ ] C4 context diagram
- [ ] C4 container diagram
- [ ] C4 component diagram for one module
- [ ] Deployment diagram
- [ ] Data model (ERD) and data dictionary
- [ ] Sequence diagram for Scenario A ("A leak in apartment 4B")

## Starting point: containers

```mermaid
flowchart LR
  user["Resident / Manager / Technician / Administrator"] -->|HTTPS| web["Web app<br/>Next.js 16"]
  web -->|HTTPS / JSON| api["API<br/>ASP.NET Core, .NET 10"]
  api --> db[("PostgreSQL 18")]
  api --> cache[("Valkey")]
```
