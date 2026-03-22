# FlowFi API

Financial Autopilot engine — .NET 9 Clean Architecture + CQRS + MediatR + PostgreSQL + Redis.

## Architecture

```
FlowFi.Domain          → Entities, enums, value objects. Zero dependencies.
FlowFi.Application     → CQRS commands/queries via MediatR. Depends on Domain only.
FlowFi.Infrastructure  → EF Core, Redis, JWT, bcrypt. Implements Application interfaces.
FlowFi.API             → ASP.NET Core controllers. Depends on Application + Infrastructure.
```

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9)
- [Docker + Docker Compose](https://docs.docker.com/get-docker/)
- [dotnet-ef tool](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)

```bash
dotnet tool install --global dotnet-ef
```

## Quick Start (Docker)

```bash
# Clone
git clone https://github.com/AbhimanyuShrestha/FlowFi-New.git
cd FlowFi-New

# Start everything (PostgreSQL + Redis + API)
docker-compose up --build

# API available at:
# http://localhost:3000/api/v1/health
# http://localhost:3000/swagger
```

## Local Development (without Docker API container)

```bash
# 1. Start only the dependencies
docker-compose up postgres redis

# 2. Run EF Core migrations
dotnet ef database update \
  --project src/FlowFi.Infrastructure \
  --startup-project src/FlowFi.API

# 3. Run the API
cd src/FlowFi.API
dotnet run
```

## Environment Variables

| Variable | Example | Description |
|----------|---------|-------------|
| `ConnectionStrings__Database` | `Host=localhost;Database=flowfi;Username=flowfi;Password=...` | PostgreSQL connection |
| `Jwt__Secret` | 32+ char random string | JWT signing key — NEVER commit |
| `Jwt__Issuer` | `flowfi-api` | JWT issuer claim |
| `Jwt__Audience` | `flowfi-app` | JWT audience claim |
| `Jwt__AccessExpirySeconds` | `900` | Access token TTL (15 min) |
| `Jwt__RefreshExpirySeconds` | `2592000` | Refresh token TTL (30 days) |
| `Redis__ConnectionString` | `localhost:6379` | Redis connection |

For local development, copy values into `src/FlowFi.API/appsettings.Development.json` (gitignored).
For production (Railway), set as environment variables directly.

## Running Tests

```bash
dotnet test
```

## Migrations

```bash
# Add a new migration
dotnet ef migrations add <MigrationName> \
  --project src/FlowFi.Infrastructure \
  --startup-project src/FlowFi.API \
  --output-dir Persistence/Migrations

# Apply migrations
dotnet ef database update \
  --project src/FlowFi.Infrastructure \
  --startup-project src/FlowFi.API
```

## API Endpoints

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| `GET` | `/api/v1/health` | ❌ | Health check |
| `POST` | `/api/v1/auth/register` | ❌ | Register |
| `POST` | `/api/v1/auth/login` | ❌ | Login |
| `POST` | `/api/v1/auth/refresh` | ❌ | Refresh token |
| `GET` | `/api/v1/dashboard` | ✅ | Full dashboard |
| `GET` | `/api/v1/transactions` | ✅ | List transactions |
| `POST` | `/api/v1/transactions` | ✅ | Create transaction |
| `PATCH` | `/api/v1/transactions/:id` | ✅ | Update transaction |
| `DELETE` | `/api/v1/transactions/:id` | ✅ | Delete transaction |
| `GET` | `/api/v1/categories` | ✅ | List categories |

Full Swagger docs available at `/swagger` when running in Development mode.

## Deployment (Railway)

1. Connect this GitHub repo to Railway
2. Add a PostgreSQL plugin — Railway auto-injects `DATABASE_URL`
3. Set environment variables:
   ```
   ConnectionStrings__Database=${{Postgres.DATABASE_URL}}
   Jwt__Secret=<generate with: openssl rand -base64 32>
   Jwt__Issuer=flowfi-api
   Jwt__Audience=flowfi-app
   Redis__ConnectionString=<upstash-url>,password=<token>,ssl=true
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://+:8080
   ```
4. Deploy — Railway uses the `Dockerfile` automatically

## Future: Supabase Migration

Change one environment variable — no code changes needed:
```
ConnectionStrings__Database=postgresql://postgres:[password]@db.[ref].supabase.co:5432/postgres
```
