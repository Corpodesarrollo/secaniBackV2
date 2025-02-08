
# Imagen base para la ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Imagen base para la construcción
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia el archivo nuget.config a la ubicación predeterminada de NuGet
COPY ["nuget.config", "/root/.nuget/NuGet/nuget.config"]

# Instalar el proveedor de credenciales de NuGet
WORKDIR /tools
RUN curl -L https://raw.githubusercontent.com/Microsoft/artifacts-credprovider/master/helpers/installcredprovider.sh | sh

# Configurar acceso al feed privado de NuGet
ARG FEED_ACCESSTOKEN
ENV VSS_NUGET_EXTERNAL_FEED_ENDPOINTS="{\"endpointCredentials\": [{\"endpoint\":\"https://saturno.minsalud.gov.co/tfs/DefaultCollection/_packaging/SISPRO.TRV/nuget/v3/index.json\", \"username\":\"docker\", \"password\":\"${FEED_ACCESSTOKEN}\"}]}"

# Copia los archivos de proyecto
COPY ["Microservicios/MSAuthentication/MSAuthentication.Api.csproj", "Microservicios/MSAuthentication/"]
COPY ["Core/Core.csproj", "Core/"]
COPY ["Infra/Infra.csproj", "Infra/"]

# Restaurar los paquetes de NuGet
RUN dotnet restore "Microservicios/MSAuthentication/MSAuthentication.Api.csproj" --configfile /root/.nuget/NuGet/nuget.config --disable-parallel --ignore-failed-sources --force --no-cache

# Copia el resto del código y construye
COPY . .
WORKDIR "/src/Microservicios/MSAuthentication"
RUN dotnet build "MSAuthentication.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publicar la aplicación
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MSAuthentication.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Imagen final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MSAuthentication.Api.dll"]
