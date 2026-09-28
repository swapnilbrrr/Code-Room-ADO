# Code-Room — Web Applications Development Coursework

[![ASP.NET Web Forms](https://img.shields.io/badge/ASP.NET-Web%20Forms-512BD4.svg)](https://dotnet.microsoft.com/apps/aspnet)
[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.7.2-512BD4.svg)](https://dotnet.microsoft.com/en-us/download/dotnet-framework)
[![ADO.NET](https://img.shields.io/badge/Data%20Access-ADO.NET-0078D4.svg)](https://learn.microsoft.com/dotnet/framework/data/adonet/)
[![SQL Server LocalDB](https://img.shields.io/badge/Database-SQL%20Server%20LocalDB-CC2927.svg)](https://learn.microsoft.com/sql/database-engine/sql-server-database-engine)
[![Build](https://github.com/swapnilbrrr/Code-Room-ADO/actions/workflows/webforms-build.yml/badge.svg)](https://github.com/swapnilbrrr/Code-Room-ADO/actions/workflows/webforms-build.yml)

Code-Room is a Web Applications Development coursework project implemented with the **ASP.NET Web Forms architecture** and the database/data-access approach demonstrated by the supplied coursework sample.

The current target is deliberately **Web Forms + C# + .NET Framework 4.7.2 + ADO.NET + SQL Server LocalDB**. It does not use ASP.NET Core MVC, Entity Framework Core, or MySQL.

## Tags

`ASP.NET Web Forms` · `C#` · `.NET Framework 4.7.2` · `ADO.NET` · `SQL Server` · `SQL Server LocalDB` · `IIS Express` · `HTML5` · `CSS3` · `JavaScript` · `Web Applications` · `Coursework`

## What the application provides

- Public pages for the Code-Room learning platform.
- Student registration and login with Forms Authentication.
- Courses, modules and lessons with progress tracking.
- Quizzes and quiz attempts with scoring.
- Challenges and learning activities.
- Resources, announcements and notifications.
- Certificates and achievements.
- Student profile/dashboard features.
- Administrator and SuperAdmin management areas.
- CRUD-backed database functionality using parameterised ADO.NET commands.
- Web Forms and server-side validation.
- CSRF synchronizer tokens for relevant state-changing forms.
- Seeded demonstration content.

## Technology stack

| Layer | Implementation |
|---|---|
| Web framework | ASP.NET Web Forms |
| Language | C# |
| Runtime | .NET Framework 4.7.2 |
| Development server | IIS Express |
| Database | SQL Server LocalDB `(LocalDB)\\MSSQLLocalDB` |
| Data access | ADO.NET |
| ORM | None |
| Front end | HTML5, CSS3, JavaScript |
| Authentication | ASP.NET Forms Authentication + Session |
| Password storage | PBKDF2 with per-user salts |

### Why Web Forms rather than MVC?

The old Code-Room implementation was an ASP.NET Core MVC application, but the current migration target is the **Web Forms architecture used by the supplied coursework sample**. Adding MVC to this solution would mix two web architectures without a requirement-based benefit.

The request flow is:

```text
Browser
  ↓
.aspx page
  ↓
.aspx.cs code-behind
  ↓
Helpers / Services
  ↓
ADO.NET repositories
  ↓
SqlConnection / SqlCommand / SqlDataReader / SqlTransaction
  ↓
SQL Server LocalDB
```

See [docs/01-ARCHITECTURE.md](docs/01-ARCHITECTURE.md) for the detailed architecture and startup/database lifecycle.

## Database setup — no manual database creation

The default connection in `CodeRoom.WebForms/Web.config` targets SQL Server LocalDB:

```text
Server=(LocalDB)\\MSSQLLocalDB;
Database=CodeRoomDb;
Integrated Security=True;
```

Windows Integrated Security means no SQL username or password is committed to the repository.

### First run

When the application starts, `Global.asax.cs` calls the database initializer. It:

1. checks whether `CodeRoomDb` exists;
2. creates it if it is missing;
3. executes `Database/CodeRoom.sql` batch-by-batch;
4. runs the baseline seeder in an ADO.NET transaction.

The initializer does not drop the existing application database.

### Existing database

The schema script uses guarded statements and the seeder checks existing baseline rows before creating missing data. Restarting the application is therefore not intended to reset the database or duplicate the baseline catalogue.

### SQL Server Express alternative

If a machine already uses SQL Server Express, the connection can be changed to:

```text
Server=.\\SQLEXPRESS;Database=CodeRoomDb;Integrated Security=True;
```

Only the `Server=` value needs to change.

## Running the project

### Prerequisites

1. Visual Studio with the **ASP.NET and web development** workload.
2. **.NET Framework 4.7.2** developer/targeting tools.
3. **SQL Server LocalDB**, normally available with the relevant Visual Studio tooling.

No MySQL server and no SQL username/password are required for the default configuration.

### Run

1. Extract the submitted ZIP.
2. Open `CodeRoom.sln`.
3. Set **CodeRoom.WebForms** as the startup project.
4. Select IIS Express.
5. Press **F5 / Start**.
6. On first startup, wait for `CodeRoomDb` to be created and seeded automatically.

## Data-layer verification

The repository contains `Tools/DataLayerTests/`, a real SQL Server acceptance harness covering:

- SQL Server connectivity.
- Missing-database creation.
- Fresh schema execution against a throwaway database.
- Table and foreign-key verification.
- Baseline seed creation and repeat/idempotent seeding.
- CRUD operations.
- Unique constraints and foreign-key behaviour.
- Transaction commit and rollback.
- Atomic XP updates.

The GitHub Actions workflow builds the solution and runs the data-layer harness against SQL Server LocalDB. This verifies the backend/data layer; a manual IIS Express/browser smoke test is still the final local check.

## Project structure

```text
Code-Room-ADO/
├── CodeRoom.sln
├── CodeRoom.WebForms/
│   ├── Authentication/
│   ├── Admin/
│   ├── Certificates/
│   ├── Challenges/
│   ├── Courses/
│   ├── Dashboard/
│   ├── Lessons/
│   ├── Notifications/
│   ├── Profile/
│   ├── Quiz/
│   ├── Controls/
│   ├── Data/
│   ├── Helpers/
│   ├── Models/
│   ├── Services/
│   ├── Assets/
│   ├── Database/CodeRoom.sql
│   ├── Site.Master
│   ├── Global.asax
│   └── Web.config
├── Tools/DataLayerTests/
├── docs/01-ARCHITECTURE.md
└── README.md
```

## Security notes

The project includes PBKDF2 password hashing with per-user salts, parameterised SQL, Forms Authentication and role checks, CSRF synchronizer tokens, local return-URL validation, HTML encoding of user-controlled display content, transactions for multi-step learning operations, and controlled avatar-upload handling.

Production deployment would require additional environment hardening such as HTTPS-only cookies and production-specific configuration.

## Migration note

The original Code-Room application was an ASP.NET Core MVC + Entity Framework Core + MySQL implementation. This repository is the **Web Forms/ADO.NET/SQL Server migration target**. The domain functionality is retained while the web, data-access and database technologies are changed to match the coursework architecture.

## Submission checklist

- [ ] Open `CodeRoom.sln` from the extracted ZIP.
- [ ] Confirm `CodeRoom.WebForms` is the startup project.
- [ ] Confirm the default connection uses `(LocalDB)\\MSSQLLocalDB`.
- [ ] Start the site on a machine with LocalDB available.
- [ ] Confirm `CodeRoomDb` is created automatically on a clean first run.
- [ ] Confirm seeded content appears.
- [ ] Register/login as a Student.
- [ ] Exercise at least one course/lesson and one quiz.
- [ ] Verify an Admin workflow if assessment requires it.
- [ ] Exclude `.vs/`, `bin/`, and `obj/` from the submission ZIP unless required.

## Repository

GitHub: [swapnilbrrr/Code-Room-ADO](https://github.com/swapnilbrrr/Code-Room-ADO)
