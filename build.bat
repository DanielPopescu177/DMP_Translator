@echo off
chcp 65001 >nul
echo ====================================
echo DMP Translator Build
echo ====================================
echo.

REM Edit game path here!
set GAME_PATH=C:\Users\grizl\AppData\Roaming\AndApp\Apps\6234675259375616\Payload

echo Building...
dotnet build -c Release

if errorlevel 1 (
    echo.
    echo Build failed!
    pause
    exit /b 1
)

echo.
echo Build success!
echo.
echo Copying DLL to game folder...

if not exist "%GAME_PATH%\Mods\" (
    echo.
    echo Mods folder not found!
    echo Please check game path: %GAME_PATH%
    pause
    exit /b 1
)

copy /Y "bin\Release\net6.0\DMP_Translator.dll" "%GAME_PATH%\Mods\"

if errorlevel 1 (
    echo.
    echo Copy failed! Check game path.
    pause
    exit /b 1
)

echo.
echo ====================================
echo Complete! Run the game.
echo ====================================
echo.
pause
