# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia i file csproj prima del resto del sorgente per migliorare la cache del restore.
COPY ["Atlas.Backend.WebApi/Atlas.Backend.WebApi.csproj", "Atlas.Backend.WebApi/"]
COPY ["Atlas.Backend.Application/Atlas.Backend.Application.csproj", "Atlas.Backend.Application/"]
COPY ["Atlas.Backend.Infrastructure/Atlas.Backend.Infrastructure.csproj", "Atlas.Backend.Infrastructure/"]
COPY ["Atlas.Backend.Core/Atlas.Backend.Core.csproj", "Atlas.Backend.Core/"]

# Ripristina tutte le dipendenze NuGet partendo dal progetto WebApi.
RUN dotnet restore "Atlas.Backend.WebApi/Atlas.Backend.WebApi.csproj"

COPY . .
WORKDIR "/src/Atlas.Backend.WebApi"
# Produce output pubblicabile senza apphost nativo per immagine più essenziale.
RUN dotnet publish "Atlas.Backend.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copia soltanto i binari pubblicati dallo stage di build.
COPY --from=build /app/publish .

#Questo comando è informativo, non è qui che si espone effettivamente la porta ma nel docker-compose.yml
EXPOSE 8080

ENTRYPOINT ["dotnet", "Atlas.Backend.WebApi.dll"]