FROM mcr.microsoft.com/azure-functions/dotnet:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["MicroBOffice/MicroBOffice.csproj", "MicroBOffice/"]
RUN dotnet restore "MicroBOffice/MicroBOffice.csproj"
COPY . .
WORKDIR "/src/MicroBOffice"
RUN dotnet build "MicroBOffice.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MicroBOffice.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV AzureWebJobsScriptRoot=/app