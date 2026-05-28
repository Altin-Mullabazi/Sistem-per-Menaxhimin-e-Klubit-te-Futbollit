#!/usr/bin/env pwsh

# P2 DETAILED ENDPOINT TEST SCRIPT
# Tests all CLUBS, PLAYERS, STADIUMS endpoints with detailed error reporting

$API_BASE = "http://localhost:5000/api"
$AUTH_URL = "$API_BASE/auth/login"
$testResults = @{
    clubs = @()
    players = @()
    stadiums = @()
    validation = @()
    authorization = @()
}

# Get JWT Token
Write-Host "========================================" -ForegroundColor Magenta
Write-Host "AUTHENTICATION" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Magenta

$loginData = @{
    email = "admin@footballclub.com"
    password = "Admin@123"
} | ConvertTo-Json

$response = Invoke-WebRequest -Uri $AUTH_URL -Method POST -Body $loginData -ContentType "application/json" -UseBasicParsing
$content = $response.Content | ConvertFrom-Json
$token = $content.accessToken
Write-Host "✓ Authentication successful" -ForegroundColor Green
Write-Host "✓ Token: $($token.Substring(0, 30))..." -ForegroundColor Green
Write-Host ""

# Test function with detailed output
function Test-Endpoint {
    param(
        [string]$name,
        [string]$uri,
        [string]$method = "GET",
        [object]$body = $null,
        [string]$token = "",
        [int]$expectedStatus = 200
    )
    
    $headers = @{
        "Content-Type" = "application/json"
    }
    
    if ($token) {
        $headers["Authorization"] = "Bearer $token"
    }
    
    $result = @{
        name = $name
        uri = $uri
        method = $method
        expectedStatus = $expectedStatus
        actualStatus = $null
        passed = $false
        response = $null
    }
    
    try {
        if ($method -eq "GET") {
            $resp = Invoke-WebRequest -Uri "$API_BASE$uri" -Method $method -Headers $headers -UseBasicParsing -ErrorAction SilentlyContinue
        } else {
            $bodyJson = $body | ConvertTo-Json -Depth 10
            $resp = Invoke-WebRequest -Uri "$API_BASE$uri" -Method $method -Body $bodyJson -Headers $headers -UseBasicParsing -ErrorAction SilentlyContinue
        }
        
        $result.actualStatus = $resp.StatusCode
        $result.response = $resp.Content
    } catch {
        $result.actualStatus = $_.Exception.Response.StatusCode.Value__
        try {
            $result.response = $_.Exception.Response.Content.ReadAsStringAsync().Result
        } catch {
            $result.response = $_.Exception.Message
        }
    }
    
    $result.passed = ($result.actualStatus -eq $expectedStatus)
    return $result
}

# ====== CLUBS ENDPOINTS ======
Write-Host "========================================" -ForegroundColor Magenta
Write-Host "CLUBS ENDPOINTS (7 tests)" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Magenta
Write-Host ""

# 1. GET /api/clubs - paginated
$r1 = Test-Endpoint "GET /clubs (paginated)" "/clubs?page=1&pageSize=10" "GET" $null ""
Write-Host "1. $($r1.name)" -ForegroundColor Cyan
Write-Host "   Status: $($r1.actualStatus) (Expected: $($r1.expectedStatus)) - $(if ($r1.passed) { 'PASS' } else { 'FAIL' })" -ForegroundColor $(if ($r1.passed) { 'Green' } else { 'Red' })
$testResults.clubs += $r1

# 2. GET /api/clubs with search
$r2 = Test-Endpoint "GET /clubs (with search)" "/clubs?search=FC" "GET" $null ""
Write-Host "2. $($r2.name)" -ForegroundColor Cyan
Write-Host "   Status: $($r2.actualStatus) - $(if ($r2.passed) { 'PASS' } else { 'FAIL' })" -ForegroundColor $(if ($r2.passed) { 'Green' } else { 'Red' })
$testResults.clubs += $r2

# 3. GET /api/clubs with filter
$r3 = Test-Endpoint "GET /clubs (with filter)" "/clubs?filter=Active" "GET" $null ""
Write-Host "3. $($r3.name)" -ForegroundColor Cyan
Write-Host "   Status: $($r3.actualStatus) - $(if ($r3.passed) { 'PASS' } else { 'FAIL' })" -ForegroundColor $(if ($r3.passed) { 'Green' } else { 'Red' })
$testResults.clubs += $r3

# 4. GET /api/clubs/{id}
$r4 = Test-Endpoint "GET /clubs/1 (specific)" "/clubs/1" "GET" $null "" 404
Write-Host "4. $($r4.name)" -ForegroundColor Cyan
Write-Host "   Status: $($r4.actualStatus) - $(if ($r4.passed) { 'PASS' } else { 'FAIL' })" -ForegroundColor $(if ($r4.passed) { 'Green' } else { 'Red' })
$testResults.clubs += $r4

# 5. POST /api/clubs (create)
$clubData = @{
    name = "Test Club"
    city = "Test City"
    founded = "2020-01-01"
}
$r5 = Test-Endpoint "POST /clubs (create)" "/clubs" "POST" $clubData $token 201
Write-Host "5. $($r5.name)" -ForegroundColor Cyan
Write-Host "   Status: $($r5.actualStatus) (Expected: 201) - $(if ($r5.passed) { 'PASS' } else { 'FAIL' })" -ForegroundColor $(if ($r5.passed) { 'Green' } else { 'Red' })
if (!$r5.passed) { Write-Host "   Response: $($r5.response.Substring(0, [math]::Min(100, $r5.response.Length)))" -ForegroundColor Yellow }
$testResults.clubs += $r5

# 6. PUT /api/clubs/{id} (update)
$updateClubData = @{
    name = "Updated Club"
    city = "Updated City"
    founded = "2020-01-01"
}
$r6 = Test-Endpoint "PUT /clubs/1 (update)" "/clubs/1" "PUT" $updateClubData $token 404
Write-Host "6. $($r6.name)" -ForegroundColor Cyan
Write-Host "   Status: $($r6.actualStatus) - $(if ($r6.passed) { 'PASS' } else { 'FAIL' })" -ForegroundColor $(if ($r6.passed) { 'Green' } else { 'Red' })
$testResults.clubs += $r6

# 7. DELETE /api/clubs/{id}
$r7 = Test-Endpoint "DELETE /clubs/999 (delete)" "/clubs/999" "DELETE" $null $token 404
Write-Host "7. $($r7.name)" -ForegroundColor Cyan
Write-Host "   Status: $($r7.actualStatus) - $(if ($r7.passed) { 'PASS' } else { 'FAIL' })" -ForegroundColor $(if ($r7.passed) { 'Green' } else { 'Red' })
$testResults.clubs += $r7
Write-Host ""

# ====== PLAYERS ENDPOINTS ======
Write-Host "========================================" -ForegroundColor Magenta
Write-Host "PLAYERS ENDPOINTS (8 tests)" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Magenta
Write-Host ""

# 1. GET /api/players - paginated
$p1 = Test-Endpoint "GET /players (paginated)" "/players?page=1&pageSize=10" "GET" $null $token
Write-Host "1. $($p1.name)" -ForegroundColor Cyan
Write-Host "   Status: $($p1.actualStatus) - $(if ($p1.passed) { 'PASS' } else { 'FAIL' })" -ForegroundColor $(if ($p1.passed) { 'Green' } else { 'Red' })
$testResults.players += $p1

# 2. GET /api/players with filters
$p2 = Test-Endpoint "GET /players (with filters)" "/players?position=Forward&status=Active" "GET" $null $token
Write-Host "2. $($p2.name)" -ForegroundColor Cyan
Write-Host "   Status: $($p2.actualStatus) - $(if ($p2.passed) { 'PASS' } else { 'FAIL' })" -ForegroundColor $(if ($p2.passed) { 'Green' } else { 'Red' })
$testResults.players += $p2

# 3. GET /api/players with search
$p3 = Test-Endpoint "GET /players (with search)" "/players?search=John" "GET" $null $token
Write-Host "3. $($p3.name)" -ForegroundColor Cyan
Write-Host "   Status: $($p3.actualStatus) - $(if ($p3.passed) { 'PASS' } else { 'FAIL' })" -ForegroundColor $(if ($p3.passed) { 'Green' } else { 'Red' })
$testResults.players += $p3

# 4. GET /api/players/club/{clubId}
$p4 = Test-Endpoint "GET /players/club/1 (by club)" "/players/club/1" "GET" $null $token 200
Write-Host "4. $($p4.name)" -ForegroundColor Cyan
Write-Host "   Status: $($p4.actualStatus) - $(if ($p4.passed) { 'PASS' } else { 'FAIL' })" -ForegroundColor $(if ($p4.passed) { 'Green' } else { 'Red' })
$testResults.players += $p4

# 5. GET /api/players/{id}
$p5 = Test-Endpoint "GET /players/1 (specific)" "/players/1" "GET" $null $token 200
Write-Host "5. $($p5.name)" -ForegroundColor Cyan
Write-Host "   Status: $($p5.actualStatus) - $(if ($p5.passed) { 'PASS' } else { 'FAIL' })" -ForegroundColor $(if ($p5.passed) { 'Green' } else { 'Red' })
$testResults.players += $p5

# 6. POST /api/players (create)
$playerData = @{
    firstName = "John"
    lastName = "Doe"
    position = "Forward"
    dateOfBirth = "2000-01-01"
    nationality = "Albania"
    jerseyNumber = 10
    clubId = 1
}
$p6 = Test-Endpoint "POST /players (create)" "/players" "POST" $playerData $token 201
Write-Host "6. $($p6.name)" -ForegroundColor Cyan
Write-Host "   Status: $($p6.actualStatus) (Expected: 201) - $(if ($p6.passed) { 'PASS' } else { 'FAIL' })" -ForegroundColor $(if ($p6.passed) { 'Green' } else { 'Red' })
if (!$p6.passed) { Write-Host "   Response: $($p6.response.Substring(0, [math]::Min(100, $p6.response.Length)))" -ForegroundColor Yellow }
$testResults.players += $p6

# 7. PUT /api/players/{id} (update)
$updatePlayerData = @{
    firstName = "Jane"
    lastName = "Doe"
    position = "Midfielder"
    dateOfBirth = "2000-01-01"
    nationality = "Albania"
    jerseyNumber = 11
    clubId = 1
}
$p7 = Test-Endpoint "PUT /players/1 (update)" "/players/1" "PUT" $updatePlayerData $token 200
Write-Host "7. $($p7.name)" -ForegroundColor Cyan
Write-Host "   Status: $($p7.actualStatus) - $(if ($p7.passed) { 'PASS' } else { 'FAIL' })" -ForegroundColor $(if ($p7.passed) { 'Green' } else { 'Red' })
$testResults.players += $p7

# 8. DELETE /api/players/{id}
$p8 = Test-Endpoint "DELETE /players/999 (delete)" "/players/999" "DELETE" $null $token 404
Write-Host "8. $($p8.name)" -ForegroundColor Cyan
Write-Host "   Status: $($p8.actualStatus) - $(if ($p8.passed) { 'PASS' } else { 'FAIL' })" -ForegroundColor $(if ($p8.passed) { 'Green' } else { 'Red' })
$testResults.players += $p8
Write-Host ""

# ====== STADIUMS ENDPOINTS ======
Write-Host "========================================" -ForegroundColor Magenta
Write-Host "STADIUMS ENDPOINTS (5 tests)" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Magenta
Write-Host ""

# 1. GET /api/stadiums - paginated
$s1 = Test-Endpoint "GET /stadiums (paginated)" "/stadiums?page=1&pageSize=10" "GET" $null $token
Write-Host "1. $($s1.name)" -ForegroundColor Cyan
Write-Host "   Status: $($s1.actualStatus) - $(if ($s1.passed) { 'PASS' } else { 'FAIL' })" -ForegroundColor $(if ($s1.passed) { 'Green' } else { 'Red' })
$testResults.stadiums += $s1

# 2. GET /api/stadiums with search
$s2 = Test-Endpoint "GET /stadiums (with search)" "/stadiums?search=Olympic" "GET" $null $token
Write-Host "2. $($s2.name)" -ForegroundColor Cyan
Write-Host "   Status: $($s2.actualStatus) - $(if ($s2.passed) { 'PASS' } else { 'FAIL' })" -ForegroundColor $(if ($s2.passed) { 'Green' } else { 'Red' })
$testResults.stadiums += $s2

# 3. GET /api/stadiums/{id}
$s3 = Test-Endpoint "GET /stadiums/1 (specific)" "/stadiums/1" "GET" $null $token 404
Write-Host "3. $($s3.name)" -ForegroundColor Cyan
Write-Host "   Status: $($s3.actualStatus) - $(if ($s3.passed) { 'PASS' } else { 'FAIL' })" -ForegroundColor $(if ($s3.passed) { 'Green' } else { 'Red' })
$testResults.stadiums += $s3

# 4. POST /api/stadiums (create)
$stadiumData = @{
    name = "Test Stadium"
    city = "Test City"
    capacity = 50000
    founded = "2010-01-01"
}
$s4 = Test-Endpoint "POST /stadiums (create)" "/stadiums" "POST" $stadiumData $token 201
Write-Host "4. $($s4.name)" -ForegroundColor Cyan
Write-Host "   Status: $($s4.actualStatus) (Expected: 201) - $(if ($s4.passed) { 'PASS' } else { 'FAIL' })" -ForegroundColor $(if ($s4.passed) { 'Green' } else { 'Red' })
if (!$s4.passed) { Write-Host "   Response: $($s4.response.Substring(0, [math]::Min(100, $s4.response.Length)))" -ForegroundColor Yellow }
$testResults.stadiums += $s4

# 5. PUT /api/stadiums/{id} (update)
$updateStadiumData = @{
    name = "Updated Stadium"
    city = "Updated City"
    capacity = 60000
    founded = "2010-01-01"
}
$s5 = Test-Endpoint "PUT /stadiums/1 (update)" "/stadiums/1" "PUT" $updateStadiumData $token 200
Write-Host "5. $($s5.name)" -ForegroundColor Cyan
Write-Host "   Status: $($s5.actualStatus) - $(if ($s5.passed) { 'PASS' } else { 'FAIL' })" -ForegroundColor $(if ($s5.passed) { 'Green' } else { 'Red' })
$testResults.stadiums += $s5
Write-Host ""

# ====== VALIDATION TESTS ======
Write-Host "========================================" -ForegroundColor Magenta
Write-Host "VALIDATION TESTS" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Magenta
Write-Host ""

# Invalid Club - empty name
$v1 = Test-Endpoint "POST /clubs (invalid - empty name)" "/clubs" "POST" @{ name = "" } $token 400
Write-Host "1. $($v1.name)" -ForegroundColor Cyan
Write-Host "   Status: $($v1.actualStatus) - $(if ($v1.passed) { 'PASS' } else { 'FAIL' })" -ForegroundColor $(if ($v1.passed) { 'Green' } else { 'Red' })
$testResults.validation += $v1

# Missing required Player fields
$v2 = Test-Endpoint "POST /players (missing fields)" "/players" "POST" @{ firstName = "Test" } $token 400
Write-Host "2. $($v2.name)" -ForegroundColor Cyan
Write-Host "   Status: $($v2.actualStatus) - $(if ($v2.passed) { 'PASS' } else { 'FAIL' })" -ForegroundColor $(if ($v2.passed) { 'Green' } else { 'Red' })
$testResults.validation += $v2
Write-Host ""

# ====== AUTHORIZATION TESTS ======
Write-Host "========================================" -ForegroundColor Magenta
Write-Host "AUTHORIZATION TESTS" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Magenta
Write-Host ""

# GET without token - public endpoint
$a1 = Test-Endpoint "GET /clubs (no token - public)" "/clubs?page=1&pageSize=10" "GET" $null "" 200
Write-Host "1. $($a1.name)" -ForegroundColor Cyan
Write-Host "   Status: $($a1.actualStatus) - $(if ($a1.passed) { 'PASS' } else { 'FAIL' })" -ForegroundColor $(if ($a1.passed) { 'Green' } else { 'Red' })
$testResults.authorization += $a1

# POST without token - protected endpoint
$a2 = Test-Endpoint "POST /clubs (no token - protected)" "/clubs" "POST" @{ name = "Test" } "" 401
Write-Host "2. $($a2.name)" -ForegroundColor Cyan
Write-Host "   Status: $($a2.actualStatus) - $(if ($a2.passed) { 'PASS' } else { 'FAIL' })" -ForegroundColor $(if ($a2.passed) { 'Green' } else { 'Red' })
$testResults.authorization += $a2
Write-Host ""

# ====== SUMMARY ======
Write-Host "========================================" -ForegroundColor Magenta
Write-Host "FINAL SUMMARY" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Magenta
Write-Host ""

$allResults = $testResults.clubs + $testResults.players + $testResults.stadiums + $testResults.validation + $testResults.authorization
$passedCount = @($allResults | Where-Object { $_.passed }).Count
$totalCount = $allResults.Count

Write-Host "CLUBS:         $(($testResults.clubs | Where-Object { $_.passed }).Count)/$($testResults.clubs.Count) passed" -ForegroundColor $(if (($testResults.clubs | Where-Object { $_.passed }).Count -eq $testResults.clubs.Count) { 'Green' } else { 'Yellow' })
Write-Host "PLAYERS:       $(($testResults.players | Where-Object { $_.passed }).Count)/$($testResults.players.Count) passed" -ForegroundColor $(if (($testResults.players | Where-Object { $_.passed }).Count -eq $testResults.players.Count) { 'Green' } else { 'Yellow' })
Write-Host "STADIUMS:      $(($testResults.stadiums | Where-Object { $_.passed }).Count)/$($testResults.stadiums.Count) passed" -ForegroundColor $(if (($testResults.stadiums | Where-Object { $_.passed }).Count -eq $testResults.stadiums.Count) { 'Green' } else { 'Yellow' })
Write-Host "VALIDATION:    $(($testResults.validation | Where-Object { $_.passed }).Count)/$($testResults.validation.Count) passed" -ForegroundColor $(if (($testResults.validation | Where-Object { $_.passed }).Count -eq $testResults.validation.Count) { 'Green' } else { 'Yellow' })
Write-Host "AUTHORIZATION: $(($testResults.authorization | Where-Object { $_.passed }).Count)/$($testResults.authorization.Count) passed" -ForegroundColor $(if (($testResults.authorization | Where-Object { $_.passed }).Count -eq $testResults.authorization.Count) { 'Green' } else { 'Yellow' })
Write-Host ""
Write-Host "TOTAL: $passedCount/$totalCount tests passed" -ForegroundColor $(if ($passedCount -eq $totalCount) { 'Green' } else { 'Yellow' })
Write-Host "PASS RATE: $([math]::Round(($passedCount/$totalCount)*100, 2))%" -ForegroundColor $(if ($passedCount -eq $totalCount) { 'Green' } else { 'Yellow' })
Write-Host ""
Write-Host "========================================" -ForegroundColor Magenta
