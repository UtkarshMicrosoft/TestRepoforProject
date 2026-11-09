---
description: Add a new API endpoint with validation, authorization and tests
---

Add a new endpoint to the ASP.NET Core API in `src/api`.

I will give you: the module, HTTP method and route, request and response shape, the roles allowed, and the requirement IDs.

Follow `.github/copilot-instructions.md`, and:

1. Put the code in `src/api/Modules/<Module>/`.
2. Use a route under `/api/v1/` with plural, kebab-case nouns.
3. Validate the input and return RFC 9457 ProblemDetails for errors (400, 403, 404, 409 or 422).
4. Enforce object-level authorization: role **and** relationship to the resource.
5. For lists, use server-side pagination (`page`, `pageSize`) and return the total count.
6. Add unit tests in `tests/Api.UnitTests` and an integration test in `tests/Api.IntegrationTests`, including one that proves an unauthorized user is rejected.
7. Do not add packages without telling me the package name and its licence.

Finish by listing every file you changed and explaining why.
