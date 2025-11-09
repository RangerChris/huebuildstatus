# HueBuildStatus

## Description

HueBuildStatus is a .NET 9 backend service that integrates with Philips Hue lighting systems to provide visual feedback for CI/CD build events. It listens for webhooks from platforms like GitHub and Azure DevOps, translating build statuses into light colors: flashing yellow for in-progress builds, green for success, and red for failure. This creates an ambient notification system that reduces the need for constant dashboard monitoring.

## Purpose
- Local service to provide Philips Hue visual feedback for CI/CD build events.

## Quick start (dev, Windows cmd.exe)

### 1) Development configuration
- Create a local development settings file with example values (do not commit keys):

  `HueBuildStatus.Api\appsettings.Development.json`

  Use the provided template in the repo as `appsettings.Development.json` and fill in your `bridgeIp` and `bridgeKey` for local testing.

### 2) Run locally
- From repository root (Windows cmd):

  `dotnet run --project HueBuildStatus.Api\HueBuildStatus.Api.csproj`

- The API will start and Swagger will be available at http://localhost:5000/swagger (port may vary).
- If you want to use Jaeger to trace requests, ensure you have a Jaeger instance running and configure the `OTLP_ENDPOINT_URL`. Start a docker instances with this command
`docker run --rm --name jaeger -p 16686:16686 -p 4317:4317 -p 4318:4318 -p 5778:5778 -p 9411:9411 cr.jaegertracing.io/jaegertracing/jaeger:latest`

### 3) Expose to the internet with ngrok (manual end-to-end testing)
- Install ngrok (https://ngrok.com/).
- Start ngrok to forward HTTP (example port 5000):

  `ngrok http 5000`

- Note the public URL (https://xyz.ngrok.io). Configure your GitHub webhook to target the public URL + the endpoint (e.g., `https://xyz.ngrok.io/webhooks/github`).

### 4) Environment variables
- For CI/production, prefer environment variables (do not commit secrets):
  - `BRIDGE_IP` (optional override)
  - `HUE_APP_KEY` (Hue API key)

### 5) Run tests
- From repository root (cmd.exe):

  `dotnet test HueBuildStatus.sln`

## Security

For security best practices, vulnerability reporting, and security audit results, please see [SECURITY.md](SECURITY.md).