# RFC 0002 — Escolha do Banco de Dados

Status: Accepted
Owner: Arquitetura / Backend

Resumo

Escolha do banco relacional: **MySQL (RDS / Self-hosted MySQL via Docker)**.

Motivação / Justificativa

- Projeto já usa `Pomelo.EntityFrameworkCore.MySql` e exemplos de configuração para MySQL.
- MySQL é amplamente suportado, leve e suficiente para o domínio do sistema (ordens de serviço, pessoas, estoques).
- Suporte a migrações via Entity Framework Core facilita deploys.

Alternativas consideradas

- PostgreSQL — robusto, melhor suporte a tipos e extensões; trade-offs mínimos neste escopo.
- NoSQL (ex: MongoDB) — não indicado para o modelo relacional e integridade necessária.

Impactos

- Migrations com EF Core 8 devem ser testadas em staging antes de produção.
- Configuração de backups, performance tuning e sizing para RDS.

Recomendações

- Em AWS, usar RDS for MySQL com multi-AZ e backups automáticos.
- Em desenvolvimento, usar container MySQL (docker/mysql) já presente no repositório.
