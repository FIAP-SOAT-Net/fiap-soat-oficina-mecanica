# Instrumentação New Relic APM - Quick Start

## ✅ Implementação Concluída

A API está **totalmente instrumentada** para observabilidade com New Relic APM.

## 📦 Pacotes Adicionados

- ✅ `NewRelic.Agent.Api` - API para custom events e métricas
- ✅ `Serilog.AspNetCore` - Logs estruturados
- ✅ `Serilog.Formatting.Compact` - Formato JSON para logs
- ✅ `AspNetCore.HealthChecks.UI.Client` - Health checks detalhados

## 🎯 Funcionalidades

### 1. Custom Events de Negócio
Eventos registrados automaticamente:
- ✅ **ServiceOrder Created** - Quando uma ordem é criada
- ✅ **ServiceOrder Updated** - Quando o status muda
- ✅ **ServiceOrder Modified** - Quando dados são alterados

Atributos capturados:
- `orderId`, `customerId`, `status`
- `duration` (tempo de execução)
- `vehicleId`, `servicesCount`
- `previousStatus`, `newStatus`

### 2. Logs Estruturados (JSON)
Todos os logs são em formato Compact JSON com:
- ✅ Timestamp ISO 8601
- ✅ Level (Information, Warning, Error)
- ✅ Correlation ID automático
- ✅ Request path e method
- ✅ Context enriquecido

### 3. Distributed Tracing
- ✅ Suporte a headers `traceparent` (W3C)
- ✅ Suporte a header `newrelic`
- ✅ Propagação automática de contexto

### 4. Health Check Detalhado
Endpoint `/health` retorna:
- ✅ Status geral da API
- ✅ Status do banco de dados
- ✅ Uso de memória
- ✅ Estatísticas de GC
- ✅ Versão e uptime

### 5. Graceful Degradation
- ✅ API funciona normalmente sem New Relic
- ✅ Erros de instrumentação são logados mas não quebram a aplicação
- ✅ Controle via configuração `NewRelic:Enabled`

## 🚀 Deploy no Kubernetes

### 1. Criar Secret com License Key

```bash
kubectl create secret generic newrelic-secret \
  --from-literal=license-key='YOUR_LICENSE_KEY_HERE' \
  --namespace=default
```

### 2. Aplicar Deployment

```bash
kubectl apply -f k8s-deployment-with-newrelic.yaml
```

O arquivo `k8s-deployment-with-newrelic.yaml` já está configurado com:
- Init container para copiar o agent
- Volume compartilhado entre containers
- Variáveis de ambiente necessárias
- Health checks e auto-scaling

## ⚙️ Configuração Local (Desenvolvimento)

1. **Desabilitar instrumentação custom** (opcional):
```json
{
  "NewRelic": {
    "Enabled": false
  }
}
```

2. **Logs continuarão em JSON estruturado** mesmo com New Relic desabilitado

3. **Rodar a aplicação**:
```bash
dotnet run --project src/Fiap.Soat.SmartMechanicalWorkshop.Api
```

## 📊 Dashboards e Queries

Ver arquivo `OBSERVABILITY.md` para:
- Queries NRQL prontas
- Configuração de dashboards
- Alertas recomendados
- Troubleshooting

## 🔍 Verificação

### Teste os Logs JSON
```bash
dotnet run --project src/Fiap.Soat.SmartMechanicalWorkshop.Api | head -5
```

Deve retornar logs em formato JSON:
```json
{"@t":"2025-12-02T10:30:00Z","@l":"Information","@m":"Application started"}
```

### Teste o Health Check
```bash
curl http://localhost:5180/health
```

Deve retornar JSON detalhado com status Healthy.

### Teste Custom Events (requer New Relic habilitado)
1. Crie uma ServiceOrder via API
2. Verifique no New Relic: `SELECT * FROM ServiceOrder SINCE 1 hour ago`

## 📁 Arquivos Criados/Modificados

### Novos Arquivos
- `OBSERVABILITY.md` - Documentação completa
- `.env.example` - Exemplo de variáveis de ambiente
- `k8s-deployment-with-newrelic.yaml` - Deployment do Kubernetes
- `src/.../Services/NewRelicInstrumentationService.cs` - Serviço de instrumentação
- `src/.../HealthChecks/DetailedHealthCheck.cs` - Health check detalhado
- `src/.../Middlewares/RequestLoggingEnrichmentMiddleware.cs` - Middleware de logs

### Arquivos Modificados
- `appsettings.json` - Configuração de logs JSON e New Relic
- `Program.cs` - Registro de health checks e middleware
- `*.csproj` - Adição de pacotes NuGet
- Handlers de ServiceOrder - Instrumentados com custom events

## 🎓 Próximos Passos

1. **Obter License Key**: https://one.newrelic.com/admin-portal/api-keys/home
2. **Deploy no Kubernetes**: Seguir instruções acima
3. **Criar Dashboards**: Usar queries do `OBSERVABILITY.md`
4. **Configurar Alertas**: Error rate, latência, uptime

## 📚 Documentação Completa

Consulte `OBSERVABILITY.md` para:
- Arquitetura detalhada
- Exemplos de queries NRQL
- Configuração de dashboards
- Troubleshooting avançado

---

**Nota**: Esta implementação está em conformidade com as melhores práticas New Relic para .NET e Kubernetes (padrão sidecar).
