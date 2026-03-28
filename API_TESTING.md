# API Testing

Base URL:

- http://localhost:5000/api

## 1) Login

Endpoint:

- POST /auth/login

Request:

```json
{
  "username": "admin",
  "password": "Admin@123"
}
```

Response (example):

```json
{
  "success": true,
  "message": "Login successful",
  "accessToken": "...",
  "refreshToken": "...",
  "user": {
    "id": "...",
    "username": "admin",
    "email": "admin@footballclub.com"
  }
}
```

## 2) Refresh Token

Endpoint:

- POST /auth/refresh

Request:

```json
{
  "refreshToken": "..."
}
```

## 3) Players CRUD (Bearer Required)

Header:

- Authorization: Bearer <accessToken>

Endpoints:

- GET /players
- GET /players/{id}
- POST /players
- PUT /players/{id}
- DELETE /players/{id}
