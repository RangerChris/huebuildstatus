Project: HueBuildStatus

Purpose:
- Help contributors and GitHub Copilot suggestions produce code and PRs consistent with this repository's conventions.

Project overview:
- A small .NET (net9.0) solution that queries Philips Hue build status and exposes an API (FastEndpoints).
- Languages: C# (primary). Tests use xUnit in HueBuildStatus.Tests.

How Copilot should help:
- Suggest code, tests, and small refactorings consistent with existing patterns.
- Prefer clarity, correctness, and minimal surface area changes over clever one-liners.
- When making code changes, include or update unit tests where appropriate.

Coding conventions / style:
- Use modern C# (nullable reference types enabled, async/await for I/O-bound work).
- Follow existing project patterns for dependency injection and layering (Core services, Api endpoints).
- Keep methods small and single-responsibility. Favor explicitness over magic.
- Format with dotnet format (if available). Use clear, descriptive names for variables, methods, and parameters.

Testing:
- Add or update xUnit tests in HueBuildStatus.Tests for behavior changes.
- Tests should follow Arrange-Act-Assert and be deterministic. Use dependency injection to mock external dependencies.

Security and secrets:
- Never suggest or insert secrets (API keys, tokens) into repository files. Use configuration and environment variables.
- Add guidance to use appsettings.Development.json for local examples only, but never commit real credentials.

Files and folders to avoid editing:
- bin/, obj/, .vs/, Rider files, and files generated at build/runtime.

Pull requests and commits:
- Keep PRs small and focused. Include a short description and reference related issues if any.
- Ensure tests pass locally before suggesting a PR.

When to ask the user:
- If a change affects behavior or public API and the intent is unclear, ask for clarification before applying.

If in doubt, prefer leaving a clear TODO comment and asking for guidance rather than guessing.