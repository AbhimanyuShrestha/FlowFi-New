# ── Stage 1: Base runtime image ───────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
RUN apt-get update && apt-get install -y curl --no-install-recommends && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8080

# ── Stage 2: Build ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files first for layer caching — restores only re-run when deps change
COPY ["src/FlowFi.API/FlowFi.API.csproj",                   "src/FlowFi.API/"]
COPY ["src/FlowFi.Application/FlowFi.Application.csproj",   "src/FlowFi.Application/"]
COPY ["src/FlowFi.Domain/FlowFi.Domain.csproj",             "src/FlowFi.Domain/"]
COPY ["src/FlowFi.Infrastructure/FlowFi.Infrastructure.csproj", "src/FlowFi.Infrastructure/"]

RUN dotnet restore "src/FlowFi.API/FlowFi.API.csproj"

# Copy source (tests excluded via .dockerignore)
COPY . .

WORKDIR "/src/src/FlowFi.API"
RUN dotnet build "FlowFi.API.csproj" -c Release -o /app/build

# ── Stage 3: Publish ───────────────────────────────────────────
FROM build AS publish
RUN dotnet publish "FlowFi.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ── Stage 4: Final image ───────────────────────────────────────
FROM base AS final
WORKDIR /app

# Non-root user for security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=publish /app/publish .

# Health check so docker-compose waits for the API to be ready
HEALTHCHECK --interval=10s --timeout=5s --start-period=15s --retries=3 \
  CMD curl -f http://localhost:8080/api/v1/health || exit 1

ENTRYPOINT ["dotnet", "FlowFi.API.dll"]
