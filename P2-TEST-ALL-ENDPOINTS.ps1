# P2 Comprehensive API Endpoint Testing Script
# Tests all CLUBS, PLAYERS, and STADIUMS endpoints

param(
    [string]$BaseUrl = "http://localhost:5000/api"
)

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
        
        $Result = if($IsPass) {"PASS"} else {"FAIL"}
        $Results += [PSCustomObject]@{
            Test = $TestCount
            Name = $Name
            Method = $Method
            Endpoint = $Endpoint
            Expected = $ExpectedStatus
            Actual = $StatusCode
            Result = $Result
        }
        
        return @{ StatusCode = $StatusCode; Body = $Response.Content }
    }
    catch {
        $StatusCode = $_.Exception.Response.StatusCode.Value
        $IsPass = ($StatusCode -eq $ExpectedStatus)
        
        if ($IsPass) { $PassCount++ } else { $FailCount++ }
        
        $Result = if($IsPass) {"PASS"} else {"FAIL"}
        $Results += [PSCustomObject]@{
            Test = $TestCount
            Name = $Name
            Method = $Method
            Endpoint = $Endpoint
            Expected = $ExpectedStatus
            Actual = $StatusCode
            Result = $Result
        }
        
        return @{ StatusCode = $StatusCode; Body = $null }
    }
}

Write-Host ""
Write-Host "====== P2 ENDPOINT TESTING ======" -ForegroundColor Cyan
Write-Host ""

# CLUBS ENDPOINTS
Write-Host "CLUBS ENDPOINTS (7 tests)" -ForegroundColor Green
Test-Endpoint "GET Clubs List" "GET" "/Clubs?page=1&pageSize=10" $null 200 | Out-Null
Test-Endpoint "GET Clubs with Search" "GET" "/Clubs?search=Manchester" $null 200 | Out-Null
Test-Endpoint "GET Clubs with Filter" "GET" "/Clubs?foundedYear=2000" $null 200 | Out-Null
Test-Endpoint "GET Specific Club" "GET" "/Clubs/1" $null 200 | Out-Null

$NewClub = @{name="TestClub$(Get-Random)"; city="TestCity"; foundedYear=2020; logoUrl="https://example.com/logo.png"; president="TestPres"; budget=1000000}
$CreateResult = Test-Endpoint "POST Create Club" "POST" "/Clubs" $NewClub 201
$ClubId = $null
if ($CreateResult.StatusCode -eq 201 -and $CreateResult.Body) {
    try {
        $ClubData = $CreateResult.Body | ConvertFrom-Json
        $ClubId = $ClubData.id
    } catch {}
}

if ($ClubId) {
    $UpdateClub = @{name="UpdatedClub$(Get-Random)"; city="UpdatedCity"; foundedYear=2021}
    Test-Endpoint "PUT Update Club" "PUT" "/Clubs/$ClubId" $UpdateClub 200 | Out-Null
    Test-Endpoint "DELETE Club" "DELETE" "/Clubs/$ClubId" $null 200 | Out-Null
}

Write-Host ""

# PLAYERS ENDPOINTS
Write-Host "PLAYERS ENDPOINTS (8 tests)" -ForegroundColor Green
Test-Endpoint "GET Players List" "GET" "/Players?page=1&pageSize=10" $null 200 | Out-Null
Test-Endpoint "GET Players by Position" "GET" "/Players?position=Forward" $null 200 | Out-Null
Test-Endpoint "GET Players by Search" "GET" "/Players?search=Kylian" $null 200 | Out-Null
Test-Endpoint "GET Players by Club" "GET" "/Players?clubId=1" $null 200 | Out-Null
Test-Endpoint "GET Specific Player" "GET" "/Players/1" $null 200 | Out-Null

$NewPlayer = @{firstName="Test"; lastName="Player"; position="Forward"; jerseyNumber=10; clubId=1; dateOfBirth="2000-01-01"; nationality="TestNation"; height=180; weight=75; status="Active"; marketValue=5000000}
$PlayerResult = Test-Endpoint "POST Create Player" "POST" "/Players" $NewPlayer 201
$PlayerId = $null
if ($PlayerResult.StatusCode -eq 201 -and $PlayerResult.Body) {
    try {
        $PlayerData = $PlayerResult.Body | ConvertFrom-Json
        $PlayerId = $PlayerData.id
    } catch {}
}

if ($PlayerId) {
    $UpdatePlayer = @{firstName="Updated"; lastName="Player"; position="Midfielder"; jerseyNumber=20; clubId=1; dateOfBirth="2000-01-01"}
    Test-Endpoint "PUT Update Player" "PUT" "/Players/$PlayerId" $UpdatePlayer 200 | Out-Null
    Test-Endpoint "DELETE Player" "DELETE" "/Players/$PlayerId" $null 200 | Out-Null
}

Write-Host ""

# STADIUMS ENDPOINTS
Write-Host "STADIUMS ENDPOINTS (5 tests)" -ForegroundColor Green
Test-Endpoint "GET Stadiums List" "GET" "/Stadiums?page=1&pageSize=10" $null 200 | Out-Null
Test-Endpoint "GET Stadiums Search" "GET" "/Stadiums?search=Test" $null 200 | Out-Null
Test-Endpoint "GET Specific Stadium" "GET" "/Stadiums/1" $null 200 | Out-Null

$NewStadium = @{name="TestStadium$(Get-Random)"; city="TestCity"; capacity=50000; yearBuilt=2020}
$StadiumResult = Test-Endpoint "POST Create Stadium" "POST" "/Stadiums" $NewStadium 201
$StadiumId = $null
if ($StadiumResult.StatusCode -eq 201 -and $StadiumResult.Body) {
    try {
        $StadiumData = $StadiumResult.Body | ConvertFrom-Json
        $StadiumId = $StadiumData.id
    } catch {}
}

if ($StadiumId) {
    $UpdateStadium = @{name="UpdatedStadium"; city="UpdatedCity"; capacity=60000; yearBuilt=2021}
    Test-Endpoint "PUT Update Stadium" "PUT" "/Stadiums/$StadiumId" $UpdateStadium 200 | Out-Null
    Test-Endpoint "DELETE Stadium" "DELETE" "/Stadiums/$StadiumId" $null 200 | Out-Null
}

Write-Host ""

# AUTHORIZATION TESTS
Write-Host "AUTHORIZATION TESTS (2 tests)" -ForegroundColor Yellow
Test-Endpoint "GET Clubs (no token - public)" "GET" "/Clubs" $null 200 $false | Out-Null
Test-Endpoint "POST Club (no token - should fail)" "POST" "/Clubs" $NewClub 401 $false | Out-Null

Write-Host ""

# VALIDATION TESTS
Write-Host "VALIDATION TESTS (2 tests)" -ForegroundColor Yellow
$InvalidClub = @{name=""; city="TestCity"; foundedYear=2020}
Test-Endpoint "POST Club (invalid - empty name)" "POST" "/Clubs" $InvalidClub 400 | Out-Null

$InvalidPlayer = @{firstName="Test"; lastName="Player"}
Test-Endpoint "POST Player (missing required)" "POST" "/Players" $InvalidPlayer 400 | Out-Null

Write-Host ""
Write-Host "====== TEST SUMMARY ======" -ForegroundColor Cyan
$Results | Format-Table -AutoSize
Write-Host ""
Write-Host "Total: $TestCount | Passed: $PassCount | Failed: $FailCount" -ForegroundColor Cyan
$PassPercentage = [math]::Round(($PassCount / $TestCount) * 100, 2)
Write-Host "Success Rate: $PassPercentage%" -ForegroundColor Cyan
Write-Host ""
