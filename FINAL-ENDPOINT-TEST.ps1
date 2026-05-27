#!/usr/bin/env pwsh
# P2 Endpoint Testing - IMPROVED VERSION
# Gets valid IDs from database before creating dependent entities

$BaseUrl = "http://localhost:5000/api"
$Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjU5MTM4YjhhLTk0ZDAtNGFkOS05NDBkLTIzYThjNzkyZjYyNCIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwiZXhwIjoxNzc5ODMxMDQ3LCJpc3MiOiJGb290YmFsbENsdWJBUEkiLCJhdWQiOiJGb290YmFsbENsdWJBcHAifQ.hn3H-dqHbnhFVbCaZ_BCLDS2Vtv2N9y-FYeEIX26DeQ"

$script:Results = @()
$script:TestNum = 0
$script:Pass = 0
$script:Fail = 0

function Test {
    param($Name, $Method, $Url, $Body, $Expected, $WithToken)
    
    $script:TestNum++
    $FullUrl = "$BaseUrl$Url"
    $Headers = @{"Content-Type" = "application/json"}
    
    if ($WithToken) {
        $Headers["Authorization"] = "Bearer $Token"
    }
    
    $Status = 0
    $ResponseBody = $null
    
    try {
        $Params = @{
            Uri = $FullUrl
            Method = $Method
            Headers = $Headers
            UseBasicParsing = $true
            TimeoutSec = 10
        }
        
        if ($Body -ne $null) {
            $Params["Body"] = ($Body | ConvertTo-Json -Depth 5 -Compress)
        }
        
        $Response = Invoke-WebRequest @Params -ErrorAction Stop
        $Status = [int]$Response.StatusCode
        $ResponseBody = $Response.Content
    }
    catch [System.Net.WebException] {
        $ErrorResponse = $_.Exception.Response
        if ($ErrorResponse) {
            $Status = [int]$ErrorResponse.StatusCode
        } else {
            $Status = 0
        }
    }
    catch {
        $Status = 0
    }
    
    $Pass = ($Status -eq $Expected)
    if ($Pass) { $script:Pass++ } else { $script:Fail++ }
    
    $Result = if ($Pass) { "PASS" } else { "FAIL" }
    $Color = if ($Pass) { "Green" } else { "Red" }
    
    $script:Results += [PSCustomObject]@{
        Num = $script:TestNum
        Name = $Name
        Method = $Method
        Expected = $Expected
        Actual = $Status
        Result = $Result
    }
    
    Write-Host "[$script:TestNum] $Name ... $Result (HTTP $Status/$Expected)" -ForegroundColor $Color
    
    return @{ Status = $Status; Body = $ResponseBody }
}

function GetFirstEntityId {
    param($EndpointPath)
    
    try {
        $Url = "$BaseUrl$EndpointPath?page=1&pageSize=1"
        $Headers = @{
            "Content-Type" = "application/json"
            "Authorization" = "Bearer $Token"
        }
        
        $Response = Invoke-WebRequest -Uri $Url -Method "GET" -Headers $Headers -UseBasicParsing -ErrorAction Stop
        $Json = $Response.Content | ConvertFrom-Json
        
        if ($Json.data -and $Json.data.Count -gt 0) {
            return $Json.data[0].id
        }
        return $null
    }
    catch {
        return $null
    }
}

Write-Host ""
Write-Host "===== P2 API ENDPOINT TESTING =====" -ForegroundColor Cyan
Write-Host ""

# Get valid IDs from database
Write-Host "Getting existing entities from database..." -ForegroundColor Gray
$ValidClubId = GetFirstEntityId "/Clubs"
$ValidStadiumId = GetFirstEntityId "/Stadiums"

Write-Host "Found Club ID: $ValidClubId" -ForegroundColor Gray
Write-Host "Found Stadium ID: $ValidStadiumId" -ForegroundColor Gray
Write-Host ""

# CLUBS ENDPOINTS
Write-Host "[CLUBS ENDPOINTS]" -ForegroundColor Yellow
Test "GET /Clubs (paginated)" "GET" "/Clubs?page=1&pageSize=10" $null 200 $true | Out-Null
Test "GET /Clubs (search)" "GET" "/Clubs?search=Test" $null 200 $true | Out-Null
Test "GET /Clubs (filter)" "GET" "/Clubs?foundedYear=2020" $null 200 $true | Out-Null

if ($ValidClubId) {
    Test "GET /Clubs/$ValidClubId (specific)" "GET" "/Clubs/$ValidClubId" $null 200 $true | Out-Null
} else {
    Write-Host "[!] Skipping specific club test (no valid ID found)" -ForegroundColor Yellow
}

$ClubPayload = @{
    name = "TestClub$(Get-Random -Maximum 9999)"
    city = "TestCity"
    foundedYear = 2020
    president = "TestPres"
    budget = 5000000
}

$CreateClubResult = Test "POST /Clubs (create)" "POST" "/Clubs" $ClubPayload 201 $true
$NewClubId = $null

if ($CreateClubResult.Status -eq 201 -and $CreateClubResult.Body) {
    try {
        $ClubJson = $CreateClubResult.Body | ConvertFrom-Json
        $NewClubId = $ClubJson.id
        Write-Host "   -> Created club ID: $NewClubId" -ForegroundColor Gray
    } catch {}
}

if ($NewClubId) {
    $UpdatePayload = @{
        name = "Updated$(Get-Random)"
        city = "UpdatedCity"
        foundedYear = 2021
    }
    Test "PUT /Clubs/$NewClubId (update)" "PUT" "/Clubs/$NewClubId" $UpdatePayload 200 $true | Out-Null
    Test "DELETE /Clubs/$NewClubId" "DELETE" "/Clubs/$NewClubId" $null 200 $true | Out-Null
}

Write-Host ""

# PLAYERS ENDPOINTS
Write-Host "[PLAYERS ENDPOINTS]" -ForegroundColor Yellow
Test "GET /Players (paginated)" "GET" "/Players?page=1&pageSize=10" $null 200 $true | Out-Null
Test "GET /Players (position)" "GET" "/Players?position=Forward" $null 200 $true | Out-Null
Test "GET /Players (search)" "GET" "/Players?search=Test" $null 200 $true | Out-Null

if ($ValidClubId) {
    Test "GET /Players (by club)" "GET" "/Players?clubId=$ValidClubId" $null 200 $true | Out-Null
} else {
    Write-Host "[!] Skipping GET /Players (by club) test (no valid club ID)" -ForegroundColor Yellow
}

Test "GET /Players/1 (specific)" "GET" "/Players/1" $null 200 $true | Out-Null

# Create player - use valid club ID if we have one
if ($ValidClubId) {
    $PlayerPayload = @{
        firstName = "TestFirst"
        lastName = "TestLast"
        position = "Forward"
        jerseyNumber = 99
        clubId = $ValidClubId
        dateOfBirth = "2000-01-01"
        nationality = "Test"
        height = 185
        weight = 85
    }
    
    $CreatePlayerResult = Test "POST /Players (create)" "POST" "/Players" $PlayerPayload 201 $true
    $NewPlayerId = $null
    
    if ($CreatePlayerResult.Status -eq 201 -and $CreatePlayerResult.Body) {
        try {
            $PlayerJson = $CreatePlayerResult.Body | ConvertFrom-Json
            $NewPlayerId = $PlayerJson.id
            Write-Host "   -> Created player ID: $NewPlayerId" -ForegroundColor Gray
        } catch {}
    }
    
    if ($NewPlayerId) {
        $PlayerUpdate = @{
            firstName = "UpdatedFirst"
            lastName = "UpdatedLast"
            position = "Midfielder"
            jerseyNumber = 88
            clubId = $ValidClubId
            dateOfBirth = "2000-01-01"
        }
        Test "PUT /Players/$NewPlayerId (update)" "PUT" "/Players/$NewPlayerId" $PlayerUpdate 200 $true | Out-Null
        Test "DELETE /Players/$NewPlayerId" "DELETE" "/Players/$NewPlayerId" $null 200 $true | Out-Null
    }
} else {
    Write-Host "[!] Skipping player creation (no valid club ID found)" -ForegroundColor Yellow
}

Write-Host ""

# STADIUMS ENDPOINTS
Write-Host "[STADIUMS ENDPOINTS]" -ForegroundColor Yellow
Test "GET /Stadiums (paginated)" "GET" "/Stadiums?page=1&pageSize=10" $null 200 $true | Out-Null
Test "GET /Stadiums (search)" "GET" "/Stadiums?search=Test" $null 200 $true | Out-Null

if ($ValidStadiumId) {
    Test "GET /Stadiums/$ValidStadiumId (specific)" "GET" "/Stadiums/$ValidStadiumId" $null 200 $true | Out-Null
} else {
    Write-Host "[!] Skipping specific stadium test (no valid ID found)" -ForegroundColor Yellow
}

$StadiumPayload = @{
    name = "TestStadium$(Get-Random)"
    city = "TestCity"
    capacity = 50000
    yearBuilt = 2020
}

$CreateStadiumResult = Test "POST /Stadiums (create)" "POST" "/Stadiums" $StadiumPayload 201 $true
$NewStadiumId = $null

if ($CreateStadiumResult.Status -eq 201 -and $CreateStadiumResult.Body) {
    try {
        $StadiumJson = $CreateStadiumResult.Body | ConvertFrom-Json
        $NewStadiumId = $StadiumJson.id
        Write-Host "   -> Created stadium ID: $NewStadiumId" -ForegroundColor Gray
    } catch {}
}

if ($NewStadiumId) {
    $StadiumUpdate = @{
        name = "UpdatedStadium"
        city = "UpdatedCity"
        capacity = 60000
        yearBuilt = 2021
    }
    Test "PUT /Stadiums/$NewStadiumId (update)" "PUT" "/Stadiums/$NewStadiumId" $StadiumUpdate 200 $true | Out-Null
    Test "DELETE /Stadiums/$NewStadiumId" "DELETE" "/Stadiums/$NewStadiumId" $null 200 $true | Out-Null
}

Write-Host ""

# AUTHORIZATION TESTS
Write-Host "[AUTHORIZATION TESTS]" -ForegroundColor Yellow
Test "GET /Clubs (no token - public)" "GET" "/Clubs" $null 200 $false | Out-Null

$NoTokenPayload = @{
    name = "ShouldFail"
    city = "Test"
    foundedYear = 2020
}
Test "POST /Clubs (no token - should fail)" "POST" "/Clubs" $NoTokenPayload 401 $false | Out-Null

Write-Host ""

# VALIDATION TESTS
Write-Host "[VALIDATION TESTS]" -ForegroundColor Yellow

$InvalidClub = @{
    name = ""
    city = "Test"
    foundedYear = 2020
}
Test "POST /Clubs (invalid - empty name)" "POST" "/Clubs" $InvalidClub 400 $true | Out-Null

$InvalidPlayer = @{
    firstName = "Test"
}
Test "POST /Players (invalid - missing fields)" "POST" "/Players" $InvalidPlayer 400 $true | Out-Null

Write-Host ""
Write-Host "===== TEST SUMMARY =====" -ForegroundColor Cyan
Write-Host "Total Tests: $($script:TestNum)" -ForegroundColor Gray
Write-Host "Passed: $($script:Pass) " -ForegroundColor Green
Write-Host "Failed: $($script:Fail) " -ForegroundColor Red

if ($script:TestNum -gt 0) {
    $Pct = [math]::Round(($script:Pass / $script:TestNum) * 100, 1)
    Write-Host "Success Rate: $Pct%" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "Detailed Results:" -ForegroundColor Cyan
$script:Results | Format-Table -AutoSize
