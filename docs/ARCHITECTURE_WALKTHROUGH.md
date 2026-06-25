# Flowly Architecture Walkthrough

This guide explains how the current Flowly backend maps to the design notes in `Finbit.pdf` and how the code is organized. The project is a domain-first personal finance API. The current strongest slice is Income, while Expense, Savings, and Investment are shaped as future-ready modules.

## 1. Product Idea

The design notes describe a personal finance administrator for people who want to organize money month by month. The repeated idea is:

```text
money enters -> money leaves -> monthly history -> remaining money awareness
```

The handwritten design also points to future assistant-like features:

- Convert currencies depending on source country and residence country.
- Upload screenshots or receipts and automatically detect expense type/category.
- Suggest better transfer routes when transfer costs are high.
- Export and import spreadsheets.
- Allow manual entry when a bank or institution is unsupported.
- Track income, expense, savings, and investment month by month.

The codebase currently implements the foundation first: domain rules for income, expenses, savings goals, investments, persistence ports, API endpoints, security, and tests.

## 2. Architecture Shape

Flowly follows Clean Architecture / Hexagonal Architecture:

```text
API -> Application -> Domain
Infrastructure -> Application ports
Infrastructure -> Domain persistence mappings
Tests -> Domain/Application/API
```

The key rule is dependency direction:

- `PersonalFinance.Domain` knows only business concepts.
- `PersonalFinance.Application` knows use cases and interfaces/ports.
- `PersonalFinance.Infrastructure` implements external details such as EF Core, in-memory stores, JWT, CORS, logging, and currency APIs.
- `PersonalFinance.API` exposes HTTP endpoints and wires the app together.

This means the business model can evolve from the design without being forced by controllers or database tables too early.

## 3. Startup Flow

The app starts in `src/PersonalFinance.API/Program.cs`.

Startup sequence:

1. Configure Serilog through `UseAppSerilog()`.
2. Register application services with `AddApplication()`.
3. Register infrastructure:
   - `UseDatabase: false` uses in-memory stores.
   - `UseDatabase: true` uses SQL Server and EF Core.
4. Register controllers, Swagger, and API versioning.
5. If using the database in development, run EF migrations.
6. Add middleware:
   - security headers
   - HTTPS redirection
   - Serilog request logging
   - CORS
   - exception handling
   - authentication/authorization
   - controllers

The default config uses in-memory persistence, so the API can run without a database.

## 4. Domain Layer

The Domain layer is the center of the project. It contains entities, aggregates, value objects, enums, events, and domain errors.

### Common Primitives

- `Entity`: gives every entity an `Id`, timestamps, equality by identity, and domain events.
- `AggregateRoot`: marks a consistency boundary. Aggregate roots are the main objects loaded and saved.
- `ValueObject`: compares objects by their component values instead of identity.
- `Result` and `Error`: model expected business failures without exception-driven control flow.
- `IDomainEvent`: domain events are MediatR notifications for in-process dispatch.

### Common Value Objects

- `Money`: amount plus currency. It rejects negative amounts.
- `CurrencyCode`: typed ISO-like 3-letter currency code, such as `COP`, `USD`, `EUR`.
- `DatePeriod`: year/month period for monthly organization.
- `TaxInfo`: gross amount, deduction rate, net amount, and deduction amount.

These value objects answer core design questions:

- "What if a value is negative?" -> `Money` and `TaxInfo` reject it.
- "How do I calculate net over gross?" -> `TaxInfo`.
- "How do I group money by month?" -> `DatePeriod`.
- "What if income is in another currency?" -> `CurrencyCode` and conversion support.

## 5. Income Domain

Income is the most complete domain slice.

Main files:

- `MonthlyIncome.cs`
- `IncomeEntry.cs`
- `IncomeType.cs`
- `IncomeTypeExtensions.cs`
- `IncomeNature.cs`
- `ContractType.cs`

`MonthlyIncome` means one user's income for one month. It owns many `IncomeEntry` records.

Important properties:

- `UserId`
- `Period`
- `HomeCurrency`
- `Entries`
- `TotalGross`
- `TotalNet`
- `TotalDeductions`
- `TotalNetInHomeCurrency`

Important behavior:

- `Create(...)`: creates the monthly income aggregate.
- `AddEntry(...)`: adds salary, bonus, freelance, prima, rental, dividend, interest, etc.
- `ConvertEntry(...)`: applies an exchange rate to one entry.

`IncomeEntry` means one income source. It stores:

- source name
- income type
- active/passive nature
- contract type
- tax info
- original currency
- optional converted amount
- received date
- notes

The default active/passive logic is:

- Passive: rental, dividend, interest.
- Active: salary, bonus, prima, freelance, international freelance, other.

This maps directly to the PDF questions I1-I6:

- Multiple income sources in one month.
- Gross/net calculation.
- Negative values rejected.
- Prima/bonus as active income.
- Extra/freelance work as active income.
- Foreign freelance income and currency conversion.

## 6. Expense Domain

Expense is present and aligned with the screenshot/category ideas from the design.

Main files:

- `MonthlyBudget.cs`
- `ExpenseEntry.cs`
- `ExpenseCategory.cs`
- `ExpenseNature.cs`

`MonthlyBudget` means one user's expenses for one month. It can link to a `MonthlyIncomeId`, which is the bridge to "money in vs money out."

`ExpenseEntry` stores:

- description
- amount
- category
- nature
- spent date
- merchant
- receipt image URL
- auto-categorized flag
- notes

The design idea "upload screenshot and automatically select expense type/category" is visible here through `ReceiptImageUrl` and `IsAutoCategorised`, but there is no OCR/AI adapter yet.

Expense categories include housing, utilities, debt, insurance, transport, food, health, education, entertainment, dining, travel, shopping, subscriptions, and other.

Expense nature is:

- obligatory
- variable
- leisure

## 7. Savings Domain

Savings is modeled as goals.

Main file:

- `SavingsGoal.cs`

A savings goal stores:

- user
- name
- goal type
- target amount
- current amount
- target date
- achieved status
- progress percent

Behavior:

- `Create(...)`
- `Deposit(...)`
- `Withdraw(...)`

The API currently exposes create and deposit, but not withdraw.

## 8. Investment Domain

Investment is modeled as an investment position.

Main file:

- `Investment.cs`

An investment stores:

- user
- name
- type
- status
- principal
- annual rate
- start date
- optional maturity date
- optional maturity value
- institution
- notes

Behavior:

- `Create(...)`
- `Liquidate(...)`
- `EstimatedYield`

The design note mentioning CDTs is represented by `InvestmentType.CDT`.

## 9. Application Layer

Application coordinates use cases. It does not own business rules. It loads aggregates, calls domain methods, saves through repository ports, and returns DTOs.

Important patterns:

- Commands change state.
- Queries read state.
- Validators check input shape.
- Handlers orchestrate work.
- DTO mappings shape output for the API.

Examples:

- `CreateMonthlyIncomeCommand` creates a monthly income record.
- `AddIncomeEntryCommand` adds one income source to an existing month.
- `ConvertIncomeCurrencyCommand` uses `ICurrencyConverter`, then updates the aggregate.
- `GetMonthlyIncomeQuery` fetches income by user and period.
- `AddExpenseCommand` adds an expense to an existing budget.
- `CreateSavingsGoalCommand` creates a savings goal.
- `DepositToSavingsCommand` deposits into a savings goal.
- `CreateInvestmentCommand` records an investment.

Pipeline behaviors:

- `ValidationBehavior`: runs FluentValidation before handlers.
- `LoggingBehavior`: logs request start, finish, slow requests, and failures.

Application ports:

- repositories
- unit of work
- currency converter

Infrastructure implements these ports.

## 10. API Layer

Controllers are thin HTTP adapters.

Current route groups:

- `/api/v1/income`
- `/api/v1/expense`
- `/api/v1/savings`
- `/api/v1/investments`

All controllers use `[Authorize]`, so clients need JWT bearer auth.

Income endpoints:

- `POST /api/v1/income/monthly`
- `GET /api/v1/income/monthly/{userId}/{year}/{month}`
- `POST /api/v1/income/monthly/{monthlyIncomeId}/entries`
- `POST /api/v1/income/monthly/{monthlyIncomeId}/entries/{entryId}/convert`

Expense endpoints:

- `GET /api/v1/expense/monthly/{userId}/{year}/{month}`
- `POST /api/v1/expense/monthly/{monthlyBudgetId}/entries`

Savings endpoints:

- `POST /api/v1/savings`
- `POST /api/v1/savings/{goalId}/deposit`

Investment endpoint:

- `POST /api/v1/investments`

Important API gap: there is no authentication/login endpoint yet, even though JWT validation and token generation service exist. There is also no public endpoint to create a monthly budget yet.

## 11. Infrastructure Layer

Infrastructure provides concrete details.

### Persistence Modes

`AddInfrastructureInMemory(...)` registers singleton `ConcurrentDictionary` stores and in-memory repositories. This is the default path and is best for early domain development.

`AddInfrastructure(...)` registers EF Core with SQL Server, repositories, and `AppDbContext`.

### EF Core

`AppDbContext` exposes aggregate sets:

- `MonthlyIncomes`
- `MonthlyBudgets`
- `SavingsGoals`
- `Investments`

It dispatches domain events before saving.

EF configurations map value objects into database columns:

- `DatePeriod` -> `PeriodYear`, `PeriodMonth`
- `CurrencyCode` -> string currency columns
- `Money` -> amount plus currency
- `TaxInfo` -> gross and deduction rate

### Currency Adapter

`ExchangeRateApiAdapter` implements `ICurrencyConverter` using ExchangeRate-API. It caches rates per base currency through `IMemoryCache`.

This is the technical support for the design goal: "convert currencies depending on source and residence country."

### Security

Security is centralized:

- JWT bearer auth
- CORS policy
- security response headers
- HTTPS redirection
- global exception handling
- Serilog logging

The base `appsettings.json` intentionally contains placeholders for secrets and API keys.

## 12. Tests And Verified State

Verified command:

```bash
dotnet test PersonalFinance.sln
```

Current result:

```text
Passed: 34 unit tests
Passed: 1 integration smoke test
Failed: 0
```

Important coverage:

- Money rejects negative amounts.
- Money requires same currency for addition/subtraction.
- TaxInfo calculates net and deduction amount.
- MonthlyIncome supports multiple income sources.
- Income active/passive classification works.
- Wrong-period income is rejected.
- Income domain events are raised.
- Savings deposits and withdrawals behave correctly.
- In-memory repository behavior works.
- AddIncomeEntry handler saves through repository and unit of work.

Current integration test is intentionally weak because authenticated test setup is not complete. It accepts either `201 Created` or `401 Unauthorized`.

## 13. What Is Implemented Vs What Is Design-Ready

Implemented strongly:

- Income domain rules.
- Gross/net income model.
- Active/passive income classification.
- Currency conversion flow shape.
- In-memory development persistence.
- EF Core persistence mappings.
- JWT-secured API shape.
- Tests for core income and value-object rules.

Design-ready but not fully implemented:

- Expense screenshot/OCR categorization.
- Transfer fee suggestions.
- Spreadsheet import/export.
- Bank/institution integration.
- Auth/login user flow.
- Monthly dashboard summary endpoint.
- Monthly budget creation endpoint.
- Savings withdrawal endpoint.
- Investment liquidation endpoint.
- Real domain event handlers.
- Production database migrations.

## 14. How To Continue Designing Safely

When adding a new design idea, move in this order:

1. Name the domain concept in plain language.
2. Decide which aggregate owns the rule.
3. Add or update value objects if the rule needs stronger typing.
4. Write domain tests for the rule.
5. Add an application command/query.
6. Add a DTO mapping.
7. Add or update repository methods only if needed.
8. Expose it through the API.
9. Add integration coverage when auth/test setup allows it.

For Flowly specifically, the next most natural design step is a monthly summary/dashboard slice:

```text
MonthlyIncome.TotalNetInHomeCurrency
- MonthlyBudget.TotalSpent
- Savings deposits for the month
= Net balance / remaining money
```

That would turn the current domain pieces into the first complete user-facing answer: "How much money remains this month?"
