# Start Backend + Frontend (two windows)
$root = $PSScriptRoot

Write-Host "Starting Backend API in a new window..."
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$root\BackendAPI'; Write-Host 'API: http://localhost:5000' -ForegroundColor Green; dotnet run --urls http://localhost:5000"

Start-Sleep -Seconds 4

Write-Host "Starting Frontend..."
Set-Location "$root\Frontend"
if (-not (Test-Path "node_modules")) {
    npm install
}
npm run dev
