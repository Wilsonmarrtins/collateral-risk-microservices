
# Collateral Risk Microservices
### Didactic Microservices Architecture for Collateral & Margin Simulation

This repository demonstrates a **didactic microservices architecture** that simulates how **collateral and margin management systems** work inside financial institutions.

The project focuses on concepts commonly found in **risk engines, custody systems, and collateral management platforms** used by investment banks and broker-dealers.

⚠️ **Important**
This project is **educational only**. It simplifies many financial and architectural concepts in order to demonstrate:

- Microservices architecture
- API Gateway pattern
- Service communication
- Collateral and margin calculations
- Financial system concepts (custody, collateral, margin allocation)

It is **not intended for production use**.

---

# 🎯 What This Project Demonstrates

This project simulates a simplified **Collateral Risk Flow** used by financial institutions.

Typical real-world flow:

1. A **client exists in the system**
2. The client **holds financial positions**
3. A **risk engine calculates exposure**
4. The system determines **required collateral**
5. If needed, the system **segregates margin**
6. Funds are transferred from **cash account → margin account**

---

# 🧠 Real Financial Concepts Modeled

The project introduces simplified versions of concepts used in banking environments:

| Concept | Description |
|------|------|
| Exposure | Total financial value of a position |
| Collateral | Financial guarantee protecting the institution |
| Margin | Portion of collateral that must be segregated |
| Haircut | Risk adjustment applied to an asset |
| Custody | Systems responsible for holding assets |
| Margin Allocation | Movement of funds to margin accounts |
| Simple Transfer | Transfer between internal accounts |

---

# 🧩 Architecture Overview

The system is composed of multiple microservices:

```
                ┌──────────────────────┐
                │      API Gateway     │
                │       (YARP)         │
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
```

---

# 📦 Services

## Customers.Api

Responsible for **client registration**.

Creates a new customer record containing:

- Name
- Document
- Generated CustomerId (GUID)

The `customerId` becomes the primary identifier used across all services.

---

## Positions.Api

Stores **financial positions held by a client**.

A position represents ownership of an asset.

Example:

Customer owns 20 shares of BTG  
Price = 1000

Fields:

- customerId
- symbol
- assetType
- quantity
- price
- currency

⚠️ Positions are **stored in memory** in this didactic version.

---

## Collateral.Api

Simulates a **risk engine** responsible for computing collateral requirements.

Process:

1. Receive `customerId`
2. Query positions
3. Calculate exposure
4. Apply haircut rules
5. Return required collateral

---

# 📊 Collateral Calculation Logic

Exposure

```
Exposure = Quantity × Price
```

Example:

```
20 shares × 1000 = 20000 exposure
```

Haircut rules

| Asset | Haircut |
|------|---------|
| CASH | 0% |
| BOND | 10% |
| EQUITY | 25% |

Required Collateral

```
RequiredCollateral = Exposure × Haircut
```

Example:

```
20000 × 0.25 = 5000
```

---

# 💰 MarginTransfer.Api

Simulates **margin allocation**.

Two internal accounts:

| Account | Meaning |
|-------|---------|
| CASH | Available funds |
| MARGIN | Collateral locked as guarantee |

Example flow:

Before transfer

```
CASH = 10000
MARGIN = 0
```

Required collateral

```
5000
```

Transfer executed

```
CASH → MARGIN
```

After transfer

```
CASH = 5000
MARGIN = 5000
```

---

# 🔄 Full Collateral Workflow

```
Client
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
Required Collateral computed
   │
   ▼
Margin Allocation
   │
   ▼
Funds transferred to Margin Account
```

---

# 📡 API Gateway

The gateway exposes a unified entry point.

Routes:

- /customers
- /positions
- /collateral
- /margin

---

# ▶️ Running the Project

## Aspire

1. Open solution
2. Run AppHost
3. Access dashboard

## Docker

```
docker compose -f infra/docker-compose.yml up --build
```

---

# 📁 Structure

```
src
 ├ Gateway.Api
 ├ Customers.Api
 ├ Positions.Api
 ├ Collateral.Api
 └ MarginTransfer.Api

infra
 └ docker-compose.yml
```

---

# 👤 Author

Wilson Martins
