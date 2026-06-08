@echo off
title Payroll Run Module - Server (keep this window OPEN)
echo ============================================================
echo   Employee Payroll Run Module
echo ============================================================
echo.
echo Starting the server... please wait a few seconds.
echo.
echo   When you see "Now listening on: http://localhost:5080",
echo   open your browser and go to:
echo.
echo        http://localhost:5080
echo.
echo   KEEP THIS WINDOW OPEN. Closing it stops the website.
echo   Press Ctrl+C here to stop the server.
echo ============================================================
echo.
cd /d "%~dp0src\Payroll.Api"
dotnet run
echo.
echo Server stopped. Press any key to close.
pause >nul
