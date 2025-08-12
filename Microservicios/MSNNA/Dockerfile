#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app

EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG FEED_ACCESSTOKEN
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
RUN curl -L https://raw.githubusercontent.com/Microsoft/artifacts-credprovider/master/helpers/installcredprovider.sh | sh
# Copia el archivo nuget.config a la ubicación predeterminada de NuGet
COPY ["nuget.config", "/root/.nuget/NuGet/nuget.config"]

COPY ["Microservicios/MSNNA/MSNNA.Api.csproj", "Microservicios/MSNNA/"]
COPY ["Infra/Infra.csproj", "Infra/"]
COPY ["Core/Core.csproj", "Core/"]

ENV FEED_ACCESSTOKEN=${FEED_ACCESSTOKEN}
ENV VSS_NUGET_EXTERNAL_FEED_ENDPOINTS="{\"endpointCredentials\": [{\"endpoint\":\"https://saturno.minsalud.gov.co/tfs/DefaultCollection/_packaging/SISPRO.TRV/nuget/v3/index.json\", \"username\":\"docker\", \"password\":\"${FEED_ACCESSTOKEN}\"}]}"

RUN dotnet restore "./Microservicios/MSNNA/MSNNA.Api.csproj" --configfile /root/.nuget/NuGet/nuget.config --disable-parallel --ignore-failed-sources --force --no-cache

COPY . .
WORKDIR "/src/Microservicios/MSNNA"
RUN dotnet build "./MSNNA.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MSNNA.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MSNNA.Api.dll"]