# Usar una imagen base de Node.js (elige una versión LTS estable)
FROM node:18-alpine

# Establecer el directorio de trabajo dentro del contenedor
WORKDIR /app

# Copiar package.json y package-lock.json
COPY package*.json ./

# Instalar dependencias (npm ci es generalmente más rápido y seguro para CI/CD,
# pero npm install también funciona)
RUN npm install

# Copiar el resto del código de la aplicación
# (Aunque se use un volumen, esto asegura que la imagen se pueda construir
# y potencialmente usar sin el volumen)
COPY . .

# Exponer el puerto por defecto de Vite
EXPOSE 5173

# Comando para iniciar el servidor de desarrollo de Vite
# '--host 0.0.0.0' es CRUCIAL para que sea accesible desde fuera del contenedor
CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0", "--port", "5173"]