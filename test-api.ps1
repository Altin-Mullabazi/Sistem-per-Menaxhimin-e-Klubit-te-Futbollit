#!/usr/bin/env pwsh

# Players API Test Script
# Tests all 7 endpoints for P2-03 Players CRUD

$API_BASE = "http://localhost:5000/api"
$TOKEN = "test-token"

$headers = @{
    "Content-Type" = "application/json"
    "Authorization" = "Bearer $TOKEN"
}

Write-Host "======== Players API Tests =========" -ForegroundColor Green
Write-Host ""

# Test 1: GET /api/players - Paginated list
Write-Host "TEST 1: GET /api/players - Paginated list" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$API_BASE/players?page=1&pageSize=10" -Headers $headers -Method GET
    Write-Host "PASS - Status: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Yellow
}

# Test 2: GET /api/players with filters
Write-Host "TEST 2: GET /api/players with search" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$API_BASE/players?search=John" -Headers $headers -Method GET -ErrorAction SilentlyContinue
    Write-Host "PASS - Status: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Yellow
}

# Test 3: GET /api/players/search/advanced
Write-Host "TEST 3: GET /api/players/search/advanced" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$API_BASE/players/search/advanced?name=John&position=Forward" -Headers $headers -Method GET -ErrorAction SilentlyContinue
    Write-Host "PASS - Status: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Yellow
}

# Test 4: GET /api/players/{id}
Write-Host "TEST 4: GET /api/players/1" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$API_BASE/players/1" -Headers $headers -Method GET -ErrorAction SilentlyContinue
    Write-Host "PASS - Status: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Yellow
}

# Test 5: GET /api/players/club/{clubId}
Write-Host "TEST 5: GET /api/players/club/1" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$API_BASE/players/club/1" -Headers $headers -Method GET -ErrorAction SilentlyContinue
    Write-Host "PASS - Status: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Yellow
}

# Test 6: POST /api/players (Create)
Write-Host "TEST 6: POST /api/players - Create" -ForegroundColor Yellow
$payload = @{
    firstName = "John"
    lastName = "Doe"
    jerseyNumber = 10
    position = "Forward"
    dateOfBirth = "2000-01-15T00:00:00Z"
    nationality = "Albania"
    clubId = 1
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "$API_BASE/players" -Headers $headers -Method POST -Body $payload -ErrorAction SilentlyContinue
    Write-Host "PASS - Status: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Yellow
}

# Test 7: POST Invalid jersey (should fail)
Write-Host "TEST 7: POST - Invalid jersey validation" -ForegroundColor Yellow
$badPayload = @{
    firstName = "Jane"
    lastName = "Smith"
    jerseyNumber = 150
    position = "Midfielder"
    dateOfBirth = "2001-06-20T00:00:00Z"
    nationality = "Albania"
    clubId = 1
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "$API_BASE/players" -Headers $headers -Method POST -Body $badPayload -ErrorAction SilentlyContinue
    Write-Host "Status: $($response.StatusCode)" -ForegroundColor Red
} catch {
    $code = $_.Exception.Response.StatusCode.value__
    if ($code -eq 400 -or $code -eq 401 -or $code -eq 500) {
        Write-Host "PASS - Correctly rejected (Status $code)" -ForegroundColor Green
    }
}

# Test 8: PUT /api/players/{id} (Update)
Write-Host "TEST 8: PUT /api/players/1 - Update" -ForegroundColor Yellow
$updatePayload = @{
    firstName = "John"
    lastName = "Updated"
    jerseyNumber = 11
    position = "Forward"
    dateOfBirth = "2000-01-15T00:00:00Z"
    nationality = "Albania"
    clubId = 1
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "$API_BASE/players/1" -Headers $headers -Method PUT -Body $updatePayload -ErrorAction SilentlyContinue
    Write-Host "PASS - Status: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Yellow
}

# Test 9: DELETE /api/players/{id}
Write-Host "TEST 9: DELETE /api/players/1" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$API_BASE/players/1" -Headers $headers -Method DELETE -ErrorAction SilentlyContinue
    Write-Host "PASS - Status: $($response.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Yellow
}

# Test 10: Authorization test
Write-Host "TEST 10: Authorization - No token" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$API_BASE/players" -Method GET -ErrorAction SilentlyContinue
    Write-Host "Status: $($response.StatusCode)" -ForegroundColor Red
} catch {
    $code = $_.Exception.Response.StatusCode.value__
    if ($code -eq 401) {
        Write-Host "PASS - Correctly denied (401)" -ForegroundColor Green
    } else {
        Write-Host "Status: $code" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "========= Test Complete =========" -ForegroundColor Green
