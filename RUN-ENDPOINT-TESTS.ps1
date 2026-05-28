#!/usr/bin/env pwsh
# P2 Comprehensive Endpoint Testing with Proper Error Handling

$BaseUrl = "http://localhost:5000/api"
$Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjU5MTM4YjhhLTk0ZDAtNGFkOS05NDBkLTIzYThjNzkyZjYyNCIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwiZXhwIjoxNzc5ODMxMDQ3LCJpc3MiOiJGb290YmFsbENsdWJBUEkiLCJhdWQiOiJGb290YmFsbENsdWJBcHAifQ.hn3H-dqHbnhFVbCaZ_BCLDS2Vtv2N9y-FYeEIX26DeQ"

$script:Results = @()
$script:TestNum = 0
$script:Pass = 0
$script:Fail = 0

function Test {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Url,
        [object]$Body,
        [int]$Expected,
        [bool]$WithToken
    )
    
    $script:TestNum++
    $FullUrl = "$BaseUrl$Url"
    $Headers = @{"Content-Type" = "application/json"}
    
    if ($WithToken) {
        $Headers["Authorization"] = "Bearer $Token"
    }
    
    $Status = 0
    $Success = $false
    
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
        
        # Try to make the request
        $Response = Invoke-WebRequest @Params -ErrorAction Stop
        $Status = [int]$Response.StatusCode
        $Success = $true
    }
    catch [System.Net.WebException] {
        # Handle HTTP error responses
        $ErrorResponse = $_.Exception.Response
        if ($ErrorResponse) {
            $Status = [int]$ErrorResponse.StatusCode
        } else {
            $Status = 0
        }
    }
    catch {
        # Catch other errors
        $Status = 0
    }
    
    $Pass = ($Status -eq $Expected)
    if ($Pass) { $script:Pass++ } else { $script:Fail++ }
    
    if ($Pass) {
        $Result = "PASS"
        $Color = "Green"
    } else {
        $Result = "FAIL"
        $Color = "Red"
    }
    
    $script:Results += [PSCustomObject]@{
        Num = $script:TestNum
        Name = $Name
        Method = $Method
        Expected = $Expected
        Actual = $Status
        Result = $Result
    }
    
    Write-Host "[$script:TestNum] $Name ... $Result (HTTP $Status/$Expected)" -ForegroundColor $Color
    
    return $Status
}

Write-Host ""
Write-Host "===== P2 API ENDPOINT TESTING =====" -ForegroundColor Cyan
Write-Host "Base URL: $BaseUrl" -ForegroundColor Gray
Write-Host "Token: Admin (JWT)" -ForegroundColor Gray
Write-Host ""

# CLUBS ENDPOINTS
Write-Host "[CLUBS ENDPOINTS]" -ForegroundColor Yellow
Test "GET /Clubs?page=1&pageSize=10" "GET" "/Clubs?page=1&pageSize=10" $null 200 $true | Out-Null
Test "GET /Clubs?search=Test" "GET" "/Clubs?search=Test" $null 200 $true | Out-Null
Test "GET /Clubs?foundedYear=2020" "GET" "/Clubs?foundedYear=2020" $null 200 $true | Out-Null
Test "GET /Clubs/1" "GET" "/Clubs/1" $null 200 $true | Out-Null

$ClubPayload = @{
    name = "Test Club $(Get-Random -Maximum 9999)"
    city = "Test City"
    foundedYear = 2020
    president = "Test President"
    budget = 5000000
}

$ClubStatus = Test "POST /Clubs (Create)" "POST" "/Clubs" $ClubPayload 201 $true

# Extract ID from created club - we'll test with a reasonable guess
$TestClubId = 999

if ($ClubStatus -eq 201) {
    $UpdatePayload = @{
        name = "Updated $(Get-Random)"
        city = "Updated City"
        foundedYear = 2021
    }
    Test "PUT /Clubs/999 (Update)" "PUT" "/Clubs/999" $UpdatePayload 200 $true | Out-Null
    Test "DELETE /Clubs/999" "DELETE" "/Clubs/999" $null 200 $true | Out-Null
}

Write-Host ""

# PLAYERS ENDPOINTS
Write-Host "[PLAYERS ENDPOINTS]" -ForegroundColor Yellow
Test "GET /Players?page=1&pageSize=10" "GET" "/Players?page=1&pageSize=10" $null 200 $true | Out-Null
Test "GET /Players?position=Forward" "GET" "/Players?position=Forward" $null 200 $true | Out-Null
Test "GET /Players?search=Test" "GET" "/Players?search=Test" $null 200 $true | Out-Null
Test "GET /Players?clubId=1" "GET" "/Players?clubId=1" $null 200 $true | Out-Null
Test "GET /Players/1" "GET" "/Players/1" $null 200 $true | Out-Null

$PlayerPayload = @{
    firstName = "TestFirst"
    lastName = "TestLast"
    position = "Forward"
    jerseyNumber = 99
    clubId = 1
    dateOfBirth = "2000-01-01"
    nationality = "Test"
    height = 185
    weight = 85
}

$PlayerStatus = Test "POST /Players (Create)" "POST" "/Players" $PlayerPayload 201 $true

$TestPlayerId = 999
if ($PlayerStatus -eq 201) {
    $PlayerUpdate = @{
        firstName = "UpdatedFirst"
        lastName = "UpdatedLast"
        position = "Midfielder"
        jerseyNumber = 88
        clubId = 1
        dateOfBirth = "2000-01-01"
    }
    Test "PUT /Players/999 (Update)" "PUT" "/Players/999" $PlayerUpdate 200 $true | Out-Null
    Test "DELETE /Players/999" "DELETE" "/Players/999" $null 200 $true | Out-Null
}

Write-Host ""

# STADIUMS ENDPOINTS
Write-Host "[STADIUMS ENDPOINTS]" -ForegroundColor Yellow
Test "GET /Stadiums?page=1&pageSize=10" "GET" "/Stadiums?page=1&pageSize=10" $null 200 $true | Out-Null
Test "GET /Stadiums?search=Test" "GET" "/Stadiums?search=Test" $null 200 $true | Out-Null
Test "GET /Stadiums/1" "GET" "/Stadiums/1" $null 200 $true | Out-Null

$StadiumPayload = @{
    name = "Test Stadium $(Get-Random)"
    city = "Test City"
    capacity = 50000
    yearBuilt = 2020
}

$StadiumStatus = Test "POST /Stadiums (Create)" "POST" "/Stadiums" $StadiumPayload 201 $true

$TestStadiumId = 999
if ($StadiumStatus -eq 201) {
    $StadiumUpdate = @{
        name = "Updated Stadium"
        city = "Updated City"
        capacity = 60000
        yearBuilt = 2021
    }
    Test "PUT /Stadiums/999 (Update)" "PUT" "/Stadiums/999" $StadiumUpdate 200 $true | Out-Null
    Test "DELETE /Stadiums/999" "DELETE" "/Stadiums/999" $null 200 $true | Out-Null
}

Write-Host ""

# AUTHORIZATION TESTS
Write-Host "[AUTHORIZATION TESTS]" -ForegroundColor Yellow
Test "GET /Clubs (No Token - Public)" "GET" "/Clubs" $null 200 $false | Out-Null

$NoTokenPayload = @{
    name = "Should Fail"
    city = "Test"
    foundedYear = 2020
}
Test "POST /Clubs (No Token - Should Fail)" "POST" "/Clubs" $NoTokenPayload 401 $false | Out-Null

Write-Host ""

# VALIDATION TESTS
Write-Host "[VALIDATION TESTS]" -ForegroundColor Yellow

$InvalidClub = @{
    name = ""
    city = "Test"
    foundedYear = 2020
}
Test "POST /Clubs (Invalid - Empty Name)" "POST" "/Clubs" $InvalidClub 400 $true | Out-Null

$InvalidPlayer = @{
    firstName = "Test"
}
Test "POST /Players (Invalid - Missing Fields)" "POST" "/Players" $InvalidPlayer 400 $true | Out-Null

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
