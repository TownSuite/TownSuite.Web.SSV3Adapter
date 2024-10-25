#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TownSuite.Web.Example/TownSuite.Web.Example.csproj", "TownSuite.Web.Example/"]
COPY ["TownSuite.Web.SSV3Adapter/TownSuite.Web.SSV3Adapter.csproj", "TownSuite.Web.SSV3Adapter/"]
COPY ["TownSuite.Web.SSV3Adapter.Interfaces/TownSuite.Web.SSV3Adapter.Interfaces.csproj", "TownSuite.Web.SSV3Adapter.Interfaces/"]
COPY ["TownSuite.Web.SSV3Adapter.Prometheus/TownSuite.Web.SSV3Adapter.Prometheus.csproj", "TownSuite.Web.SSV3Adapter.Prometheus/"]
RUN dotnet restore "TownSuite.Web.Example/TownSuite.Web.Example.csproj"
COPY . .
WORKDIR "/src/TownSuite.Web.Example"
RUN dotnet build "TownSuite.Web.Example.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TownSuite.Web.Example.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TownSuite.Web.Example.dll"]
