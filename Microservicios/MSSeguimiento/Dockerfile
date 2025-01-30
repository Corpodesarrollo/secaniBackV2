#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

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


COPY ["Microservicios/MSSeguimiento/MSSeguimiento.Api.csproj", "Microservicios/MSSeguimiento/"]
COPY ["Core/Core.csproj", "Core/"]
COPY ["Infra/Infra.csproj", "Infra/"]
RUN dotnet restore "./Microservicios/MSSeguimiento/MSSeguimiento.Api.csproj" --configfile /root/.nuget/NuGet/nuget.config --disable-parallel --ignore-failed-sources --force --no-cache

COPY . .
WORKDIR "/src/Microservicios/MSSeguimiento"
RUN dotnet build "./MSSeguimiento.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MSSeguimiento.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MSSeguimiento.Api.dll"]