# ADR-003: Use JWT Authentication

## Status

Accepted

## Context

OpsSphere is an internal enterprise operations platform used by Admins, Operations Managers, Supervisors, Agents, and Viewers.

The platform must protect API endpoints and restrict access to operational data such as tickets, customers, users, roles, permissions, SLA state, dashboards, reports, and audit history.

OpsSphere also needs to support role-based authorization and scope-based visibility.

Users must only access the data and actions allowed by their role and assigned operational scope.

Examples:

```text
- Admins can manage users, roles, permissions, and operational structure.
- Operations Managers can view operational data within their assigned regions.
- Supervisors can manage tickets within assigned accounts or campaigns.
- Agents can work only on tickets within their assigned scope.
- Viewers can access read-only operational information.
```

Because OpsSphere uses an Angular frontend and an ASP.NET Core Web API backend, the authentication approach must work well with a browser-based frontend calling protected API endpoints over HTTP.

## Decision

OpsSphere will use JWT-based authentication for API access.

After a successful login, the backend will issue a JSON Web Token that the Angular frontend can include in subsequent API requests.

Protected API endpoints must require a valid token.

The backend will validate the token before allowing access to protected resources.

Role-based authorization and scope-based authorization will be enforced by the backend.

The frontend may use role and scope information to improve navigation and hide unauthorized actions, but frontend visibility must not be treated as the security boundary.

## Rationale

JWT authentication is a good fit for OpsSphere because it works naturally with a frontend SPA and a backend Web API.

The Angular frontend can authenticate once, receive a token, and send that token with API requests.

The ASP.NET Core Web API can validate the token and identify the authenticated user before applying authorization rules.

This supports the runtime flow:

```text
User
  → Angular Login Page
  → ASP.NET Core Auth Endpoint
  → Validate Credentials
  → Verify Active User
  → Load Roles and Scope
  → Generate JWT
  → Return Token to Frontend
  → Frontend Stores Token
  → User Accesses Protected Features
```

JWT also helps keep authentication separate from the frontend and business workflows.

The token establishes user identity, while the application still enforces authorization through roles, permissions, and operational scope.

This is important because OpsSphere must prevent unauthorized access to sensitive operational data.

## Consequences

### Positive Consequences

- JWT works well with Angular and ASP.NET Core Web API.
- Protected API endpoints can validate authentication consistently.
- The backend can identify the authenticated user for authorization and audit logging.
- The system can support role-based access control.
- The system can support scope-based visibility checks.
- The frontend can call APIs without depending on server-rendered sessions.
- The approach is common in modern API-based applications.
- The decision aligns with the selected .NET and Angular architecture.

### Negative Consequences

- Token handling must be implemented carefully to avoid security weaknesses.
- Token expiration and refresh behavior must be designed clearly.
- If a token is compromised, it may remain usable until expiration unless revocation is implemented.
- Storing tokens in the browser introduces security considerations.
- JWT payloads must not contain sensitive data.
- Logout behavior is less direct than server-side session invalidation unless token revocation or short-lived tokens are used.
- Scope and permission changes may not take effect immediately if old tokens remain valid.

## Alternatives Considered

### Cookie-Based Authentication

Cookie-based authentication is common for server-rendered web applications.

Rejected as the primary approach because OpsSphere is designed as an Angular SPA calling an ASP.NET Core Web API. JWT provides a clearer API-oriented authentication model for this architecture.

Cookie authentication may still be considered in the future if the frontend and backend deployment model changes or if a same-site browser security model becomes preferable.

### Server-Side Sessions

Server-side sessions would allow the backend to maintain active session state.

Rejected because OpsSphere does not require server-side session storage for the initial version. A stateless token approach is simpler for the Angular plus Web API model.

Server-side session tracking may be considered later if stricter revocation, device management, or enterprise session controls are required.

### Enterprise SSO

Enterprise SSO through an identity provider would be appropriate in a real production enterprise environment.

Rejected for the initial version because production-grade SSO is outside the MVP scope.

OpsSphere should remain Azure-ready and integration-ready, but the initial version will use local application authentication with JWT.

### API Keys

API keys are useful for machine-to-machine integrations.

Rejected for user authentication because OpsSphere is primarily used by internal human users with roles, permissions, and operational scopes.

API keys may be considered in the future for integrations, exports, automation, or external reporting pipelines.

## Implementation Notes

The authentication flow should follow this structure:

```text
1. User submits login credentials from Angular.
2. ASP.NET Core Auth endpoint receives the login request.
3. Application layer validates credentials.
4. System verifies that the user is active.
5. System loads the user's role and operational scope.
6. Token service generates a JWT.
7. API returns the token and minimal user profile information.
8. Angular stores the token according to the chosen security strategy.
9. Angular sends the token with protected API requests.
10. Backend validates the token.
11. Backend applies role-based and scope-based authorization.
12. Backend records audit-relevant actions with the authenticated user identity.
```

Protected endpoints must enforce:

```text
- Valid authentication token.
- Active user status where applicable.
- Required role.
- Required permission.
- Required operational scope.
```

JWT claims should be minimal.

Recommended claim types may include:

```text
- User identifier
- Email or username
- Role
- Scope reference when appropriate
- Token expiration
```

JWT claims should not include sensitive operational data.

Token expiration should be configured intentionally.

For the MVP, a simple expiration strategy is acceptable.

Future improvements may include:

```text
- Refresh tokens
- Token revocation
- Login audit events
- Failed login tracking
- Account lockout policies
- Enterprise SSO integration
- Azure Key Vault for signing secrets
```

## Decision Scope

This ADR defines JWT as the authentication approach for OpsSphere API access.

It does not define the full authorization matrix, password policy, refresh token strategy, token storage strategy, or enterprise SSO integration.

Those details should be handled in security documentation and implementation-specific tasks.