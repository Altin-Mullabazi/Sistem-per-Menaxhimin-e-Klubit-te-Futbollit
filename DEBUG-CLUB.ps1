# Debug club creation error

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

# Test payload - exact same as P2-FULL-TEST.ps1
$payload = @{
    name = "Test FC"
    city = "Test City"
    foundedYear = 2020
} | ConvertTo-Json -Depth 10

Write-Host "Payload:"
Write-Host $payload
Write-Host ""

try {
    $resp = Invoke-WebRequest -Uri "http://localhost:5000/api/clubs" `
        -Method POST `
        -Headers $headers `
        -Body $payload `
        -UseBasicParsing `
        -ErrorAction Stop
    
    Write-Host "SUCCESS - Status: $($resp.StatusCode)"
    Write-Host $resp.Content
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
