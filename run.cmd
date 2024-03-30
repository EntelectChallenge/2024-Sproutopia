@echo off
setlocal

set "IMAGE_NAME=sproutopia"
set "FORCE_BUILD=false"

REM Check for command-line argument to force build
:parse_args
if "%~1" == "" goto :end_parse_args
if /i "%~1" == "-b" (
    set "FORCE_BUILD=true"
    shift
    goto :parse_args
)
if /i "%~1" == "--build" (
    set "FORCE_BUILD=true"
    shift
    goto :parse_args
)

echo Unknown parameter passed: %1
echo Usage: %0 [OPTION]
echo OPTIONS:
echo   -b, --build                  force build of docker image even if it already exists
exit /b 1
:end_parse_args

REM Check if the Docker image already exists or force build
if "%FORCE_BUILD%" == "true" goto :build_image
docker image inspect "%IMAGE_NAME%" >nul 2>nul
if %errorlevel% neq 0 goto :build_image
echo Docker image '%IMAGE_NAME%' already exists. Skipping build.
goto :run_container

REM Build the Docker image
:build_image
echo Building Docker image '%IMAGE_NAME%'...
docker build --no-cache --build-arg CONFIGURATION=Debug -t "%IMAGE_NAME%" .

REM Run the Docker container
:run_container
docker run -it --rm -p 5000:5000 --name "%IMAGE_NAME%" "%IMAGE_NAME%"
