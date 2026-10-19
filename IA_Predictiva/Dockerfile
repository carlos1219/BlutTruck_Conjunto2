# 1) Imagen base Python
FROM python:3.14-slim

# 2) Directorio de trabajo
WORKDIR /app

# 3) Copiamos e instalamos dependencias
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# 4) Copiamos el código y modelos
COPY . .

# 5) Exponemos el puerto (si tu script levanta un servidor, p.ej. Flask en 5000)
#    si tu script es solo procesamiento por CLI, no necesitas EXPOSE ni CMD de servicio web
EXPOSE 5000

# 6) Ejecutar sin privilegios de root
RUN useradd --create-home --uid 10001 appuser
USER appuser

# 7) Comando por defecto
CMD ["python", "ataque.py"]
