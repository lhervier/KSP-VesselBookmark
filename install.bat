@echo off
setlocal

echo =====================================
echo Removing existing Mod folder
echo =====================================

rmdir /s /q "%KSPDIR%\GameData\VesselBookmarkMod"
if errorlevel 1 (
    echo ERROR: Failed to remove the Mod folder
    exit /b 1
)

echo.
echo =====================================
echo Unzipping Mod
echo =====================================

powershell -NoProfile -ExecutionPolicy Bypass -Command "Expand-Archive -Path 'Release\VesselBookmarkMod.zip' -DestinationPath '%KSPDIR%\GameData\VesselBookmarkMod' -Force"
if errorlevel 1 (
    echo ERROR: Failed to unzip the Mod
    exit /b 1
)

echo.
echo Mod installed
