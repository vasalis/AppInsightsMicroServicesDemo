FROM mcr.microsoft.com/azure-functions/dotnet:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["MicroDevices/MicroDevices.csproj", "MicroDevices/"]
RUN dotnet restore "MicroDevices/MicroDevices.csproj"
COPY . .
WORKDIR "/src/MicroDevices"
RUN dotnet build "MicroDevices.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MicroDevices.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV AzureWebJobsScriptRoot=/app