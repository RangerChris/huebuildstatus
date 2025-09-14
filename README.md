# huebuildstatus

[![CI](https://github.com/RangerChris/huebuildstatus/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/RangerChris/huebuildstatus/actions/workflows/ci.yml)
[![Codecov](https://codecov.io/gh/RangerChris/huebuildstatus/branch/main/graph/badge.svg)](https://codecov.io/gh/RangerChris/huebuildstatus)


This project will create a .NET 9 backend application that provides visual feedback for software development workflows. 

The application will act as a bridge between CI/CD platforms (Azure DevOps, GitHub) and a local Philips Hue lighting system. 
By changing the color and state of a designated Hue light, developers receive immediate, ambient notifications about 
the status of their code commits, builds, and deployments.