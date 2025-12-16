FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
WORKDIR /app
EXPOSE 8080

# RUN apk add --no-cache curl tar && \
#     curl -SL "https://download.newrelic.com/dot_net_agent/latest_release/newrelic-dotnet-agent_10.30.0_amd64.tar.gz" \
#     -o /tmp/newrelic-agent.tar.gz && \
#     mkdir -p /usr/local/newrelic-dotnet-agent && \
#     tar -xzf /tmp/newrelic-agent.tar.gz -C /usr/local/newrelic-dotnet-agent && \
#     rm /tmp/newrelic-agent.tar.gz && \
#     apk del curl tar

# ENV CORECLR_ENABLE_PROFILING=1 \
#     CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A} \
#     CORECLR_NEWRELIC_HOME=/usr/local/newrelic-dotnet-agent \
#     CORECLR_PROFILER_PATH=/usr/local/newrelic-dotnet-agent/libNewRelicProfiler.so \
#     NEW_RELIC_DISTRIBUTED_TRACING_ENABLED=true \
#     NEW_RELIC_LOG_LEVEL=info

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
COPY . .

RUN apt-get update && apt-get install -y wget ca-certificates gnupg \
&& echo 'deb http://apt.newrelic.com/debian/ newrelic non-free' | tee /etc/apt/sources.list.d/newrelic.list \
&& wget https://download.newrelic.com/548C16BF.gpg \
&& apt-key add 548C16BF.gpg \
&& apt-get update \
&& apt-get install -y 'newrelic-dotnet-agent' \
&& rm -rf /var/lib/apt/lists/*

# Enable the agent
ENV CORECLR_ENABLE_PROFILING=1 \
CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A} \
CORECLR_NEWRELIC_HOME=/usr/local/newrelic-dotnet-agent \
CORECLR_PROFILER_PATH=/usr/local/newrelic-dotnet-agent/libNewRelicProfiler.so \
NEW_RELIC_DISTRIBUTED_TRACING_ENABLED=true \
NEW_RELIC_LOG_LEVEL=info

RUN dotnet build "src/Fiap.Soat.SmartMechanicalWorkshop.Api/Fiap.Soat.SmartMechanicalWorkshop.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Fiap.Soat.SmartMechanicalWorkshop.Api/Fiap.Soat.SmartMechanicalWorkshop.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create directory for Data Protection keys
RUN mkdir -p /app/keys && chmod 755 /app/keys

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Fiap.Soat.SmartMechanicalWorkshop.Api.dll"]
