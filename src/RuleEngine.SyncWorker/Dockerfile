FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/RuleEngine.SyncWorker/RuleEngine.SyncWorker.csproj", "src/RuleEngine.SyncWorker/"]
RUN dotnet restore "src/RuleEngine.SyncWorker/RuleEngine.SyncWorker.csproj"
COPY . .
WORKDIR "/src/src/RuleEngine.SyncWorker"
RUN dotnet build "./RuleEngine.SyncWorker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RuleEngine.SyncWorker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RuleEngine.SyncWorker.dll"]
