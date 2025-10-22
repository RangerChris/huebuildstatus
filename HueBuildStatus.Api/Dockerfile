FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["HueBuildStatus.Api/HueBuildStatus.Api.csproj", "HueBuildStatus.Api/"]
COPY ["HueBuildStatus.Core/HueBuildStatus.Core.csproj", "HueBuildStatus.Core/"]
RUN dotnet restore "HueBuildStatus.Api/HueBuildStatus.Api.csproj"
COPY . .
WORKDIR "/src/HueBuildStatus.Api"
RUN dotnet build "./HueBuildStatus.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./HueBuildStatus.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
RUN rm -f /app/publish/appsettings*.json

FROM base AS final
USER root
WORKDIR /app
COPY --from=publish /app/publish .
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
USER $APP_UID
HEALTHCHECK CMD curl --fail http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "HueBuildStatus.Api.dll"]