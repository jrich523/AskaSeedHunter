@echo off
setlocal
REM Package SeedHunter for Thunderstore

set "VERSION=1.0.0"
set "PACKAGE_NAME=SeedHunter-%VERSION%"
set "PACKAGE_DIR=build\thunderstore\%PACKAGE_NAME%"

echo ========================================
echo Packaging SeedHunter for Thunderstore
echo Version: %VERSION%
echo ========================================

REM Clean and create package directory
if exist "build\thunderstore" rmdir /s /q "build\thunderstore"
mkdir "%PACKAGE_DIR%"

REM Build the project
echo.
echo [1/5] Building Release version...
"C:\Program Files\dotnet\dotnet.exe" build SeedHunter.csproj -c Release
if errorlevel 1 (
    echo Build failed!
    exit /b 1
)

REM Copy required files
echo [2/5] Copying DLL...
copy /Y "bin\Release\net6.0\SeedHunter.dll" "%PACKAGE_DIR%\SeedHunter.dll"

echo [3/5] Copying metadata files...
copy /Y "manifest.json" "%PACKAGE_DIR%\manifest.json"
copy /Y "README.md" "%PACKAGE_DIR%\README.md"
copy /Y "icon.png" "%PACKAGE_DIR%\icon.png"

REM Create CHANGELOG.md if it doesn't exist
if not exist "CHANGELOG.md" (
    echo # Changelog > "%PACKAGE_DIR%\CHANGELOG.md"
    echo. >> "%PACKAGE_DIR%\CHANGELOG.md"
    echo ## 1.0.0 >> "%PACKAGE_DIR%\CHANGELOG.md"
    echo - Initial release >> "%PACKAGE_DIR%\CHANGELOG.md"
) else (
    copy /Y "CHANGELOG.md" "%PACKAGE_DIR%\CHANGELOG.md"
)

REM Create the zip file
echo [4/5] Creating zip archive...
cd build\thunderstore
powershell -Command "Compress-Archive -Path '%PACKAGE_NAME%\*' -DestinationPath '%PACKAGE_NAME%.zip' -Force"
cd ..\..

echo [5/5] Cleaning up...
REM Keep the package folder for inspection
REM rmdir /s /q "%PACKAGE_DIR%"

echo.
echo ========================================
echo SUCCESS! Package created:
echo build\thunderstore\%PACKAGE_NAME%.zip
echo ========================================
echo.
echo Next steps:
echo 1. Upload to https://thunderstore.io/
echo 2. Create GitHub release with tag v%VERSION%
echo.
