HueBuildStatus

Purpose
- Local service to provide Philips Hue visual feedback for CI/CD build events.

Quick start (dev, Windows cmd.exe)

1) Development configuration
- Create a local development settings file with example values (do not commit keys):

  HueBuildStatus.Api\appsettings.Development.json

  Use the provided template in the repo as `appsettings.Development.json` and fill in your `bridgeIp` and `bridgeKey` for local testing.

2) Run locally
- From repository root (Windows cmd):

  dotnet run --project HueBuildStatus.Api\HueBuildStatus.Api.csproj

- The API will start and Swagger will be available at http://localhost:5000/swagger (port may vary).

3) Expose to the internet with ngrok (manual end-to-end testing)
- Install ngrok (https://ngrok.com/).
- Start ngrok to forward HTTP (example port 5000):

  ngrok http 5000

- Note the public URL (https://xyz.ngrok.io). Configure your GitHub webhook to target the public URL + the endpoint (e.g., https://xyz.ngrok.io/webhooks/github).

4) Environment variables
- For CI/production, prefer environment variables (do not commit secrets):
  - BRIDGE_IP (optional override)
  - HUE_APP_KEY (Hue API key)

5) Run tests
- From repository root (cmd.exe):

  dotnet test HueBuildStatus.sln

Notes
- The repo uses FastEndpoints for the API and xUnit + Moq for tests. Follow TDD for future changes.
- Never commit real API keys or secrets. Use env vars or local `appsettings.Development.json` (not checked in) for development.

Files of interest
- HueBuildStatus.Api: API endpoints and Program.cs
- HueBuildStatus.Core: core services and interfaces
- HueBuildStatus.Tests: unit + integration tests

If you want, I can create a script to run the API and start ngrok together (local-only).