🩺 Proyecto de Monitorización Cardíaca con IA
Este proyecto consiste en un sistema para monitorizar posibles ataques cardíacos mediante el uso de una pulsera inteligente. Está compuesto por distintos módulos interconectados que procesan y visualizan los datos del paciente en tiempo real.

🔧 Componentes del sistema
Aplicación móvil: desarrollada en Dart, permite al paciente estar conectado con el sistema desde su dispositivo.

Aplicación web: desarrollada en Vue.js, pensada para que médicos o cuidadores puedan consultar la información.

Backend principal: desarrollado en C#, actúa como intermediario entre el frontend (móvil y web), la base de datos y la IA.

Servidor de IA: desarrollado en Python, procesa los datos de la pulsera y realiza predicciones o alertas basadas en modelos entrenados.

⚙️ Despliegue
Antes de desplegar el sistema completo, es necesario realizar las siguientes configuraciones:

URLs de conexión
Actualizar las URLs tanto del frontend (móvil y web) como del backend para que apunten a la ubicación final del servidor de IA en Python.

Firebase
El archivo de configuración necesario para que el backend (C#) se conecte a Firebase no está en el repositorio por motivos de seguridad. Este archivo debe añadirse manualmente y se proporcionará por separado.

Docker
En el archivo docker-compose.yml hay que actualizar las rutas de los proyectos (móvil, web, backend y servidor de IA) según su ubicación final en el entorno de despliegue.