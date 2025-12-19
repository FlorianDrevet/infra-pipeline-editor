FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["InfraFlowSculptor.Api/InfraFlowSculptor.Api.csproj", "InfraFlowSculptor.Api/"]
COPY ["InfraFlowSculptor.Application/VPD.Application.csproj", "InfraFlowSculptor.Application/"]
COPY ["InfraFlowSculptor.Domain/InfraFlowSculptor.Domain.csproj", "InfraFlowSculptor.Domain/"]
COPY ["InfraFlowSculptor.Contracts/InfraFlowSculptor.Contracts.csproj", "InfraFlowSculptor.Contracts/"]
COPY ["InfraFlowSculptor.Infrastructure/InfraFlowSculptor.Infrastructure.csproj", "InfraFlowSculptor.Infrastructure/"]
RUN dotnet restore "InfraFlowSculptor.Api/InfraFlowSculptor.Api.csproj"
COPY . .
WORKDIR "/src/InfraFlowSculptor.Api"
RUN dotnet build "InfraFlowSculptor.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "InfraFlowSculptor.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InfraFlowSculptor.Api.dll"]
