# 01 — Architecture

This document describes the actual Code-Room Web Forms implementation and follows the same documentation role as the architecture document in the supplied coursework sample.

## Technology stack

| Layer | Choice |
|---|---|
| Web framework | ASP.NET **Web Forms** (.aspx + C# code-behind) |
| Runtime | **.NET Framework 4.7.2** |
| Development server | **IIS Express** |
| Database | **SQL Server LocalDB** ((LocalDB)\\MSSQLLocalDB) |
| Data access | **ADO.NET** (SqlConnection, SqlCommand, SqlDataReader, SqlTransaction) |
| ORM | None |
| Front end | HTML5, CSS3 and JavaScript |
| Authentication | Forms Authentication + Session + role checks |
| Password storage | PBKDF2 with per-user salts |

## Application shape

Site.Master provides the shared page shell and navigation. Individual .aspx pages contain presentation markup; their .aspx.cs code-behind handles the Web Forms lifecycle, validation, input handling and binding.

Business/application workflows are kept in Helpers/ and Services/. SQL-specific operations are handled by the ADO.NET repositories in Data/.

```text
Browser
  │ HTTP GET / POST
  ▼
IIS Express
  ▼
ASP.NET Web Forms page lifecycle
  ▼
<Page>.aspx.cs
  │
  ├── Helpers / Services
  │        ▼
  │   ADO.NET repositories
  │        ▼
  │   SqlConnection / SqlCommand / SqlDataReader / SqlTransaction
  │        ▼
  │   SQL Server LocalDB
  ▼
Rendered HTML + Site.Master + Assets + JavaScript
```

## Request lifecycle

### 1. Application startup

Global.asax.cs calls DatabaseInitializer.InitializeWithSeedData() during Application_Start.

```text
Application_Start
      │
      ▼
DatabaseInitializer
      │
      ├── read DefaultConnection
      ├── check sys.databases
      ├── CREATE DATABASE if CodeRoomDb is missing
      ├── execute Database/CodeRoom.sql batch-by-batch
      └── DbSeeder.Seed() in an ADO.NET transaction
```

If initialization fails, the exception is logged and rethrown so the application does not continue against a broken database.

### 2. Page request

For a protected page, authentication/authorization checks run first. The page then loads data through repositories/services and binds the result to Web Forms controls.

### 3. Form submission / postback

State-changing postbacks validate submitted values, authentication/authorization state and applicable CSRF tokens before calling the relevant workflow. Multi-step operations use transactions where required, then the page redirects or renders the result.

## Database initialization and seeding

The default configuration uses Windows Integrated Security with LocalDB. Therefore the lecturer does not need a SQL username/password or a manually created CodeRoomDb.

### Clean first run

```text
No CodeRoomDb
      │
      ▼
DatabaseInitializer
      │
      ├── CREATE DATABASE CodeRoomDb
      ├── execute CodeRoom.sql
      │       └── tables, keys, constraints, indexes
      └── DbSeeder.Seed()
              └── transactional baseline data
```

### Existing database

```text
CodeRoomDb exists
      │
      ▼
DatabaseInitializer
      │
      ├── execute guarded schema batches
      └── DbSeeder.Seed()
              ├── detect existing baseline rows
              ├── create missing rows
              └── repair selected seed values where required
```

The initializer does not drop the application database. The schema script and seeder are designed to be repeatable rather than resetting the database on every start.

## Data access pattern

Repositories use direct ADO.NET:

```text
Repository
   │
   ├── SqlConnection
   ├── SqlCommand
   ├── SqlParameter
   ├── SqlDataReader
   └── SqlTransaction
```

User-controlled values are supplied as SQL parameters rather than concatenated into query text.

Transactions are used for multi-step operations such as registration, enrolment, learning activity completion, quiz submission, achievements/streak updates and certificate creation.

## Authentication and authorization

```text
Login / Register
       │
       ▼
UserRepository
       │
       ▼
PBKDF2 password verification
       │
       ▼
Forms Authentication ticket
       │
       ▼
Session identity
       │
       ├── Student
       ├── Admin
       └── SuperAdmin
```

Protected pages also perform application-level role checks. This is intentionally implemented for Web Forms rather than MVC controller filters.

## Main project responsibilities

| Area | Responsibility |
|---|---|
| Authentication/ | Registration, login, logout and access-denied pages |
| Admin/ | Administrative management operations |
| Courses/ | Course catalogue, details and enrolment |
| Lessons/ | Lesson display and completion |
| Quiz/ | Quiz presentation, submission and results |
| Challenges/ | Challenge presentation and completion |
| Dashboard/ | Student dashboard and progress |
| Profile/ | Profile and avatar management |
| Notifications/ | User notifications |
| Certificates/ | Certificate views |
| Controls/ | Reusable Web Forms controls |
| Data/ | ADO.NET repositories, initializer and seeder |
| Helpers/ | Cross-cutting authentication, CSRF and security helpers |
| Services/ | Multi-step application workflows |
| Models/ | Domain/data models |
| Database/ | SQL Server schema |
| Assets/ | Front-end assets |

## Verification

Tools/DataLayerTests is a real SQL Server acceptance harness. It covers connectivity, missing-database creation, fresh schema execution, table/foreign-key checks, seed creation and repeat seeding, CRUD, constraints, transactions and atomic XP updates.

The GitHub Actions workflow runs the solution build and data-layer harness against SQL Server LocalDB.

This verifies the backend/data layer; it does not replace a manual IIS Express/browser smoke test.

## Deployment boundary

The expected assessment flow is:

```text
Extract ZIP
   ↓
Open CodeRoom.sln
   ↓
Run CodeRoom.WebForms with IIS Express
   ↓
LocalDB creates/opens CodeRoomDb
   ↓
Schema + seed initialize automatically
   ↓
Use the application
```

No MVC controller layer, Entity Framework Core context, MySQL server or manual SQL credential setup is part of this target architecture.
