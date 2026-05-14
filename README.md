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

> **Note:** EF Core migrations and seed data are out of scope for this infrastructure setup. This Docker Compose configuration is for local development only — not for production or CI.

### Frontend API Base URL
The Angular development environment is configured in `frontend/src/environments/environment.development.ts`. The `apiBaseUrl` property should point to your locally running backend API (e.g., `http://localhost:5000/api` or `https://localhost:7024/api` per `launchSettings.json`).

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
