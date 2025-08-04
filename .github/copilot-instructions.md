<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

# TaskManager - Task Scheduler Application

This is a .NET microservices application for task scheduling with the following architecture:

## Project Structure
- **TaskManager.Shared**: Common models, gRPC contracts, and shared services
- **TaskManager.Api**: REST API service that communicates with clients
- **TaskManager.Scheduler**: Scheduler service that manages tasks in memory

## Communication Patterns
- **gRPC**: Synchronous communication between API and Scheduler for CRUD operations
- **RabbitMQ**: Asynchronous messaging for scheduling/unscheduling tasks and notifications

## Key Technologies
- .NET 9.0
- gRPC for inter-service communication
- RabbitMQ for message queuing
- ASP.NET Core for REST API
- In-memory storage for tasks

## Development Guidelines
- Use dependency injection for all services
- Follow async/await patterns consistently
- Implement proper error handling and logging
- Use the shared models and DTOs from TaskManager.Shared
- Maintain clean separation between API and business logic layers
