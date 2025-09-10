@echo off
setlocal enabledelayedexpansion

REM Root folder (current dir by default)
set "root=%CD%"

REM Recursively list all directories, excluding unwanted ones
for /f "delims=" %%D in ('
    dir /ad /b /s "%root%" ^
    ^| findstr /vi /c:"\.git" /c:"\.vs" /c:"\bin" /c:"\obj"
') do (
    REM List files in this directory
    for %%F in ("%%~fD\*") do (
        if exist "%%~fF" (
            echo %%~fF %%~zF
        )
    )
)

pause
endlocal