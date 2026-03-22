FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/FlowFi.API/FlowFi.API.csproj",            "src/FlowFi.API/"]
COPY ["src/FlowFi.Application/FlowFi.Application.csproj", "src/FlowFi.Application/"]
COPY ["src/FlowFi.Domain/FlowFi.Domain.csproj",       "src/FlowFi.Domain/"]
COPY ["src/FlowFi.Infrastructure/FlowFi.Infrastructure.csproj", "src/FlowFi.Infrastructure/"]

RUN dotnet restore "src/FlowFi.API/FlowFi.API.csproj"

COPY . .
WORKDIR "/src/src/FlowFi.API"
RUN dotnet build "FlowFi.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FlowFi.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FlowFi.API.dll"]
