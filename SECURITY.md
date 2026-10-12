# Política de seguridad

## Reportar una vulnerabilidad

Si encuentras una vulnerabilidad en este proyecto, **no abras un issue público**.
Usa [Private vulnerability reporting](../../security/advisories/new) de GitHub y
describe el problema, los pasos para reproducirlo y su impacto. Responderé lo antes posible.

## Gestión de secretos

Este repositorio no contiene credenciales. La configuración sensible se inyecta
por variables de entorno (ver `.env.example`). Los controles automáticos
(gitleaks, CodeQL, Dependabot y Trivy) se ejecutan en cada push y pull request.
