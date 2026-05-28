#!/usr/bin/env pwsh

Write-Host "`n=== TESTING POST /clubs WITH DETAILED RESPONSE ===" -ForegroundColor Magenta

# Authenticate
$loginResp = Invoke-WebRequest -Uri "http://localhost:5000/api/auth/login" -Method POST `
  -Body (@{ email = "admin@footballclub.com"; password = "Admin@123" } | ConvertTo-Json) `
  -ContentType "application/json" -UseBasicParsing
$token = ($loginResp.Content | ConvertFrom-Json).accessToken
Write-Host "Authenticated`n" -ForegroundColor Green

$headers = @{"Authorization" = "Bearer $token"; "Content-Type" = "application/json"}

# Test payload
$payload = @{
    name = "Test FC"
    city = "Test City"
    foundedYear = 2020
} | ConvertTo-Json

Write-Host "Payload:" -ForegroundColor Yellow
Write-Host $payload -ForegroundColor Cyan
Write-Host ""

# Send request
try {
    Write-Host "Sending POST /clubs..." -ForegroundColor Yellow
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/clubs" -Method POST `
      -Body $payload `
      -Headers $headers `
      -UseBasicParsing
    Write-Host "Success! Status: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "Response: $($response.Content)" -ForegroundColor Cyan
} catch {
    Write-Host "Failed! Status: $($_.Exception.Response.StatusCode.Value__)" -ForegroundColor Red
    
    # Try to read response body
    try {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        $reader.Close()
        Write-Host "Response body: $responseBody" -ForegroundColor Red
    } catch {
        Write-Host "Could not read response body: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`n=== END TEST ===" -ForegroundColor Magenta
