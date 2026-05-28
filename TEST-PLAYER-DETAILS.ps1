# Test player creation with detailed response

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
Write-Host "Token: $token`n"

# Test payload
$payload = @{
    firstName = "John"
    lastName = "Doe"
    position = "Forward"
    jerseyNumber = 10
    clubId = 1
    dateOfBirth = "2000-01-01T00:00:00Z"
    nationality = "Albanian"
} | ConvertTo-Json

Write-Host "Payload:"
Write-Host $payload
Write-Host ""

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/players" `
        -Method POST `
        -Headers $headers `
        -Body $payload `
        -UseBasicParsing `
        -ErrorAction Stop
    
    Write-Host "SUCCESS - Status: $($response.StatusCode)"
    Write-Host $response.Content
}
catch {
    Write-Host "ERROR - Status: $($_.Exception.Response.StatusCode.Value__)"
    Write-Host "Response:"
    try {
        $errorResponse = $_.Exception.Response.GetResponseStream()
        $reader = [System.IO.StreamReader]::new($errorResponse)
        $errorContent = $reader.ReadToEnd()
        Write-Host $errorContent
    }
    catch {
        Write-Host "Could not read error response"
    }
}
