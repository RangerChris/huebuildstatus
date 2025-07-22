Of course. Here is the updated Project Requirements Plan (PRP) based on the specified template. This document will serve as our guide for the project.

---

### **PRP: Hue DevOps Status Light**

**1. Executive Summary**

This project will create a .NET 9 backend application that provides visual feedback for software development workflows. The application will act as a bridge between CI/CD platforms (Azure DevOps, GitHub) and a local Philips Hue lighting system. By changing the color and state of a designated Hue light, developers receive immediate, ambient notifications about the status of their code commits, builds, and deployments. The application will be built using the FastEndpoints framework and will be developed following strict Test-Driven Development (TDD) principles to ensure high quality and reliability. The development process will be iterative, with the user approving and committing each completed task.

**2. User Stories**

*   **As a developer,** I want to see my Hue light turn green when my Azure DevOps build succeeds, so I get immediate positive feedback without checking the dashboard.
*   **As a developer,** I want to see my Hue light turn red when my build fails, so I am instantly notified that an issue needs my attention.
*   **As a team member,** I want the light to flash yellow while a build is in progress, so I know not to merge other changes until it completes.
*   **As a developer,** I want the light to pulse blue when a new commit is pushed to the main branch on GitHub, so I'm aware of recent changes from my team.
*   **As a system administrator,** I want to securely configure the application with my Hue Bridge IP and API key via a configuration file, so my system credentials are not hardcoded.
*   **As a system administrator,** I want the application to be able to find my Hue Bridge on the network automatically, to simplify the initial setup.

**3. Business/System Context**

This system aims to improve developer experience and team awareness by integrating physical, visual notifications into the DevOps lifecycle. It solves the problem of "notification fatigue" from emails and chat messages by providing a clear, ambient status indicator. The application will run as a local service, listening for webhook events from cloud services (GitHub, Azure DevOps) and translating them into commands sent to the local Philips Hue Bridge. All interaction with the Hue API will be based on the official v2 RESTful interface.

**4. Scope**

*   **In-Scope:**
    *   Backend service built on .NET 9, C#, and the FastEndpoints library.
    *   Integration with the Philips Hue API v2 for light control (on/off, color, brightness).
    *   Automatic discovery of the Hue Bridge on the local network.
    *   Endpoints to receive and process webhooks from:
        *   GitHub (for `push` events).
        *   Azure DevOps (for build completion events).
    *   Secure API endpoints using a configurable API key.
    *   Configuration managed through `appsettings.json`.
    *   Development methodology will be Test-Driven Development (TDD) using xUnit.
    *   Target code coverage of at least 80%.

*   **Out-of-Scope:**
    *   A graphical user interface (GUI) for configuration or management.
    *   Support for other CI/CD platforms (e.g., Jenkins, GitLab).
    *   Support for complex lighting animations beyond simple state changes (e.g., multi-step sequences). This functionality is handled by the Hue Entertainment API, which is not part of this project.
    *   A user/account management system.
    *   Persistence of event history to a database.

**5. Solution Plan**

The solution will be developed iteratively through four distinct phases. We will begin by building a robust service to handle all communication with the Philips Hue Bridge. Next, we will create the public-facing API endpoints that will receive webhooks. The third phase will focus on integrating these components and externalizing all configuration. The final phase will be dedicated to hardening the application with comprehensive error handling and final documentation. Each task within a phase will be committed only after its corresponding tests pass.

**6. Architecture and Design**

*   **Core Framework:** .NET 9 with FastEndpoints for creating a lightweight, high-performance API.
*   **Language:** C# 13.
*   **Testing Framework:** xUnit for test definitions, Moq for creating mock dependencies, and FluentAssertions for readable assertions.
*   **Design Principles:**
    *   **Test-Driven Development (TDD):** Every piece of functionality will begin with a failing test, followed by the minimal code required to make it pass, and then refactoring.
    *   **Dependency Injection (DI):** Services (like the Hue interaction service) will be abstracted behind interfaces and injected into their dependencies, enabling loose coupling and high testability.
    *   **Service Abstraction:** All external interactions (e.g., HTTP calls to the Hue Bridge) will be wrapped in a dedicated service (`IHueDiscoveryService`, `IHueControlService`) to isolate them from the core application logic.
    *   **Configuration-driven:** All sensitive or environment-specific values (API keys, Bridge IP) will be managed via `appsettings.json`.

**7. Implementation Plan**

The project will be built in an agent mode, with code generated for approval after each task.

*   **Phase 1: Philips Hue Interaction Module**
    *   **Task 1.1:** Setup project structure, test framework, and dependencies. Write tests for and implement Hue Bridge discovery logic. **(Completed)**
    *   **Task 1.2:** Write tests for and implement the authentication process to generate a new `appkey` by pressing the link button on the bridge. **(Completed)**
    *   **Task 1.3:** Write tests for and implement a `HueLightService` that can turn a light on/off. **(Completed)**
    *   **Task 1.4:** Extend `HueLightService` tests and implementation to support changing color and brightness. **(Completed)**

*   **Phase 2: Backend API with FastEndpoints**
    *   **Task 2.1:** Write a test for a simple `/health` endpoint and implement it.
    *   **Task 2.2:** Write tests for an endpoint to receive a GitHub `push` event. Implement the endpoint with mocked service logic.
    *   **Task 2.3:** Write tests for an endpoint to receive an Azure DevOps `build.complete` event. Implement the endpoint with mocked service logic.
    *   **Task 2.4:** Write tests for and implement API key security on the webhook endpoints.

*   **Phase 3: Integration and Configuration**
    *   **Task 3.1:** Write tests for and implement a configuration service to load settings from `appsettings.json`.
    *   **Task 3.2:** Refactor the application to use the configuration service and integrate the `HueLightService` with the API endpoints.
    *   **Task 3.3:** Perform manual end-to-end testing with ngrok and a real GitHub webhook.
    *   **Task 3.4:** Perform manual end-to-end testing with ngrok and a real Azure DevOps service hook.

*   **Phase 4: Error Handling and Refinement**
    *   **Task 4.1:** Write tests for and implement robust error handling (e.g., Hue Bridge is offline, invalid webhook payload).
    *   **Task 4.2:** Implement structured logging throughout the application.
    *   **Task 4.3:** Review code against the 80% coverage target and add tests where necessary.
    *   **Task 4.4:** Create the `README.md` documentation explaining configuration and API usage.

**8. Testing & Validation**

*   **Methodology:** Test-Driven Development is mandatory.
*   **Unit Tests:** xUnit and Moq will be used to test every class and method in isolation. Test cases will cover both success paths and expected failure scenarios (e.g., invalid input, exceptions from dependencies).
*   **Code Coverage:** The project will strive for a minimum of 80% line coverage.
*   **Failure Scenarios to Test:**
    *   Hue Bridge not found on the network.
    *   Hue Bridge is offline during a control command.
    *   Received webhook payload is malformed or missing required fields.
    *   Received webhook is from an unauthorized source (invalid API key).
    *   Configuration file is missing or contains invalid entries.
*   **End-to-End Validation:** Manual testing will be performed using `ngrok` to expose the local API to the public internet, allowing real webhooks from GitHub and Azure DevOps to be received and processed.

**9. Milestones**

*   **Milestone 1:** Core Hue interaction is complete and tested. (End of Phase 1)
*   **Milestone 2:** Secure API endpoints for receiving events are functional. (End of Phase 2)
*   **Milestone 3:** Full end-to-end flow is integrated, configured, and manually validated. (End of Phase 3)
*   **Milestone 4:** Project is production-ready with robust error handling, logging, and documentation. (End of Phase 4)