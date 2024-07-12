FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY NETCoreBot NETCoreBot
COPY NETCoreBot.sln ./

RUN dotnet restore
RUN dotnet publish NETCoreBot --configuration Debug --output ./publish
RUN find ./publish

FROM mcr.microsoft.com/dotnet/runtime:8.0

WORKDIR /app

# The directory of the built code to copy into this image, to be able to run the bot.
COPY --from=build /app/publish/ .

# The entrypoint to run the bot
CMD ["dotnet", "NETCoreBot.dll"]
