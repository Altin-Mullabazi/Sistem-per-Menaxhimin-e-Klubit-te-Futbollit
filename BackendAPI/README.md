# Backend API (Minimal Scope)

## Cfare perfshin

- JWT Authentication (login)
- Refresh Token endpoint
- Refresh Token persisted ne MSSQL
- Protected Players CRUD endpoints
- Swagger
- CORS per frontend

## Run

```bash
dotnet build
dotnet run
```

## Endpoints

Auth:

- POST /api/auth/login
- POST /api/auth/refresh

Players (JWT required):

- GET /api/players
- GET /api/players/{id}
- POST /api/players
- PUT /api/players/{id}
- DELETE /api/players/{id}
