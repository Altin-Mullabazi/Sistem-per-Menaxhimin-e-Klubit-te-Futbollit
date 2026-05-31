# P2 Comprehensive API Endpoint Testing Script
# Tests all CLUBS, PLAYERS, and STADIUMS endpoints
# With Authorization and Validation checks

$BaseUrl = "http://localhost:5000/api"
$AdminToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjU5MTM4YjhhLTk0ZDAtNGFkOS05NDBkLTIzYThjNzkyZjYyNCIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwiZXhwIjoxNzc5ODMxMDQ3LCJpc3MiOiJGb290YmFsbENsdWJBUEkiLCJhdWQiOiJGb290YmFsbENsdWJBcHAifQ.hn3H-dqHbnhFVbCaZ_BCLDS2Vtv2N9y-FYeEIX26DeQ"

$Results = @()
$TestCount = 0
$PassCount = 0
$FailCount = 0

function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Endpoint,
        [object]$Body = $null,
        [int]$ExpectedStatus = 200,
        [bool]$UseToken = $true
    )
    
    $TestCount++
    $Url = "$BaseUrl$Endpoint"
    $Headers = @{"Content-Type" = "application/json"}
    
    if ($UseToken) {
        $Headers["Authorization"] = "Bearer $AdminToken"
    }
    
    try {
        $Params = @{
            Uri = $Url
            Method = $Method
            Headers = $Headers
            ErrorAction = "SilentlyContinue"
        }
        
        if ($Body) {
            $Params["Body"] = ($Body | ConvertTo-Json -Depth 10)
        }
        
        $Response = Invoke-WebRequest @Params
        $StatusCode = $Response.StatusCode
        $IsPass = ($StatusCode -eq $ExpectedStatus)
        
        if ($IsPass) { $PassCount++ } else { $FailCount++ }
        
        $Results += [PSCustomObject]@{
            TestNumber = $TestCount
            Name = $Name
            Method = $Method
            Endpoint = $Endpoint
            ExpectedStatus = $ExpectedStatus
            ActualStatus = $StatusCode
            Result = if($IsPass) {"✓ PASS"} else {"✗ FAIL"}
            Details = "Success"
        }
        
        return @{ StatusCode = $StatusCode; Body = $Response.Content }
    }
    catch {
        $StatusCode = $_.Exception.Response.StatusCode.Value
        $IsPass = ($StatusCode -eq $ExpectedStatus)
        
        if ($IsPass) { $PassCount++ } else { $FailCount++ }
        
        $Results += [PSCustomObject]@{
            TestNumber = $TestCount
            Name = $Name
            Method = $Method
            Endpoint = $Endpoint
            ExpectedStatus = $ExpectedStatus
            ActualStatus = $StatusCode
            Result = if($IsPass) {"✓ PASS"} else {"✗ FAIL"}
            Details = $_.Exception.Message
        }
        
        return @{ StatusCode = $StatusCode; Body = $_.Exception.Response.Content }
    }
}

Write-Host "════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "P2 COMPREHENSIVE ENDPOINT TESTING" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# ============= CLUBS ENDPOINTS =============
Write-Host "TESTING CLUBS ENDPOINTS (7 endpoints)" -ForegroundColor Green
Write-Host "─────────────────────────────────────" -ForegroundColor Green

# 1. GET /clubs - list paginated
Test-Endpoint -Name "GET Clubs List (Paginated)" -Method "GET" -Endpoint "/Clubs?page=1&pageSize=10" -ExpectedStatus 200 | Out-Null

# 2. GET /clubs - with search
Test-Endpoint -Name "GET Clubs with Search" -Method "GET" -Endpoint "/Clubs?search=Manchester" -ExpectedStatus 200 | Out-Null

# 3. GET /clubs - with filter
Test-Endpoint -Name "GET Clubs with Filter" -Method "GET" -Endpoint "/Clubs?foundedYear=2000" -ExpectedStatus 200 | Out-Null

# 4. GET /clubs/{id} - specific club
$ClubsListResult = Test-Endpoint -Name "GET Specific Club" -Method "GET" -Endpoint "/Clubs/1" -ExpectedStatus 200

# 5. POST /clubs - create new
$NewClub = @{
    name = "Test Club $(Get-Random)"
    city = "Test City"
    foundedYear = 2020
    logoUrl = "https://example.com/logo.png"
    president = "Test President"
    budget = 1000000
}
$CreateClubResult = Test-Endpoint -Name "POST Create Club" -Method "POST" -Endpoint "/Clubs" -Body $NewClub -ExpectedStatus 201

# Extract club ID from response if successful
$CreatedClubId = $null
if ($CreateClubResult.StatusCode -eq 201) {
    try {
        $ClubData = $CreateClubResult.Body | ConvertFrom-Json
        $CreatedClubId = $ClubData.id
        Write-Host "Created Club ID: $CreatedClubId" -ForegroundColor Gray
    }
    catch { }
}

# 6. PUT /clubs/{id} - update club
if ($CreatedClubId) {
    $UpdateClub = @{
        name = "Updated Club $(Get-Random)"
        city = "Updated City"
        foundedYear = 2021
    }
    Test-Endpoint -Name "PUT Update Club" -Method "PUT" -Endpoint "/Clubs/$CreatedClubId" -Body $UpdateClub -ExpectedStatus 200 | Out-Null
}

# 7. DELETE /clubs/{id} - delete club
if ($CreatedClubId) {
    Test-Endpoint -Name "DELETE Club" -Method "DELETE" -Endpoint "/Clubs/$CreatedClubId" -ExpectedStatus 200 | Out-Null
}

Write-Host ""

# ============= PLAYERS ENDPOINTS =============
Write-Host "TESTING PLAYERS ENDPOINTS (8 endpoints)" -ForegroundColor Green
Write-Host "──────────────────────────────────────" -ForegroundColor Green

# 1. GET /players - list paginated
Test-Endpoint -Name "GET Players List (Paginated)" -Method "GET" -Endpoint "/Players?page=1&pageSize=10" -ExpectedStatus 200 | Out-Null

# 2. GET /players - with position filter
Test-Endpoint -Name "GET Players with Position Filter" -Method "GET" -Endpoint "/Players?position=Forward" -ExpectedStatus 200 | Out-Null

# 3. GET /players - with search
Test-Endpoint -Name "GET Players with Search" -Method "GET" -Endpoint "/Players?search=Kylian" -ExpectedStatus 200 | Out-Null

# 4. GET /players - by club
Test-Endpoint -Name "GET Players by Club" -Method "GET" -Endpoint "/Players?clubId=1" -ExpectedStatus 200 | Out-Null

# 5. GET /players/{id} - specific player
Test-Endpoint -Name "GET Specific Player" -Method "GET" -Endpoint "/Players/1" -ExpectedStatus 200 | Out-Null

# 6. POST /players - create new player
$NewPlayer = @{
    firstName = "Test"
    lastName = "Player"
    position = "Forward"
    jerseyNumber = 10
    clubId = 1
    dateOfBirth = "2000-01-01"
    nationality = "Test Nation"
    height = 180
    weight = 75
    status = "Active"
    marketValue = 5000000
}
$CreatePlayerResult = Test-Endpoint -Name "POST Create Player" -Method "POST" -Endpoint "/Players" -Body $NewPlayer -ExpectedStatus 201

# Extract player ID from response
$CreatedPlayerId = $null
if ($CreatePlayerResult.StatusCode -eq 201) {
    try {
        $PlayerData = $CreatePlayerResult.Body | ConvertFrom-Json
        $CreatedPlayerId = $PlayerData.id
        Write-Host "Created Player ID: $CreatedPlayerId" -ForegroundColor Gray
    }
    catch { }
}

# 7. PUT /players/{id} - update player
if ($CreatedPlayerId) {
    $UpdatePlayer = @{
        firstName = "Updated"
        lastName = "Player"
        position = "Midfielder"
        jerseyNumber = 20
        clubId = 1
        dateOfBirth = "2000-01-01"
    }
    Test-Endpoint -Name "PUT Update Player" -Method "PUT" -Endpoint "/Players/$CreatedPlayerId" -Body $UpdatePlayer -ExpectedStatus 200 | Out-Null
}

# 8. DELETE /players/{id} - delete player
if ($CreatedPlayerId) {
    Test-Endpoint -Name "DELETE Player" -Method "DELETE" -Endpoint "/Players/$CreatedPlayerId" -ExpectedStatus 200 | Out-Null
}

Write-Host ""

# ============= STADIUMS ENDPOINTS =============
Write-Host "TESTING STADIUMS ENDPOINTS (5 endpoints)" -ForegroundColor Green
Write-Host "───────────────────────────────────────" -ForegroundColor Green

# 1. GET /stadiums - list paginated
Test-Endpoint -Name "GET Stadiums List (Paginated)" -Method "GET" -Endpoint "/Stadiums?page=1&pageSize=10" -ExpectedStatus 200 | Out-Null

# 2. GET /stadiums - with search/filter
Test-Endpoint -Name "GET Stadiums with Search/Filter" -Method "GET" -Endpoint "/Stadiums?search=Test" -ExpectedStatus 200 | Out-Null

# 3. GET /stadiums/{id} - specific stadium
Test-Endpoint -Name "GET Specific Stadium" -Method "GET" -Endpoint "/Stadiums/1" -ExpectedStatus 200 | Out-Null

# 4. POST /stadiums - create new
$NewStadium = @{
    name = "Test Stadium $(Get-Random)"
    city = "Test City"
    capacity = 50000
    yearBuilt = 2020
}
$CreateStadiumResult = Test-Endpoint -Name "POST Create Stadium" -Method "POST" -Endpoint "/Stadiums" -Body $NewStadium -ExpectedStatus 201

# Extract stadium ID
$CreatedStadiumId = $null
if ($CreateStadiumResult.StatusCode -eq 201) {
    try {
        $StadiumData = $CreateStadiumResult.Body | ConvertFrom-Json
        $CreatedStadiumId = $StadiumData.id
        Write-Host "Created Stadium ID: $CreatedStadiumId" -ForegroundColor Gray
    }
    catch { }
}

# 5. PUT /stadiums/{id} - update stadium
if ($CreatedStadiumId) {
    $UpdateStadium = @{
        name = "Updated Stadium"
        city = "Updated City"
        capacity = 60000
        yearBuilt = 2021
    }
    Test-Endpoint -Name "PUT Update Stadium" -Method "PUT" -Endpoint "/Stadiums/$CreatedStadiumId" -Body $UpdateStadium -ExpectedStatus 200 | Out-Null
}

# 6. DELETE /stadiums/{id} - delete stadium
if ($CreatedStadiumId) {
    Test-Endpoint -Name "DELETE Stadium" -Method "DELETE" -Endpoint "/Stadiums/$CreatedStadiumId" -ExpectedStatus 200 | Out-Null
}

Write-Host ""

# ============= AUTHORIZATION TESTS =============
Write-Host "TESTING AUTHORIZATION (without token)" -ForegroundColor Yellow
Write-Host "──────────────────────────────────────" -ForegroundColor Yellow

# GET should work without token (public)
Test-Endpoint -Name "GET Clubs without Token (public)" -Method "GET" -Endpoint "/Clubs" -ExpectedStatus 200 -UseToken $false | Out-Null

# POST should fail without token (403/401)
Test-Endpoint -Name "POST Club without Token (should fail)" -Method "POST" -Endpoint "/Clubs" -Body $NewClub -ExpectedStatus 401 -UseToken $false | Out-Null

Write-Host ""

# ============= VALIDATION TESTS =============
Write-Host "TESTING VALIDATION (invalid data)" -ForegroundColor Yellow
Write-Host "─────────────────────────────────" -ForegroundColor Yellow

# POST with invalid data
$InvalidClub = @{
    name = ""  # Empty name - should fail
    city = "Test City"
    foundedYear = 2020
}
Test-Endpoint -Name "POST Club with Invalid Data (empty name)" -Method "POST" -Endpoint "/Clubs" -Body $InvalidClub -ExpectedStatus 400 | Out-Null

# POST player with missing required field
$InvalidPlayer = @{
    firstName = "Test"
    lastName = "Player"
    # Missing required fields
}
Test-Endpoint -Name "POST Player with Missing Fields" -Method "POST" -Endpoint "/Players" -Body $InvalidPlayer -ExpectedStatus 400 | Out-Null

Write-Host ""
Write-Host "════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "TEST SUMMARY" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════════════" -ForegroundColor Cyan

$Results | Format-Table -AutoSize

Write-Host ""
Write-Host "TOTAL: $TestCount | PASSED: $PassCount ✓ | FAILED: $FailCount ✗" -ForegroundColor Cyan
$PassPercentage = [math]::Round(($PassCount / $TestCount) * 100, 2)
Write-Host "SUCCESS RATE: $PassPercentage%" -ForegroundColor Cyan

# Export to file
$ReportPath = "P2-SWAGGER-TEST-RESULTS.txt"
$Results | Out-String | Out-File -FilePath $ReportPath
Write-Host ""
Write-Host "Results saved to: $ReportPath" -ForegroundColor Green
