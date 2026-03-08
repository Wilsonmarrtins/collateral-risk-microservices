Collateral Risk Microservices (Didático)
Este repositório contém um projeto didático para demonstrar, na prática, como funciona uma arquitetura de microserviços com:

APIs independentes (Customers, Positions, Collateral)
API Gateway usando YARP (Yet Another Reverse Proxy)
Orquestração para subir tudo junto (via .NET Aspire AppHost) e também via Docker Compose
⚠️ IMPORTANTE (Didático)

Este projeto NÃO é produção. Foi construído para estudo e demonstração de conceitos: microserviços, comunicação HTTP entre serviços, gateway/reverse proxy, roteamento, Swagger, e orquestração.

🎯 O que o projeto simula
Um fluxo comum em ambientes de risco/garantias:

Cadastrar um cliente
Registrar posições (simulação de compra/posse de ativos)
Calcular o collateral (garantia) necessário com base nas posições e em regras simples de “haircut”
🧩 Arquitetura e serviços
1) Customers.Api
Responsável por cadastro de clientes.

Cria cliente com name e document
Retorna um id (GUID) para ser usado nas demais APIs
2) Positions.Api
Responsável por armazenar posições do cliente (in-memory).

Uma posição representa que o cliente possui um ativo (ex.: PETR4, VALE3, AAPL etc.)
Campos:
customerId
symbol
assetType (CASH | BOND | EQUITY)
quantity
price
currency
Nesta versão didática, as posições ficam em memória (não persistem em banco).

3) Collateral.Api
Responsável por calcular exposição e collateral necessário.

Fluxo:

Recebe um customerId
Chama a Positions.Api para obter as posições desse cliente
Calcula:
Exposure = quantity * price
Haircut (regra simples por tipo de ativo)
RequiredCollateral = exposure * haircut
Soma os totais e devolve um resumo
4) Gateway.Api (YARP)
O gateway “junta” as APIs em um único endereço e faz roteamento por path:

/customers/** → encaminha para Customers.Api (/v1/customers/**)
/positions/** → encaminha para Positions.Api (/v1/positions/**)
/collateral/** → encaminha para Collateral.Api (/v1/collateral/**)
🧠 Como funciona o cálculo de Collateral (didático)
O cálculo implementado é uma regra didática baseada em “haircuts”.

Haircuts (didático)
CASH → 0%
BOND → 10%
EQUITY → 25%
outros → 30%
Cálculo por posição
Exposure = quantity * price
RequiredCollateral = Exposure * Haircut
Exemplo rápido
Se o cliente tem:

10 ações de PETR4 a R$ 35,00
assetType = EQUITY → haircut 25%
Então:

Exposure = 10 * 35 = 350
RequiredCollateral = 350 * 0.25 = 87.5
⚠️ Em sistemas reais, haircuts vêm de regras regulatórias, risco, volatilidade, liquidez, ratings etc. Aqui é simplificado para facilitar o entendimento.

✅ Exemplos de uso (Postman / curl)
Abaixo uso como base o Gateway exposto em:

http://localhost:5000

0) Pré-requisito
No Postman, envie JSON com:

Header: Content-Type: application/json
1) Criar cliente
POST http://localhost:5000/customers

Body:

{
  "name": "Wilson Martins",
  "document": "39700427803"
}
Resposta (exemplo):

{
  "id": "fb58614a-128a-4460-a38c-2f6e79e901b8"
}
Guarde o id, ele será o customerId nas próximas chamadas.

2) Registrar posição (simular compra/posse de ativos)
POST http://localhost:5000/positions

Body (exemplo em BRL):

{
  "customerId": "fb58614a-128a-4460-a38c-2f6e79e901b8",
  "symbol": "PETR4",
  "assetType": "EQUITY",
  "quantity": 10,
  "price": 35.00,
  "currency": "BRL"
}
Outra posição:

{
  "customerId": "fb58614a-128a-4460-a38c-2f6e79e901b8",
  "symbol": "VALE3",
  "assetType": "EQUITY",
  "quantity": 5,
  "price": 70.00,
  "currency": "BRL"
}
Resposta (exemplo):

{
  "message": "Position upserted."
}
3) Listar posições do cliente
GET http://localhost:5000/positions?customerId=fb58614a-128a-4460-a38c-2f6e79e901b8

Resposta (exemplo):

[
  {
    "customerId": "fb58614a-128a-4460-a38c-2f6e79e901b8",
    "symbol": "VALE3",
    "assetType": "EQUITY",
    "quantity": 5,
    "price": 70,
    "currency": "BRL",
    "updatedAt": "2026-03-05T02:10:00Z"
  },
  {
    "customerId": "fb58614a-128a-4460-a38c-2f6e79e901b8",
    "symbol": "PETR4",
    "assetType": "EQUITY",
    "quantity": 10,
    "price": 35,
    "currency": "BRL",
    "updatedAt": "2026-03-05T02:05:00Z"
  }
]
4) Calcular collateral
POST http://localhost:5000/collateral/calculate

Body:

{
  "customerId": "fb58614a-128a-4460-a38c-2f6e79e901b8"
}
Resposta (exemplo):

{
  "customerId": "fb58614a-128a-4460-a38c-2f6e79e901b8",
  "totalExposure": 700,
  "totalRequiredCollateral": 175,
  "items": [
    {
      "symbol": "PETR4",
      "assetType": "EQUITY",
      "exposure": 350,
      "haircut": 0.25,
      "requiredCollateral": 87.5,
      "currency": "BRL"
    },
    {
      "symbol": "VALE3",
      "assetType": "EQUITY",
      "exposure": 350,
      "haircut": 0.25,
      "requiredCollateral": 87.5,
      "currency": "BRL"
    }
  ]
}
▶️ Como executar
Você tem duas opções principais:

Opção A) Executar com .NET Aspire (recomendado no DEV)
Abra a solução no Visual Studio
Defina o projeto CollateralPlayground.AppHost como Startup Project
Rode (F5)
Abra o Dashboard do Aspire e você verá:
Todos os serviços
Logs centralizados
Links para Swagger/URLs
✅ Vantagem: um único “start” sobe tudo e você tem observabilidade básica no dashboard.

Opção B) Executar com Docker Compose
Pré-requisitos
Docker Desktop instalado e rodando
Subir tudo
Este projeto possui a pasta infra/ com o compose.

No terminal, na raiz do repositório (onde estão src/ e infra/), rode:

docker compose -f infra/docker-compose.yml up --build
Parar tudo
docker compose -f infra/docker-compose.yml down
URLs (quando rodando via compose)
Geralmente você acessa pelo Gateway:

Gateway: http://localhost:5000
E os serviços diretos (se você expôs portas no compose):

Customers.Api: http://localhost:5001/swagger
Positions.Api: http://localhost:5002/swagger
Collateral.Api: http://localhost:5003/swagger
Observação: as portas podem variar conforme seu docker-compose.yml.

🔎 Swagger
Quando rodando localmente, cada API expõe seu Swagger em /swagger.

Customers.Api → /swagger
Positions.Api → /swagger
Collateral.Api → /swagger
O Gateway pode (opcionalmente) agregar endpoints do Swagger de cada serviço.

📁 Estrutura do repositório
src/
Gateway.Api
Customers.Api
Positions.Api
Collateral.Api
infra/
docker-compose.yml
docs/
materiais auxiliares (prints, diagramas, etc.)
CollateralPlayground/ (ou pasta similar da solução)
solução/arquivos do Aspire AppHost
✅ O que este projeto demonstra
Separação por responsabilidade (cada serviço com seu domínio)
Comunicação HTTP entre serviços
API Gateway com YARP (reverse proxy)
Transforms e roteamento por path
Swagger para testar endpoints
Orquestração (Aspire) e alternativa com Docker Compose
🚫 O que este projeto NÃO é (limitações propositalmente didáticas)
Não possui autenticação/autorização
Positions é in-memory (sem banco)
Haircuts são regras simplificadas
Não tem observabilidade completa (OTel + tracing distribuído + métricas)
Não tem resilência avançada (retry/circuit breaker) configurada
👤 Autor
Wilson Martins
GitHub: https://github.com/Wilsonmartins
