using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ============================
// Margin Transfer API (in-memory)
// ============================

var accountsDb = new ConcurrentDictionary<Guid, AccountLedger>();
var transfersDb = new ConcurrentDictionary<Guid, List<MarginTransfer>>();

// Helpers
static decimal Round2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

static AccountLedger GetOrCreateLedger(ConcurrentDictionary<Guid, AccountLedger> db, Guid customerId)
{
    return db.GetOrAdd(customerId, _ => new AccountLedger(
        CustomerId: customerId,
        CashBalance: 0m,
        MarginBalance: 0m,
        UpdatedAt: DateTimeOffset.UtcNow
    ));
}

var margin = app.MapGroup("/v1/margin")
    .WithTags("Margin");

// ----------------------------
// POST /v1/margin/accounts/init
// Inicializa/ajusta saldo (para testes)
// ----------------------------
margin.MapPost("/accounts/init", (InitAccountRequest req) =>
{
    if (req.CustomerId == Guid.Empty)
        return Results.BadRequest(new { error = "CustomerId is required." });

    if (req.CashBalance < 0 || req.MarginBalance < 0)
        return Results.BadRequest(new { error = "Balances must be >= 0." });

    var ledger = new AccountLedger(
        CustomerId: req.CustomerId,
        CashBalance: Round2(req.CashBalance),
        MarginBalance: Round2(req.MarginBalance),
        UpdatedAt: DateTimeOffset.UtcNow
    );

    accountsDb[req.CustomerId] = ledger;

    return Results.Ok(new { message = "Account ledger initialized.", ledger });
})
.WithName("InitAccount");

// ----------------------------
// GET /v1/margin/accounts?customerId=...
// Consulta saldos
// ----------------------------
margin.MapGet("/accounts", (Guid customerId) =>
{
    if (customerId == Guid.Empty)
        return Results.BadRequest(new { error = "customerId query param is required." });

    var ledger = accountsDb.TryGetValue(customerId, out var acc)
        ? acc
        : new AccountLedger(customerId, 0m, 0m, DateTimeOffset.UtcNow);

    return Results.Ok(ledger);
})
.WithName("GetAccount");

// ----------------------------
// POST /v1/margin/transfers/simple
// Simple Transfer: CASH -> MARGIN (ou o inverso, se quiser liberar margem)
// ----------------------------
margin.MapPost("/transfers/simple", (CreateMarginTransferRequest req) =>
{
    if (req.CustomerId == Guid.Empty)
        return Results.BadRequest(new { error = "CustomerId is required." });

    if (req.Amount <= 0)
        return Results.BadRequest(new { error = "Amount must be > 0." });

    var from = (req.FromAccount ?? "").Trim().ToUpperInvariant();
    var to = (req.ToAccount ?? "").Trim().ToUpperInvariant();

    // Permitimos apenas CASH <-> MARGIN
    var allowed = (from == "CASH" && to == "MARGIN") || (from == "MARGIN" && to == "CASH");
    if (!allowed)
        return Results.BadRequest(new { error = "Invalid transfer. Use FromAccount=CASH ToAccount=MARGIN or FromAccount=MARGIN ToAccount=CASH." });

    // Pega ledger (cria se não existir)
    var ledger = GetOrCreateLedger(accountsDb, req.CustomerId);

    // Como é in-memory, vamos “travar” por cliente para evitar corrida
    // (em prod seria transação no banco + lock/optimistic concurrency)
    lock (ledger.LockObj)
    {
        var amount = Round2(req.Amount);

        decimal cash = ledger.CashBalance;
        decimal marginBal = ledger.MarginBalance;

        // Valida saldo
        if (from == "CASH" && cash < amount)
            return Results.BadRequest(new { error = "Insufficient CASH balance.", cashBalance = cash, marginBalance = marginBal });

        if (from == "MARGIN" && marginBal < amount)
            return Results.BadRequest(new { error = "Insufficient MARGIN balance.", cashBalance = cash, marginBalance = marginBal });

        // Aplica transferência
        if (from == "CASH")
        {
            cash -= amount;
            marginBal += amount;
        }
        else // from == "MARGIN"
        {
            marginBal -= amount;
            cash += amount;
        }

        // Atualiza ledger
        var updated = ledger with
        {
            CashBalance = Round2(cash),
            MarginBalance = Round2(marginBal),
            UpdatedAt = DateTimeOffset.UtcNow
        };
        accountsDb[req.CustomerId] = updated;

        // Registra transfer
        var transfer = new MarginTransfer(
            Id: Guid.NewGuid(),
            CustomerId: req.CustomerId,
            Type: "SIMPLE_TRANSFER",
            FromAccount: from,
            ToAccount: to,
            Amount: amount,
            Currency: (req.Currency ?? "BRL").Trim().ToUpperInvariant(),
            Reason: (req.Reason ?? "MARGIN_ALLOCATION").Trim(),
            CreatedAt: DateTimeOffset.UtcNow
        );

        var list = transfersDb.GetOrAdd(req.CustomerId, _ => new List<MarginTransfer>());
        list.Add(transfer);

        return Results.Ok(new
        {
            message = "Transfer executed.",
            transfer,
            ledgerAfter = updated
        });
    }
})
.WithName("SimpleTransfer");

// ----------------------------
// GET /v1/margin/transfers?customerId=...
// Histórico de transfers
// ----------------------------
margin.MapGet("/transfers", (Guid customerId) =>
{
    if (customerId == Guid.Empty)
        return Results.BadRequest(new { error = "customerId query param is required." });

    var list = transfersDb.TryGetValue(customerId, out var txs)
        ? txs.OrderByDescending(x => x.CreatedAt).ToList()
        : new List<MarginTransfer>();

    return Results.Ok(list);
})
.WithName("ListTransfers");

app.Run();

// ============================
// Contracts
// ============================

record InitAccountRequest(Guid CustomerId, decimal CashBalance, decimal MarginBalance);

record CreateMarginTransferRequest(
    Guid CustomerId,
    decimal Amount,
    string FromAccount, // CASH | MARGIN
    string ToAccount,   // CASH | MARGIN
    string? Currency,
    string? Reason
);

record AccountLedger(
    Guid CustomerId,
    decimal CashBalance,
    decimal MarginBalance,
    DateTimeOffset UpdatedAt
)
{
    // Para lock por cliente (in-memory)
    public object LockObj { get; } = new();
}

record MarginTransfer(
    Guid Id,
    Guid CustomerId,
    string Type,        // SIMPLE_TRANSFER
    string FromAccount, // CASH | MARGIN
    string ToAccount,   // CASH | MARGIN
    decimal Amount,
    string Currency,
    string Reason,
    DateTimeOffset CreatedAt
);