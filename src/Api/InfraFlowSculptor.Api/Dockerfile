FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY Directory.Build.props .
COPY Directory.Packages.props .
COPY NuGet.config .
COPY ["src/Api/InfraFlowSculptor.Api/InfraFlowSculptor.Api.csproj", "InfraFlowSculptor.Api/"]
COPY ["src/Api/InfraFlowSculptor.Application/InfraFlowSculptor.Application.csproj", "InfraFlowSculptor.Application/"]
COPY ["src/Api/InfraFlowSculptor.Domain/InfraFlowSculptor.Domain.csproj", "InfraFlowSculptor.Domain/"]
COPY ["src/Api/InfraFlowSculptor.Contracts/InfraFlowSculptor.Contracts.csproj", "InfraFlowSculptor.Contracts/"]
COPY ["src/Api/InfraFlowSculptor.Infrastructure/InfraFlowSculptor.Infrastructure.csproj", "InfraFlowSculptor.Infrastructure/"]
COPY ["src/Api/InfraFlowSculptor.BicepGeneration/InfraFlowSculptor.BicepGeneration.csproj", "InfraFlowSculptor.BicepGeneration/"]
COPY ["src/Api/InfraFlowSculptor.GenerationCore/InfraFlowSculptor.GenerationCore.csproj", "InfraFlowSculptor.GenerationCore/"]
COPY ["src/Api/InfraFlowSculptor.PipelineGeneration/InfraFlowSculptor.PipelineGeneration.csproj", "InfraFlowSculptor.PipelineGeneration/"]
COPY ["src/Shared/InfraFlowSculptor.WebDefaults/InfraFlowSculptor.WebDefaults.csproj", "/Shared/InfraFlowSculptor.WebDefaults/"]
RUN dotnet restore "InfraFlowSculptor.Api/InfraFlowSculptor.Api.csproj"
COPY src/Api/InfraFlowSculptor.Api/ InfraFlowSculptor.Api/
COPY src/Api/InfraFlowSculptor.Application/ InfraFlowSculptor.Application/
COPY src/Api/InfraFlowSculptor.Domain/ InfraFlowSculptor.Domain/
COPY src/Api/InfraFlowSculptor.Contracts/ InfraFlowSculptor.Contracts/
COPY src/Api/InfraFlowSculptor.Infrastructure/ InfraFlowSculptor.Infrastructure/
COPY src/Api/InfraFlowSculptor.BicepGeneration/ InfraFlowSculptor.BicepGeneration/
COPY src/Api/InfraFlowSculptor.GenerationCore/ InfraFlowSculptor.GenerationCore/
COPY src/Api/InfraFlowSculptor.PipelineGeneration/ InfraFlowSculptor.PipelineGeneration/
COPY src/Shared/InfraFlowSculptor.WebDefaults/ /Shared/InfraFlowSculptor.WebDefaults/
WORKDIR "/src/InfraFlowSculptor.Api"
RUN dotnet build "InfraFlowSculptor.Api.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "InfraFlowSculptor.Api.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "InfraFlowSculptor.Api.dll"]
