@echo off
setlocal enabledelayedexpansion

REM ========================================
REM SeedHunter - Unified Package Builder
REM Generates both Thunderstore and Nexus packages
REM ========================================

REM Read version from VERSION file
if not exist "VERSION" (
    echo ERROR: VERSION file not found!
    exit /b 1
)

set /p VERSION=<VERSION
set "PACKAGE_NAME=SeedHunter-%VERSION%"

echo ========================================
echo Packaging SeedHunter v%VERSION%
echo ========================================
echo.

REM Clean build directory
if exist "build" rmdir /s /q "build"
mkdir "build\thunderstore\%PACKAGE_NAME%"
mkdir "build\nexus"

REM ========================================
REM Step 1: Build the project
REM ========================================
echo [1/5] Building Release version...
"C:\Program Files\dotnet\dotnet.exe" build SeedHunter.csproj -c Release
if errorlevel 1 (
    echo Build failed!
    exit /b 1
)
echo.

REM ========================================
REM Step 2: Update manifest.json with current version
REM ========================================
echo [2/5] Updating manifest version to %VERSION%...
powershell -Command "(Get-Content manifest.json) -replace '\"version_number\": \".*\"', '\"version_number\": \"%VERSION%\"' | Set-Content manifest.json"
echo.

REM ========================================
REM Step 3: Create Thunderstore Package
REM ========================================
echo [3/5] Creating Thunderstore package...

REM Copy files to Thunderstore package
copy /Y "bin\Release\net6.0\SeedHunter.dll" "build\thunderstore\%PACKAGE_NAME%\SeedHunter.dll" >nul
copy /Y "manifest.json" "build\thunderstore\%PACKAGE_NAME%\manifest.json" >nul
copy /Y "README.md" "build\thunderstore\%PACKAGE_NAME%\README.md" >nul
copy /Y "icon.png" "build\thunderstore\%PACKAGE_NAME%\icon.png" >nul

REM Copy or create CHANGELOG
if exist "CHANGELOG.md" (
    copy /Y "CHANGELOG.md" "build\thunderstore\%PACKAGE_NAME%\CHANGELOG.md" >nul
) else (
    echo # Changelog > "build\thunderstore\%PACKAGE_NAME%\CHANGELOG.md"
    echo. >> "build\thunderstore\%PACKAGE_NAME%\CHANGELOG.md"
    echo ## %VERSION% >> "build\thunderstore\%PACKAGE_NAME%\CHANGELOG.md"
    echo - Initial release >> "build\thunderstore\%PACKAGE_NAME%\CHANGELOG.md"
)

REM Create Thunderstore zip
cd build\thunderstore
powershell -Command "Compress-Archive -Path '%PACKAGE_NAME%\*' -DestinationPath '%PACKAGE_NAME%.zip' -Force" >nul
cd ..\..
echo   Created: build\thunderstore\%PACKAGE_NAME%.zip
echo.

REM ========================================
REM Step 4: Create Nexus Mods Package
REM ========================================
echo [4/5] Creating Nexus Mods package...

REM Create Nexus package structure
mkdir "build\nexus\%PACKAGE_NAME%"

REM Copy DLL
copy /Y "bin\Release\net6.0\SeedHunter.dll" "build\nexus\%PACKAGE_NAME%\SeedHunter.dll" >nul

REM Convert README.md to README.txt for Nexus
powershell -Command "(Get-Content README.md) | Set-Content 'build\nexus\%PACKAGE_NAME%\README.txt'" >nul

REM Create installation instructions
(
echo SeedHunter v%VERSION% - ASKA World Seed Explorer
echo ========================================
echo.
echo INSTALLATION:
echo 1. Install BepInEx 6 IL2CPP for ASKA
echo 2. Place SeedHunter.dll in ASKA\BepInEx\plugins\ folder
echo 3. Launch the game
echo.
echo For detailed instructions, see README.txt
echo.
echo Download BepInEx: https://thunderstore.io/c/aska/p/BepInEx/BepInExPack_IL2CPP/
) > "build\nexus\%PACKAGE_NAME%\INSTALLATION.txt"

REM Create Nexus zip
cd build\nexus
powershell -Command "Compress-Archive -Path '%PACKAGE_NAME%\*' -DestinationPath 'SeedHunter-Nexus-%VERSION%.zip' -Force" >nul
cd ..\..
echo   Created: build\nexus\SeedHunter-Nexus-%VERSION%.zip
echo.

REM ========================================
REM Step 5: Summary
REM ========================================
echo [5/5] Package complete!
echo.
echo ========================================
echo SUCCESS! Packages created:
echo ========================================
echo.
echo Thunderstore (for r2modman/mod managers):
echo   build\thunderstore\%PACKAGE_NAME%.zip
echo   Size:
dir "build\thunderstore\%PACKAGE_NAME%.zip" | find "%PACKAGE_NAME%.zip"
echo.
echo Nexus Mods (manual installation):
echo   build\nexus\SeedHunter-Nexus-%VERSION%.zip
echo   Size:
dir "build\nexus\SeedHunter-Nexus-%VERSION%.zip" | find "SeedHunter-Nexus-%VERSION%.zip"
echo.
echo ========================================
echo Next Steps:
echo ========================================
echo.
echo 1. Upload to Thunderstore: https://thunderstore.io/
echo 2. Upload to Nexus Mods: https://www.nexusmods.com/aska
echo 3. Create GitHub release with tag v%VERSION%
echo.
