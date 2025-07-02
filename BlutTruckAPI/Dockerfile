# 1) Build stage: compila tu solución
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos el .sln y el csproj a un lugar temporario
COPY ["BlutTruck.sln", "./"]
COPY ["BlutTruck/BlutTruck.csproj", "BlutTruck/"]

# Restauramos paquetes
RUN dotnet restore "BlutTruck.sln"

# Copiamos todo el código y publicamos en Release
COPY . .
RUN dotnet publish "BlutTruck.sln" -c Release -o /app/publish

# 2) Runtime stage: empaquetamos solo lo publicado
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 3000
ENTRYPOINT ["dotnet", "BlutTruck.dll"]
