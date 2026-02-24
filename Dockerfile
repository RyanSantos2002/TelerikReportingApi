# Imagem base para rodar o app (apenas o runtime do .NET 9)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
# O .NET 8 escuta por padrão na porta 8080 em containers
EXPOSE 8080 

# Imagem do SDK para compilar a aplicação
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia apenas o .csproj primeiro para restaurar os pacotes (aproveita cache do Docker)
COPY ["TelerikReportingApi.csproj", "."]
RUN dotnet restore "./TelerikReportingApi.csproj"

# Copia o resto dos arquivos e compila
COPY . .
WORKDIR "/src/."
RUN dotnet build "./TelerikReportingApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publica a aplicação
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TelerikReportingApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Gera a imagem final baseada na imagem de runtime (mais leve)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TelerikReportingApi.dll"]
