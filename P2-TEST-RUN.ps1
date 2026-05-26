#!/usr/bin/env pwsh

# P2 FINAL TEST - SIMPLIFIED VERSION

Write-Host "`n========================================" -ForegroundColor Magenta
Write-Host "P2 API ENDPOINT TESTING" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Magenta

# Authenticate
Write-Host "Step 1: Authenticating..." -ForegroundColor Yellow
$loginResp = Invoke-WebRequest -Uri "http://localhost:5000/api/auth/login" -Method POST `
  -Body (@{ email = "admin@footballclub.com"; password = "Admin@123" } | ConvertTo-Json) `
  -ContentType "application/json" -UseBasicParsing
$token = ($loginResp.Content | ConvertFrom-Json).accessToken
Write-Host "OK - Token received`n" -ForegroundColor Green

$passed = 0
$failed = 0

# Helper function
function Test-API {
    param([string]$name, [string]$method, [string]$path, [object]$body, [bool]$auth, [int]$expectedStatus)
    
    $url = "http://localhost:5000/api$path"
    $headers = @{"Content-Type" = "application/json"}
    if ($auth) { $headers["Authorization"] = "Bearer $token" }
    
    try {
        if ($method -eq "GET") {
            $r = Invoke-WebRequest -Uri $url -Method GET -Headers $headers -UseBasicParsing
        } else {
            $r = Invoke-WebRequest -Uri $url -Method $method -Body ($body | ConvertTo-Json) -Headers $headers -UseBasicParsing
        }
        $status = $r.StatusCode
    } catch {
        $status = $_.Exception.Response.StatusCode.Value__
    }
    
    $ok = ($status -eq $expectedStatus)
    if ($ok) {
        Write-Host "  [PASS] $name (Status: $status)" -ForegroundColor Green
        return $true
    } else {
        Write-Host "  [FAIL] $name (Status: $status, Expected: $expectedStatus)" -ForegroundColor Red
        return $false
    }
}

# CLUBS
Write-Host "CLUBS ENDPOINTS:" -ForegroundColor Cyan
if (Test-API "GET /clubs paginated" "GET" "/clubs?page=1&pageSize=10" $null $false 200) { $passed++ } else { $failed++ }
if (Test-API "GET /clubs search" "GET" "/clubs?search=FC" $null $false 200) { $passed++ } else { $failed++ }
if (Test-API "GET /clubs filter" "GET" "/clubs?filter=Active" $null $false 200) { $passed++ } else { $failed++ }
if (Test-API "GET /clubs/:id (404)" "GET" "/clubs/999" $null $false 404) { $passed++ } else { $failed++ }
if (Test-API "POST /clubs create" "POST" "/clubs" @{name="Test";city="City";foundedYear=2020} $true 201) { $passed++ } else { $failed++ }
if (Test-API "PUT /clubs/:id (404)" "PUT" "/clubs/999" @{name="Test";foundedYear=2020} $true 404) { $passed++ } else { $failed++ }
if (Test-API "DELETE /clubs/:id (404)" "DELETE" "/clubs/999" $null $true 404) { $passed++ } else { $failed++ }
Write-Host ""

# PLAYERS
Write-Host "PLAYERS ENDPOINTS:" -ForegroundColor Cyan
if (Test-API "GET /players paginated" "GET" "/players?page=1&pageSize=10" $null $true 200) { $passed++ } else { $failed++ }
if (Test-API "GET /players filters" "GET" "/players?position=Forward&status=Active" $null $true 200) { $passed++ } else { $failed++ }
if (Test-API "GET /players search" "GET" "/players?search=John" $null $true 200) { $passed++ } else { $failed++ }
if (Test-API "GET /players/club/:id" "GET" "/players/club/999" $null $true 200) { $passed++ } else { $failed++ }
if (Test-API "GET /players/:id (200)" "GET" "/players/1" $null $true 200) { $passed++ } else { $failed++ }
if (Test-API "POST /players create" "POST" "/players" @{firstName="John";lastName="Doe";position="Forward";jerseyNumber=10;clubId=1;dateOfBirth="2000-01-01"} $true 201) { $passed++ } else { $failed++ }
if (Test-API "PUT /players/:id (200)" "PUT" "/players/1" @{firstName="Jane";lastName="Doe";position="Midfielder";jerseyNumber=11;clubId=1} $true 200) { $passed++ } else { $failed++ }
if (Test-API "DELETE /players/:id (404)" "DELETE" "/players/999" $null $true 404) { $passed++ } else { $failed++ }
Write-Host ""

# STADIUMS
Write-Host "STADIUMS ENDPOINTS:" -ForegroundColor Cyan
if (Test-API "GET /stadiums paginated" "GET" "/stadiums?page=1&pageSize=10" $null $true 200) { $passed++ } else { $failed++ }
if (Test-API "GET /stadiums search" "GET" "/stadiums?search=Olympic" $null $true 200) { $passed++ } else { $failed++ }
if (Test-API "GET /stadiums/:id (200)" "GET" "/stadiums/1" $null $true 200) { $passed++ } else { $failed++ }
if (Test-API "POST /stadiums create" "POST" "/stadiums" @{name="Stadium";city="City";capacity=50000;yearBuilt=2010} $true 201) { $passed++ } else { $failed++ }
if (Test-API "PUT /stadiums/:id (200)" "PUT" "/stadiums/1" @{name="Stadium";city="City";capacity=60000;yearBuilt=2010} $true 200) { $passed++ } else { $failed++ }
Write-Host ""

# VALIDATION
Write-Host "VALIDATION:" -ForegroundColor Cyan
if (Test-API "POST invalid club" "POST" "/clubs" @{name=""} $true 400) { $passed++ } else { $failed++ }
if (Test-API "POST missing fields" "POST" "/players" @{firstName="Test"} $true 400) { $passed++ } else { $failed++ }
Write-Host ""

# AUTHORIZATION
Write-Host "AUTHORIZATION:" -ForegroundColor Cyan
if (Test-API "GET without token" "GET" "/clubs?page=1&pageSize=10" $null $false 200) { $passed++ } else { $failed++ }
if (Test-API "POST without token (401)" "POST" "/clubs" @{name="Test"} $false 401) { $passed++ } else { $failed++ }
Write-Host ""

# Summary
$total = $passed + $failed
$rate = if ($total -gt 0) { [math]::Round(($passed/$total)*100, 2) } else { 0 }

Write-Host "========================================" -ForegroundColor Magenta
Write-Host "RESULTS" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Magenta
Write-Host "Total Tests: $total" -ForegroundColor Cyan
Write-Host "Passed: $passed" -ForegroundColor Green
Write-Host "Failed: $failed" -ForegroundColor $(if ($failed -eq 0) { "Green" } else { "Red" })
Write-Host "Pass Rate: $rate%" -ForegroundColor $(if ($rate -eq 100) { "Green" } else { "Yellow" })
Write-Host "========================================`n" -ForegroundColor Magenta
