# Gift E-Commerce

## Project Overview
Gift E-Commerce is a **Microservices-based e-commerce system** that allows users to browse and purchase gifts, manage inventory, offers, carts, and orders in a **scalable and maintainable architecture**.  

The project follows **Microservices principles**, with clear service boundaries, independent databases, and **vertical slicing** for separation of concerns.  

It implements **CQRS (Command Query Responsibility Segregation)** to separate read and write operations, improving performance and maintainability.  

All services expose **Minimal APIs** for lightweight, fast communication, with **validation implemented for each command** to ensure data integrity.

---

## Services
- **User Service:** Manages user data and registration.  
- **Identity Service:** Custom authentication and login using JWT, no external packages.  
- **Product/Category Service:** Manages products and categories.  
- **Inventory Service:** Tracks stock levels, integrated with Redis for caching and performance.  
- **Cart Service:** Handles shopping carts and integrates with Order Service.  
- **Order Service:** Processes orders and ensures consistency across services using RabbitMQ + MassTransit.  
- **Offer Service:** Handles promotions and discounts, linked to products and orders.

---

## Architecture
- **Microservices architecture** with **API Gateway** for unified entry points and security.  
- **Vertical slicing** to maintain clear responsibilities for each service.  
- Each service has its own database for **loose coupling**.  
- Implements **CQRS** pattern: commands and queries are separated for scalability.  
- All APIs use **Minimal API approach** for lightweight endpoints.  
- **Validators** implemented for every command to enforce business rules.  
- **Logging:** Serilog used for all commands/queries for traceability.  
- **Service communication:** HTTP calls and messaging via RabbitMQ + MassTransit.

---

## Technologies
- **Backend:** .NET Core, Web API, Minimal APIs  
- **Database:** SQL Server, EF Core  
- **Authentication:** JWT (custom Identity Service)  
- **Messaging/Queue:** RabbitMQ, MassTransit  
- **Caching:** Redis  
- **Logging:** Serilog  
- **Validation:** FluentValidation (for all commands)  
- **Containerization:** Docker & Docker Compose  

---

## Challenges & Learnings
- Ensuring **reliable communication between services** and handling message failures.  
- Implementing **CQRS** with validators for each command to maintain data integrity.  
- Coordinating **vertical slicing** across multiple services to maintain boundaries.  
- Leading a team working on Microservices for the first time, and ensuring collaboration and code consistency.  
- Gained deep practical experience in **distributed systems, performance optimization, and traceability**.

---

## Notes
This project demonstrates how to build a **scalable, distributed system** with modern patterns like **CQRS, Minimal APIs, and vertical slicing**, reflecting **practical problem-solving and architectural decisions**.
