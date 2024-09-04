FROM public.ecr.aws/docker/library/gcc:12-bookworm AS deps

ENV VCPKG_FORCE_SYSTEM_BINARIES=true

RUN apt-get update && \
    apt-get install -y curl zip unzip tar git cmake ninja-build

WORKDIR /app

RUN git clone https://github.com/microsoft/vcpkg ./vcpkg && \
    ./vcpkg/bootstrap-vcpkg.sh && \
    ./vcpkg/vcpkg install fmt microsoft-signalr

FROM public.ecr.aws/docker/library/gcc:12-bookworm AS build

# Install deps
RUN apt-get update && \
    apt-get install -y curl zip unzip tar git cmake g++ make

WORKDIR /app

COPY . ./
COPY --from=deps /app/vcpkg ./vcpkg

RUN mkdir -p build/ && \
    cd ./build && \
    cmake -DCMAKE_BUILD_TYPE=Release -DCMAKE_CXX_COMPILER=g++ -S .. -B . && \
    make

FROM public.ecr.aws/docker/library/debian:bookworm-slim

ENV RUNNER_IPV4=http://localhost
ENV BOT_NICKNAME=
ENV TOKEN=

WORKDIR /app
COPY --from=build /app/build/CPPBot ./

CMD ["/app/CPPBot"]
