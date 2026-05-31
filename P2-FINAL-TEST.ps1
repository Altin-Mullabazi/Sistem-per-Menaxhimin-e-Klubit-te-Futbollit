#!/usr/bin/env pwsh

# P2 FINAL ENDPOINT TEST SCRIPT - CORRECTED
# Tests all CLUBS, PLAYERS, STADIUMS endpoints with correct field names

$API_BASE = "http://localhost:5000/api"
$AUTH_URL = "$API_BASE/auth/login"

Write-Host "========================================" -ForegroundColor Magenta
Write-Host "P2 API ENDPOINT TEST SUITE - FINAL" -ForegroundColor White  
Write-Host "========================================" -ForegroundColor Magenta
Write-Host ""

# Get JWT Token
Write-Host "AUTHENTICATION" -ForegroundColor Yellow
$loginData = @{
    email = "admin@footballclub.com"
    password = "Admin@123"
} | ConvertTo-Json

$response = Invoke-WebRequest -Uri $AUTH_URL -Method POST -Body $loginData -ContentType "application/json" -UseBasicParsing
$content = $response.Content | ConvertFrom-Json
$token = $content.accessToken
Write-Host "[OK] Authenticated as admin@footballclub.com" -ForegroundColor Green
Write-Host ""

$testResults = @()

# Test wrapper function
function Run-Test {
    param([string]$name, [string]$method, [string]$endpoint, [object]$data = $null, [bool]$withToken = $true, [int]$expectedStatus = 200)
    
    $headers = @{ "Content-Type" = "application/json" }
    if ($withToken) { $headers["Authorization"] = "Bearer $token" }
    
    $url = "$API_BASE$endpoint"
    $statusCode = $null
    $success = $false
    
    try {
        if ($method -eq "GET") {
            $r = Invoke-WebRequest -Uri $url -Method GET -Headers $headers -UseBasicParsing
            $statusCode = $r.StatusCode
        } else {
            $bodyJson = $data | ConvertTo-Json -Depth 10
            $r = Invoke-WebRequest -Uri $url -Method $method -Body $bodyJson -Headers $headers -UseBasicParsing
            $statusCode = $r.StatusCode
        }
        $success = ($statusCode -eq $expectedStatus)
    } catch {
        $statusCode = $_.Exception.Response.StatusCode.Value__
        $success = ($statusCode -eq $expectedStatus)
    }
    
    $status = if ($success) { "[PASS]" } else { "[FAIL]" }
    $color = if ($success) { "Green" } else { "Red" }
    Write-Host "  $status | $name | Status: $statusCode (Expected: $expectedStatus)" -ForegroundColor $color
    
    $testResults += @{ name = $name; passed = $success }
    return $success
}

# CLUBS ENDPOINTS
Write-Host "CLUBS (7 endpoints)" -ForegroundColor Cyan
Write-Host "────────────────────" -ForegroundColor Cyan
Run-Test "GET paginated" "GET" "/clubs?page=1&pageSize=10" $null $false 200
Run-Test "GET with search" "GET" "/clubs?search=FC" $null $false 200
Run-Test "GET with filter" "GET" "/clubs?filter=Active" $null $false 200
Run-Test "GET specific (404 - no data)" "GET" "/clubs/999" $null $false 404
Run-Test "POST create" "POST" "/clubs" @{ name = "New Club"; city = "City"; foundedYear = 2020 } $true 201
Run-Test "PUT update (404 - doesn't exist)" "PUT" "/clubs/999" @{ name = "Update"; foundedYear = 2021 } $true 404
Run-Test "DELETE delete (404 - doesn't exist)" "DELETE" "/clubs/999" $null $true 404
Write-Host ""

# PLAYERS ENDPOINTS  
Write-Host "PLAYERS (8 endpoints)" -ForegroundColor Cyan
Write-Host "────────────────────" -ForegroundColor Cyan
Run-Test "GET paginated" "GET" "/players?page=1&pageSize=10" $null $true 200
Run-Test "GET with filters" "GET" "/players?position=Forward&status=Active" $null $true 200
Run-Test "GET with search" "GET" "/players?search=John" $null $true 200
Run-Test "GET by club" "GET" "/players/club/999" $null $true 200
Run-Test "GET specific (200 - seed data exists)" "GET" "/players/1" $null $true 200
Run-Test "POST create" "POST" "/players" @{ firstName = "Test"; lastName = "Player"; position = "Forward"; jerseyNumber = 10; clubId = 1; dateOfBirth = "2000-01-01" } $true 201
Run-Test "PUT update (200 - seed data exists)" "PUT" "/players/1" @{ firstName = "Updated"; lastName = "Player"; position = "Midfielder"; jerseyNumber = 11; clubId = 1 } $true 200
Run-Test "DELETE delete (404 - doesn't exist)" "DELETE" "/players/999" $null $true 404
Write-Host ""

# STADIUMS ENDPOINTS
Write-Host "STADIUMS (5 endpoints)" -ForegroundColor Cyan
Write-Host "────────────────────" -ForegroundColor Cyan
Run-Test "GET paginated" "GET" "/stadiums?page=1&pageSize=10" $null $true 200
Run-Test "GET with search" "GET" "/stadiums?search=Olympic" $null $true 200
Run-Test "GET specific (200 - seed data exists)" "GET" "/stadiums/1" $null $true 200
Run-Test "POST create" "POST" "/stadiums" @{ name = "Test Stadium"; city = "City"; capacity = 50000; yearBuilt = 2010 } $true 201
Run-Test "PUT update (200 - seed data exists)" "PUT" "/stadiums/1" @{ name = "Updated Stadium"; city = "City"; capacity = 60000; yearBuilt = 2010 } $true 200
Write-Host ""

# VALIDATION TESTS
Write-Host "VALIDATION (2 tests)" -ForegroundColor Cyan
Write-Host "────────────────────" -ForegroundColor Cyan
Run-Test "POST invalid club" "POST" "/clubs" @{ name = "" } $true 400
Run-Test "POST incomplete player" "POST" "/players" @{ firstName = "Test" } $true 400
Write-Host ""

# AUTHORIZATION TESTS
Write-Host "AUTHORIZATION (2 tests)" -ForegroundColor Cyan
Write-Host "────────────────────────" -ForegroundColor Cyan
Run-Test "GET without token (public)" "GET" "/clubs?page=1&pageSize=10" $null $false 200
Run-Test "POST without token (401)" "POST" "/clubs" @{ name = "Test" } $false 401
Write-Host ""

# SUMMARY
$passedCount = @($testResults | Where-Object { $_.passed }).Count
$totalCount = $testResults.Count
$passRate = [math]::Round(($passedCount / $totalCount) * 100, 2)

Write-Host "========================================" -ForegroundColor Magenta
Write-Host "FINAL RESULTS" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Magenta
Write-Host "Total Tests: $totalCount" -ForegroundColor Cyan
Write-Host "Passed: $passedCount" -ForegroundColor Green
Write-Host "Failed: $($totalCount - $passedCount)" -ForegroundColor $(if ($totalCount - $passedCount -eq 0) { "Green" } else { "Red" })
Write-Host "Pass Rate: $passRate%" -ForegroundColor $(if ($passRate -eq 100) { "Green" } else { "Yellow" })
Write-Host "========================================" -ForegroundColor Magenta
Write-Host ""

if ($passRate -eq 100) {
    Write-Host "ALL TESTS PASSED!" -ForegroundColor Green
} else {
    Write-Host "Some tests failed. Review results above." -ForegroundColor Yellow
}
Write-Host ""
