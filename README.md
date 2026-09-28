# Code-Room

Code-Room is a Web Applications Development coursework project migrated to the prescribed ASP.NET Web Forms architecture.

## Technology Stack

- **ASP.NET Web Forms**
- **C#**
- **.NET Framework 4.7.2**
- **ADO.NET**
- **Microsoft SQL Server / SQL Server Express**
- HTML5, CSS3 and JavaScript for the presentation layer

The migrated application does **not** use Entity Framework Core or MySQL.

## Project Structure

```text
Code-Room-ADO/
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
│   ├── Database/
│   │   └── CodeRoom.sql
│   ├── Site.Master
│   ├── Global.asax
│   └── Web.config
├── Tools/
│   └── DataLayerTests/
└── CodeRoom.sln
```

## Architecture

The application follows the Web Forms + ADO.NET pattern demonstrated by the supplied coursework sample:

```text
.aspx page
   ↓
.aspx.cs code-behind
   ↓
Helpers / Services
   ↓
ADO.NET Repository
   ↓
SqlConnection / SqlCommand / SqlDataReader / SqlTransaction
   ↓
Microsoft SQL Server
```

### Data access

Repositories use parameterised SQL with:

- `SqlConnection`
- `SqlCommand`
- `SqlDataReader`
- `SqlDataAdapter` / `DataTable` where tabular binding is appropriate
- `SqlTransaction`

No ORM is used by the migrated application.

### Authentication

Authentication uses:

- PBKDF2 password hashing with per-user salts
- ASP.NET Forms Authentication for the protected browser ticket
- Session state for the current user snapshot
- role checks for Student, Admin and SuperAdmin
- Web Forms validation controls
- synchronizer tokens for state-changing authentication forms
- local-only return URL validation

## Database Setup

The application uses the `DefaultConnection` connection string in:

```text
CodeRoom.WebForms/Web.config
```

The default development configuration targets:

```text
Server=.\\SQLEXPRESS;Database=CodeRoomDb;Integrated Security=True
```

If SQL Server Express is installed under another instance, update the connection string for the local machine.

The application initializer:

1. checks whether `CodeRoomDb` exists;
2. creates it when necessary;
3. executes `Database/CodeRoom.sql` batch-by-batch;
4. runs the idempotent baseline seed inside an ADO.NET transaction.

A failed seed is rolled back rather than leaving a partially seeded database.

## Running the project

1. Install Visual Studio with **ASP.NET and web development** and **.NET Framework 4.7.2 developer tools**.
2. Install SQL Server Express or SQL Server LocalDB.
3. Open **CodeRoom.sln**.
4. Check the `DefaultConnection` value in `CodeRoom.WebForms/Web.config`.
5. Set `CodeRoom.WebForms` as the startup project.
6. Run with IIS Express.

## Data-layer verification

The repository contains:

```text
Tools/DataLayerTests
```

This console harness exercises the ADO.NET layer against SQL Server, including schema creation, seed idempotency, CRUD operations, foreign-key behaviour and transaction/atomic-update checks.

## Migration status

The repository contains the completed Web Forms/ADO.NET migration target. The original MVC implementation is maintained separately in the reference repository and is not part of this target solution.
