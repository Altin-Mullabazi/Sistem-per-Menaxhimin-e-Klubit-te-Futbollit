#!/usr/bin/env pwsh

# P2 COMPREHENSIVE ENDPOINT TEST SCRIPT WITH AUTHENTICATION
# Tests all CLUBS, PLAYERS, STADIUMS endpoints

$API_BASE = "http://localhost:5000/api"
$AUTH_URL = "$API_BASE/auth/login"
$results = @()
$testCount = 0
$passCount = 0
$token = ""

# Color functions
function Write-Pass {
    param([string]$message)
    Write-Host $message -ForegroundColor Green
}

function Write-Fail {
    param([string]$message)
    Write-Host $message -ForegroundColor Red
}

function Write-Info {
    param([string]$message)
    Write-Host $message -ForegroundColor Cyan
}

function Write-Test {
    param([string]$message)
    Write-Host "`n$message" -ForegroundColor Yellow
}

# Get JWT Token
function Get-AuthToken {
    Write-Test "AUTHENTICATION"
    Write-Info "[AUTH] Logging in with admin credentials..."
    
    $loginData = @{
        email = "admin@footballclub.com"
        password = "Admin@123"
    } | ConvertTo-Json
    
    try {
        $response = Invoke-WebRequest -Uri $AUTH_URL -Method POST -Body $loginData -ContentType "application/json" -UseBasicParsing -ErrorAction SilentlyContinue
        
        if ($response.StatusCode -eq 200) {
            $content = $response.Content | ConvertFrom-Json
            $jwt = $content.accessToken
            Write-Pass "[AUTH] Login successful! Token obtained."
            return $jwt
        } else {
            Write-Fail "[AUTH] Login failed with status $($response.StatusCode)"
            return $null
        }
    } catch {
        $statusCode = $_.Exception.Response.StatusCode.Value__
        Write-Fail "[AUTH] Login failed with status $statusCode"
        return $null
    }
}

# Test function
function Test-Endpoint {
    param(
        [string]$name,
        [string]$uri,
        [string]$method = "GET",
        [object]$body = $null,
        [string]$token = "",
        [int]$expectedStatus = 200
    )
    
    $testCount++
    Write-Info "[$testCount] Testing: $name"
    Write-Info "     Method: $method | URI: $uri"
    
    $headers = @{
        "Content-Type" = "application/json"
    }
    
    if ($token) {
        $headers["Authorization"] = "Bearer $token"
    }
    
    try {
        if ($method -eq "GET") {
            $response = Invoke-WebRequest -Uri "$API_BASE$uri" -Method $method -Headers $headers -UseBasicParsing -ErrorAction SilentlyContinue
        } else {
            $bodyJson = $body | ConvertTo-Json -Depth 10
            $response = Invoke-WebRequest -Uri "$API_BASE$uri" -Method $method -Body $bodyJson -Headers $headers -UseBasicParsing -ErrorAction SilentlyContinue
        }
        
        $status = $response.StatusCode
        
        if ($status -eq $expectedStatus) {
            Write-Pass "     Status: $status (PASS)"
            $script:passCount++
            $results += @{
                Test = $name
                Status = $status
                Result = "PASS"
            }
        } else {
            Write-Fail "     Status: $status (Expected: $expectedStatus - FAIL)"
            $results += @{
                Test = $name
                Status = $status
                Result = "FAIL"
            }
        }
    } catch {
        $statusCode = $_.Exception.Response.StatusCode.Value__
        if ($statusCode -eq $expectedStatus) {
            Write-Pass "     Status: $statusCode (PASS)"
            $script:passCount++
            $results += @{
                Test = $name
                Status = $statusCode
                Result = "PASS"
            }
        } else {
            Write-Fail "     Status: $statusCode (Expected: $expectedStatus - FAIL)"
            $results += @{
                Test = $name
                Status = $statusCode
                Result = "FAIL"
            }
        }
    }
}

Write-Host "========================================" -ForegroundColor Magenta
Write-Host "P2 API ENDPOINT TEST SUITE" -ForegroundColor Magenta
Write-Host "========================================`n" -ForegroundColor Magenta

# Get authentication token
$token = Get-AuthToken
if (-not $token) {
    Write-Fail "Failed to obtain authentication token. Exiting."
    exit 1
}

# ====== CLUBS ENDPOINTS ======
Write-Test "CLUBS ENDPOINTS (7 tests)"

# 1. GET /api/clubs - paginated (NO AUTH)
Test-Endpoint "GET /api/clubs (paginated)" "/clubs?page=1&pageSize=10" "GET" $null ""

# 2. GET /api/clubs with search (NO AUTH)
Test-Endpoint "GET /api/clubs (with search)" "/clubs?search=FC" "GET" $null ""

# 3. GET /api/clubs with filter (NO AUTH)
Test-Endpoint "GET /api/clubs (with filter)" "/clubs?filter=Active" "GET" $null ""

# 4. GET /api/clubs/{id} (NO AUTH - should be 404 if doesn't exist)
Test-Endpoint "GET /api/clubs/1 (specific)" "/clubs/1" "GET" $null "" 404

# 5. POST /api/clubs (create) - WITH AUTH
$clubData = @{
    name = "Test FC Unity"
    city = "Test City"
    founded = "2020-01-01"
    stadium = "Test Stadium"
}
Test-Endpoint "POST /api/clubs (create)" "/clubs" "POST" $clubData $token 201

# 6. PUT /api/clubs/{id} (update) - WITH AUTH
$updateClubData = @{
    name = "Updated Test FC"
    city = "Updated City"
    founded = "2020-01-01"
    stadium = "Updated Stadium"
}
Test-Endpoint "PUT /api/clubs/1 (update)" "/clubs/1" "PUT" $updateClubData $token 200

# 7. DELETE /api/clubs/{id} - WITH AUTH
Test-Endpoint "DELETE /api/clubs/999 (delete)" "/clubs/999" "DELETE" $null $token 404

# ====== PLAYERS ENDPOINTS ======
Write-Test "PLAYERS ENDPOINTS (8 tests)"

# 1. GET /api/players - paginated (WITH AUTH - checking protected)
Test-Endpoint "GET /api/players (paginated)" "/players?page=1&pageSize=10" "GET" $null $token

# 2. GET /api/players with filters (WITH AUTH)
Test-Endpoint "GET /api/players (with filters)" "/players?position=Forward&status=Active" "GET" $null $token

# 3. GET /api/players with search (WITH AUTH)
Test-Endpoint "GET /api/players (with search)" "/players?search=John" "GET" $null $token

# 4. GET /api/players/club/{clubId} (WITH AUTH)
Test-Endpoint "GET /api/players/club/1 (by club)" "/players/club/1" "GET" $null $token

# 5. GET /api/players/{id} (WITH AUTH - should be 404 if doesn't exist)
Test-Endpoint "GET /api/players/1 (specific)" "/players/1" "GET" $null $token 404

# 6. POST /api/players (create) - WITH AUTH
$playerData = @{
    firstName = "Test"
    lastName = "Player"
    position = "Forward"
    dateOfBirth = "2000-01-01"
    nationality = "Albania"
    jerseyNumber = 10
    clubId = 1
}
Test-Endpoint "POST /api/players (create)" "/players" "POST" $playerData $token 201

# 7. PUT /api/players/{id} (update) - WITH AUTH
$updatePlayerData = @{
    firstName = "Updated"
    lastName = "Player"
    position = "Midfielder"
    dateOfBirth = "2000-01-01"
    nationality = "Albania"
    jerseyNumber = 11
    clubId = 1
}
Test-Endpoint "PUT /api/players/1 (update)" "/players/1" "PUT" $updatePlayerData $token 200

# 8. DELETE /api/players/{id} - WITH AUTH
Test-Endpoint "DELETE /api/players/999 (delete)" "/players/999" "DELETE" $null $token 404

# ====== STADIUMS ENDPOINTS ======
Write-Test "STADIUMS ENDPOINTS (5 tests)"

# 1. GET /api/stadiums - paginated (WITH AUTH)
Test-Endpoint "GET /api/stadiums (paginated)" "/stadiums?page=1&pageSize=10" "GET" $null $token

# 2. GET /api/stadiums with search (WITH AUTH)
Test-Endpoint "GET /api/stadiums (with search)" "/stadiums?search=Olympic" "GET" $null $token

# 3. GET /api/stadiums/{id} (WITH AUTH - should be 404 if doesn't exist)
Test-Endpoint "GET /api/stadiums/1 (specific)" "/stadiums/1" "GET" $null $token 404

# 4. POST /api/stadiums (create) - WITH AUTH
$stadiumData = @{
    name = "Test Stadium"
    city = "Test City"
    capacity = 50000
    founded = "2010-01-01"
}
Test-Endpoint "POST /api/stadiums (create)" "/stadiums" "POST" $stadiumData $token 201

# 5. PUT /api/stadiums/{id} (update) - WITH AUTH
$updateStadiumData = @{
    name = "Updated Test Stadium"
    city = "Updated City"
    capacity = 60000
    founded = "2010-01-01"
}
Test-Endpoint "PUT /api/stadiums/1 (update)" "/stadiums/1" "PUT" $updateStadiumData $token 200

# ====== VALIDATION TESTS ======
Write-Test "VALIDATION TESTS (2 tests)"

# Invalid data - should return 400
$invalidClubData = @{ name = "" }
Test-Endpoint "POST /api/clubs (invalid data)" "/clubs" "POST" $invalidClubData $token 400

# Missing required fields - should return 400
$incompletePlayerData = @{ firstName = "Test" }
Test-Endpoint "POST /api/players (missing fields)" "/players" "POST" $incompletePlayerData $token 400

# ====== AUTHORIZATION TESTS ======
Write-Test "AUTHORIZATION TESTS (2 tests)"

# GET without token - should succeed (public endpoint)
Test-Endpoint "GET /api/clubs (no token - public)" "/clubs?page=1&pageSize=10" "GET" $null "" 200

# POST without token - should fail with 401
$clubData = @{
    name = "Test FC"
    city = "Test City"
    founded = "2020-01-01"
    stadium = "Test Stadium"
}
Test-Endpoint "POST /api/clubs (no token - protected)" "/clubs" "POST" $clubData "" 401

# ====== SUMMARY ======
Write-Host "`n========================================" -ForegroundColor Magenta
Write-Host "TEST SUMMARY" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta
Write-Host "Total Tests: $testCount" -ForegroundColor Cyan
Write-Pass "Passed: $passCount"
Write-Fail "Failed: $($testCount - $passCount)"
Write-Host ""

# Print detailed results
Write-Host "DETAILED RESULTS:" -ForegroundColor Yellow
Write-Host ""
$results | ForEach-Object {
    $idx = [array]::IndexOf($results, $_) + 1
    $resultColor = if ($_.Result -eq "PASS") { "Green" } else { "Red" }
    Write-Host "[$('{0:D2}' -f $idx)] $($_.Test)" -ForegroundColor Cyan
    Write-Host "     Status: $($_.Status) - $($_.Result)" -ForegroundColor $resultColor
}

Write-Host "`n========================================" -ForegroundColor Magenta
if ($testCount -gt 0) {
    $passRate = [math]::Round(($passCount/$testCount)*100, 2)
    Write-Host "PASS RATE: $passRate% ($passCount/$testCount)" -ForegroundColor Magenta
}
Write-Host "========================================`n" -ForegroundColor Magenta
