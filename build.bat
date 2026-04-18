@echo off
echo ========================================
echo Building GMTPC.Tool (Release x64)
echo ========================================
echo.

cd /d "%~dp0"

set "MSBUILD="
if exist "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD=C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"
) else if exist "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD=C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
) else if exist "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe" (
    set "MSBUILD=C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
)

if "%MSBUILD%"=="" (
    echo [ERROR] MSBuild not found! Please install Visual Studio or Build Tools.
    pause
    exit /b 1
)

echo Using: %MSBUILD%
echo.

"%MSBUILD%" GMTPC.Tool.csproj /p:Configuration=Release /p:Platform=x64 /nologo /v:minimal
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERROR] Build failed!
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo ========================================
echo Build successful!
echo ========================================
echo.

echo Copying exe to root folder...
copy /Y "bin\x64\Release\net48\GMTPC.Tool.exe" "GMTPC.Tool.exe" >nul
if %ERRORLEVEL% NEQ 0 (
    echo [WARNING] Failed to copy exe to root folder.
    goto end
) else (
    echo [OK] Exe copied to root: GMTPC.Tool.exe
)
echo.

<<<<<<< HEAD
echo Adding exe to git...
git add GMTPC.Tool.exe
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Failed to add exe to git.
    goto end
)

echo.
set /p commit_msg="Enter commit message: "
if "%commit_msg%"=="" (
    echo [WARNING] No commit message provided. Skipping commit.
    goto end
)

git commit -m "%commit_msg%"
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Commit failed.
    goto end
)

echo.
echo Pushing to remote repository...
git push
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Push failed.
    goto end
)

echo [OK] Commit and push successful!

:end
=======
echo Adding to git...
git add .
echo Enter commit message:
set /p COMMIT_MSG=
git commit -m "%COMMIT_MSG%"
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Commit failed!
    pause
    exit /b %ERRORLEVEL%
)
echo Pushing to remote...
git push
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Push failed!
    pause
    exit /b %ERRORLEVEL%
)

>>>>>>> 3fa1fd0 (fix UI)
echo Done!
pause