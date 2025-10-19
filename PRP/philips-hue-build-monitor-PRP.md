### **PRP: Hue DevOps Status Light**

**1. Executive Summary**

This project will create a .NET 9 backend application that provides visual feedback for software development workflows. 
The application will act as a bridge between CI/CD platforms (Azure DevOps, GitHub) and a local Philips Hue lighting system. 
By changing the color and state of a designated Hue light, developers receive immediate, ambient notifications about the status of their code commits, builds, and deployments.

When building, the light will flash yellow to show that a build is in progress. 
Upon build completion, the light will turn green for success or red for failure. 
This system aims to reduce the cognitive load of monitoring build statuses through traditional means (emails, dashboards) by providing an intuitive, physical indicator.

**2. User Stories**

*   **As a developer,** I want to see my Hue light turn green when my build succeeds, so I get immediate positive feedback without checking the dashboard.
*   **As a developer,** I want to see my Hue light turn red when my build fails, so I am instantly notified that an issue needs my attention.
*   **As a developer,** I want the light to flash yellow while a build is in progress. This way, I know that the system is actively working on my changes.
*   **As a system administrator,** I want to securely configure the application with my Hue Bridge IP and API key via a configuration file, so my system credentials are not hardcoded.
*   **As a system administrator,** I want the application to be able to find my Hue Bridge on the network automatically, to simplify the initial setup.

**3. Business/System Context**

This system aims to improve developer experience and team awareness by integrating physical, 
visual notifications into the DevOps lifecycle. 

It solves the problem of "notification fatigue" from emails and chat messages by providing a clear, ambient status indicator. 
The application will run as a local service, listening for webhook events from cloud services (GitHub actions, Azure DevOps) and translating them into commands sent to the local Philips Hue Bridge. 
All interaction with the Hue API will be based on the official Hue v2 RESTful interface.

**4. Scope**

*   **In-Scope:**
    *   Backend service built on .NET 9, C#, and the FastEndpoints library.
    *   Integration with the Philips Hue API v2 for light control (on/off, color, brightness).
    *   Automatic discovery of the Hue Bridge on the local network.
    *   Endpoints to receive and process webhooks from:
        *   GitHub.
        *   Azure DevOps.
    *   Secure API endpoints using a configurable API key.
    *   Configuration managed through `appsettings.json`.
    *   Development methodology will be Test-Driven Development (TDD) using xUnit.V3.
    *   Target code coverage of at least 80%.

*   **Out-of-Scope:**
    *   A graphical user interface (GUI) for configuration or management.
    *   Support for other CI/CD platforms (e.g., Jenkins, GitLab).
    *   Support for complex lighting animations beyond simple state changes (e.g., multi-step sequences). This functionality is handled by the Hue Entertainment API, which is not part of this project.
    *   A user/account management system.
    *   Persistence of event history to a database.

**5. Solution Plan**

The solution will be developed iteratively through four distinct phases. 

- We will begin by building a robust service to handle all communication with the Philips Hue Bridge using the HueApi. 
- Next, we will create the public-facing API endpoints that will receive webhooks. 
- The third phase will focus on integrating these components and externalizing all configuration. 
- The final phase will be dedicated to hardening the application with comprehensive error handling and final documentation. 

Each task within a phase will be committed only after its corresponding tests pass.

**6. Architecture and Design**

*   See copilot-instructions.md for details on the architecture and design.

**7. Implementation Plan**

The project will be built in an agent mode, with code generated for approval after each task.

✅ means the task have been completed

☐ means task is planned, but not executed

*   **Phase 1: Philips Hue Interaction Module**
    *   **Task 1.1:** Setup project structure, test framework, and dependencies. Write tests for and implement Hue Bridge discovery logic. ✅
        *   **Task 1.2:** Write unit tests for and implement a `HueLightService` that discovers the bridge IP address, if the IP is not already provided in the appsettings.json. The IP is returned, but not stored anywhere. It's up to the user of the system to set it in appsettings.json using the setting 'bridgeIp' ✅
        *   **Task 1.3:** Write unit tests for and implement a `HueLightService` that discovers the bridge IP address, if the IP is not already provided in the appsettings.json. The IP is returned, but not stored anywhere. It's up to the user of the system to set it in appsettings.json using the setting 'bridgeKey' ✅
    *   **Task 1.4:** Extend `HueLightService` with a way to get a list of all available lights. The list has the id and name of the light. ✅
    *   **Task 1.5:** Extend `HueLightService` with a way to get a specific light, by providing the name of the light. ✅
    *   **Task 1.6:** Extend `HueLightService` with a method to take a snapshot of the lights state.  ✅
    *   **Task 1.7:** Extend `HueLightService` with a method to set color (red, green, yellow). Brightness is always 100. Use previous snapshot method before setting the light and then show the color for 2 seconds, then restore the previous state again. ✅
    *   **Task 1.8:** Extend `HueLightService` with a method to flash the light (on/off/on/off) for 5 seconds. Use the same pattern to take snapshot and restore as in task 1.7 ✅

*   **Phase 2: Backend API with FastEndpoints**
    *   **Task 2.1:** Write a integration test for a simple `/health` endpoint that checks if we have a hue bridge ip and key and implement it. ✅
    *   **Task 2.2:** Write a integration test for the endpoint '/hue/discover' that uses the `HueLightService` from task 1.2 and implement it. ✅
        *   **Task 2.3:** Write a integration test for the endpoint '/hue/register' that uses the `HueLightService` from task 1.3 and implement it. ✅
    *   **Task 2.4:** Write a integration test for the endpoint '/hue/getalllights' that uses the `HueLightService` from task 1.4 and implement it. ✅
    *   **Task 2.5:** Write a integration test for the endpoint '/hue/getlight' that uses the `HueLightService` from task 1.5 and implement it. ✅
    *   **Task 2.6:** Write a integration test for the endpoint '/hue/setlight' that uses the `HueLightService` from task 1.6 + 1.7 and implement it. ✅
        *   **Task 2.6:** Write a integration test for the endpoint '/hue/pulsatelight' that uses the `HueLightService` from task 1.6 + 1.8 and implement it. ✅

*   **Phase 3: Integration and Configuration**
    *   **Task 3.1:** Write tests for and implement a configuration service to load settings from `appsettings.json`. ✅
    *   **Task 3.2:** Refactor the application to use the configuration service and integrate the `HueLightService` with the API endpoints. ✅
    *   **Task 3.3:** Perform manual end-to-end testing with ngrok and a real GitHub webhook. ✅
    *   **Task 3.4:** Perform manual end-to-end testing with ngrok and a real Azure DevOps service hook. ✅

*   **Phase 4: Error Handling and Refinement**
    *   **Task 4.1:** Write tests for and implement robust error handling (e.g., Hue Bridge is offline, invalid webhook payload). ✅
    *   **Task 4.2:** Implement structured logging throughout the application. ✅
    *   **Task 4.3:** Review code against the 80% coverage target and add tests where necessary. ☐
    *   **Task 4.4:** Create documentation using the FastEndpoints explaining configuration and API usage. ☐

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