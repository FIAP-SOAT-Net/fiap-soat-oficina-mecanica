# 🚀 Quick Start - Load Test Suite
## ✅ Compilação Bem-Sucedida!
O projeto foi configurado com sucesso e está pronto para uso.
## 📋 Estrutura do Projeto
```
Fiap.Soat.SmartMechanicalWorkshop.Loadtest/
├── Config/
│   └── LoadTestConfig.cs           # Configurações do teste
├── Helpers/
│   ├── ApiClient.cs                # Cliente HTTP com autenticação
│   ├── DataProvider.cs             # Busca dados existentes da API
│   └── ServiceOrderDataGenerator.cs # Gera dados fake realistas
├── Models/
│   └── ApiModels.cs                # DTOs da API
├── Scenarios/
│   ├── ServiceOrderLifecycleScenario.cs  # Fluxo completo de Service Orders
│   └── ReadOperationsScenario.cs         # Operações de leitura (GETs)
├── Program.cs                      # Orquestrador principal
├── appsettings.json                # Configurações (editar aqui!)
└── README.md                       # Documentação completa
```
## ⚙️ Configuração Rápida
**Edite o arquivo `appsettings.json`:**
```json
{
  "LoadTest": {
    "ApiBaseUrl": "http://k8s-smartwor-apiservi-29c7b6c8ec-d1c38ef3146a61cd.elb.us-west-2.amazonaws.com:5180",
    "LoginEmail": "joao.silva@email.com",
    "LoginPassword": "Pa$$w0rd!",
    "TestDuration": "00:05:00",     // Duração do teste
    "VirtualUsers": 5,               // Usuários concorrentes
    "RampUpSeconds": 30,             // Tempo de ramp-up
    "ServiceOrdersToCreate": 20      // (não usado atualmente)
  }
}
```
## 🏃 Como Executar
### 1. Teste Rápido (1 minuto, 2 usuários)
```bash
# Edite appsettings.json com:
# "TestDuration": "00:01:00"
# "VirtualUsers": 2
dotnet run
```
### 2. Teste Normal (5 minutos, 5 usuários)
```bash
# Configuração padrão do appsettings.json
dotnet run
```
### 3. Teste Prolongado (10 minutos, 10 usuários)
```bash
# Edite appsettings.json com:
# "TestDuration": "00:10:00"
# "VirtualUsers": 10
dotnet run
```
## 📊 O que o Teste Faz
### Cenário 1: Service Order Lifecycle (Principal)
Cria Service Orders e executa o fluxo completo de status:
1. **Received** (criação)
2. **UnderDiagnosis** 
3. **WaitingApproval**
4. **InProgress**
5. **Completed**
6. **Delivered** (final)
### Cenário 2: Read Operations (Secundário)
Executa GETs aleatórios nos endpoints:
- `/api/v1/people`
- `/api/v1/vehicles`
- `/api/v1/availableservices`
- `/api/v1/serviceorders`
## 📈 Relatórios
Após a execução, os relatórios são gerados em:
```
./reports/
├── load_test_report.html    # Relatório visual completo
├── load_test_report.md      # Formato Markdown
└── load_test_report.txt     # Formato texto simples
```
## 🔍 Visualização no NewRelic
Após executar os testes, acesse o NewRelic e verifique:
1. **APM → Transactions**: Veja a latência e throughput por endpoint
2. **APM → Distributed Tracing**: Visualize o fluxo completo de cada ordem
3. **APM → Databases**: Queries executadas e performance
4. **Infrastructure → Kubernetes**: CPU/Memory dos pods
## ⚠️ Recomendações para Free Tier
Para **evitar custos** no free tier da AWS:
- ✅ Use no máximo 10 usuários virtuais
- ✅ Limite a duração para 5-10 minutos
- ✅ Execute os testes em horários de baixo uso
- ✅ Monitore o dashboard da AWS durante a execução
## 🛠️ Troubleshooting
### Problema: "Authentication failed"
**Solução**: Verifique email/senha no `appsettings.json`
### Problema: "No data found in database"
**Solução**: A API precisa ter clientes, veículos e serviços cadastrados
### Problema: Alta taxa de erro
**Solução**: 
- Reduza `VirtualUsers` para 3-5
- Aumente `RampUpSeconds` para 60
- Verifique se o EKS não está com recursos limitados
### Problema: Conexão timeout
**Solução**: 
- Verifique conectividade com o EKS
- Teste manualmente com: `curl http://k8s-smartwor-apiservi-...elb.../api/v1/people`
## 📚 Arquitetura Técnica
**Frameworks utilizados:**
- **NBomber 6.1.0**: Framework de load testing para .NET
- **Bogus 35.6.1**: Geração de dados fake realistas
- **.NET 9.0**: Runtime
**Padrões implementados:**
- ✅ Clean Architecture (separação de concerns)
- ✅ Dependency Injection
- ✅ Configuration Pattern
- ✅ Data Provider Pattern (reutiliza dados existentes)
## 🎯 Objetivos Alcançados
✅ Gera massa de dados realistas para o NewRelic
✅ Valida o fluxo completo de Service Orders
✅ Testa operações de leitura em múltiplos endpoints
✅ Otimizado para ambientes com recursos limitados (Free Tier)
✅ Relatórios detalhados em múltiplos formatos
---
**Pronto para executar/Users/igortessaro/Documents/repos/fiap/fiap-soat-oficina-mecanica/tests/Fiap.Soat.SmartMechanicalWorkshop.Loadtest && dotnet build* 🚀
Execute `dotnet run` e acompanhe os logs e relatórios gerados.
