# IPL Store

An ecommerce platform for selling official IPL franchise merchandise — jerseys, caps, flags, and more — built with ASP.NET Core Web API and Angular.

## Tech Stack

| Layer          | Technology                                  |
|----------------|---------------------------------------------|
| Backend API    | ASP.NET Core Web API (.NET 10)              |
| ORM            | Entity Framework Core 10                    |
| Database       | SQL Server                                  |
| Frontend       | Angular 22 with Angular Material            |
| Testing        | MSTest + Moq                                |

## Project Structure

```text
IPLStore/
├── IPLStore.Domain/            # Entities: Product, Cart, Order, Franchise
├── IPLStore.Application/       # Services, interfaces, DTOs, Result pattern
├── IPLStore.Infrastructure/    # EF Core DbContext, repository, migrations, seed data
├── IPLStore.API/               # ASP.NET Core controllers and DI setup
├── IPLStore.Tests/             # Unit tests for all service classes
IPLStore.UI/
└── ipl-store/                  # Angular SPA (standalone components, signals)
    ├── guards/                 # Route guards (auth guard)
    ├── services/               # ApiService, AuthService
    └── pages/                  # home, product-detail, cart, orders, login
```

## Features

* User login page with username capture (extensible for auth providers)
* Route guard redirects unauthenticated users to the login page
* Product listing with prices, pagination, and franchise/type filters
* Product detail page with description, stock info, and add-to-cart
* Search by product name, type, or franchise
* Shopping cart with add, update quantity, and remove
* Checkout that validates stock and decrements inventory
* Order history with expandable order details
* Concurrency handling via EF Core row versioning
* Global exception handler with structured problem+json responses

## Prerequisites

* .NET 10 SDK
* Node.js 20+ and npm
* SQL Server (LocalDB or full instance)

## Getting Started

### Backend

```bash
cd IPLStore

# Apply migrations and seed data
dotnet ef database update --project IPLStore.Infrastructure --startup-project IPLStore.API

# Run the API (defaults to https://localhost:7147)
dotnet run --project IPLStore.API
```

Swagger UI is available at `https://localhost:7147/swagger` in Development mode.

### EF Core Migrations

```bash
cd IPLStore

# Create a new migration after model changes
dotnet ef migrations add <MigrationName> --project IPLStore.Infrastructure --startup-project IPLStore.API

# Apply pending migrations to the database
dotnet ef database update --project IPLStore.Infrastructure --startup-project IPLStore.API

# Remove the last unapplied migration
dotnet ef migrations remove --project IPLStore.Infrastructure --startup-project IPLStore.API

# Generate a SQL script for deployment (idempotent — safe to re-run)
dotnet ef migrations script --idempotent --project IPLStore.Infrastructure --startup-project IPLStore.API --output migrate.sql

# Apply migrations at startup (alternative to CLI — add to Program.cs)
# using var scope = app.Services.CreateScope();
# var db = scope.ServiceProvider.GetRequiredService<IPLStoreDbContext>();
# db.Database.Migrate();
```

### Frontend

```bash
cd IPLStore.UI/ipl-store

npm install
npm start
```

The Angular dev server starts at `http://localhost:4200` and proxies `/api` requests to the backend.

## API Endpoints

| Method   | Route                              | Description                 |
|----------|------------------------------------|-----------------------------|
| `GET`    | `/api/products`                    | Search/list products        |
| `GET`    | `/api/products/{id}`               | Product details             |
| `GET`    | `/api/cart/{userId}`               | Get user cart               |
| `POST`   | `/api/cart/{userId}/items`         | Add item to cart            |
| `PUT`    | `/api/cart/{userId}/items/{id}`    | Update cart item quantity   |
| `DELETE` | `/api/cart/{userId}/items/{id}`    | Remove cart item            |
| `POST`   | `/api/orders/checkout/{userId}`    | Place order from cart       |
| `GET`    | `/api/orders/{userId}`             | Order history (paginated)   |
| `GET`    | `/api/orders/{userId}/{orderId}`   | Single order details        |

## Running Tests

```bash
cd IPLStore
dotnet test
```

## Authentication

The app uses a simple username-based login stored in `sessionStorage`. The `AuthService` manages user state via Angular signals and provides `login()`, `logout()`, `username()`, and `isLoggedIn()` methods. All routes except `/login` are protected by an `authGuard` that redirects unauthenticated users.

This design is intentionally minimal and ready to be extended with a real auth provider (e.g., Microsoft Entra ID, Auth0) by updating only the `AuthService`.

## Architecture

The solution follows a layered/clean architecture:

* **Domain** — entities with behavior (`Cart.AddOrUpdateItem`, `Order.FormOrderFromCart`)
* **Application** — service interfaces, DTOs (records), and a `Result<T>` pattern for typed error handling
* **Infrastructure** — single repository implementing separated read/write interfaces (CQRS-lite)
* **API** — thin controllers that delegate to services and map `Result<T>` to HTTP responses
