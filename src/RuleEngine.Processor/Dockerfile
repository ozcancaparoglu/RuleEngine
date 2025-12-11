FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/RuleEngine.Processor/RuleEngine.Processor.csproj", "src/RuleEngine.Processor/"]
COPY ["src/RuleEngine.Compiler/RuleEngine.Compiler.csproj", "src/RuleEngine.Compiler/"]
COPY ["src/RuleEngine.Core/RuleEngine.Core.csproj", "src/RuleEngine.Core/"]
COPY ["src/RuleEngine.Data/RuleEngine.Data.csproj", "src/RuleEngine.Data/"]
RUN dotnet restore "src/RuleEngine.Processor/RuleEngine.Processor.csproj"
COPY . .
WORKDIR "/src/src/RuleEngine.Processor"
RUN dotnet build "./RuleEngine.Processor.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RuleEngine.Processor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RuleEngine.Processor.dll"]
