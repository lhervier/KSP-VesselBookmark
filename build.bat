@echo off
setlocal

echo =========
echo Building 
echo =========

echo Removing Release folder
rmdir /s /q Release
if errorlevel 1 (
    echo ERROR: Failed to remove the Release folder
    exit /b 1
)

echo Creating Release folder
mkdir Release
if errorlevel 1 (
    echo ERROR: Failed to create the Release folder
    exit /b 1
)
mkdir Release\VesselBookmarkMod
if errorlevel 1 (
    echo ERROR: Failed to create the Mod folder
    exit /b 1
)

mkdir Release\VesselBookmarkMod\vessel_types
if errorlevel 1 (
    echo ERROR: Failed to create the vessel types folder
    exit /b 1
)

mkdir Release\VesselBookmarkMod\buttons
if errorlevel 1 (
    echo ERROR: Failed to create the buttons folder
    exit /b 1
)

echo Building Mod DLL
dotnet build
if errorlevel 1 (
    echo ERROR: Failed to build the Mod DLL
    exit /b 1
)

echo Copying Mod dll files
copy /y "Output\bin\VesselBookmarkMod.dll" "Release\VesselBookmarkMod"
if errorlevel 1 (
    echo ERROR: Failed to copy the Mod DLL
    exit /b 1
)

echo Copying ModuleManager config file
copy /y "GameData\VesselBookmarkMod\VesselBookmarkMod.cfg" "Release\VesselBookmarkMod"
if errorlevel 1 (
    echo ERROR: Failed to copy the config file
    exit /b 1
)

echo Copying icon file
copy /y "GameData\VesselBookmarkMod\icon.png" "Release\VesselBookmarkMod"
if errorlevel 1 (
    echo ERROR: Failed to copy the icon file
    exit /b 1
)

echo Copying vessel types icons files
copy /y "GameData\VesselBookmarkMod\vessel_types\*" "Release\VesselBookmarkMod\vessel_types"
if errorlevel 1 (
    echo ERROR: Failed to copy the vessel types files
    exit /b 1
)

echo Copying buttons icons files
copy /y "GameData\VesselBookmarkMod\buttons\*" "Release\VesselBookmarkMod\buttons"
if errorlevel 1 (
    echo ERROR: Failed to copy the buttons files
    exit /b 1
)

echo Zipping Mod
powershell -Command "Compress-Archive -Path 'Release\VesselBookmarkMod\*' -DestinationPath 'Release\VesselBookmarkMod.zip' -Force"
if errorlevel 1 (
    echo ERROR: Failed to zip the Mod
    exit /b 1
)

echo Removing Mod folder
rmdir /s /q Release\VesselBookmarkMod
if errorlevel 1 (
    echo ERROR: Failed to remove the Mod folder
    exit /b 1
)


echo Build Complete
