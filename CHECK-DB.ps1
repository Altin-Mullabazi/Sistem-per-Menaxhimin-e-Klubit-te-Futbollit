#!/usr/bin/env pwsh

Write-Host "`n=== DATABASE CONTENT CHECK ===" -ForegroundColor Magenta

# Authenticate
$loginResp = Invoke-WebRequest -Uri "http://localhost:5000/api/auth/login" -Method POST `
  -Body (@{ email = "admin@footballclub.com"; password = "Admin@123" } | ConvertTo-Json) `
  -ContentType "application/json" -UseBasicParsing
$token = ($loginResp.Content | ConvertFrom-Json).accessToken
Write-Host "Authenticated`n" -ForegroundColor Green

# Check all endpoints for data
$endpoints = @(
    ("/clubs?page=1&pageSize=10", "Clubs"),
    ("/players?page=1&pageSize=10", "Players"),
    ("/stadiums?page=1&pageSize=10", "Stadiums"),
    ("/seasons?page=1&pageSize=10", "Seasons"),
    ("/sponsors?page=1&pageSize=10", "Sponsors")
)

foreach ($endpoint in $endpoints) {
    $path = $endpoint[0]
    $name = $endpoint[1]
    Write-Host "Checking $name..." -ForegroundColor Yellow
    try {
        $r = Invoke-WebRequest -Uri "http://localhost:5000/api$path" -Method GET `
          -Headers @{"Authorization" = "Bearer $token"} -UseBasicParsing
        $data = $r.Content | ConvertFrom-Json
        if ($data.items) {
            $count = ($data.items | Measure-Object).Count
            Write-Host "  Count: $count" -ForegroundColor Green
            if ($count -gt 0) {
                $data.items | Select-Object -First 2 | ForEach-Object {
                    Write-Host "    - ID: $($_.id), Name: $($_.name)" -ForegroundColor Cyan
                }
            }
        } else {
            Write-Host "  Response type: $($data.GetType().Name)" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "  Error: $($_.Exception.Response.StatusCode.Value__)" -ForegroundColor Red
    }
}

Write-Host "`n=== END CHECK ===" -ForegroundColor Magenta
