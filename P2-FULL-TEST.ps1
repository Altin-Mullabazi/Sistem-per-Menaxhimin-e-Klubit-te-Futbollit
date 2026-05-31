#!/usr/bin/env pwsh

Write-Host "`n========================================" -ForegroundColor Magenta
Write-Host "P2 API ENDPOINT TESTING - WITH SEEDING" -ForegroundColor Cyan
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
$clubId = 0
$playerId = 0
$stadiumId = 0

# Helper function
function Test-API {
    param([string]$name, [string]$method, [string]$path, [object]$body, [bool]$auth, [int]$expectedStatus, [ref]$returnId)
    
    $url = "http://localhost:5000/api$path"
    $headers = @{"Content-Type" = "application/json"}
    if ($auth) { $headers["Authorization"] = "Bearer $token" }
    
    try {
        if ($method -eq "GET") {
            $r = Invoke-WebRequest -Uri $url -Method GET -Headers $headers -UseBasicParsing
        } else {
            $r = Invoke-WebRequest -Uri $url -Method $method -Body ($body | ConvertTo-Json -Depth 10) -Headers $headers -UseBasicParsing
        }
        $status = $r.StatusCode
        if ($returnId -and ($status -eq 201 -or $status -eq 200)) {
            $content = $r.Content | ConvertFrom-Json
            $returnId.Value = $content.data.id
        }
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

# Step 2: Create test data
Write-Host "Step 2: Creating test data..." -ForegroundColor Yellow

# Use unique names with timestamp to avoid duplicate constraints
$timestamp = (Get-Date).ToString("yyyyMMddHHmmss")

# Create Club
if (Test-API "POST /clubs create" "POST" "/clubs" @{name="TestClub_$timestamp";city="Test City";foundedYear=2020} $true 201 ([ref]$clubId)) {
    $passed++
    Write-Host "         Club ID: $clubId`n" -ForegroundColor Cyan
} else {
    $failed++
    Write-Host "         ERROR: Could not create club`n" -ForegroundColor Red
    Write-Host "======================================== STOPPING TESTS ========================================`n" -ForegroundColor Red
    exit 1
}

# Create Stadium
if (Test-API "POST /stadiums create" "POST" "/stadiums" @{name="TestStadium_$timestamp";city="Test City";capacity=50000;yearBuilt=2010} $true 201 ([ref]$stadiumId)) {
    $passed++
    Write-Host "         Stadium ID: $stadiumId`n" -ForegroundColor Cyan
} else {
    $failed++
    Write-Host "         WARNING: Could not create stadium`n" -ForegroundColor Yellow
}

# Create Player
if (Test-API "POST /players create" "POST" "/players" @{firstName="Test";lastName="Player";position="Forward";jerseyNumber=10;clubId=$clubId;dateOfBirth="2000-01-01"} $true 201 ([ref]$playerId)) {
    $passed++
    Write-Host "         Player ID: $playerId`n" -ForegroundColor Cyan
} else {
    $failed++
    Write-Host "         ERROR: Could not create player`n" -ForegroundColor Red
}

# Step 3: Test all endpoints
Write-Host "Step 3: Testing endpoints..." -ForegroundColor Yellow

Write-Host "`nCLUBS ENDPOINTS:" -ForegroundColor Cyan
if (Test-API "GET /clubs paginated" "GET" "/clubs?page=1&pageSize=10" $null $false 200) { $passed++ } else { $failed++ }
if (Test-API "GET /clubs search" "GET" "/clubs?search=Test" $null $false 200) { $passed++ } else { $failed++ }
if (Test-API "GET /clubs filter" "GET" "/clubs?filter=Active" $null $false 200) { $passed++ } else { $failed++ }
if (Test-API "GET /clubs/:id" "GET" "/clubs/$clubId" $null $false 200) { $passed++ } else { $failed++ }
if (Test-API "PUT /clubs/:id update" "PUT" "/clubs/$clubId" @{name="Updated FC";foundedYear=2020} $true 200) { $passed++ } else { $failed++ }
if (Test-API "DELETE /clubs/:id" "DELETE" "/clubs/$clubId" $null $true 204) { $passed++ } else { $failed++ }
Write-Host ""

Write-Host "PLAYERS ENDPOINTS:" -ForegroundColor Cyan
if (Test-API "GET /players paginated" "GET" "/players?page=1&pageSize=10" $null $true 200) { $passed++ } else { $failed++ }
if (Test-API "GET /players filters" "GET" "/players?position=Forward&status=Active" $null $true 200) { $passed++ } else { $failed++ }
if (Test-API "GET /players search" "GET" "/players?search=Test" $null $true 200) { $passed++ } else { $failed++ }
if (Test-API "GET /players/:id" "GET" "/players/$playerId" $null $true 200) { $passed++ } else { $failed++ }
if (Test-API "PUT /players/:id update" "PUT" "/players/$playerId" @{firstName="Updated";lastName="Player";position="Midfielder";jerseyNumber=11;clubId=$clubId;dateOfBirth="2000-01-01"} $true 200) { $passed++ } else { $failed++ }
if (Test-API "DELETE /players/:id" "DELETE" "/players/$playerId" $null $true 204) { $passed++ } else { $failed++ }
Write-Host ""

Write-Host "STADIUMS ENDPOINTS:" -ForegroundColor Cyan
if (Test-API "GET /stadiums paginated" "GET" "/stadiums?page=1&pageSize=10" $null $true 200) { $passed++ } else { $failed++ }
if (Test-API "GET /stadiums search" "GET" "/stadiums?search=Test" $null $true 200) { $passed++ } else { $failed++ }
if (Test-API "GET /stadiums/:id" "GET" "/stadiums/$stadiumId" $null $true 200) { $passed++ } else { $failed++ }
if (Test-API "PUT /stadiums/:id update" "PUT" "/stadiums/$stadiumId" @{name="Updated Stadium";city="Test City";capacity=60000;yearBuilt=2010} $true 200) { $passed++ } else { $failed++ }
if (Test-API "DELETE /stadiums/:id" "DELETE" "/stadiums/$stadiumId" $null $true 204) { $passed++ } else { $failed++ }
Write-Host ""

Write-Host "VALIDATION:" -ForegroundColor Cyan
if (Test-API "POST invalid club" "POST" "/clubs" @{name=""} $true 400) { $passed++ } else { $failed++ }
if (Test-API "POST missing fields" "POST" "/players" @{firstName="Test"} $true 400) { $passed++ } else { $failed++ }
Write-Host ""

Write-Host "AUTHORIZATION:" -ForegroundColor Cyan
if (Test-API "GET without token" "GET" "/clubs?page=1&pageSize=10" $null $false 200) { $passed++ } else { $failed++ }
if (Test-API "POST without token" "POST" "/clubs" @{name="Test"} $false 401) { $passed++ } else { $failed++ }
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

if ($rate -eq 100) {
    Write-Host "ALL TESTS PASSED!" -ForegroundColor Green
} else {
    Write-Host "Some tests failed. Review above." -ForegroundColor Yellow
}
