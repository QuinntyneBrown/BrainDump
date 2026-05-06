# Multi-stage build for BrainDump.Api.
# Build context is the repository root (see docker-compose.yml) so that the
# Domain/Application/Infrastructure project references resolve.

ARG DOTNET_VERSION=9.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

# Copy only the csproj files first to maximize Docker layer cache hits on
# subsequent builds when only source files change.
COPY backend/BrainDump.sln ./backend/
COPY backend/src/BrainDump.Domain/BrainDump.Domain.csproj          ./backend/src/BrainDump.Domain/
COPY backend/src/BrainDump.Application/BrainDump.Application.csproj ./backend/src/BrainDump.Application/
COPY backend/src/BrainDump.Infrastructure/BrainDump.Infrastructure.csproj ./backend/src/BrainDump.Infrastructure/
COPY backend/src/BrainDump.Api/BrainDump.Api.csproj                ./backend/src/BrainDump.Api/

RUN dotnet restore backend/src/BrainDump.Api/BrainDump.Api.csproj

# Now copy the source and publish.
COPY backend/src ./backend/src
RUN dotnet publish backend/src/BrainDump.Api/BrainDump.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS runtime
WORKDIR /app

# curl is used by the docker-compose healthcheck (GET /healthz). Installed
# while still root, before dropping privileges below.
RUN apt-get update \
 && apt-get install -y --no-install-recommends curl \
 && rm -rf /var/lib/apt/lists/*

# Run as the non-root `app` user that ships with the aspnet image.
USER app

# Kestrel inside the container listens on 8080; docker-compose maps host:5153 → container:8080.
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BrainDump.Api.dll"]
