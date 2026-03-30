# Football Club CRUD App

CRUD aplikacion me .NET Core Web API dhe ReactJS.

## Cfare perfshin

- CRUD per `Players`
- JWT Access Token + Refresh Token
- Refresh Token i ruajtur ne MSSQL
- React Context API per state management
- Axios per API calls
- Bearer token vetem ne `Authorization` header
- Nuk perdoret `localStorage` ose `sessionStorage` per token
- `.env.development` dhe `.env.production` ne frontend

## Tech Stack

- Backend: ASP.NET Core 8, EF Core, SQL Server
- Frontend: React 18, TypeScript, Vite

## Struktura e Projektit

- `BackendAPI/`
- `Frontend/`

## Prerequisites

- .NET 8 SDK
- Node.js 18+
- SQL Server (ose SQL Express)

## Konfigurimi i Databazes

Ne `BackendAPI/appsettings.Development.json`, ndrysho `ConnectionStrings:DefaultConnection` sipas SQL Server-it tend lokal.

Shembull:

```json
"ConnectionStrings": {
	"DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=FootballClubDB_Dev;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True"
}
```

## Si me e startu

1. Backend

```bash
cd BackendAPI
dotnet run
```

API/Swagger: `http://localhost:5000`

2. Frontend

```bash
cd Frontend
npm install
npm run dev
```

Frontend: zakonisht `http://localhost:5173`

## Login per test

- Username: `admin`
- Password: `Admin@123`

## Build Validation

```bash
cd BackendAPI
dotnet build
```

```bash
cd Frontend
npm run build
```

## Git Workflow (i rekomanduar)

- puno ne `dev`
- hap PR `dev -> main`
- merge ne `main` pas review

## Dokumentet Tjera

- `SETUP.md`
- `QUICK_START.md`
- `API_TESTING.md`
- `PROJECT_SUMMARY.md`
- `COMPLETION_CHECKLIST.md`
