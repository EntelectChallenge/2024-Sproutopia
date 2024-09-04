cmake_minimum_required(VERSION 3.25)

# Add VCPKG cmake toolchain file
set(CMAKE_TOOLCHAIN_FILE "${CMAKE_CURRENT_SOURCE_DIR}/vcpkg/scripts/buildsystems/vcpkg.cmake"
        CACHE STRING "Vcpkg toolchain file")

project(CPPBot)

set(CMAKE_CXX_STANDARD 17)

add_compile_options(-fms-extensions)

# Find and link microsoft-signalr library
find_package(microsoft-signalr REQUIRED)
find_package(fmt REQUIRED)
link_libraries(microsoft-signalr::microsoft-signalr)
link_libraries(fmt::fmt)

add_executable(CPPBot main.cpp service/BotService.cpp)
