# ─── Stage 1: Build ───────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY *.sln ./
COPY FIXIT.API/*.csproj ./FIXIT.API/
COPY FIXIT.Application/*.csproj ./FIXIT.Application/
COPY FIXIT.Infrastructure/*.csproj ./FIXIT.Infrastructure/
COPY FIXIT.Presentation/*.csproj ./FIXIT.Presentation/
COPY FIXIT.Domain/*.csproj ./FIXIT.Domain/

RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish FIXIT.API/FIXIT.API.csproj -c Release -o /out

# ─── Stage 2: Runtime ─────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /out .

EXPOSE 8080

ENTRYPOINT ["dotnet", "FIXIT.API.dll"]