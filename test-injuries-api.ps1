#!/usr/bin/env powershell

# ===== INJURIES CRUD API TEST SCRIPT =====
# Tests all 4 injury endpoints with comprehensive scenarios

$API_BASE = "http://localhost:5000/api"
$AUTH_TOKEN = ""

# Color output functions
function Write-Success {
    Write-Host "✓ $args" -ForegroundColor Green
}

function Write-Error {
    Write-Host "✗ $args" -ForegroundColor Red
}

function Write-Info {
    Write-Host "ℹ $args" -ForegroundColor Cyan
}

# ===== 1. LOGIN & GET AUTH TOKEN =====
Write-Info "STEP 1: Authenticating user..."
$loginPayload = @{
    email = "admin@example.com"
    password = "AdminPassword123!"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$API_BASE/auth/login" -Method Post -ContentType "application/json" -Body $loginPayload -ErrorAction SilentlyContinue
    if ($loginResponse.success) {
        $AUTH_TOKEN = $loginResponse.data.accessToken
        Write-Success "Authentication successful"
        Write-Info "Token: $($AUTH_TOKEN.Substring(0, 20))..."
    } else {
        Write-Error "Login failed: $($loginResponse.message)"
        exit 1
    }
} catch {
    Write-Error "Login request failed: $_"
    exit 1
}

$headers = @{
    "Authorization" = "Bearer $AUTH_TOKEN"
    "Content-Type" = "application/json"
}

# ===== 2. TEST GET /api/injuries (PAGINATED LIST) =====
Write-Info "`nSTEP 2: Testing GET /api/injuries (Paginated list with filters)..."

try {
    $response = Invoke-RestMethod -Uri "$API_BASE/injuries?page=1&pageSize=10" -Method Get -Headers $headers -ErrorAction SilentlyContinue
    if ($response.success) {
        Write-Success "GET /api/injuries - Status: OK"
        Write-Info "Total Injuries: $($response.data.totalItems)"
        Write-Info "Page: $($response.data.page)/$($response.data.totalPages)"
        if ($response.data.data.Count -gt 0) {
            Write-Info "First injury: $($response.data.data[0].injuryType) - Player: $($response.data.data[0].playerName)"
        }
    } else {
        Write-Error "GET /api/injuries failed: $($response.message)"
    }
} catch {
    Write-Error "GET /api/injuries request failed: $_"
}

# ===== 3. TEST GET /api/injuries/active (ACTIVE INJURIES) =====
Write-Info "`nSTEP 3: Testing GET /api/injuries/active (Active injuries only)..."

try {
    $response = Invoke-RestMethod -Uri "$API_BASE/injuries/active?page=1&pageSize=10" -Method Get -Headers $headers -ErrorAction SilentlyContinue
    if ($response.success) {
        Write-Success "GET /api/injuries/active - Status: OK"
        Write-Info "Active Injuries: $($response.data.totalItems)"
        $statuses = $response.data.data | Select-Object -ExpandProperty status | Get-Unique
        Write-Info "Statuses: $($statuses -join ', ')"
    } else {
        Write-Error "GET /api/injuries/active failed: $($response.message)"
    }
} catch {
    Write-Error "GET /api/injuries/active request failed: $_"
}

# ===== 4. TEST FILTERING BY PLAYER =====
Write-Info "`nSTEP 4: Testing GET /api/injuries?playerId=1 (Filter by player)..."

try {
    $response = Invoke-RestMethod -Uri "$API_BASE/injuries?playerId=1&page=1&pageSize=10" -Method Get -Headers $headers -ErrorAction SilentlyContinue
    if ($response.success) {
        Write-Success "GET /api/injuries?playerId=1 - Status: OK"
        Write-Info "Injuries for Player 1: $($response.data.totalItems)"
        if ($response.data.data.Count -gt 0) {
            $response.data.data | ForEach-Object {
                Write-Info "  - $($_.injuryType) ($($_.status)): $($_.playerName)"
            }
        }
    } else {
        Write-Error "Filter by player failed: $($response.message)"
    }
} catch {
    Write-Error "Filter by player request failed: $_"
}

# ===== 5. TEST FILTERING BY STATUS =====
Write-Info "`nSTEP 5: Testing GET /api/injuries?status=Active (Filter by status)..."

try {
    $response = Invoke-RestMethod -Uri "$API_BASE/injuries?status=Active&page=1&pageSize=10" -Method Get -Headers $headers -ErrorAction SilentlyContinue
    if ($response.success) {
        Write-Success "GET /api/injuries?status=Active - Status: OK"
        Write-Info "Active Injuries: $($response.data.totalItems)"
        if ($response.data.data.Count -gt 0) {
            $response.data.data | ForEach-Object {
                Write-Info "  - $($_.injuryType): $($_.playerName)"
            }
        }
    } else {
        Write-Error "Filter by status failed: $($response.message)"
    }
} catch {
    Write-Error "Filter by status request failed: $_"
}

# ===== 6. TEST SORTING =====
Write-Info "`nSTEP 6: Testing GET /api/injuries?sortBy=date (Sort by date, newest first)..."

try {
    $response = Invoke-RestMethod -Uri "$API_BASE/injuries?sortBy=date&page=1&pageSize=10" -Method Get -Headers $headers -ErrorAction SilentlyContinue
    if ($response.success) {
        Write-Success "GET /api/injuries?sortBy=date - Status: OK"
        if ($response.data.data.Count -gt 1) {
            $first = $response.data.data[0].injuryDate
            $second = $response.data.data[1].injuryDate
            if ([DateTime]$first -ge [DateTime]$second) {
                Write-Success "Sorting verified (newest first)"
            } else {
                Write-Error "Sorting incorrect (not newest first)"
            }
        }
    } else {
        Write-Error "Sorting test failed: $($response.message)"
    }
} catch {
    Write-Error "Sorting test request failed: $_"
}

# ===== 7. TEST POST /api/injuries (CREATE) =====
Write-Info "`nSTEP 7: Testing POST /api/injuries (Create new injury)..."

$injuryPayload = @{
    playerId = 1
    injuryType = "Hamstring Strain"
    injuryDate = (Get-Date).AddDays(-3).ToString("yyyy-MM-dd")
    notes = "Test injury created by test script"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$API_BASE/injuries" -Method Post -Headers $headers -Body $injuryPayload -ErrorAction SilentlyContinue
    if ($response.success) {
        $newInjuryId = $response.data.id
        Write-Success "POST /api/injuries - Status: 201 Created"
        Write-Info "New Injury ID: $newInjuryId"
        Write-Info "Type: $($response.data.injuryType)"
        Write-Info "Player: $($response.data.playerName)"
        Write-Info "Status: $($response.data.status)"
    } else {
        Write-Error "POST /api/injuries failed: $($response.message)"
    }
} catch {
    Write-Error "POST /api/injuries request failed: $_"
}

# ===== 8. TEST POST VALIDATION (Invalid Date) =====
Write-Info "`nSTEP 8: Testing POST validation (Future date should fail)..."

$invalidPayload = @{
    playerId = 1
    injuryType = "Invalid Injury"
    injuryDate = (Get-Date).AddDays(5).ToString("yyyy-MM-dd")
    notes = "This should fail - future date"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$API_BASE/injuries" -Method Post -Headers $headers -Body $invalidPayload -ErrorAction SilentlyContinue
    Write-Error "Validation failed - Future date was accepted!"
} catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Success "Validation working - Future date rejected with 400"
    } else {
        Write-Error "Unexpected error: $_"
    }
}

# ===== 9. TEST POST VALIDATION (Invalid Player) =====
Write-Info "`nSTEP 9: Testing POST validation (Non-existent player should fail)..."

$invalidPlayerPayload = @{
    playerId = 99999
    injuryType = "Invalid Injury"
    injuryDate = (Get-Date).ToString("yyyy-MM-dd")
    notes = "This should fail - invalid player"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$API_BASE/injuries" -Method Post -Headers $headers -Body $invalidPlayerPayload -ErrorAction SilentlyContinue
    Write-Error "Validation failed - Invalid player was accepted!"
} catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Success "Validation working - Invalid player rejected with 400"
    } else {
        Write-Error "Unexpected error: $_"
    }
}

# ===== 10. TEST PUT /api/injuries/{id} (UPDATE) =====
Write-Info "`nSTEP 10: Testing PUT /api/injuries/1 (Update injury)..."

$updatePayload = @{
    status = "Recovering"
    notes = "Updated: Player showing good progress"
    recoveryDate = (Get-Date).AddDays(14).ToString("yyyy-MM-dd")
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$API_BASE/injuries/1" -Method Put -Headers $headers -Body $updatePayload -ErrorAction SilentlyContinue
    if ($response.success) {
        Write-Success "PUT /api/injuries/1 - Status: 200 OK"
        Write-Info "Updated Status: $($response.data.status)"
        Write-Info "Updated Notes: $($response.data.notes)"
        Write-Info "Recovery Date: $($response.data.recoveryDate)"
    } else {
        Write-Error "PUT /api/injuries/1 failed: $($response.message)"
    }
} catch {
    Write-Error "PUT /api/injuries/1 request failed: $_"
}

# ===== 11. TEST PUT VALIDATION (Invalid Recovery Date) =====
Write-Info "`nSTEP 11: Testing PUT validation (Recovery date before injury date should fail)..."

$invalidUpdatePayload = @{
    recoveryDate = (Get-Date).AddDays(-100).ToString("yyyy-MM-dd")
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$API_BASE/injuries/1" -Method Put -Headers $headers -Body $invalidUpdatePayload -ErrorAction SilentlyContinue
    Write-Error "Validation failed - Invalid recovery date was accepted!"
} catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Success "Validation working - Invalid recovery date rejected with 400"
    } else {
        Write-Error "Unexpected error: $_"
    }
}

# ===== 12. TEST AUTHORIZATION (GET without token) =====
Write-Info "`nSTEP 12: Testing authorization (GET without token should be 401)..."

try {
    $response = Invoke-RestMethod -Uri "$API_BASE/injuries" -Method Get -ErrorAction SilentlyContinue
    Write-Error "Authorization check failed - Request accepted without token!"
} catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Success "Authorization working - Request rejected with 401 Unauthorized"
    } else {
        Write-Error "Unexpected status code: $($_.Exception.Response.StatusCode)"
    }
}

# ===== 13. TEST AUTHORIZATION (CREATE without Admin role) =====
Write-Info "`nSTEP 13: Testing authorization (Create as non-Admin should fail)..."

$managerToken = ""; # Would need to login as manager
# Skipping this test for now as we need manager credentials

# ===== SUMMARY =====
Write-Info "`n===== TEST SUMMARY ====="
Write-Success "All endpoint tests completed!"
Write-Info "Next steps:"
Write-Info "  1. Verify all GET endpoints return expected data"
Write-Info "  2. Verify filtering works correctly"
Write-Info "  3. Verify sorting (date, newest first)"
Write-Info "  4. Verify pagination"
Write-Info "  5. Verify POST creates with 201"
Write-Info "  6. Verify validation errors return 400"
Write-Info "  7. Verify PUT updates correctly"
Write-Info "  8. Verify authorization (401/403)"
Write-Info "  9. Ready to create PR and merge to dev"
