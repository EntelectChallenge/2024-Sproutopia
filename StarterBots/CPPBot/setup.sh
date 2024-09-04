#!/usr/bin/env bash

setup_vcpkg() {
  if [[ ! -d ./vcpkg ]]; then
    echo "Cloning VCPKG..."
    git clone https://github.com/microsoft/vcpkg vcpkg > /dev/null
  fi
  echo "Bootstrapping VCPKG..."
  ./vcpkg/bootstrap-vcpkg.sh > /dev/null
  echo "Installing dependencies..."
  ./vcpkg/vcpkg install fmt
  ./vcpkg/vcpkg install --head microsoft-signalr
}

setup_compile_commands() {
  echo "Setting up compile_commands.json"
  mkdir -p build/
  cmake -DCMAKE_EXPORT_COMPILE_COMMANDS=true -S . -B ./build > /dev/null 2>&1
  mv ./build/compile_commands.json ./
  rm -r ./build/*
}

setup_vcpkg
setup_compile_commands
