# Debug PUT /players error

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
Write-Host "Token obtained`n"

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# First create a player to update
$clubPayload = @{
    name = "UpdateTestClub_$(Get-Date -Format 'yyyyMMddHHmmss')"
    city = "Test City"
    foundedYear = 2020
} | ConvertTo-Json

$clubResp = Invoke-WebRequest -Uri "http://localhost:5000/api/clubs" `
    -Method POST `
    -Headers $headers `
    -Body $clubPayload `
    -UseBasicParsing

$clubId = ($clubResp.Content | ConvertFrom-Json).data.id
Write-Host "Club created: ID=$clubId`n"

# Create player
$playerPayload = @{
    firstName = "John"
    lastName = "Doe"
    position = "Forward"
    jerseyNumber = 10
    clubId = $clubId
    dateOfBirth = "2000-01-01T00:00:00Z"
} | ConvertTo-Json

$playerResp = Invoke-WebRequest -Uri "http://localhost:5000/api/players" `
    -Method POST `
    -Headers $headers `
    -Body $playerPayload `
    -UseBasicParsing

$playerId = ($playerResp.Content | ConvertFrom-Json).data.id
Write-Host "Player created: ID=$playerId`n"

# Now try to update
Write-Host "=== ATTEMPTING PUT /players/$playerId ==="
$updatePayload = @{
    firstName = "UpdatedJohn"
    lastName = "UpdatedDoe"
    position = "Midfielder"
    jerseyNumber = 15
    clubId = $clubId
    dateOfBirth = "2000-01-01T00:00:00Z"
    nationality = "German"
} | ConvertTo-Json

Write-Host "Update Payload:"
Write-Host $updatePayload
Write-Host ""

try {
    $updateResp = Invoke-WebRequest -Uri "http://localhost:5000/api/players/$playerId" `
        -Method PUT `
        -Headers $headers `
        -Body $updatePayload `
        -UseBasicParsing `
        -ErrorAction Stop
    
    Write-Host "SUCCESS - Status: $($updateResp.StatusCode)"
    Write-Host $updateResp.Content
}
catch {
    Write-Host "ERROR - Status: $($_.Exception.Response.StatusCode.Value__)"
    try {
        $errorStream = $_.Exception.Response.GetResponseStream()
        $reader = [System.IO.StreamReader]::new($errorStream)
        $content = $reader.ReadToEnd()
        Write-Host "Response: $content"
    }
    catch {
        Write-Host "Could not read response"
    }
}
