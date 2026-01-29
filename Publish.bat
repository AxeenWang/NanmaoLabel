@echo off
chcp 65001 >nul
setlocal

echo ========================================
echo   NanmaoLabel Publish Script
echo ========================================
echo.

set PROJECT_DIR=NanmaoLabelPOC
set OUTPUT_DIR=publish
set CONFIGURATION=Release
set RUNTIME=win-x64

echo [1/3] 清理舊的發布目錄...
if exist "%OUTPUT_DIR%" rmdir /s /q "%OUTPUT_DIR%"

echo [2/3] 發布應用程式...
echo      Configuration: %CONFIGURATION%
echo      Runtime: %RUNTIME%
echo.

dotnet publish %PROJECT_DIR% ^
    -c %CONFIGURATION% ^
    -r %RUNTIME% ^
    -o %OUTPUT_DIR% ^
    --self-contained false ^
    -p:PublishSingleFile=false

if %ERRORLEVEL% neq 0 (
    echo.
    echo [錯誤] 發布失敗！
    pause
    exit /b 1
)

echo.
echo [3/3] 發布完成！
echo.
echo 輸出目錄: %CD%\%OUTPUT_DIR%
echo.

dir /b "%OUTPUT_DIR%\*.exe"

echo.
echo ========================================
pause
