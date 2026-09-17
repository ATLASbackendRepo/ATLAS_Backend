# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ATLAS/ATLAS.csproj", "ATLAS/"]
COPY ["ATLAsBLL/ATLAsBLL.csproj", "ATLAsBLL/"]
COPY ["ATLAsDCL/ATLAsDCL.csproj", "ATLAsDCL/"]
COPY ["ATLASDomain/ATLASDomain.csproj", "ATLASDomain/"]
RUN dotnet restore "./ATLAS/ATLAS.csproj"
COPY . .
WORKDIR "/src/ATLAS"
RUN dotnet build "./ATLAS.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ATLAS.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final

# curl is required by the docker-compose healthcheck against /readyz.
USER root
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

# Run as a non-root user. APP_UID is provided as a build arg by the
# Visual Studio Docker tooling; the explicit default keeps bare `docker build` working.
ARG APP_UID=1654
USER ${APP_UID}

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ATLAS.dll"]