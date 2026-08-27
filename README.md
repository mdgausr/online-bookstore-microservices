# Online Bookstore Microservices

This repository contains a starter scaffold for an online bookstore implemented as microservices using .NET 8, SQL Server (no EF — Dapper), RabbitMQ for messaging, and an Angular frontend with Angular Material.

What is included in this initial scaffold:
- Services (skeletons): api-gateway (YARP), catalog, basket, orders, identity, payments
- Shared contracts/messages project
- Dockerfiles for each service
- docker-compose.yml to run SQL Server + RabbitMQ + all services locally
- Kubernetes manifests in /k8s for deployments and services (example manifests)
- Frontend folder with Angular project skeleton instructions
- README with run instructions and next steps

Defaults:
- SQL Server: sa / P@ssw0rd!
- Admin user (seed placeholder): admin@example.com / P@ssw0rd!
- Stripe keys: placeholders (configure via environment variables)

Next steps I will take after you review this scaffold:
- Implement full functionality in each service (repositories with Dapper, message handlers, controllers, unit tests)
- Complete Angular frontend UI pages and theme
- Add detailed seed SQL scripts, Postman collection, and end-to-end smoke tests

Run locally (quick):
1. Install Docker & Docker Compose
2. From repo root: docker-compose up --build
3. Services will be available at their configured ports. See each service's README for details.

Note: This is the initial commit scaffolding. Full feature implementation will be pushed in follow-up commits.
