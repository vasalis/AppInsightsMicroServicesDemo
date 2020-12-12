FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["MicroAlerts/MicroAlerts/MicroAlerts.csproj", "MicroAlerts/MicroAlerts/"]
RUN dotnet restore "MicroAlerts/MicroAlerts/MicroAlerts.csproj"
COPY . .
WORKDIR "/src/MicroAlerts/MicroAlerts"
RUN dotnet build "MicroAlerts.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MicroAlerts.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MicroAlerts.dll"]