FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
# Mirror repository root inside /repo so Directory.Build.props and
# Directory.Packages.props are found by all projects (including
# src/Shared/InfraFlowSculptor.WebDefaults) when searching upward.
WORKDIR /repo
COPY Directory.Build.props .
COPY Directory.Packages.props .
COPY NuGet.config .
COPY .editorconfig .
# Copy .csproj files first to maximise layer-cache reuse for dotnet restore.
COPY ["src/Api/InfraFlowSculptor.Api/InfraFlowSculptor.Api.csproj", "src/Api/InfraFlowSculptor.Api/"]
COPY ["src/Api/InfraFlowSculptor.Application/InfraFlowSculptor.Application.csproj", "src/Api/InfraFlowSculptor.Application/"]
COPY ["src/Api/InfraFlowSculptor.Domain/InfraFlowSculptor.Domain.csproj", "src/Api/InfraFlowSculptor.Domain/"]
COPY ["src/Api/InfraFlowSculptor.Contracts/InfraFlowSculptor.Contracts.csproj", "src/Api/InfraFlowSculptor.Contracts/"]
COPY ["src/Api/InfraFlowSculptor.Infrastructure/InfraFlowSculptor.Infrastructure.csproj", "src/Api/InfraFlowSculptor.Infrastructure/"]
COPY ["src/Api/InfraFlowSculptor.BicepGeneration/InfraFlowSculptor.BicepGeneration.csproj", "src/Api/InfraFlowSculptor.BicepGeneration/"]
COPY ["src/Api/InfraFlowSculptor.GenerationCore/InfraFlowSculptor.GenerationCore.csproj", "src/Api/InfraFlowSculptor.GenerationCore/"]
COPY ["src/Api/InfraFlowSculptor.PipelineGeneration/InfraFlowSculptor.PipelineGeneration.csproj", "src/Api/InfraFlowSculptor.PipelineGeneration/"]
COPY ["src/Shared/InfraFlowSculptor.WebDefaults/InfraFlowSculptor.WebDefaults.csproj", "src/Shared/InfraFlowSculptor.WebDefaults/"]
RUN dotnet restore "src/Api/InfraFlowSculptor.Api/InfraFlowSculptor.Api.csproj"
COPY src/Api/ src/Api/
COPY src/Shared/InfraFlowSculptor.WebDefaults/ src/Shared/InfraFlowSculptor.WebDefaults/
WORKDIR "/repo/src/Api/InfraFlowSculptor.Api"
RUN dotnet build "InfraFlowSculptor.Api.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "InfraFlowSculptor.Api.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "InfraFlowSculptor.Api.dll"]
