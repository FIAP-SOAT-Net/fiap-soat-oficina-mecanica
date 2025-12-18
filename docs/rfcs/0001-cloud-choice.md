# RFC 0001 — Escolha da Nuvem

Status: Accepted
Owner: Arquitetura / DevOps

Resumo

Escolha da nuvem para hospedagem da aplicação: **AWS (EKS + RDS)**.

Motivação / Justificativa

- O repositório já contém infra-as-code e referências a Terraform e EKS.
- AWS oferece serviços gerenciados (EKS, RDS, ELB, ACM) que reduzem esforço operacional.
- Boa integração com CI/CD (GitHub Actions), monitoramento e auto-scaling.

Alternativas consideradas

- Google Cloud (GKE) — similar, custo/integração OK, mas equipe já possui templates Terraform para AWS.
- Azure (AKS) — compatível, mas menos prioridade para infraestrutura existente.

Impactos

- Uso de RDS para banco relacional (MySQL).
- Provisionamento via Terraform (terraform/README.md).
- Necessidade de configurar secrets e IAM roles para CI/CD.

Recomendações

- Manter Terraform como fonte de verdade para infra.
- Usar EKS para workloads, RDS para banco, e CloudWatch/NewRelic para monitoramento.
