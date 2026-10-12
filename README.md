# 🩺 BlutTruck: monitorización cardíaca con IA

Sistema (TFG) para detectar posibles ataques cardíacos a partir de los datos de una pulsera inteligente. Cuatro módulos conectados:

| Módulo | Tecnología | Carpeta |
|---|---|---|
| App móvil (paciente) | Dart | repositorio aparte |
| Panel web (médicos / cuidadores) | Vue.js + Vite | `blutTruck-Front/` |
| Backend principal | C# / .NET 8 | `BlutTruckAPI/` |
| Servidor de IA | Python (Flask, SVM de scikit-learn) | `IA_Predictiva/` |

El backend .NET hace de intermediario entre las apps, Firebase y el servidor de IA, que procesa los datos y genera predicciones y alertas.

## Seguridad (DevSecOps)

- Sin secretos en el repositorio: toda la configuración sensible va por variables de entorno (`.env.example`).
- GitHub Actions: CodeQL (C#, JavaScript y Python), gitleaks, Trivy sobre dependencias/configuración y sobre las tres imágenes Docker.
- Dependabot para NuGet, npm, pip, Docker y Actions.
- Contenedores ejecutados como usuario no root.
- Política de divulgación en [SECURITY.md](SECURITY.md).

## Despliegue

```bash
cp .env.example .env     # rellena los valores (Firebase, credenciales del backend)
docker compose up --build
```

Antes de desplegar en otro entorno, apunta las URLs del frontend y del backend al servidor de IA final (`VITE_API_BASE_URL` y `ApiSettings:PredictBaseUrl`).
