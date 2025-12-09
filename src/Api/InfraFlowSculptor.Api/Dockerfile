FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["VPD.Api/VPD.Api.csproj", "VPD.Api/"]
COPY ["VPD.Application/VPD.Application.csproj", "VPD.Application/"]
COPY ["VPD.Domain/VPD.Domain.csproj", "VPD.Domain/"]
COPY ["VPD.Contracts/VPD.Contracts.csproj", "VPD.Contracts/"]
COPY ["VPD.Infrastructure/VPD.Infrastructure.csproj", "VPD.Infrastructure/"]
RUN dotnet restore "VPD.Api/VPD.Api.csproj"
COPY . .
WORKDIR "/src/VPD.Api"
RUN dotnet build "VPD.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "VPD.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VPD.Api.dll"]
