#!/usr/bin/env pwsh
# P2 Comprehensive Endpoint Testing

$BaseUrl = "http://localhost:5000/api"
$Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjU5MTM4YjhhLTk0ZDAtNGFkOS05NDBkLTIzYThjNzkyZjYyNCIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwiZXhwIjoxNzc5ODMxMDQ3LCJpc3MiOiJGb290YmFsbENsdWJBUEkiLCJhdWQiOiJGb290YmFsbENsdWJBcHAifQ.hn3H-dqHbnhFVbCaZ_BCLDS2Vtv2N9y-FYeEIX26DeQ"

$script:Results = @()
$script:TestNum = 0

function Test {
    param($Name, $Method, $Url, $Body, $Expected, $WithToken)
    
    $script:TestNum++
    $FullUrl = "$BaseUrl$Url"
    $Headers = @{"Content-Type" = "application/json"}
    
    if ($WithToken) {
        $Headers["Authorization"] = "Bearer $Token"
    }
    
    try {
        $Params = @{
            Uri = $FullUrl
            Method = $Method
            Headers = $Headers
            UseBasicParsing = $true
            ErrorAction = "SilentlyContinue"
        }
        
        if ($Body) {
            $Params["Body"] = ($Body | ConvertTo-Json -Depth 5 -Compress)
        }
        
        $Response = Invoke-WebRequest @Params
        $Status = $Response.StatusCode
    }
    catch {
        $Status = $_.Exception.Response.StatusCode.Value
    }
    
    $Pass = ($Status -eq $Expected)
    if ($Pass) { $Result = "PASS" } else { $Result = "FAIL" }
    
    $script:Results += [PSCustomObject]@{
        Num = $script:TestNum
        Name = $Name
        Method = $Method
        Expected = $Expected
        Actual = $Status
        Result = $Result
    }
    
    if ($Pass) {
        Write-Host "[$script:TestNum] $Name ... $Result (HTTP $Status)" -ForegroundColor Green
    } else {
        Write-Host "[$script:TestNum] $Name ... $Result (HTTP $Status)" -ForegroundColor Red
    }
    
    return $Status
}

Write-Host ""
Write-Host "===== P2 API ENDPOINT TESTING =====" -ForegroundColor Cyan
Write-Host ""

# CLUBS
Write-Host "CLUBS ENDPOINTS" -ForegroundColor Yellow
Test "GET /clubs (paginated)" "GET" "/Clubs?page=1&pageSize=10" $null 200 $true | Out-Null
Test "GET /clubs (search)" "GET" "/Clubs?search=Man" $null 200 $true | Out-Null
Test "GET /clubs (filter)" "GET" "/Clubs?foundedYear=2000" $null 200 $true | Out-Null
Test "GET /clubs/:id" "GET" "/Clubs/1" $null 200 $true | Out-Null

$Club = @{name="Club$(Get-Random)"; city="City"; foundedYear=2020; president="P"; budget=1000000}
$ClubId = $null
$Status = Test "POST /clubs (create)" "POST" "/Clubs" $Club 201 $true
if ($Status -eq 201) { $ClubId = 1 }

if ($ClubId) {
    Test "PUT /clubs/:id (update)" "PUT" "/Clubs/$ClubId" $Club 200 $true | Out-Null
    Test "DELETE /clubs/:id" "DELETE" "/Clubs/$ClubId" $null 200 $true | Out-Null
}

Write-Host ""

# PLAYERS
Write-Host "PLAYERS ENDPOINTS" -ForegroundColor Yellow
Test "GET /players (paginated)" "GET" "/Players?page=1&pageSize=10" $null 200 $true | Out-Null
Test "GET /players (position)" "GET" "/Players?position=Forward" $null 200 $true | Out-Null
Test "GET /players (search)" "GET" "/Players?search=Mbapp" $null 200 $true | Out-Null
Test "GET /players (clubId)" "GET" "/Players?clubId=1" $null 200 $true | Out-Null
Test "GET /players/:id" "GET" "/Players/1" $null 200 $true | Out-Null

$Player = @{firstName="Test"; lastName="P"; position="FW"; jerseyNumber=10; clubId=1; dateOfBirth="2000-01-01"}
$PlayerId = $null
$Status = Test "POST /players (create)" "POST" "/Players" $Player 201 $true
if ($Status -eq 201) { $PlayerId = 1 }

if ($PlayerId) {
    Test "PUT /players/:id (update)" "PUT" "/Players/$PlayerId" $Player 200 $true | Out-Null
    Test "DELETE /players/:id" "DELETE" "/Players/$PlayerId" $null 200 $true | Out-Null
}

Write-Host ""

# STADIUMS
Write-Host "STADIUMS ENDPOINTS" -ForegroundColor Yellow
Test "GET /stadiums (paginated)" "GET" "/Stadiums?page=1&pageSize=10" $null 200 $true | Out-Null
Test "GET /stadiums (search)" "GET" "/Stadiums?search=Test" $null 200 $true | Out-Null
Test "GET /stadiums/:id" "GET" "/Stadiums/1" $null 200 $true | Out-Null

$Stadium = @{name="Stad$(Get-Random)"; city="City"; capacity=50000; yearBuilt=2020}
$StadId = $null
$Status = Test "POST /stadiums (create)" "POST" "/Stadiums" $Stadium 201 $true
if ($Status -eq 201) { $StadId = 1 }

if ($StadId) {
    Test "PUT /stadiums/:id (update)" "PUT" "/Stadiums/$StadId" $Stadium 200 $true | Out-Null
    Test "DELETE /stadiums/:id" "DELETE" "/Stadiums/$StadId" $null 200 $true | Out-Null
}

Write-Host ""

# AUTHORIZATION
Write-Host "AUTHORIZATION TESTS" -ForegroundColor Yellow
Test "GET /clubs (public - no token)" "GET" "/Clubs" $null 200 $false | Out-Null
Test "POST /clubs (no token - should fail)" "POST" "/Clubs" $Club 401 $false | Out-Null

Write-Host ""

# VALIDATION
Write-Host "VALIDATION TESTS" -ForegroundColor Yellow
$BadClub = @{name=""; city="C"; foundedYear=2020}
Test "POST /clubs (invalid - empty name)" "POST" "/Clubs" $BadClub 400 $true | Out-Null

$BadPlayer = @{firstName="T"}
Test "POST /players (missing fields)" "POST" "/Players" $BadPlayer 400 $true | Out-Null

Write-Host ""
Write-Host "===== SUMMARY =====" -ForegroundColor Cyan
$Pass = ($script:Results | Where-Object Result -eq "PASS").Count
$Fail = ($script:Results | Where-Object Result -eq "FAIL").Count
Write-Host "Total Tests: $($script:Results.Count)"
Write-Host "Passed: $Pass"
Write-Host "Failed: $Fail"
if ($script:Results.Count -gt 0) {
    $Pct = [math]::Round(($Pass / $script:Results.Count) * 100, 1)
    Write-Host "Success Rate: $Pct%"
}
Write-Host ""

$script:Results | Format-Table -AutoSize
