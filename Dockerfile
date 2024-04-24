# Prevent .NET CLI from sending telemetry data
ARG DOTNET_CLI_TELEMETRY_OPTOUT=1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the default value for the CONFIGURATION argument
ARG CONFIGURATION=Release

WORKDIR /app

COPY Domain ./Domain
COPY Logger ./Logger
COPY Runner ./Runner
COPY Sproutopia ./Sproutopia

RUN dotnet restore Sproutopia/
RUN dotnet publish Sproutopia/Sproutopia.csproj --configuration $CONFIGURATION --output ./publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

ENV LOG_DIR /var/log/sproutopia
RUN mkdir -p /var/log/sproutopia

WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5000

CMD ["dotnet", "Sproutopia.dll"]