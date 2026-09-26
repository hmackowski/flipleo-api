# Flipleo API

The REST API behind [Flipleo](https://flipleo.com), a profit tracker for resellers: people who buy items (often at online auctions), fix or upgrade them, and resell them. The API stores each user's auctions, flips and add-ons, and calculates parts cost, profit and ROI.

It's used by the [Flipleo web app](../flipleo.UI) (Angular) and stores its data in the [Flipleo database](../flipleo.Database) (SQL Server).

## Tech stack

- **ASP.NET Core 9** Web API (controllers)
- **Entity Framework Core 9** (database first) with a Unit of Work and generic repositories
- **SQL Server**
- **JWT** bearer authentication with our own `UserAccount` table (PBKDF2 password hashes)
- Built-in **rate limiting** on the sign-in endpoints
- **OpenAPI + Swagger UI** in Development

## Solution layout

References only point downward: Api → Services → Repository → Core.

| Project | Responsibility |
|---|---|
| `FlipLeo.Api` | Controllers, `Program.cs`, JWT, rate limiting, global error handler, current user, email sending |
| `FlipLeo.Services` | Business rules and validation, per-user data scoping, mapping entities to DTOs |
| `FlipLeo.Repository` | `FlipLeoContext`, entities, EF configurations, repositories, `FlipLeoUnitOfWork` (audit columns + soft delete) |
| `FlipLeo.Core` | DTOs, exceptions, constants, shared interfaces |

## Getting started

**Prerequisites:** .NET 9 SDK, SQL Server with the `FlipLeo` database (publish the [database project](../flipleo.Database)).

1. Check the connection string in `FlipLeo.Api/appsettings.Development.json`.
2. Set the JWT signing key (a secret, never committed). From the `FlipLeo.Api` folder:
   ```bash
   dotnet user-secrets set "Jwt:SigningKey" "<a long random string, at least 32 characters>"
   ```
3. Run it:
   ```bash
   dotnet run --project FlipLeo.Api
   ```
4. Open Swagger UI at `http://localhost:5142/swagger`. Call `POST /api/auth/register` or `login`, then click **Authorize** and paste the token.

### Email (password reset)

With no `Email:Host` configured, Development **writes reset links to the console** instead of sending them. To send real email, set the SMTP settings (password in user-secrets):

```bash
dotnet user-secrets set "Email:Host" "smtp.gmail.com"
dotnet user-secrets set "Email:Username" "you@gmail.com"
dotnet user-secrets set "Email:Password" "<app password>"
dotnet user-secrets set "Email:FromAddress" "you@gmail.com"
```

Other options (smtp4dev, Mailpit, Resend) are described in `FlipLeo.Api/Utilities/EmailSettings.cs`.

## Endpoints

Everything is under `/api` and requires a JWT, except the four public auth endpoints.

| Area | Routes |
|---|---|
| Auth | `POST auth/register`, `POST auth/login`, `POST auth/forgot-password`, `POST auth/reset-password` (public); `GET auth/me` |
| Auctions | `GET/POST/PUT auctions`, `GET/DELETE auctions/{id}` |
| Flips | `GET/POST/PUT flip-records`, `GET/DELETE flip-records/{id}`, `POST flip-records/{id}/add-ons`, `PUT flip-records/add-ons`, `DELETE flip-records/add-ons/{id}` |
| My Add-Ons | `GET/POST/PUT add-on-presets`, `DELETE add-on-presets/{id}` |
| Lookups | `GET lookups/auction-sites`, `GET lookups/flip-statuses` |

## Key rules

- **Every query is scoped to the signed-in user** in the service layer, and ids sent by the client (auction, preset, status) are checked before saving.
- **Nothing is hard deleted.** Deletes set `IsActive = 0` through the Unit of Work, and query filters hide inactive rows.
- **Audit columns** (`CreatedBy`, `UpdatedDate`, …) are filled in automatically on commit.
- **Profit is calculated, never stored**, and only counts once a flip is Sold.
- Errors are thrown as `NotFoundException` / `BadRequestException` / `UnauthorizedException` / `ConflictException` and returned as ProblemDetails by `GlobalExceptionHandler`.

Full conventions, patterns and decisions are in the **FlipLeo Developer Handbook** (`FlipLeo Developer Handbook.docx`).
