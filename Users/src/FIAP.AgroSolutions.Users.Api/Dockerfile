# Estágio 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar arquivos da solução e dos projetos
COPY ["FIAP.AgroSolutions.Users.Api/FIAP.AgroSolutions.Users.Api.csproj", "FIAP.AgroSolutions.Users.Api/"]
COPY ["FIAP.AgroSolutions.Users.Application/FIAP.AgroSolutions.Users.Application.csproj", "FIAP.AgroSolutions.Users.Application/"]
COPY ["FIAP.AgroSolutions.Users.Domain/FIAP.AgroSolutions.Users.Domain.csproj", "FIAP.AgroSolutions.Users.Domain/"]
COPY ["FIAP.AgroSolutions.Users.Infrastructure/FIAP.AgroSolutions.Users.Infrastructure.csproj", "FIAP.AgroSolutions.Users.Infrastructure/"]

# Restaurar dependências
RUN dotnet restore "FIAP.AgroSolutions.Users.Api/FIAP.AgroSolutions.Users.Api.csproj"

# Copiar todos os arquivos fonte
COPY . .

# Compilar e publicar
WORKDIR "/src/FIAP.AgroSolutions.Users.Api"
RUN dotnet build "FIAP.AgroSolutions.Users.Api.csproj" -c Release -o /app/build
RUN dotnet publish "FIAP.AgroSolutions.Users.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Instalar wget para verificações de saúde
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*

# Criar usuário não-root por segurança
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copiar aplicação publicada
COPY --from=build /app/publish .

# Definir propriedade
RUN chown -R appuser:appuser /app

# Trocar para usuário não-root
USER appuser

# Expor porta
EXPOSE 8080
EXPOSE 8081

# Definir variáveis de ambiente
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Verificação de saúde
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

# Ponto de entrada
ENTRYPOINT ["dotnet", "FIAP.AgroSolutions.Users.Api.dll"]

