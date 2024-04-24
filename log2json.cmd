@echo off
setlocal enabledelayedexpansion

rem Check if correct number of arguments are provided
if "%~2"=="" (
    echo Usage: script.cmd input_filename output_filename
    exit /b 1
)

rem Check if the input file exists
if not exist "%~1" (
    echo Input file '%~1' not found.
    exit /b 1
)

rem Read the content of the input file
set "totalLines=0"
for /f %%A in ('type "%~1" ^| find /c /v ""') do set "totalLines=%%A"

rem Open the output file for writing
(
    rem Add [ at the start of the output file
    echo [

    rem Loop through each line in the input file
    set "lineNumber=0"
    for /f "usebackq delims=" %%L in ("%~1") do (
        set /a "lineNumber+=1"
        set "line=%%L"

        rem Append a comma to the end of each line except the last line
        if !lineNumber! lss %totalLines% (
            echo !line!,
        ) else (
            echo !line!
        )
    )

    rem Add ] at the end of the output file
    echo ]
) > "%~2"

rem End of script
