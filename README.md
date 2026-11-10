# RaceDay — Full-Stack Event Management Platform

> Part 2 (RESTful API) + Part 3 (MVC Web Application)  
> Built with ASP.NET Core 8, Entity Framework Core, SQL Server

---

## 📋 System Description

**RaceDay** is an event management platform for South Africa's road running, walking, and cycling community. It consists of two applications:

| Project | Technology | Port |
|---------|-----------|------|
| `RaceDayAPI` | ASP.NET Core 8 Web API | `http://localhost:5000` |
| `RaceDay.MVC` | ASP.NET Core 8 MVC | `http://localhost:5001` |

The MVC has **no direct database access** — all data operations are made through the API via `HttpClient`.

---

## 👥 Roles

| Feature | Organiser | Participant |
|---------|-----------|-------------|
| Browse & view events | ✅ | ✅ |
| Create / edit / delete events | ✅ | ❌ |
| Upload event banner image | ✅ | ❌ |
| Manage categories | ✅ | ❌ |
| View all enrolments for an event | ✅ | ❌ |
| Capture / edit / delete results | ✅ | ❌ |
| Enrol in events | ❌ | ✅ |
| View own enrolments | ❌ | ✅ |
| View own race results | ❌ | ✅ |
| Update profile | ✅ | ✅ |

---

## 🚀 Setup & Run (Local)

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (local or Docker) — connection string in `RaceDayAPI/appsettings.json`
- Visual Studio 2022 or VS Code

### Step 1 — Configure the database connection

Edit `RaceDayAPI/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=RaceDay;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Step 2 — Start the API

```bash
cd RaceDayAPI
dotnet run
```

The API starts at `http://localhost:5000`.  
Swagger UI: `http://localhost:5000/swagger`

> On first run, `db.Database.Migrate()` automatically creates all tables and seeds the 4 categories.  
> To add test users, use the Swagger UI to POST to `/api/auth/register`.

### Step 3 — Start the MVC

In a second terminal:

```bash
cd RaceDay.MVC
dotnet run
```

The MVC starts at `http://localhost:5001`.

---

## 🔐 Authentication

Authentication is **session-based**. When a user logs in via the MVC:

1. MVC POSTs credentials to `API /api/auth/login`
2. The API validates, creates a server-side session, and returns a **session cookie** (`.RaceDay.Session`)
3. The MVC's `HttpClient` stores that cookie automatically (via `CookieContainer`)
4. All subsequent API calls send the cookie, so the API knows who is logged in
5. The MVC also stores the user's ID, role, and name in its own session for rendering navigation

**No JWT tokens are used.** Role enforcement happens at the API level on every request.

---

## 🐳 Docker

### Build the MVC image

```bash
docker build -t raceday-mvc .
```

### Run the container

```bash
docker run -d \
  -p 8080:80 \
  -e ApiSettings__BaseUrl=http://host.docker.internal:5000/ \
  --name raceday-mvc \
  raceday-mvc
```

Then visit: `http://localhost:8080`

> On Linux, replace `host.docker.internal` with your host machine's IP address.

---

## 🧪 Unit Tests

Tests are in `RaceDayAPI.Tests/` and use an **in-memory database** — no SQL Server required.

```bash
cd RaceDayAPI.Tests
dotnet test --verbosity normal
```

Test coverage:
- `AuthTests` — registration, login, duplicate email, wrong password, profile access
- `EventTests` — public access, role enforcement (Organiser/Participant/Unauthenticated)
- `EnrolmentTests` — enrol, cancel, duplicate enrolment, cross-user cancellation

---

## 📁 Project Structure

```
RaceDay/
├── RaceDayAPI/                     ← Part 2: RESTful API
│   ├── Controllers/
│   │   ├── AuthController.cs       register, login, logout
│   │   ├── EventsController.cs     CRUD events + enrolments view + banner
│   │   ├── EnrolmentsController.cs enrol, cancel, list participant's enrolments
│   │   ├── ResultsController.cs    capture, update, delete, view results
│   │   ├── CategoriesController.cs CRUD categories
│   │   ├── UsersController.cs      profile get/update
│   │   └── RoutesController.cs     route info per event
│   ├── DTOs/
│   │   ├── AuthDTOs.cs             RegisterDto, LoginDto, UserResponseDto, etc.
│   │   └── ResourceDTOs.cs         Event, Category, Enrolment, Result, Route DTOs
│   ├── Models/                     EF Core entity classes
│   ├── Data/AppDbContext.cs         DbContext with relationships + category seed
│   ├── Middleware/SessionExtensions.cs  IsAuthenticated(), IsOrganiser(), etc.
│   ├── Migrations/                 EF Core migration files
│   └── docs/
│       ├── raceday_database.sql    SQL Server script (alternative to migrations)
│       ├── api_endpoint_plan.md    Part 1 endpoint plan
│       └── erd diagram.png         Entity Relationship Diagram
│
├── RaceDayAPI.Tests/               ← Part 2: Unit tests (xUnit + in-memory DB)
│   ├── AuthTests.cs
│   ├── EventTests.cs
│   ├── EnrolmentTests.cs
│   └── Helpers/TestFactory.cs
│
├── RaceDay.MVC/                    ← Part 3: MVC Web Application
│   ├── Controllers/
│   │   ├── BaseController.cs       Session helpers + role guards
│   │   ├── AuthController.cs       Register, Login, Logout
│   │   ├── HomeController.cs       Public event listing + search/filter
│   │   ├── EventsController.cs     Dashboard, CRUD, Enrolments view
│   │   ├── EnrolmentsController.cs Participant enrol/cancel/list
│   │   ├── ResultsController.cs    Capture/view/edit results
│   │   ├── CategoriesController.cs Manage categories
│   │   └── ProfileController.cs    View/update profile
│   ├── Services/RaceDayApiService.cs  All HttpClient calls to API
│   ├── Models/ApiModels.cs           ViewModels + DTOs matching API responses
│   ├── Helpers/SessionHelper.cs      MVC session key constants
│   ├── Views/                        All Razor views
│   └── wwwroot/css/site.css          RaceDay brand design system
│
├── Dockerfile                      Builds the MVC into a container
├── docker-compose.yml              One-command startup
└── .github/workflows/ci.yml        CI/CD: build API + tests + build MVC + Docker
```

---

## 🗄️ Database Schema

Six tables matching the ERD:

| Table | Description |
|-------|-------------|
| `Users` | Organisers and Participants (BCrypt hashed passwords) |
| `Categories` | Age/distance categories (e.g. Road Running, Cycling) |
| `Events` | Events with name, date, location, type, distance, banner |
| `Enrolments` | Links Participant ↔ Event (unique per pair) |
| `Results` | Finish time and position per enrolment (one per enrolment) |
| `Routes` | Optional route info per event (one per event) |

### Running the SQL script manually (alternative to EF migrations)

Open `RaceDayAPI/docs/raceday_database.sql` in SSMS and run it on a clean SQL Server instance.  
This creates all tables, constraints, and seeds 4 categories.

> **Login with seeded users:** All seed users have password `Password1!`

---

## ⚙️ CI/CD

GitHub Actions (`.github/workflows/ci.yml`) runs three jobs on every push:

1. **API** — restore → build → run all unit tests → verify folder structure  
2. **MVC** — restore → build → publish → verify folder structure  
3. **Docker** — build Docker image (runs after both API and MVC pass)

**Screenshot of green build:** *(add screenshot here)*

---

## 🎥 Video Presentation

**YouTube link:** *(add your unlisted YouTube link here)*

---

## 🗒️ AI Tool Disclosure

AI tools were used in planning, debugging, and scaffolding boilerplate. All architecture decisions, business logic, and final implementation were reviewed and authored by the student.
