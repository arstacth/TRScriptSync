@echo off
cd /d "%~dp0"

echo === Debug (test) ===
dotnet build -c Debug
if errorlevel 1 exit /b 1
echo Debug exe: bin\Debug\net8.0-windows\TRScriptSync.exe
echo.

echo === Release single EXE ===
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o bin\publish
if errorlevel 1 exit /b 1
echo Single exe: bin\publish\TRScriptSync.exe
