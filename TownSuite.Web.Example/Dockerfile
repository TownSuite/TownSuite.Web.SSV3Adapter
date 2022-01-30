#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["TownSuite.Web.Example/TownSuite.Web.Example.csproj", "TownSuite.Web.Example/"]
COPY ["TownSuite.Web.SSV3Facade/TownSuite.Web.SSV3Facade.csproj", "TownSuite.Web.SSV3Facade/"]
COPY ["TownSuite.Web.SSV3Facade.Interfaces/TownSuite.Web.SSV3Facade.Interfaces.csproj", "TownSuite.Web.SSV3Facade.Interfaces/"]
COPY ["TownSuite.Web.SSV3Facade.Prometheus/TownSuite.Web.SSV3Facade.Prometheus.csproj", "TownSuite.Web.SSV3Facade.Prometheus/"]
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
