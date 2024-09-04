#!/usr/bin/env bash

mkdir -p build/
pushd build
cmake -DCMAKE_CXX_COMPILER=g++ -DCMAKE_BUILD_TYPE=RelWithDebInfo -S .. -B .
make

popd
