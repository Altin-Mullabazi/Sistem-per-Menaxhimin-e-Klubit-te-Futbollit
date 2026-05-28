# Comprehensive test - Create Club, then Player

$login = @{
    email = "admin@footballclub.com"
    password = "Admin@123"
} | ConvertTo-Json

$loginResp = Invoke-WebRequest -Uri "http://localhost:5000/api/auth/login" `
    -Method POST `
    -Body $login `
    -ContentType "application/json" `
    -UseBasicParsing

$token = ($loginResp.Content | ConvertFrom-Json).accessToken
Write-Host "Token obtained"

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# 1. Create Club first
Write-Host "`n=== CREATING CLUB ==="
$clubPayload = @{
    name = "Test Club FC"
    city = "Test City"
    foundedYear = 2020
} | ConvertTo-Json

try {
    $clubResp = Invoke-WebRequest -Uri "http://localhost:5000/api/clubs" `
        -Method POST `
        -Headers $headers `
        -Body $clubPayload `
        -UseBasicParsing `
        -ErrorAction Stop
    
    $clubData = $clubResp.Content | ConvertFrom-Json
    $clubId = $clubData.data.id
    Write-Host "Club created: ID=$clubId"
}
catch {
    Write-Host "ERROR creating club: $($_.Exception.Response.StatusCode.Value__)"
    Write-Host $_.Exception.Response.GetResponseStream() | ConvertFrom-Json
    exit
}

# 2. Now create Player using the club ID
Write-Host "`n=== CREATING PLAYER WITH CLUB ID $clubId ==="
$playerPayload = @{
    firstName = "John"
    lastName = "Doe"
    position = "Forward"
    jerseyNumber = 10
    clubId = $clubId
    dateOfBirth = "2000-01-01T00:00:00Z"
    nationality = "Albanian"
} | ConvertTo-Json

Write-Host "Payload: $playerPayload"

try {
    $playerResp = Invoke-WebRequest -Uri "http://localhost:5000/api/players" `
        -Method POST `
        -Headers $headers `
        -Body $playerPayload `
        -UseBasicParsing `
        -ErrorAction Stop
    
    Write-Host "SUCCESS - Status: $($playerResp.StatusCode)"
    $playerData = $playerResp.Content | ConvertFrom-Json
    Write-Host $playerResp.Content | ConvertFrom-Json | ConvertTo-Json
}
catch {
    Write-Host "ERROR - Status: $($_.Exception.Response.StatusCode.Value__)"
    try {
        $errorResponse = $_.Exception.Response.GetResponseStream()
        $reader = [System.IO.StreamReader]::new($errorResponse)
        $errorContent = $reader.ReadToEnd()
        Write-Host "Response: $errorContent"
    }
    catch {
        Write-Host "Could not read error response"
    }
}
