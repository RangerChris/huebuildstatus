Project: HueBuildStatus

Purpose:
- Help GitHub Copilot suggestions produce code and PRs consistent with this repository's conventions.
- Provide context on architecture, design, and testing approaches.
- Encourage best practices like TDD, DI, SOLID principles and clear naming.

Project overview:
- Details what to build and the progress can be found in PRP/philips-hue-build-monitor-PRP.md
- Mark tasks as complete in the PRP document when done. Done means code is written, tests are passing and PR is created for review.

How Copilot should help:
- Suggest code, tests, and small refactorings consistent with existing patterns.
- Prefer clarity, correctness, and minimal surface area changes over clever one-liners.
- When making code changes, include or update unit tests where appropriate.
- All new features should be created in TDD style: write tests first, then implement minimal code to pass tests, then refactor.

Coding conventions / style:
- Use github copilot in agent mode to create code and PRs.
- Use MCP server tools like context7 for up-to-date documentation on libraries like FastEndpoints and HueApi
- Use modern C# (nullable reference types enabled, async/await for I/O-bound work).
- Use featured-based architecture with clear separation of concerns.
- Follow SOLID principles and best practices for maintainability and testability.
- Use dependency injection for services and abstractions.
- Use FastEndpoints for API endpoints. API can be found at https://api-ref.fast-endpoints.com/api/FastEndpoints.html
- Keep methods small and single-responsibility. Favor explicitness over magic.
- Use TDD approach: write tests first, then implement minimal code to pass tests, then refactor.
- Only use https://api.nuget.org/v3/index.json as source when adding nuget packages
- When running something in the terminal, prefer using `dotnet` CLI commands over IDE-specific commands. Assume the developer is running powershell in Windows Terminal.
- When creating code, do not add any comments unless absolutely necessary.

#Techstack
- .NET 9, latest C# version
- FastEndpoints for building APIs
- Hue.Api https://github.com/michielpost/Q42.HueApi for interacting with Philips Hue lights
- Serilog for logging
- xUnit (xunit.v3) for unit testing
- Moq for mocking dependencies in tests
- FluentAssertions (version 7.2.0) for readable assertions in tests, never update to latest version
- Assume the developer is using Rider 2025.2.2 and works on a Windows 11 PC. 

Testing:
- Add or update xUnit (xunit.v3) tests in HueBuildStatus.Tests for behavior changes.
- Tests should follow Arrange-Act-Assert and be deterministic. Use dependency injection to mock external dependencies.
- Use Testcontainers or in-memory alternatives for integration tests if needed.
- Aim for high code coverage, but prioritize meaningful tests over coverage percentage.

Security and secrets:
- Never suggest or insert secrets (API keys, tokens) into repository files. Use configuration and environment variables.
- Add guidance to use appsettings.Development.json for local examples only, but never commit real credentials.

Files and folders to avoid editing:
- bin/, obj/, .vs/, Rider files, and files generated at build/runtime.

Pull requests and commits:
- Keep PRs small and focused. Include a short description and reference related issues if any.
- Ensure tests pass locally before suggesting a PR.
- In agent mode, use github MCP tools to create PRs for review. If not available, ask if docker container with MCP server is running
- Use conventional commits style for commit messages (e.g., feat:, fix:, docs:, style:, refactor:, test:, chore:).

When to ask the user:
- If a change affects behavior or public API and the intent is unclear, ask for clarification before applying.

If in doubt, prefer leaving a clear TODO comment and asking for guidance rather than guessing.