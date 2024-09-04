# C++ Starter Bot

## Prerequisites

### Linux

- Git
- g++ >= 6
- CMake
- GNU Make
- pkg-config
- tar, zip, unzip, curl

### macOS

- Everything from Linux
- Apple Developer Tools (macOS)

### Windows

- Windows 10 or newer (64-bit)
- Windows Subsystem for Linux (WSL)
- The same dependencies as Linux installed within WSL

## Setup

Project setup is easy, simply run:

```shell
./setup.sh
```

This will install and bootstrap vcpkg, as well as install the required packages.

It will also copy the `compile_commands.json` file to the root of the project to enable
intellisense support.

## Building

To build the bot, run:

```shell
./build.sh
```

This will configure and build the bot using CMake. You can find output in the `build` folder. 

## Running

To execute the bot, after building, run:

```shell
./run.sh
```

## Building the docker

To build the docker image, run:

```shell
docker build .
```
