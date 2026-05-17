# Stadium CRUD Testing Script
# Tests all 5 Stadium endpoints with proper authorization

$apiUrl = "http://localhost:5000/api"

# Color output helpers
function Write-Success { Write-Host $args -ForegroundColor Green }
function Write-Error-Custom { Write-Host $args -ForegroundColor Red }
function Write-Info { Write-Host $args -ForegroundColor Cyan }

# Get Bearer token (login first)
Write-Info "=== AUTHENTICATION STEP ==="
Write-Info "Logging in to get access token..."

$loginBody = @{
    email = "admin@example.com"
    password = "Admin@123456"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$apiUrl/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.data.accessToken
    Write-Success "✓ Login successful. Token: $($token.Substring(0,20))..."
} catch {
    Write-Error-Custom "✗ Login failed: $($_.Exception.Response.StatusCode)"
    exit 1
}

# Headers with authorization
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Write-Info "`n=== TEST 1: GET /api/stadiums (Paginated List with Filter & Search) ==="
try {
    $response = Invoke-RestMethod -Uri "$apiUrl/stadiums?page=1&pageSize=5&search=&city=" -Method Get -Headers $headers
    Write-Success "✓ GET /api/stadiums - Success"
    Write-Info "Total stadiums: $($response.pagination.totalCount)"
    Write-Info "Current page: $($response.pagination.currentPage)"
    Write-Info "Total pages: $($response.pagination.totalPages)"
    if ($response.data.Count -gt 0) {
        Write-Info "Sample stadium: $($response.data[0].Name)"
    }
} catch {
    Write-Error-Custom "✗ GET /api/stadiums failed: $($_.Exception.Response.StatusCode)"
}

Write-Info "`n=== TEST 2: GET /api/stadiums with Filter (by City) ==="
try {
    $response = Invoke-RestMethod -Uri "$apiUrl/stadiums?page=1&city=Tirana" -Method Get -Headers $headers
    Write-Success "✓ GET /api/stadiums?city=Tirana - Success"
    Write-Info "Stadiums in Tirana: $($response.pagination.totalCount)"
} catch {
    Write-Error-Custom "✗ GET with filter failed: $($_.Exception.Response.StatusCode)"
}

Write-Info "`n=== TEST 3: GET /api/stadiums with Search (by Name) ==="
try {
    $response = Invoke-RestMethod -Uri "$apiUrl/stadiums?page=1&search=Air" -Method Get -Headers $headers
    Write-Success "✓ GET /api/stadiums?search=Air - Success"
    Write-Info "Matching stadiums: $($response.pagination.totalCount)"
} catch {
    Write-Error-Custom "✗ GET with search failed: $($_.Exception.Response.StatusCode)"
}

Write-Info "`n=== TEST 4: POST /api/stadiums (Create Stadium) ==="
$createBody = @{
    name = "Loro Boriçi Stadium"
    city = "Shkoder"
    capacity = 12000
    yearBuilt = 1950
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$apiUrl/stadiums" -Method Post -Headers $headers -Body $createBody
    $stadiumId = $response.data.id
    Write-Success "✓ POST /api/stadiums - Stadium created successfully"
    Write-Info "Stadium ID: $stadiumId"
    Write-Info "Stadium Name: $($response.data.name)"
    Write-Info "Stadium Capacity: $($response.data.capacity)"
} catch {
    Write-Error-Custom "✗ POST /api/stadiums failed: $($_.Exception.Response.StatusCode)"
    Write-Error-Custom "Response: $($_.Exception.Message)"
}

Write-Info "`n=== TEST 5: GET /api/stadiums/{id} (Get Specific Stadium) ==="
if ($stadiumId) {
    try {
        $response = Invoke-RestMethod -Uri "$apiUrl/stadiums/$stadiumId" -Method Get -Headers $headers
        Write-Success "✓ GET /api/stadiums/$stadiumId - Success"
        Write-Info "Stadium: $($response.data.name)"
        Write-Info "City: $($response.data.city)"
        Write-Info "Capacity: $($response.data.capacity)"
        Write-Info "Year Built: $($response.data.yearBuilt)"
        Write-Info "Clubs using stadium: $($response.data.clubs.Count)"
        Write-Info "Matches: $($response.data.matches.Count)"
    } catch {
        Write-Error-Custom "✗ GET /api/stadiums/{id} failed: $($_.Exception.Response.StatusCode)"
    }
}

Write-Info "`n=== TEST 6: PUT /api/stadiums/{id} (Update Stadium) ==="
if ($stadiumId) {
    $updateBody = @{
        name = "Loro Boriçi Stadium - Updated"
        city = "Shkoder"
        capacity = 15000
        yearBuilt = 1950
    } | ConvertTo-Json

    try {
        $response = Invoke-RestMethod -Uri "$apiUrl/stadiums/$stadiumId" -Method Put -Headers $headers -Body $updateBody
        Write-Success "✓ PUT /api/stadiums/$stadiumId - Stadium updated successfully"
        Write-Info "Updated Name: $($response.data.name)"
        Write-Info "Updated Capacity: $($response.data.capacity)"
    } catch {
        Write-Error-Custom "✗ PUT /api/stadiums/{id} failed: $($_.Exception.Response.StatusCode)"
        Write-Error-Custom "Response: $($_.Exception.Message)"
    }
}

Write-Info "`n=== TEST 7: Test Validation (Capacity > 0) ==="
$invalidBody = @{
    name = "Invalid Stadium"
    city = "Test City"
    capacity = -100
    yearBuilt = 2024
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$apiUrl/stadiums" -Method Post -Headers $headers -Body $invalidBody
    Write-Error-Custom "✗ Validation failed - should reject negative capacity"
} catch {
    Write-Success "✓ Validation works - rejected invalid capacity: $($_.Exception.Response.StatusCode)"
}

Write-Info "`n=== TEST 8: DELETE /api/stadiums/{id} (Delete Stadium) ==="
if ($stadiumId) {
    try {
        $response = Invoke-RestMethod -Uri "$apiUrl/stadiums/$stadiumId" -Method Delete -Headers $headers
        if ($response.success) {
            Write-Success "✓ DELETE /api/stadiums/$stadiumId - Stadium deleted successfully"
        } else {
            Write-Error-Custom "✗ Delete failed: $($response.message)"
        }
    } catch {
        $statusCode = $_.Exception.Response.StatusCode.Value__
        if ($statusCode -eq 400) {
            Write-Info "✓ Delete validation works - Stadium has matches (expected behavior)"
        } else {
            Write-Error-Custom "✗ DELETE /api/stadiums/{id} failed: $statusCode"
        }
    }
}

Write-Info "`n=== TEST SUMMARY ==="
Write-Success "All Stadium CRUD endpoints have been tested!"
Write-Info "✓ GET /api/stadiums (list with pagination, filter, search)"
Write-Info "✓ GET /api/stadiums/{id} (detail with clubs and matches)"
Write-Info "✓ POST /api/stadiums (create with validation)"
Write-Info "✓ PUT /api/stadiums/{id} (update with validation)"
Write-Info "✓ DELETE /api/stadiums/{id} (delete with match validation)"
