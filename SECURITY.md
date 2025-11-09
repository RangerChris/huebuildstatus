# Security Policy

## Reporting Security Issues

If you discover a security vulnerability in HueBuildStatus, please report it by opening a GitHub issue or contacting the maintainers directly. Please do not publicly disclose the vulnerability until it has been addressed.

## Security Audit Results

This document contains the results of a comprehensive security audit performed on the HueBuildStatus repository.

### ✅ No Critical Security Issues Found

The security audit has verified that:

- **No hardcoded secrets** are present in the codebase
- **No secrets committed to git history** were found
- **NuGet packages** have been checked against the GitHub Advisory Database - all packages are free from known vulnerabilities
- **Configuration files** properly use empty placeholders for sensitive data
- **Build and test pipeline** passes all tests (108/108)

### ⚠️ Security Recommendations

While no critical security vulnerabilities were found, the following recommendations should be considered for enhanced security:

#### 1. Webhook Signature Verification

**Issue**: The GitHub webhook endpoint (`/webhooks/github`) currently accepts requests without verifying the GitHub webhook signature.

**Risk**: Medium - Allows unauthorized parties to trigger webhook processing if the endpoint URL is discovered.

**Recommendation**: Implement GitHub webhook signature verification using the `X-Hub-Signature-256` header. See [GitHub's documentation on securing webhooks](https://docs.github.com/en/developers/webhooks-and-events/webhooks/securing-your-webhooks).

**Example Implementation**:
```csharp
private bool VerifyGitHubSignature(string payload, string signature, string secret)
{
    using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
    var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
    var expectedSignature = "sha256=" + BitConverter.ToString(hash).Replace("-", "").ToLower();
    return signature == expectedSignature;
}
```

#### 2. Dockerfile Security Hardening

**Issue**: The original Dockerfile temporarily switched to root user to install curl for health checks.

**Risk**: Low - While the container returned to non-root user after installation, this increased the attack surface during the build process.

**Resolution**: ✅ **FIXED** - Removed the root user escalation and curl installation. The application now runs entirely as a non-root user.

**Note**: Docker HEALTHCHECK has been removed from the Dockerfile. If you need health checks, consider:
1. Implementing health checks at the orchestration layer (Kubernetes liveness/readiness probes can use the `/health` endpoint)
2. Using an external monitoring solution
3. If you must add HEALTHCHECK to the Dockerfile, consider using `wget` which may be pre-installed, or use a multi-stage build to copy curl binary without needing root access in the final image.

Example for Kubernetes:
```yaml
livenessProbe:
  httpGet:
    path: /health
    port: 8080
  initialDelaySeconds: 10
  periodSeconds: 10
```

#### 3. Environment Variable Configuration

**Current State**: Good - The application properly uses configuration from environment variables and appsettings.json files.

**Recommendation**: Update documentation to emphasize:
- Never commit `appsettings.Development.json` with real credentials
- Use environment variables for production deployments
- Consider using secret management solutions (Azure Key Vault, AWS Secrets Manager, etc.) for production

#### 4. Rate Limiting

**Issue**: No rate limiting is currently implemented on webhook endpoints.

**Risk**: Low to Medium - Could allow abuse or denial of service attacks.

**Recommendation**: Implement rate limiting on webhook endpoints using FastEndpoints rate limiting features or ASP.NET Core middleware.

#### 5. CORS Configuration

**Current State**: Not explicitly configured (using defaults).

**Recommendation**: If the API will be called from browsers, explicitly configure CORS with specific origins rather than allowing all origins.

## Security Best Practices

### For Developers

1. **Never commit secrets**: Use `.gitignore` to exclude sensitive files like:
   - `*.env`
   - `appsettings.Development.json` (already in .gitignore via `*.env`)
   - Any files containing API keys, passwords, or tokens

2. **Use environment variables**: For production and CI/CD:
   ```bash
   bridgeIp=<your-bridge-ip>
   bridgeKey=<your-app-key>
   LightName=<your-light-name>
   ```

3. **Keep dependencies updated**: Regularly check for updates to NuGet packages and Docker base images.

4. **Review pull requests**: Always review code changes for potential security issues before merging.

### For Deployment

1. **Use HTTPS**: Always deploy behind HTTPS in production.

2. **Implement webhook signature verification**: Before deploying to production, implement webhook signature verification.

3. **Use minimal Docker images**: The current setup uses official Microsoft .NET images which are well-maintained. Keep these updated.

4. **Network security**: Deploy behind a firewall or API gateway with appropriate access controls.

5. **Secrets management**: Use secure secret management solutions:
   - Azure Key Vault
   - AWS Secrets Manager
   - HashiCorp Vault
   - Kubernetes Secrets

## Dependency Security

All NuGet packages have been verified against the GitHub Advisory Database:

- FastEndpoints 7.1.0 ✅
- FastEndpoints.Swagger 7.1.0 ✅
- Microsoft.AspNetCore.OpenApi 9.0.10 ✅
- OpenTelemetry packages (1.13.x) ✅
- HueApi 3.0.0 ✅
- All testing dependencies ✅

**Recommendation**: Set up automated dependency scanning using:
- GitHub Dependabot (already available on GitHub)
- NuGet package vulnerability scanning in CI/CD

## Docker Image Security

The Dockerfile uses official Microsoft .NET images:
- `mcr.microsoft.com/dotnet/aspnet:9.0` - Runtime base image
- `mcr.microsoft.com/dotnet/sdk:9.0` - Build image

These are well-maintained and regularly updated by Microsoft. Ensure you rebuild images regularly to get security updates.

## CI/CD Security

The GitHub Actions workflow (`ci.yml`) follows good practices:
- Uses specific action versions (e.g., `actions/checkout@v4`)
- Uses secrets appropriately (`CODECOV_TOKEN`)
- No hardcoded credentials in workflow files

**Recommendation**: Consider adding:
- CodeQL analysis to the CI pipeline
- Automated dependency scanning
- Container scanning for Docker images

## Audit History

- **2025-11-09**: Initial security audit completed
  - No critical vulnerabilities found
  - No secrets in codebase or git history
  - All dependencies verified against GitHub Advisory Database
  - Several recommendations provided for security hardening
