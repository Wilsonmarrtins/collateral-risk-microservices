# Collateral Risk Microservices ### A Didactic Microservices Architecture for Collateral & Margin Simulation ![.NET](https://img.shields.io/badge/.NET-9-blue) ![Architecture](https://img.shields.io/badge/architecture-microservices-blueviolet) ![Status](https://img.shields.io/badge/status-educational-orange) ![License](https://img.shields.io/badge/license-MIT-green) This repository demonstrates a **didactic microservices architecture** that simulates how **collateral and margin management systems** operate inside financial institutions such as **investment banks, broker‑dealers, and clearing houses**. The goal of this project is to illustrate how modern backend architectures are structured when building **risk engines, collateral services, and margin allocation systems**. ⚠️ **Important** This project is **educational only**. Many financial and architectural aspects are simplified to focus on learning concepts such as: - Microservices architecture - API Gateway pattern - Service‑to‑service communication - Collateral calculation - Margin allocation - Resilience patterns (Retry, Circuit Breaker) - Shared kernel / building blocks - Minimal APIs with ASP.NET Core - Service layer architecture This repository is **not intended for production use**. --- # 🎯 Project Objective The system simulates a simplified **Collateral Risk Flow**, a process commonly found in financial institutions. Typical real‑world workflow: 1. A **client exists in the system** 2. The client **holds financial positions** 3. A **risk engine calculates exposure** 4. The system determines **required collateral** 5. If necessary, the system **segregates margin** 6. Funds move from **cash account → margin account** --- # 🧠 Financial Concepts Modeled This project introduces simplified versions of financial concepts used in banking environments. | Concept | Description | |------|------| | Exposure | Financial value of a position | | Collateral | Financial guarantee protecting the institution | | Margin | Portion of collateral segregated to mitigate risk | | Haircut | Risk adjustment applied to assets | | Custody | Systems responsible for holding client assets | | Margin Allocation | Segregation of funds into margin accounts | | Internal Transfer | Movement between internal ledgers | --- # 🧩 System Architecture The system is composed of multiple microservices behind an **API Gateway**.
┌──────────────────────┐
                │      API Gateway     │
                │        (YARP)        │
                └──────────┬───────────┘
                           │
        ┌──────────────────┼──────────────────┐
        │                  │                  │
 ┌─────────────┐   ┌─────────────┐   ┌─────────────┐
 │ Customers   │   │ Positions   │   │  Collateral │
 │   Service   │   │   Service   │   │   Service   │
 └─────────────┘   └─────────────┘   └──────┬──────┘
                                            │
                                    ┌─────────────┐
                                    │ MarginTransfer│
                                    │    Service    │
                                    └─────────────┘
--- # 🏗 Architectural Patterns Used ## Microservices Each domain capability is implemented as an independent service. ## API Gateway All external access goes through the **Gateway.Api** using **YARP (Reverse Proxy)**. ## Service Layer Business logic is isolated from HTTP endpoints. ## Shared Kernel (BuildingBlocks) Reusable components shared across services. ## Resilience Between Services Service communication uses **HttpClient + Polly**. Implemented patterns: - Retry with exponential backoff - Circuit breaker - Timeout control ## Minimal APIs All services use **ASP.NET Core Minimal APIs** for lightweight endpoints. --- # 📦 Services ## Customers.Api Responsible for **client registration**. Creates a new customer record containing: - Name - Document - Generated CustomerId (GUID) Example response:
json
{
  "id": "7c37dc36-1054-4fb2-a5fb-a1f9112e0e39"
}
The customerId becomes the primary identifier across all services. --- ## Positions.Api Stores **financial positions held by a client**. A position represents ownership of an asset. Example: Customer owns **20 shares of BTG** Price = **1000** Fields: - customerId - symbol - assetType - quantity - price - currency ⚠️ Positions are **stored in memory** in this didactic implementation. --- ## Collateral.Api Simulates a **risk engine** responsible for computing collateral requirements. Workflow: 1. Receive customerId 2. Query Positions service 3. Calculate exposure 4. Apply haircut rules 5. Return required collateral --- ## MarginTransfer.Api Simulates **margin allocation and internal ledger transfers**. Two internal accounts exist for each client: | Account | Meaning | |-------|---------| | CASH | Available funds | | MARGIN | Collateral locked as guarantee | Example: Before transfer
CASH = 10000
MARGIN = 0
Required collateral
5000
Transfer executed
CASH → MARGIN
After transfer
CASH = 5000
MARGIN = 5000
--- # 📊 Collateral Calculation Logic Exposure
Exposure = Quantity × Price
Example
20 × 1000 = 20000
Haircut rules | Asset | Haircut | |------|---------| | CASH | 0% | | BOND | 10% | | EQUITY | 25% | Required collateral
RequiredCollateral = Exposure × Haircut
Example
20000 × 0.25 = 5000
--- # 🔄 Full Collateral Workflow
Client registered
       │
       ▼
Positions recorded
       │
       ▼
Exposure calculated
       │
       ▼
Haircut applied
       │
       ▼
Required collateral computed
       │
       ▼
Margin allocation
       │
       ▼
Funds transferred to Margin account
--- # ⚙️ Resilience Between Services Communication between services uses **HttpClient + Polly**. Implemented policies: - Retry (exponential backoff) - Circuit Breaker - Timeout Example flow:
Collateral.Api
      │
      ▼
HttpClient
      │
      ▼
Retry Policy
      │
      ▼
Circuit Breaker
      │
      ▼
Positions.Api
This prevents cascading failures in distributed systems. --- # 🧱 Shared Building Blocks A shared project called **BuildingBlocks** contains reusable components. Example:
ServiceResult<T>
Used to standardize service responses and error handling. Example:
csharp
return ServiceResult<Guid>.Success(customerId);
or
csharp
return ServiceResult<Guid>.Fail("Customer not found");
--- # 📁 Project Structure
src
 ├ Gateway.Api
 ├ Customers.Api
 ├ Positions.Api
 ├ Collateral.Api
 ├ MarginTransfer.Api
 └ BuildingBlocks

infra
 └ docker-compose.yml

docs
 └ architecture
Each API follows a simplified internal structure:
Api
 ├ Endpoints
 ├ Services
 └ Models
--- # 📡 API Gateway The gateway exposes a unified entry point. Routes:
/customers
/positions
/collateral
/margin
Clients interact only with the gateway. --- # ▶️ Running the Project ## Using Aspire 1. Open the solution 2. Run **CollateralPlayground.AppHost** 3. Access the Aspire dashboard --- ## Using Docker
docker compose -f infra/docker-compose.yml up --build
--- # 🚀 Future Improvements Possible enhancements for this architecture: - Database persistence - Event-driven architecture - Message broker (RabbitMQ / Kafka) - Observability (OpenTelemetry) - Distributed tracing - Integration tests - Authentication / authorization - Container orchestration (Kubernetes) --- # 👤 Author Wilson Martins da Silva
