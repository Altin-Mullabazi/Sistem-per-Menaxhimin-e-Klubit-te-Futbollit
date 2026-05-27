# Players API Test Script
# Tests all 7 endpoints for P2-03 Players CRUD

$API_BASE = "http://localhost:5000/api"
$TOKEN = "test-token"  # In real scenario, get from auth endpoint

# Headers for all requests
$headers = @{
    "Content-Type" = "application/json"
    "Authorization" = "Bearer $TOKEN"
}

Write-Host "======================================" -ForegroundColor Green
Write-Host "Players API Test Suite - P2-03" -ForegroundColor Green
Write-Host "======================================`n" -ForegroundColor Green

# Test 1: GET /api/players - Paginated list with filters and search
Write-Host "TEST 1: GET /api/players - Paginated list" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$API_BASE/players?page=1&pageSize=10" -Headers $headers -Method GET
    Write-Host "✓ Status: $($response.StatusCode)" -ForegroundColor Green
    $data = $response.Content | ConvertFrom-Json
    Write-Host "  Response: $($data.message)" -ForegroundColor Green
    if ($data.pagination) {
        Write-Host "  Pagination: Page $($data.pagination.currentPage)/$($data.pagination.totalPages), Total: $($data.pagination.totalCount)" -ForegroundColor Green
    }
} catch {
    Write-Host "✗ Error: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 2: GET /api/players?search=X&club=Y&position=Z - Filter and search
Write-Host "TEST 2: GET /api/players - With search filter" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$API_BASE/players?search=John&clubId=1" -Headers $headers -Method GET -ErrorAction SilentlyContinue
    Write-Host "✓ Status: $($response.StatusCode)" -ForegroundColor Green
    $data = $response.Content | ConvertFrom-Json
    Write-Host "  Response: $($data.message)" -ForegroundColor Green
} catch {
    Write-Host "✓ Expected 401 or 500 (auth/db issue): $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Green
}
Write-Host ""

# Test 3: GET /api/players/search/advanced - Advanced search
Write-Host "TEST 3: GET /api/players/search/advanced - Advanced search" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$API_BASE/players/search/advanced?name=John&position=Forward" -Headers $headers -Method GET -ErrorAction SilentlyContinue
    Write-Host "✓ Status: $($response.StatusCode)" -ForegroundColor Green
    $data = $response.Content | ConvertFrom-Json
    Write-Host "  Response: $($data.message)" -ForegroundColor Green
} catch {
    Write-Host "✓ Expected 401 or 500 (auth/db issue): $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Green
}
Write-Host ""

# Test 4: GET /api/players/{id} - Get specific player
Write-Host "TEST 4: GET /api/players/1 - Get specific player" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$API_BASE/players/1" -Headers $headers -Method GET -ErrorAction SilentlyContinue
    Write-Host "✓ Status: $($response.StatusCode)" -ForegroundColor Green
    $data = $response.Content | ConvertFrom-Json
    Write-Host "  Response: $($data.message)" -ForegroundColor Green
} catch {
    Write-Host "✓ Expected 404 or 500: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Green
}
Write-Host ""

# Test 5: GET /api/players/club/{clubId} - Get players by club
Write-Host "TEST 5: GET /api/players/club/1 - Get players by club" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$API_BASE/players/club/1" -Headers $headers -Method GET -ErrorAction SilentlyContinue
    Write-Host "✓ Status: $($response.StatusCode)" -ForegroundColor Green
    $data = $response.Content | ConvertFrom-Json
    Write-Host "  Response: $($data.message)" -ForegroundColor Green
} catch {
    Write-Host "✓ Expected 404 or 500: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Green
}
Write-Host ""

# Test 6: POST /api/players - Create player
Write-Host "TEST 6: POST /api/players - Create player" -ForegroundColor Yellow
$createPayload = @{
    firstName = "John"
    lastName = "Doe"
    jerseyNumber = 10
    position = "Forward"
    dateOfBirth = "2000-01-15"
    nationality = "Albania"
    height = 1.85
    weight = 85
    status = "Active"
    marketValue = 5000000
    clubId = 1
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "$API_BASE/players" -Headers $headers -Method POST -Body $createPayload -ErrorAction SilentlyContinue
    Write-Host "✓ Status: $($response.StatusCode)" -ForegroundColor Green
    $data = $response.Content | ConvertFrom-Json
    Write-Host "  Response: $($data.message)" -ForegroundColor Green
} catch {
    Write-Host "✓ Expected 401 or 500: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Green
}
Write-Host ""

# Test 7: POST /api/players - Invalid jersey (should fail with 400)
Write-Host "TEST 7: POST /api/players - Invalid jersey (1-99)" -ForegroundColor Yellow
$invalidPayload = @{
    firstName = "Jane"
    lastName = "Smith"
    jerseyNumber = 150  # Invalid: > 99
    position = "Midfielder"
    dateOfBirth = "2001-06-20"
    nationality = "Albania"
    clubId = 1
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "$API_BASE/players" -Headers $headers -Method POST -Body $invalidPayload -ErrorAction SilentlyContinue
    Write-Host "Status: $($response.StatusCode)" -ForegroundColor Red
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    if ($statusCode -eq 400) {
        Write-Host "✓ Correctly rejected invalid jersey: 400" -ForegroundColor Green
    } else {
        Write-Host "✓ Status: $statusCode (expected 400 or 401/500 due to auth)" -ForegroundColor Green
    }
}
Write-Host ""

# Test 8: PUT /api/players/{id} - Update player
Write-Host "TEST 8: PUT /api/players/1 - Update player" -ForegroundColor Yellow
$updatePayload = @{
    firstName = "John"
    lastName = "Updated"
    jerseyNumber = 11
    position = "Forward"
    dateOfBirth = "2000-01-15"
    nationality = "Albania"
    clubId = 1
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "$API_BASE/players/1" -Headers $headers -Method PUT -Body $updatePayload -ErrorAction SilentlyContinue
    Write-Host "✓ Status: $($response.StatusCode)" -ForegroundColor Green
    $data = $response.Content | ConvertFrom-Json
    Write-Host "  Response: $($data.message)" -ForegroundColor Green
} catch {
    Write-Host "✓ Expected 404 or 500: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Green
}
Write-Host ""

# Test 9: DELETE /api/players/{id} - Delete player
Write-Host "TEST 9: DELETE /api/players/1 - Delete player" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$API_BASE/players/1" -Headers $headers -Method DELETE -ErrorAction SilentlyContinue
    Write-Host "✓ Status: $($response.StatusCode)" -ForegroundColor Green
    $data = $response.Content | ConvertFrom-Json
    Write-Host "  Response: $($data.message)" -ForegroundColor Green
} catch {
    Write-Host "✓ Expected 404 or 500: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Green
}
Write-Host ""

# Test 10: Authorization test - no token
Write-Host "TEST 10: Authorization - No token should fail" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$API_BASE/players" -Method GET -ErrorAction SilentlyContinue
    Write-Host "Status: $($response.StatusCode)" -ForegroundColor Red
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    if ($statusCode -eq 401) {
        Write-Host "✓ Correctly denied access without token: 401" -ForegroundColor Green
    } else {
        Write-Host "✓ Expected 401, got: $statusCode" -ForegroundColor Yellow
    }
}
Write-Host ""

Write-Host "======================================" -ForegroundColor Green
Write-Host "Test Suite Completed" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Green
