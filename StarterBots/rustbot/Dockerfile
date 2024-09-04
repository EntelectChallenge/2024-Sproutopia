FROM public.ecr.aws/docker/library/rust:slim-bullseye AS build

WORKDIR /app

RUN apt update && apt install -y pkg-config libssl-dev

ADD Cargo.toml Cargo.lock ./
ADD src src

RUN cargo build --release

FROM public.ecr.aws/docker/library/debian:bullseye-slim

ENV TOKEN=
ENV BOT_NICKNAME=
ENV RUNNER_IPV4=

WORKDIR /app

COPY --from=build /app/target/release ./

CMD ["/app/rustbot"]
