FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Menggunakan tanda bintang agar otomatis mencari file .csproj apa pun
COPY ["*.csproj", "."]
RUN dotnet restore

COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# SAMAKAN dengan nama hasil compile .dll kamu (misal: AbsensiApi.dll)
ENTRYPOINT ["dotnet", "AbsensiApi.dll"]
