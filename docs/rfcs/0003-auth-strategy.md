# RFC 0003 — Estratégia de Autenticação

Status: Accepted
Owner: Backend / Segurança

Resumo

Estratégia de autenticação: **JWT Bearer tokens** com ASP.NET Core Identity/Provider.

Motivação / Justificativa

- Arquitetura da API é RESTful e precisa ser stateless para escalabilidade.
- JWT permite distribuição simples do token entre clientes (web, mobile) e serviços.
- Integração nativa com `Microsoft.AspNetCore.Authentication.JwtBearer` facilita implementação.

Alternativas consideradas

- Cookies de autenticação (mais adequado para aplicações web tradicionais).
- OAuth2 / OpenID Connect com provedor externo (ex: Cognito, Auth0) — opção futura para SSO.

Considerações de Segurança

- Tokens devem ser curtos (exp) e usar refresh tokens para manter sessão sem expor credenciais.
- Armazenar secret keys em secrets manager (AWS Secrets Manager / GitHub Secrets para CI).
- Implementar rotação de chaves e validação de claims/roles.

Recomendações

- Implementar refresh tokens e revogação (blacklist) se necessário.
- Em produção avaliar uso de um provedor OIDC (Cognito) se for necessário SSO/SSO corporativo.
