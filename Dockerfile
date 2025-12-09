FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
WORKDIR /app
EXPOSE 8080

# Instalar New Relic .NET Agent
ENV NEWRELIC_AGENT_VERSION=10.30.0
RUN apk add --no-cache curl unzip && \
    curl -SL "https://download.newrelic.com/dot_net_agent/latest_release/newrelic-dotnet-agent_${NEWRELIC_AGENT_VERSION}_amd64.tar.gz" -o /tmp/newrelic-agent.tar.gz && \
    mkdir -p /usr/local/newrelic-dotnet-agent && \
    tar -xzf /tmp/newrelic-agent.tar.gz -C /usr/local/newrelic-dotnet-agent && \
    rm /tmp/newrelic-agent.tar.gz && \
    apk del curl unzip

ENV CORECLR_ENABLE_PROFILING=1
ENV CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A}
ENV CORECLR_NEWRELIC_HOME=/usr/local/newrelic-dotnet-agent
ENV CORECLR_PROFILER_PATH=/usr/local/newrelic-dotnet-agent/libNewRelicProfiler.so
ENV NEW_RELIC_DISTRIBUTED_TRACING_ENABLED=true
ENV NEW_RELIC_LOG_CONSOLE=1
ENV NEW_RELIC_LOG_LEVEL=info

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
COPY . .
RUN dotnet build "src/Fiap.Soat.SmartMechanicalWorkshop.Api/Fiap.Soat.SmartMechanicalWorkshop.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Fiap.Soat.SmartMechanicalWorkshop.Api/Fiap.Soat.SmartMechanicalWorkshop.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Fiap.Soat.SmartMechanicalWorkshop.Api.dll"]
