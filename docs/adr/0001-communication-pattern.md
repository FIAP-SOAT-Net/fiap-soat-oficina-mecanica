# ADR 0001 — Padrão de Comunicação

Status: Accepted
Decisão: Usar HTTP/REST síncrono para APIs públicas e comunicação assíncrona via message broker (event-driven) internamente.

Contexto

O sistema é uma API RESTful principal que expõe endpoints para clientes (web/mobile). Para operações que demandam desacoplamento (notificações, integrações com terceiros, processamento demorado), é desejável um fluxo assíncrono.

Alternativas consideradas

- Apenas REST síncrono: simples, mas acoplamento e latência para operações longas.
- gRPC para comunicações internas: alto desempenho, mas maior complexidade e necessidade de compatibilidade entre clientes.
- Message broker (Kafka/RabbitMQ) para eventos assíncronos: ideal para desacoplamento e reprocessamento.

Decisão

- Expor API pública via HTTP/REST (ASP.NET Core controllers).
- Internamente, usar um message broker para eventos (ex.: Kafka ou RabbitMQ conforme infra) para integrações assíncronas e processamento em background.

Motivação

- REST é mais simples para clientes externos e já alinhado com o projeto.
- Event-driven permite escalabilidade, tolerância a falhas e reprocessamento de eventos.
- Evita sobrecarregar APIs com tarefas de longa duração.

Consequências/Impactos

- Adicionar componentes de infraestrutura (broker) e observabilidade para eventos.
- Requer estratégia de garantia de entrega (at-least-once / exactly-once) conforme broker escolhido.
- Testes e mecanismos de retry/poison-queue necessários.

Notas

- Escolher o broker concreto (Kafka vs RabbitMQ) ficará em uma RFC/infra-issue dependendo do ambiente alvo.
