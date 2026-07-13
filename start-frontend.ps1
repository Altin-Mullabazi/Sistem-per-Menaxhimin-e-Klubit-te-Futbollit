# Start React frontend (http://localhost:5173) — API proxied to :5000
Set-Location $PSScriptRoot\Frontend
Write-Host "Starting frontend at http://localhost:5173 ..."
Write-Host "Make sure BackendAPI is running first (.\start-backend.ps1)"
npm run dev
