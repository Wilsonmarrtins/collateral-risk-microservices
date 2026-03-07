# Collateral Risk Microservices

### Didactic Microservices Architecture with Keycloak Authentication

![.NET](https://img.shields.io/badge/.NET-9-blue)
![Architecture](https://img.shields.io/badge/architecture-microservices-blueviolet)
![Auth](https://img.shields.io/badge/authentication-keycloak-orange)
![License](https://img.shields.io/badge/license-MIT-green)

This repository demonstrates a **didactic microservices architecture**
that simulates how **collateral and margin management systems** work
inside financial institutions such as **investment banks, broker‑dealers
and clearing houses**.

The project focuses on demonstrating how **modern backend systems are
structured**, including:

-   Microservices
-   API Gateway
-   Authentication with Keycloak
-   JWT validation
-   Service‑to‑service communication
-   Collateral calculation
-   Margin allocation
-   Resilience patterns

⚠️ **Important**\
This project is **educational only**. Several aspects are simplified to
focus on architecture and concepts.

------------------------------------------------------------------------

# 🎯 Project Objective

The system simulates a simplified **Collateral Risk Flow** used in
financial institutions.

Typical workflow:

1.  A **client is registered**
2.  The client **holds financial positions**
3.  Exposure is calculated
4.  The system determines **required collateral**
5.  Funds are **segregated into margin accounts**
6.  Internal transfer from **cash → margin** occurs

------------------------------------------------------------------------

# 🧠 Financial Concepts Modeled

  Concept             Description
  ------------------- ---------------------------------------------------
  Exposure            Financial value of a position
  Collateral          Financial guarantee protecting the institution
  Margin              Portion of collateral segregated to mitigate risk
  Haircut             Risk adjustment applied to assets
  Custody             Systems responsible for holding client assets
  Margin Allocation   Segregation of funds into margin accounts
  Internal Transfer   Movement between internal ledgers

------------------------------------------------------------------------

# 🧩 System Architecture

The system is composed of multiple **microservices behind an API
Gateway**.

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

Each service is responsible for a **specific domain capability**.

------------------------------------------------------------------------

# 🔐 Authentication Architecture

Authentication is handled by **Keycloak**, which acts as the **Identity
Provider (IdP)**.

Keycloak is responsible for:

-   User authentication
-   Identity management
-   JWT token generation

Important:

**Keycloak is NOT part of the microservice processing flow.**\
It only **issues tokens used to access the APIs**.

------------------------------------------------------------------------

# 🔄 Authentication Flow

                ┌──────────────┐
                │    Client    │
                │ Postman/Web  │
                └──────┬───────┘
                       │
                       │ Login
                       ▼
                ┌──────────────┐
                │   Keycloak   │
                │ Identity Provider
                └──────┬───────┘
                       │
                JWT Access Token
                       │
                       ▼
                ┌──────────────┐
                │  API Gateway │
                │     YARP     │
                └──────┬───────┘
                       │
           ┌───────────┼───────────┐
           ▼           ▼           ▼
      Customers     Positions    Collateral
        Service       Service      Service
                                          │
                                          ▼
                                   MarginTransfer
                                       Service

Flow explanation:

1️⃣ Client authenticates with **Keycloak**\
2️⃣ Keycloak returns **JWT access token**\
3️⃣ Client calls the **API Gateway** with the token\
4️⃣ Gateway validates the token\
5️⃣ Request is routed to the correct microservice

Example request header:

    Authorization: Bearer <access_token>

------------------------------------------------------------------------

# 🏗 Architectural Patterns Used

## Microservices

Each domain capability is implemented as an independent service.

## API Gateway

All external access goes through the **Gateway.Api** using **YARP**.

Responsibilities:

-   Routing
-   Token validation
-   Single entry point

## Service Layer

Business logic is separated from HTTP endpoints.

## Shared Kernel (BuildingBlocks)

Reusable components shared across services.

## Resilience Between Services

Communication between services uses **HttpClient + Polly**.

Implemented patterns:

-   Retry
-   Circuit Breaker
-   Timeout

## Minimal APIs

All services use **ASP.NET Core Minimal APIs**.

------------------------------------------------------------------------

# 📦 Services

## Customers.Api

Responsible for **client registration**.

Creates a new customer with:

-   Name
-   Document
-   Generated `CustomerId` (GUID)

Example response:

``` json
{
  "id": "7c37dc36-1054-4fb2-a5fb-a1f9112e0e39"
}
```

------------------------------------------------------------------------

## Positions.Api

Stores **financial positions owned by a client**.

Fields:

-   customerId
-   symbol
-   assetType
-   quantity
-   price
-   currency

⚠️ Positions are **stored in memory** in this demo.

------------------------------------------------------------------------

## Collateral.Api

Simulates a **risk engine**.

Workflow:

1.  Receives `customerId`
2.  Queries Positions service
3.  Calculates exposure
4.  Applies haircut rules
5.  Returns required collateral

------------------------------------------------------------------------

## MarginTransfer.Api

Simulates **margin allocation and internal transfers**.

Two internal accounts exist:

  Account   Meaning
  --------- -------------------
  CASH      Available balance
  MARGIN    Locked collateral

Example:

Before transfer

    CASH = 10000
    MARGIN = 0

Required collateral

    5000

Transfer executed

    CASH → MARGIN

After transfer

    CASH = 5000
    MARGIN = 5000

------------------------------------------------------------------------

# 📊 Collateral Calculation Logic

Exposure

    Exposure = Quantity × Price

Example

    20 × 1000 = 20000

Haircut rules

  Asset    Haircut
  -------- ---------
  CASH     0%
  BOND     10%
  EQUITY   25%

Required collateral

    RequiredCollateral = Exposure × Haircut

Example

    20000 × 0.25 = 5000

------------------------------------------------------------------------

# ⚙️ Keycloak Configuration

Keycloak runs as a **container managed by .NET Aspire**.

Default configuration:

    Realm: collateral-playground
    Client: collateral-api

Token endpoint:

    http://localhost:8090/realms/collateral-playground/protocol/openid-connect/token

------------------------------------------------------------------------

# 📁 Project Structure

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

Each API follows this simplified structure:

    Api
     ├ Endpoints
     ├ Services
     └ Models

------------------------------------------------------------------------

# 📡 Gateway Routes

All external requests go through the gateway.

Available routes:

    /customers
    /positions
    /collateral
    /margin

All routes require **JWT authentication**.

------------------------------------------------------------------------

# ▶️ Running the Project

## Using Aspire

1.  Open the solution
2.  Run **CollateralPlayground.AppHost**
3.  Aspire will start all services including **Keycloak**
4.  Access the Aspire dashboard

------------------------------------------------------------------------

## Using Docker

    docker compose -f infra/docker-compose.yml up --build

------------------------------------------------------------------------

# 🚀 Future Improvements

Possible improvements:

-   Database persistence
-   Event‑driven architecture
-   Message broker (RabbitMQ / Kafka)
-   Observability (OpenTelemetry)
-   Distributed tracing
-   Role‑based authorization
-   Kubernetes deployment

------------------------------------------------------------------------

# 👤 Author

Wilson Martins da Silva
