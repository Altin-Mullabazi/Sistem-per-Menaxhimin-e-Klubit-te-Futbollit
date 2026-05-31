#!/usr/bin/env pwsh

# P2 COMPREHENSIVE ENDPOINT TEST SCRIPT
# Tests all CLUBS, PLAYERS, STADIUMS endpoints

$API_BASE = "http://localhost:5000/api"
$results = @()
$testCount = 0
$passCount = 0

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

# Test function
function Test-Endpoint {
    param(
        [string]$name,
        [string]$uri,
        [string]$method = "GET",
        [object]$body = $null,
        [hashtable]$headers = @{},
        [int]$expectedStatus = 200
    )
    
    $testCount++
    Write-Info "[$testCount] Testing: $name"
    Write-Info "     Method: $method | URI: $uri"
    
    try {
        if ($method -eq "GET") {
            $response = Invoke-WebRequest -Uri "$API_BASE$uri" -Method $method -Headers $headers -ErrorAction SilentlyContinue
        } else {
            $bodyJson = $body | ConvertTo-Json -Depth 10
            $response = Invoke-WebRequest -Uri "$API_BASE$uri" -Method $method -Body $bodyJson -Headers $headers -ContentType "application/json" -ErrorAction SilentlyContinue
        }
        
        $status = $response.StatusCode
        $content = $response.Content | ConvertFrom-Json
        
        if ($status -eq $expectedStatus) {
            Write-Pass "     Status: $status (PASS)"
            $passCount++
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
        
        return $response
    } catch {
        $statusCode = $_.Exception.Response.StatusCode.Value__
        if ($statusCode -eq $expectedStatus) {
            Write-Pass "     Status: $statusCode (PASS)"
            $passCount++
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

# ====== CLUBS ENDPOINTS ======
Write-Test "CLUBS ENDPOINTS (7 tests)"

# 1. GET /api/clubs - paginated
Test-Endpoint "GET /api/clubs (paginated)" "/clubs?page=1&pageSize=10" "GET"

# 2. GET /api/clubs with search
Test-Endpoint "GET /api/clubs (with search)" "/clubs?search=FC" "GET"

# 3. GET /api/clubs with filter
Test-Endpoint "GET /api/clubs (with filter)" "/clubs?filter=Active" "GET"

# 4. GET /api/clubs/{id}
Test-Endpoint "GET /api/clubs/1 (specific)" "/clubs/1" "GET"

# 5. POST /api/clubs (create)
$clubData = @{
    name = "Test FC"
    city = "Test City"
    founded = "2020-01-01"
    stadium = "Test Stadium"
}
Test-Endpoint "POST /api/clubs (create)" "/clubs" "POST" $clubData

# 6. PUT /api/clubs/{id} (update)
$updateClubData = @{
    name = "Updated Test FC"
    city = "Updated City"
    founded = "2020-01-01"
    stadium = "Updated Stadium"
}
Test-Endpoint "PUT /api/clubs/1 (update)" "/clubs/1" "PUT" $updateClubData

# 7. DELETE /api/clubs/{id}
Test-Endpoint "DELETE /api/clubs/999 (delete)" "/clubs/999" "DELETE" $null @{} 200

# ====== PLAYERS ENDPOINTS ======
Write-Test "PLAYERS ENDPOINTS (8 tests)"

# 1. GET /api/players - paginated
Test-Endpoint "GET /api/players (paginated)" "/players?page=1&pageSize=10" "GET"

# 2. GET /api/players with filters
Test-Endpoint "GET /api/players (with filters)" "/players?position=Forward&status=Active" "GET"

# 3. GET /api/players with search
Test-Endpoint "GET /api/players (with search)" "/players?search=John" "GET"

# 4. GET /api/players/club/{clubId}
Test-Endpoint "GET /api/players/club/1 (by club)" "/players/club/1" "GET"

# 5. GET /api/players/{id}
Test-Endpoint "GET /api/players/1 (specific)" "/players/1" "GET"

# 6. POST /api/players (create)
$playerData = @{
    firstName = "Test"
    lastName = "Player"
    position = "Forward"
    dateOfBirth = "2000-01-01"
    nationality = "Albania"
    jerseyNumber = 10
    clubId = 1
}
Test-Endpoint "POST /api/players (create)" "/players" "POST" $playerData

# 7. PUT /api/players/{id} (update)
$updatePlayerData = @{
    firstName = "Updated"
    lastName = "Player"
    position = "Midfielder"
    dateOfBirth = "2000-01-01"
    nationality = "Albania"
    jerseyNumber = 11
    clubId = 1
}
Test-Endpoint "PUT /api/players/1 (update)" "/players/1" "PUT" $updatePlayerData

# 8. DELETE /api/players/{id}
Test-Endpoint "DELETE /api/players/999 (delete)" "/players/999" "DELETE" $null @{} 200

# ====== STADIUMS ENDPOINTS ======
Write-Test "STADIUMS ENDPOINTS (5 tests)"

# 1. GET /api/stadiums - paginated
Test-Endpoint "GET /api/stadiums (paginated)" "/stadiums?page=1&pageSize=10" "GET"

# 2. GET /api/stadiums with search/filter
Test-Endpoint "GET /api/stadiums (with search)" "/stadiums?search=Olympic" "GET"

# 3. GET /api/stadiums/{id}
Test-Endpoint "GET /api/stadiums/1 (specific)" "/stadiums/1" "GET"

# 4. POST /api/stadiums (create)
$stadiumData = @{
    name = "Test Stadium"
    city = "Test City"
    capacity = 50000
    founded = "2010-01-01"
}
Test-Endpoint "POST /api/stadiums (create)" "/stadiums" "POST" $stadiumData

# 5. PUT /api/stadiums/{id} (update)
$updateStadiumData = @{
    name = "Updated Test Stadium"
    city = "Updated City"
    capacity = 60000
    founded = "2010-01-01"
}
Test-Endpoint "PUT /api/stadiums/1 (update)" "/stadiums/1" "PUT" $updateStadiumData

# ====== VALIDATION TESTS ======
Write-Test "VALIDATION TESTS"

# Invalid data - should return 400
$invalidClubData = @{ name = "" }  # Missing required fields
Test-Endpoint "POST /api/clubs (invalid data)" "/clubs" "POST" $invalidClubData 400

# Missing required fields - should return 400
$incompletePlayerData = @{ firstName = "Test" }  # Missing other required fields
Test-Endpoint "POST /api/players (missing fields)" "/players" "POST" $incompletePlayerData 400

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
$results | Format-Table -AutoSize -Property @{Name="Test"; Expression={$_.Test}}, @{Name="Status"; Expression={$_.Status}}, @{Name="Result"; Expression={if ($_.Result -eq "PASS") {Write-Host $_.Result -ForegroundColor Green -NoNewline; $_.Result} else {Write-Host $_.Result -ForegroundColor Red -NoNewline; $_.Result}}}

Write-Host "`n========================================" -ForegroundColor Magenta
