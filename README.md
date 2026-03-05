
# Collateral Risk Microservices (Didático)

Este repositório contém um **projeto didático** para demonstrar, na prática, como funciona uma arquitetura de **microserviços** com:

- **APIs independentes** (Customers, Positions, Collateral, MarginTransfer)
- **API Gateway** usando **YARP (Yet Another Reverse Proxy)**
- **Orquestração** via **.NET Aspire AppHost**
- Execução alternativa via **Docker Compose**

> ⚠️ **IMPORTANTE (Didático)**
>
> Este projeto **NÃO é produção**. Foi construído para **estudo** e demonstração de conceitos:
> microserviços, comunicação HTTP entre serviços, gateway/reverse proxy, roteamento, Swagger, e orquestração.

---

# 🎯 O que o projeto simula

Um fluxo simplificado de sistemas de **risco e collateral utilizados em bancos**.

1. **Cadastrar um cliente**
2. **Registrar posições** (ativos na carteira)
3. **Calcular o collateral / margem necessária**
4. **Executar transferência de margem** (segregar garantia)

---

# 🧩 Arquitetura e serviços

## 1) Customers.Api

Responsável pelo **cadastro de clientes**.

Cria cliente com:

- `name`
- `document`

Retorna:

- `customerId`

Esse ID será usado nas demais APIs.

---

## 2) Positions.Api

Responsável por armazenar as **posições financeiras do cliente**.

Uma posição representa um ativo na carteira.

Campos:

- `customerId`
- `symbol`
- `assetType` (CASH | BOND | EQUITY)
- `quantity`
- `price`
- `currency`

> Nesta versão didática as posições são **in-memory**.

---

## 3) Collateral.Api

Responsável por calcular:

- **Exposure**
- **Haircut**
- **Required Collateral**

Fluxo:

1. Recebe `customerId`
2. Busca posições na Positions.Api
3. Calcula exposição
4. Aplica regra de haircut
5. Retorna margem necessária

### Fórmulas

Exposure:

```
exposure = quantity * price
```

Required Collateral:

```
requiredCollateral = exposure * haircut
```

Haircuts (didático):

```
CASH   = 0%
BOND   = 10%
EQUITY = 25%
OTHER  = 30%
```

---

## 4) MarginTransfer.Api

Simula a **movimentação financeira da margem**.

Após calcular o collateral, o sistema precisa **segregar o valor como garantia**.

Essa API representa a movimentação:

```
CASH → MARGIN
```

Tipos de conta:

- **CASH** → dinheiro disponível
- **MARGIN** → dinheiro bloqueado como garantia

Esse processo é conhecido como:

**Simple Transfer**

---

# 🔄 Fluxo completo do sistema

### 1️⃣ Cliente possui ativos

Exemplo:

```
20 ações do BTG
Preço = 1000
```

```
Exposure = 20 * 1000
Exposure = 20000
```

---

### 2️⃣ Sistema calcula o risco

Haircut para ações:

```
25%
```

```
Required Collateral = 20000 * 0.25
Required Collateral = 5000
```

---

### 3️⃣ Sistema verifica contas

Exemplo:

```
CASH   = 10000
MARGIN = 0
```

---

### 4️⃣ Sistema cria uma boleta

Boleta representa uma **instrução de movimentação financeira**.

```
Transferir 5000
de CASH → MARGIN
```

---

### 5️⃣ Transferência executada

Após transferência:

```
CASH   = 5000
MARGIN = 5000
```

Agora o cliente possui garantia suficiente.

---

# 🧪 Exemplos de uso

Base URL:

```
http://localhost:5000
```

---

## Criar cliente

POST

```
/customers
```

Body

```json
{
  "name": "Wilson Martins",
  "document": "39700427803"
}
```

---

## Registrar posição

POST

```
/positions
```

```json
{
  "customerId": "GUID",
  "symbol": "PETR4",
  "assetType": "EQUITY",
  "quantity": 10,
  "price": 35,
  "currency": "BRL"
}
```

---

## Calcular collateral

POST

```
/collateral/calculate
```

```json
{
  "customerId": "GUID"
}
```

---

## Inicializar contas

POST

```
/margin/accounts/init
```

```json
{
  "customerId": "GUID",
  "cashBalance": 10000,
  "marginBalance": 0
}
```

---

## Executar transferência de margem

POST

```
/margin/transfers/simple
```

```json
{
  "customerId": "GUID",
  "amount": 5000,
  "fromAccount": "CASH",
  "toAccount": "MARGIN",
  "currency": "BRL",
  "reason": "MARGIN_ALLOCATION_FROM_COLLATERAL"
}
```

---

# ▶️ Como executar

## Opção 1 — .NET Aspire

1. Abrir solução no Visual Studio
2. Definir **AppHost** como startup
3. Executar

O dashboard exibirá:

- logs
- serviços
- endpoints

---

## Opção 2 — Docker Compose

Na raiz do projeto:

```
docker compose -f infra/docker-compose.yml up --build
```

Parar:

```
docker compose -f infra/docker-compose.yml down
```

---

# 📁 Estrutura

```
src/
 ├── Gateway.Api
 ├── Customers.Api
 ├── Positions.Api
 ├── Collateral.Api
 └── MarginTransfer.Api

infra/
 └── docker-compose.yml

docs/
 └── diagramas
```

---

# ✅ O que o projeto demonstra

- Arquitetura de microserviços
- API Gateway com YARP
- Comunicação entre APIs
- Cálculo de risco e collateral
- Simulação de margem
- Transferência financeira entre contas
- Conceitos usados em sistemas de risco bancário

---

# 👤 Autor

Wilson Martins

GitHub:
https://github.com/Wilsonmartins
