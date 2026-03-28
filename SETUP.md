# Setup Guide

## 1. Prerequisites

- .NET 8 SDK
- Node.js 18+
- MSSQL Server

## 2. Backend Setup

1. Shko ne folderin BackendAPI.
2. Konfiguro connection string ne appsettings.json.
3. Ekzekuto:

```bash
dotnet build
dotnet run
```

Backend nis ne http://localhost:5000.

## 3. Frontend Setup

1. Shko ne folderin Frontend.
2. Krijo ose verifiko .env files.
3. Ekzekuto:

```bash
npm install
npm run dev
```

Frontend zakonisht nis ne http://localhost:5173.

## 4. Environment Files

- .env.development
- .env.production

Shembull:

```env
VITE_API_URL=http://localhost:5000/api
```
