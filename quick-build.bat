@echo off
setlocal enabledelayedexpansion
REM Quick build and deploy to ASKA

REM Check for environment variable override first
if defined ASKA_PATH (
    set "GAME_PATH=%ASKA_PATH%"
    goto :build
)

REM Common Steam installation paths
set "STEAM_PATHS[0]=C:\Program Files (x86)\Steam"
set "STEAM_PATHS[1]=C:\Steam"
set "STEAM_PATHS[2]=D:\Steam"

REM Try to find Steam installation
set "STEAM_PATH="
for /L %%i in (0,1,2) do (
    if exist "!STEAM_PATHS[%%i]!\steamapps\libraryfolders.vdf" (
        set "STEAM_PATH=!STEAM_PATHS[%%i]!"
        goto :found_steam
    )
)

echo ERROR: Could not find Steam installation.
echo Please set ASKA_PATH environment variable to your ASKA installation folder.
echo Example: set ASKA_PATH=F:\SteamLibrary\steamapps\common\ASKA
exit /b 1

:found_steam
echo Found Steam at: %STEAM_PATH%

REM Parse libraryfolders.vdf to find all Steam libraries
set "VDF_FILE=%STEAM_PATH%\steamapps\libraryfolders.vdf"
set "FOUND_ASKA="

REM Extract paths from VDF and check for ASKA
for /f "usebackq tokens=*" %%a in ("%VDF_FILE%") do (
    set "LINE=%%a"
    echo !LINE! | findstr /C:"path" >nul
    if !errorlevel! equ 0 (
        REM Extract the path value (format: "path" "C:\\Path\\Here")
        for /f "tokens=2 delims=	 " %%b in ("%%a") do (
            set "LIBPATH=%%~b"
            REM Replace double backslashes with single
            set "LIBPATH=!LIBPATH:\\=\!"
            echo Checking: !LIBPATH!\steamapps\common\ASKA
            if exist "!LIBPATH!\steamapps\common\ASKA\BepInEx\plugins" (
                set "GAME_PATH=!LIBPATH!\steamapps\common\ASKA"
                set "FOUND_ASKA=1"
                goto :build
            )
        )
    )
)

if not defined FOUND_ASKA (
    echo ERROR: Could not find ASKA installation in any Steam library.
    echo Please set ASKA_PATH environment variable to your ASKA installation folder.
    echo Example: set ASKA_PATH=F:\SteamLibrary\steamapps\common\ASKA
    exit /b 1
)

:build
echo.
echo ========================================
echo Building SeedHunter...
echo ========================================
"C:\Program Files\dotnet\dotnet.exe" build "%~dp0SeedHunter.csproj" -c Release
if errorlevel 1 (
    echo Build failed!
    exit /b 1
)

echo.
echo ========================================
echo Deploying to: %GAME_PATH%\BepInEx\plugins\
echo ========================================
copy /Y "%~dp0bin\Release\net6.0\SeedHunter.dll" "%GAME_PATH%\BepInEx\plugins\SeedHunter.dll"
if errorlevel 1 (
    echo Deploy failed!
    exit /b 1
)

echo.
echo ========================================
echo Deployed successfully to:
echo %GAME_PATH%\BepInEx\plugins\SeedHunter.dll
echo ========================================
