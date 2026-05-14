# opssphere
Enterprise Support Operations Platform built with .NET 10, Angular, SQL Server, Clean Architecture, CQRS, JWT authentication, role-based authorization, audit logging, Docker and CI/CD.

## Backend Local Validation

The backend solution uses `OpsSphere.slnx` and targets .NET 10.

```bash
dotnet restore OpsSphere.slnx
dotnet build OpsSphere.slnx
dotnet test OpsSphere.slnx
```

## Frontend Local Validation

The Angular frontend lives under `frontend/`, uses Angular Material for the MVP UI foundation, and follows the `/api` base URL convention for business API calls.

```bash
cd frontend
npm install
npm run build
npm run test
npm start
```

## Local Infrastructure (Docker Compose)

### Requirements
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) or Docker Engine with Compose plugin installed.

### Setup
1. Copy the example environment file and set your local password:
   ```bash
   cp .env.example .env
   ```
   Edit `.env` and replace the placeholder password in `MSSQL_SA_PASSWORD`.

2. Validate the Compose configuration:
   ```bash
   docker compose config
   ```

3. Start SQL Server:
   ```bash
   docker compose up -d
   ```

4. Check container status:
   ```bash
   docker compose ps
   ```

### Local Connection String
Use the following connection string in `appsettings.Development.json` or .NET user secrets (replace the password):
```
Server=localhost,1433;Database=OpsSphereDb;User Id=sa;Password=<YOUR_LOCAL_SA_PASSWORD>;TrustServerCertificate=True;
```

> **Note:** Connection strings and passwords must not be committed to source control. Use `appsettings.Development.json` (gitignored locally), environment variables, or .NET user secrets for local development.

### Common Commands
| Command | Description |
|---|---|
| `docker compose up -d` | Start SQL Server in background |
| `docker compose ps` | Check container status |
| `docker compose down` | Stop containers (data preserved) |
| `docker compose down -v` | Stop and delete local database volume |

> **Note:** This Docker Compose configuration is for local development only — not for production or CI.

## EF Core Migrations

### Prerequisites
- SQL Server running locally (via Docker Compose above)
- Set your local password in `.env` (copy from `.env.example`)
- Update `appsettings.Development.json` with your local password (do not commit)

### Manage Migrations

**List migrations:**
```bash
dotnet ef migrations list --project src/OpsSphere.Infrastructure --startup-project src/OpsSphere.Api
```

**Apply migration to local database:**
```bash
dotnet ef database update --project src/OpsSphere.Infrastructure --startup-project src/OpsSphere.Api
```

**Add a new migration (development only):**
```bash
dotnet ef migrations add <MigrationName> --project src/OpsSphere.Infrastructure --startup-project src/OpsSphere.Api --output-dir Persistence/Migrations
```

> **Security:** Never commit `.env` or real passwords. Use `appsettings.Development.json` (gitignored locally), .NET user secrets, or environment variables.
> **Seed data:** Fictional seed data is applied by the API at startup only when the environment is `Development` or `Testing` and `SeedData:Enabled` is `true`.
> **Local only:** These commands apply to local development. Do not apply migrations directly to production.

## Local Seed Data

OpsSphere includes deterministic fictional seed data for local development, demos, and automated tests. The seed uses fixed IDs and natural keys so it can be run repeatedly without creating duplicates.

Seed execution is intentionally runtime-based through Infrastructure, not EF migration `HasData`. It runs only when both conditions are true:

- ASP.NET Core environment is `Development` or `Testing`
- `SeedData:Enabled` is `true`

The local demo users are:

| Email | Role | Scope |
|---|---|---|
| `admin@opssphere.local` | Admin | Role-based administrative access |
| `manager.latam@opssphere.local` | OperationsManager | LATAM region |
| `supervisor.novabank@opssphere.local` | Supervisor | NOVABANK account |
| `agent.novabank@opssphere.local` | Agent | NOVABANK-CC campaign |
| `viewer.latam@opssphere.local` | Viewer | LATAM region |

The local/demo password is `OpsSphere123!`. It is hashed before persistence and is not stored as plaintext or logged. This password is for local development and demos only.

Seeded organization data is fictional:

- Regions: LATAM / Latin America, NA / North America
- Countries: MX / Mexico, CO / Colombia, CR / Costa Rica, US / United States
- Accounts: NovaBank, Streamly, Shopora, AeroLink
- Campaigns: Credit Card Support, Fraud Review Support, Creator Support, Account Access Support, Travel Support

No production secrets, real company data, customer data, employee data, tokens, or production credentials are committed.

### Local Apply Flow

```bash
docker compose up -d
dotnet restore OpsSphere.slnx
dotnet build OpsSphere.slnx
dotnet ef database update --project src/OpsSphere.Infrastructure --startup-project src/OpsSphere.Api
dotnet run --project src/OpsSphere.Api
dotnet test OpsSphere.slnx
```

### Local Reset Flow

```bash
docker compose down -v
docker compose up -d
dotnet ef database update --project src/OpsSphere.Infrastructure --startup-project src/OpsSphere.Api
dotnet run --project src/OpsSphere.Api
```

### Frontend API Base URL
The Angular development environment is configured in `frontend/src/environments/environment.development.ts`. The `apiBaseUrl` property should point to your locally running backend API (e.g., `http://localhost:5000/api` or `https://localhost:7024/api` per `launchSettings.json`).

## Local JWT Authentication

The API uses JWT bearer authentication for internal users. Local development needs these configuration values:

```json
"Jwt": {
  "Issuer": "OpsSphere",
  "Audience": "OpsSphere.Angular",
  "ExpirationMinutes": 60,
  "SigningKey": "<local-development-only-signing-key>"
}
```

`appsettings.json` intentionally keeps `Jwt:SigningKey` blank. Use .NET user secrets, environment variables, or a clearly local-only development value. Do not commit production signing keys or real secrets. `appsettings.Development.json` contains only a fictional local signing key for developer convenience.

Seeded login users are fictional and share the local/demo password `OpsSphere123!`. Passwords are hashed before persistence, and passwords, password hashes, JWT tokens, Authorization headers, signing keys, and connection strings must not be logged.

Useful auth smoke checks after the API starts:

```bash
dotnet run --project src/OpsSphere.Api
```

- `POST /api/auth/login` with `agent.novabank@opssphere.local` and `OpsSphere123!` should return a bearer token.
- `GET /api/auth/me` should return 401 without a token and the current seeded profile with a valid token.
- `GET /api/auth/protected-smoke` should return 401 without a token and 200 with a valid token.

## Documentation Approach

OpsSphere follows a practical enterprise documentation workflow inspired by:

- Business Analysis practices for business needs, stakeholders, requirements, and process modeling.
- Project Management practices for project definition, scope, risks, and approval criteria.
- Software Architecture documentation practices for architecture decisions, system context, deployment view, and quality attributes.
- C4 and UML diagrams for visual communication of architecture, use cases, domain models, and runtime behavior.

The documentation is organized from business understanding to technical implementation:

1. Business Discovery
2. Project Definition
3. Requirements Analysis
4. Process and Domain Modeling
5. Architecture and Technical Design
6. Delivery Planning

## Implementation Guardrails

Implementation work should follow `docs/22-implementation-guardrails.md` for issue scope, branch naming, PR validation, architecture boundaries, MVP limits, and Definition of Done.
