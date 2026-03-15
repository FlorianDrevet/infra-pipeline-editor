FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BicepGenerator.Api/BicepGenerator.Api.csproj", "BicepGenerator.Api/"]
COPY ["BicepGenerator.Application/BicepGenerator.Application.csproj", "BicepGenerator.Application/"]
COPY ["BicepGenerator.Domain/BicepGenerator.Domain.csproj", "BicepGenerator.Domain/"]
COPY ["BicepGenerator.Contracts/BicepGenerator.Contracts.csproj", "BicepGenerator.Contracts/"]
COPY ["BicepGenerator.Infrastructure/BicepGenerator.Infrastructure.csproj", "BicepGenerator.Infrastructure/"]
RUN dotnet restore "BicepGenerator.Api/BicepGenerator.Api.csproj"
COPY BicepGenerator.Api/ BicepGenerator.Api/
COPY BicepGenerator.Application/ BicepGenerator.Application/
COPY BicepGenerator.Domain/ BicepGenerator.Domain/
COPY BicepGenerator.Contracts/ BicepGenerator.Contracts/
COPY BicepGenerator.Infrastructure/ BicepGenerator.Infrastructure/
WORKDIR "/src/BicepGenerator.Api"
RUN dotnet build "BicepGenerator.Api.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BicepGenerator.Api.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "BicepGenerator.Api.dll"]
