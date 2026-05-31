#!/usr/bin/env pwsh

Write-Host "`n=== DETAILED ERROR INVESTIGATION ===" -ForegroundColor Magenta

# Authenticate
$loginResp = Invoke-WebRequest -Uri "http://localhost:5000/api/auth/login" -Method POST `
  -Body (@{ email = "admin@footballclub.com"; password = "Admin@123" } | ConvertTo-Json) `
  -ContentType "application/json" -UseBasicParsing
$token = ($loginResp.Content | ConvertFrom-Json).accessToken
Write-Host "Authenticated`n" -ForegroundColor Green

# Test 1: Check if stadium 1 exists
Write-Host "1. Checking Stadium ID 1:" -ForegroundColor Yellow
try {
    $r = Invoke-WebRequest -Uri "http://localhost:5000/api/stadiums/1" -Method GET `
      -Headers @{"Authorization" = "Bearer $token"} -UseBasicParsing
    Write-Host "  Status: $($r.StatusCode)" -ForegroundColor Green
    Write-Host "  Content: $($r.Content | ConvertFrom-Json | ConvertTo-Json)" -ForegroundColor Cyan
} catch {
    Write-Host "  Status: $($_.Exception.Response.StatusCode.Value__)" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Post new club
Write-Host "`n2. POST /clubs (new):" -ForegroundColor Yellow
$clubData = @{ name = "TestClub"; city = "TestCity"; foundedYear = 2020 }
try {
    $r = Invoke-WebRequest -Uri "http://localhost:5000/api/clubs" -Method POST `
      -Body ($clubData | ConvertTo-Json) `
      -Headers @{"Authorization" = "Bearer $token"; "Content-Type" = "application/json"} `
      -UseBasicParsing
    Write-Host "  Status: $($r.StatusCode)" -ForegroundColor Green
    Write-Host "  Response: $($r.Content)" -ForegroundColor Cyan
} catch {
    Write-Host "  Status: $($_.Exception.Response.StatusCode.Value__)" -ForegroundColor Red
    Write-Host "  Error: Check exception details" -ForegroundColor Red
}

# Test 3: POST new player
Write-Host "`n3. POST /players (new):" -ForegroundColor Yellow
$playerData = @{
    firstName = "John"
    lastName = "Test"
    position = "Forward"
    jerseyNumber = 55
    clubId = 1
    dateOfBirth = "2000-01-01"
}
try {
    $r = Invoke-WebRequest -Uri "http://localhost:5000/api/players" -Method POST `
      -Body ($playerData | ConvertTo-Json) `
      -Headers @{"Authorization" = "Bearer $token"; "Content-Type" = "application/json"} `
      -UseBasicParsing
    Write-Host "  Status: $($r.StatusCode)" -ForegroundColor Green
    Write-Host "  Response: $($r.Content)" -ForegroundColor Cyan
} catch {
    Write-Host "  Status: $($_.Exception.Response.StatusCode.Value__)" -ForegroundColor Red
    Write-Host "  Error message: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 4: POST new stadium
Write-Host "`n4. POST /stadiums (new):" -ForegroundColor Yellow
$stadiumData = @{ name = "TestStadium"; city = "TestCity"; capacity = 50000; yearBuilt = 2015 }
try {
    $r = Invoke-WebRequest -Uri "http://localhost:5000/api/stadiums" -Method POST `
      -Body ($stadiumData | ConvertTo-Json) `
      -Headers @{"Authorization" = "Bearer $token"; "Content-Type" = "application/json"} `
      -UseBasicParsing
    Write-Host "  Status: $($r.StatusCode)" -ForegroundColor Green
    Write-Host "  Response: $($r.Content)" -ForegroundColor Cyan
} catch {
    Write-Host "  Status: $($_.Exception.Response.StatusCode.Value__)" -ForegroundColor Red
    Write-Host "  Error message: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 5: List all stadiums
Write-Host "`n5. GET /stadiums (all):" -ForegroundColor Yellow
try {
    $r = Invoke-WebRequest -Uri "http://localhost:5000/api/stadiums?page=1&pageSize=10" -Method GET `
      -Headers @{"Authorization" = "Bearer $token"} -UseBasicParsing
    $data = $r.Content | ConvertFrom-Json
    Write-Host "  Status: $($r.StatusCode)" -ForegroundColor Green
    Write-Host "  Stadium count: $(($data.items | Measure-Object).Count)" -ForegroundColor Cyan
    if ($data.items -and $data.items.Count -gt 0) {
        Write-Host "  First stadium: $($data.items[0].name) (ID: $($data.items[0].id))" -ForegroundColor Cyan
    }
} catch {
    Write-Host "  Status: $($_.Exception.Response.StatusCode.Value__)" -ForegroundColor Red
}

Write-Host "`n=== END INVESTIGATION ===" -ForegroundColor Magenta
