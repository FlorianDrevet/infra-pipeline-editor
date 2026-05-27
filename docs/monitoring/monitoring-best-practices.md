# Monitoring, Logging & Metrics — Guide d'apprentissage et bonnes pratiques

> **Objectif** : Rendre InfraFlowSculptor industrialisable avec un monitoring de niveau production intégré à Azure Application Insights, en suivant les bonnes pratiques des grands projets .NET + Angular.

---

## Table des matières

1. [État actuel du projet (Audit)](#1-état-actuel-du-projet-audit)
2. [Architecture de monitoring recommandée](#2-architecture-de-monitoring-recommandée)
3. [Les 3 piliers de l'observabilité](#3-les-3-piliers-de-lobservabilité)
4. [Backend .NET — Intégration Application Insights](#4-backend-net--intégration-application-insights)
5. [Frontend Angular — Intégration Application Insights](#5-frontend-angular--intégration-application-insights)
6. [Structured Logging — Bonnes pratiques](#6-structured-logging--bonnes-pratiques)
7. [Métriques personnalisées](#7-métriques-personnalisées)
8. [Distributed Tracing](#8-distributed-tracing)
9. [Health Checks & Alerting](#9-health-checks--alerting)
10. [Configuration par environnement](#10-configuration-par-environnement)
11. [Plan d'implémentation](#11-plan-dimplémentation)
12. [Références](#12-références)

---

## 1. État actuel du projet (Audit)

### Backend (.NET 10)

| Aspect | État | Commentaire |
|--------|------|-------------|
| OpenTelemetry SDK | ✅ Configuré | `Infrastructure.DependencyInjection.AddObservability()` |
| Traces (ASP.NET Core) | ✅ Actif | Instrumentation HTTP entrante/sortante |
| Traces (EF Core / SQL) | ❌ Absent | Pas de `AddEntityFrameworkCoreInstrumentation()` |
| Métriques (Runtime) | ✅ Actif | `AddRuntimeInstrumentation()` |
| Métriques (ASP.NET Core) | ✅ Actif | `AddAspNetCoreInstrumentation()` |
| Métriques (HTTP Client) | ✅ Actif | `AddHttpClientInstrumentation()` |
| Métriques personnalisées | ❌ Absent | Aucun `Meter`/`Counter`/`Histogram` custom |
| Export OTLP | ✅ Conditionnel | Seulement si `OTEL_EXPORTER_OTLP_ENDPOINT` est défini |
| Export Azure Monitor | ❌ Absent | Pas de `Azure.Monitor.OpenTelemetry.AspNetCore` |
| Structured Logging | ⚠️ Minimal | 3-4 usages de `ILogger` dans l'infra, aucun dans les handlers |
| LoggerMessage source gen | ❌ Absent | Pas de `[LoggerMessage]` attribut |
| ActivitySource custom | ⚠️ Minimal | Seulement `DbMigrations` |
| Health Checks | ✅ Basique | `/health` + `/alive` (self check uniquement) |
| Health Check DB | ❌ Absent | Pas de health check PostgreSQL |
| Serilog/NLog | ❌ Non utilisé | Logging natif Microsoft.Extensions.Logging |
| MediatR Pipeline Logging | ❌ Absent | Pas de `LoggingBehavior` |

### Frontend (Angular 21)

| Aspect | État | Commentaire |
|--------|------|-------------|
| OpenTelemetry Web SDK | ✅ Configuré | `TelemetryService` avec traces OTLP |
| Propagation traceparent | ✅ Actif | `FetchInstrumentation` avec `propagateTraceHeaderCorsUrls` |
| Export vers Aspire | ✅ Actif | Via proxy `/otlp/v1/traces` |
| Application Insights JS SDK | ❌ Absent | Pas de `@microsoft/applicationinsights-web` |
| Error tracking (global) | ❌ Absent | Pas de `ErrorHandler` global |
| Page view tracking | ❌ Absent | — |
| Click analytics | ❌ Absent | — |
| Métriques navigateur (Core Web Vitals) | ❌ Absent | — |
| Production telemetry | ❌ Non fonctionnel | `otlpEnabled` absent en config production |

### Aspire / Orchestration

| Aspect | État | Commentaire |
|--------|------|-------------|
| ServiceDefaults OTel | ✅ Actif | `ConfigureOpenTelemetry()` dans `Extensions.cs` |
| Dashboard local | ✅ Actif | Traces/metrics/logs visibles via Aspire Dashboard |
| Application Insights resource | ❌ Non wired | Pas de `AddAzureApplicationInsights` dans AppHost |

---

## 2. Architecture de monitoring recommandée

```
┌─────────────────────────────────────────────────────────────────┐
│                        PRODUCTION                                │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌─────────────────┐         ┌──────────────────────────┐      │
│  │  Angular SPA    │────────►│  Application Insights    │      │
│  │  (JS SDK)       │         │  (Frontend resource)     │      │
│  └────────┬────────┘         └────────────┬─────────────┘      │
│           │ traceparent                    │                     │
│  ┌────────▼────────┐         ┌────────────▼─────────────┐      │
│  │  .NET API       │────────►│  Application Insights    │      │
│  │  (OTel Distro)  │         │  (Backend resource)      │      │
│  └────────┬────────┘         └────────────┬─────────────┘      │
│           │                                │                     │
│  ┌────────▼────────┐         ┌────────────▼─────────────┐      │
│  │  PostgreSQL     │         │  Log Analytics Workspace  │      │
│  │  (diagnostics)  │         │  (centralisation)        │      │
│  └─────────────────┘         └──────────────────────────┘      │
│                                                                  │
├─────────────────────────────────────────────────────────────────┤
│                        LOCAL (Aspire)                            │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────────┐                                            │
│  │  Aspire Dashboard│◄── OTLP ── API + Frontend (dev)           │
│  │  (traces/logs)  │                                            │
│  └─────────────────┘                                            │
└─────────────────────────────────────────────────────────────────┘
```

### Pourquoi 2 ressources Application Insights ?

| Approche | Avantages | Inconvénients |
|----------|-----------|---------------|
| **1 seule ressource** (recommandé pour les petits projets) | Corrélation auto entre front et back | Bruit si trop de page views |
| **2 ressources séparées** (recommandé pour les gros projets) | Isolation des coûts, contrôle fin des quotas, sécurité (connection string frontend exposée) | Cross-resource queries nécessaires pour la corrélation |

**Recommandation pour InfraFlowSculptor** : **1 seule ressource** Application Insights car :
- C'est un SaaS B2B avec un nombre modéré d'utilisateurs
- La corrélation end-to-end (front → back) est essentielle pour le debug
- Le volume de telemetry sera gérable

---

## 3. Les 3 piliers de l'observabilité

### Logs (Structured Logging)

**Quoi** : Événements discrets horodatés avec un contexte structuré (pas des strings brutes).

**Bonnes pratiques** :
- Utiliser des **message templates** (`"Processing project {ProjectId}"`) — jamais d'interpolation (`$"Processing project {id}"`)
- Niveaux : `Trace` < `Debug` < `Information` < `Warning` < `Error` < `Critical`
- Ajouter des **scopes** pour corréler (TraceId, UserId, ProjectId)
- En production : `Information` minimum ; en dev : `Debug`

### Metrics

**Quoi** : Mesures numériques agrégées dans le temps (counters, gauges, histograms).

**Bonnes pratiques** :
- Métriques de business : nombre de projets créés, générations Bicep, etc.
- Métriques techniques : durée des handlers, taille des réponses, erreurs de validation
- Utiliser des `Meter` / `Counter<T>` / `Histogram<T>` .NET natifs

### Traces (Distributed Tracing)

**Quoi** : Suivi d'une requête à travers tous les services (frontend → API → DB).

**Bonnes pratiques** :
- Chaque service injecte un `traceparent` HTTP header
- OpenTelemetry propage automatiquement les contextes
- Application Insights affiche l'Application Map et la vue end-to-end

---

## 4. Backend .NET — Intégration Application Insights

### Package recommandé

```xml
<!-- Azure Monitor OpenTelemetry Distro — la voie officielle Microsoft 2024+ -->
<PackageReference Include="Azure.Monitor.OpenTelemetry.AspNetCore" />
```

Ce package **ne remplace pas** la configuration OTel existante. Il s'y **ajoute** en tant qu'exporter supplémentaire.

### Configuration minimale

```csharp
// Dans AddObservability() ou dans Program.cs
if (!string.IsNullOrWhiteSpace(configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
{
    services.AddOpenTelemetry().UseAzureMonitor();
}
```

### Ce que `UseAzureMonitor()` active automatiquement

| Feature | Inclus |
|---------|--------|
| ASP.NET Core traces | ✅ |
| HTTP Client traces | ✅ |
| SQL Client traces | ✅ |
| ASP.NET Core metrics | ✅ |
| HTTP Client metrics | ✅ |
| Runtime metrics | ✅ |
| Live Metrics | ✅ |
| Log export vers App Insights | ✅ |
| Azure resource detection (App Service, Container App) | ✅ |
| Sampling adaptatif | ✅ |

### Ajouts recommandés en plus du Distro

```csharp
services.AddOpenTelemetry()
    .UseAzureMonitor()
    .WithTracing(tracing =>
    {
        // EF Core — essentiel pour voir les requêtes SQL
        tracing.AddEntityFrameworkCoreInstrumentation(options =>
        {
            options.SetDbStatementForText = true; // voir le SQL (attention PII)
        });
        
        // Sources custom de l'application
        tracing.AddSource("InfraFlowSculptor.Api");
        tracing.AddSource("InfraFlowSculptor.BicepGeneration");
    })
    .WithMetrics(metrics =>
    {
        // Métriques custom de l'application
        metrics.AddMeter("InfraFlowSculptor.Api");
        metrics.AddMeter("InfraFlowSculptor.BicepGeneration");
    });
```

### Cohabitation OTLP + Azure Monitor

Le pattern courant pour les gros projets :
- **Local/Aspire** → export OTLP vers le dashboard local
- **Production** → export Azure Monitor (et optionnellement OTLP vers un collecteur)

```csharp
// Pattern conditionnel recommandé
var useAzureMonitor = !string.IsNullOrWhiteSpace(
    configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);
var useOtlp = !string.IsNullOrWhiteSpace(
    configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

var otelBuilder = services.AddOpenTelemetry();

if (useAzureMonitor)
{
    otelBuilder.UseAzureMonitor();
}

if (useOtlp)
{
    otelBuilder.UseOtlpExporter();
}
```

---

## 5. Frontend Angular — Intégration Application Insights

### Approche recommandée par Microsoft

> **Important** : Microsoft recommande le **Application Insights JavaScript SDK** (`@microsoft/applicationinsights-web`) pour le monitoring navigateur, **pas** OpenTelemetry Web SDK.  
> Cite : "The Application Insights JavaScript SDK is the supported client-side browser instrumentation path. It doesn't use OpenTelemetry, and customers aren't expected to migrate browser JavaScript monitoring to OpenTelemetry."

### Packages à installer

```bash
npm install @microsoft/applicationinsights-web @microsoft/applicationinsights-angularplugin-js
```

### Optionnel (click analytics)
```bash
npm install @microsoft/applicationinsights-clickanalytics-js
```

### Service Angular d'initialisation

```typescript
import { Injectable } from '@angular/core';
import { ApplicationInsights } from '@microsoft/applicationinsights-web';
import { AngularPlugin } from '@microsoft/applicationinsights-angularplugin-js';
import { Router } from '@angular/router';

@Injectable({ providedIn: 'root' })
export class ApplicationInsightsService {
  private appInsights?: ApplicationInsights;
  private initialized = false;

  initialize(router: Router, connectionString: string): void {
    if (this.initialized || !connectionString) return;
    this.initialized = true;

    const angularPlugin = new AngularPlugin();
    
    this.appInsights = new ApplicationInsights({
      config: {
        connectionString,
        extensions: [angularPlugin],
        extensionConfig: {
          [angularPlugin.identifier]: { router },
        },
        enableAutoRouteTracking: true,    // Page views automatiques
        enableCorsCorrelation: true,       // Corrélation avec le backend
        enableRequestHeaderTracking: true,
        enableResponseHeaderTracking: true,
        distributedTracingMode: 2,         // W3C TraceContext
      },
    });

    this.appInsights.loadAppInsights();
  }

  trackEvent(name: string, properties?: Record<string, string>): void {
    this.appInsights?.trackEvent({ name }, properties);
  }

  trackException(error: Error, properties?: Record<string, string>): void {
    this.appInsights?.trackException({ exception: error }, properties);
  }

  trackMetric(name: string, average: number): void {
    this.appInsights?.trackMetric({ name, average });
  }
}
```

### Cohabitation avec le TelemetryService existant (OTel)

| Environnement | TelemetryService (OTel) | App Insights JS SDK |
|---------------|------------------------|---------------------|
| **Aspire local** | ✅ actif (OTLP → dashboard) | ❌ désactivé |
| **Production** | ❌ désactivé | ✅ actif |
| **Preview/Staging** | Optionnel | ✅ actif |

Configuration dans `environment.ts` (production) :
```typescript
export const environment: EnvironmentInterface = {
  production: true,
  api_url: '',
  appInsightsConnectionString: 'InstrumentationKey=xxx;IngestionEndpoint=...',
  msalConfig: { ... },
};
```

### Ce que ça apporte

- **Page views** automatiques (via Angular Router)
- **Exceptions** non catchées automatiquement tracées
- **Corrélation** des traces front → back via `traceparent`/`Request-Id`
- **Click Analytics** (optionnel) pour analyser les interactions
- **Session/User** tracking pour analyser les parcours

---

## 6. Structured Logging — Bonnes pratiques

### Pattern recommandé : `[LoggerMessage]` Source Generator

C'est la meilleure pratique pour les projets .NET en production (performance + typage fort).

```csharp
// Fichier dédié par domaine : ProjectLogMessages.cs
namespace InfraFlowSculptor.Application.Projects;

internal static partial class ProjectLogMessages
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Creating project {ProjectName} for user {UserId}")]
    internal static partial void LogProjectCreation(
        this ILogger logger, string projectName, string userId);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Warning,
        Message = "Project {ProjectId} not found for user {UserId}")]
    internal static partial void LogProjectNotFound(
        this ILogger logger, Guid projectId, string userId);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Error,
        Message = "Failed to generate Bicep for project {ProjectId}")]
    internal static partial void LogBicepGenerationFailed(
        this ILogger logger, Guid projectId, Exception exception);
}
```

### Avantages par rapport à `logger.LogInformation(...)`

| Aspect | Extension methods | Source Generator |
|--------|------------------|-----------------|
| Performance | Boxing des value types | Zero allocation |
| Template parsing | À chaque appel | Compile-time |
| Event ID | Souvent omis | Obligatoire |
| Typage | Faible (params object[]) | Fort (paramètres typés) |
| Code analysis | CA1848 warning | Clean |

### MediatR Logging Behavior (manquant)

Un `LoggingBehavior` est standard dans les grands projets CQRS :

```csharp
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        
        logger.LogInformation(
            "Handling {RequestName} {@Request}", requestName, request);

        var stopwatch = Stopwatch.StartNew();
        var response = await next(cancellationToken);
        stopwatch.Stop();

        logger.LogInformation(
            "Handled {RequestName} in {ElapsedMs}ms",
            requestName, stopwatch.ElapsedMilliseconds);

        return response;
    }
}
```

### Convention d'Event IDs recommandée

| Range | Domaine |
|-------|---------|
| 1000-1099 | Projects |
| 1100-1199 | Resources |
| 1200-1299 | Bicep Generation |
| 1300-1399 | Pipeline Generation |
| 1400-1499 | Infrastructure / Auth |
| 1500-1599 | Import / IaC |
| 2000-2099 | MCP |
| 9000-9099 | Health / Diagnostics |

---

## 7. Métriques personnalisées

### Définir des métriques métier

```csharp
// Fichier : InfraFlowSculptor.Infrastructure/Diagnostics/ApplicationMetrics.cs
using System.Diagnostics.Metrics;

public sealed class ApplicationMetrics
{
    public const string MeterName = "InfraFlowSculptor.Api";

    private readonly Counter<long> _projectsCreated;
    private readonly Counter<long> _bicepGenerations;
    private readonly Counter<long> _bicepGenerationErrors;
    private readonly Histogram<double> _bicepGenerationDuration;
    private readonly Counter<long> _pipelineGenerations;

    public ApplicationMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);

        _projectsCreated = meter.CreateCounter<long>(
            "infraflow.projects.created",
            unit: "{project}",
            description: "Number of projects created");

        _bicepGenerations = meter.CreateCounter<long>(
            "infraflow.bicep.generations",
            unit: "{generation}",
            description: "Number of Bicep generation requests");

        _bicepGenerationErrors = meter.CreateCounter<long>(
            "infraflow.bicep.generation_errors",
            unit: "{error}",
            description: "Number of failed Bicep generations");

        _bicepGenerationDuration = meter.CreateHistogram<double>(
            "infraflow.bicep.generation_duration",
            unit: "ms",
            description: "Duration of Bicep generation");

        _pipelineGenerations = meter.CreateCounter<long>(
            "infraflow.pipeline.generations",
            unit: "{generation}",
            description: "Number of pipeline generation requests");
    }

    public void RecordProjectCreated() => _projectsCreated.Add(1);
    public void RecordBicepGeneration() => _bicepGenerations.Add(1);
    public void RecordBicepGenerationError() => _bicepGenerationErrors.Add(1);
    public void RecordBicepGenerationDuration(double ms) => _bicepGenerationDuration.Record(ms);
    public void RecordPipelineGeneration() => _pipelineGenerations.Add(1);
}
```

### Enregistrement DI

```csharp
services.AddSingleton<ApplicationMetrics>();
services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics.AddMeter(ApplicationMetrics.MeterName));
```

### Visualisation dans Application Insights

Les métriques custom apparaissent dans :
- **Metrics Explorer** → Custom Metrics → `infraflow.*`
- **Workbooks** → dashboards personnalisés
- **Alerts** → seuils automatiques

---

## 8. Distributed Tracing

### Comment ça fonctionne dans InfraFlowSculptor

```
Browser (Angular)
  │ traceparent: 00-<trace-id>-<span-id>-01
  ▼
API (ASP.NET Core)  ←─── OpenTelemetry extrait le contexte
  │ Activity.Current = span lié au parent browser
  ▼
Handler (MediatR)   ←─── même trace-id propagé
  │
  ├──► PostgreSQL (EF Core)
  │    └── span SQL visible dans la trace
  │
  └──► Bicep Generator API (HTTP)
       └── traceparent propagé au 2e service
```

### Créer des spans personnalisés

```csharp
// Pour instrumenter une opération métier critique
private static readonly ActivitySource Source = new("InfraFlowSculptor.BicepGeneration");

public async Task<BicepOutput> GenerateAsync(Project project)
{
    using var activity = Source.StartActivity("GenerateBicep");
    activity?.SetTag("project.id", project.Id.Value.ToString());
    activity?.SetTag("project.name", project.Name);
    activity?.SetTag("resource.count", project.Resources.Count);

    try
    {
        var result = await InternalGenerate(project);
        activity?.SetStatus(ActivityStatusCode.Ok);
        return result;
    }
    catch (Exception ex)
    {
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        activity?.RecordException(ex);
        throw;
    }
}
```

### Application Map dans Azure

Application Insights génère automatiquement un **Application Map** qui montre :
- Les services et leurs dépendances
- Les taux d'erreur par connexion
- La latence entre composants

---

## 9. Health Checks & Alerting

### Health Checks recommandés pour la production

```csharp
services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"])
    .AddNpgSql(
        connectionString,
        name: "postgresql",
        tags: ["ready", "db"])
    .AddUrlGroup(
        new Uri("https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration"),
        name: "entra-id",
        tags: ["ready", "auth"]);
```

### Alerting dans Azure Monitor

| Alerte | Condition | Sévérité |
|--------|-----------|----------|
| Taux d'erreur 5xx > 5% | Custom metric | Sev 1 |
| Temps de réponse P95 > 2s | Request duration | Sev 2 |
| Exception rate spike | Smart Detection | Sev 1 |
| Health check failure | Availability test | Sev 0 |
| Bicep generation failure rate > 10% | Custom metric | Sev 2 |

### Availability Tests (ping)

Azure Monitor permet de créer des **URL Ping Tests** qui vérifient périodiquement que `/health` répond 200 depuis plusieurs régions Azure.

---

## 10. Configuration par environnement

### appsettings.json (défaut)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Warning"
    }
  }
}
```

### appsettings.Production.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "APPLICATIONINSIGHTS_CONNECTION_STRING": "Set via Azure App Configuration or env var"
}
```

### appsettings.Development.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

### Variables d'environnement en production

| Variable | Valeur | Obligatoire |
|----------|--------|-------------|
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | `InstrumentationKey=...;IngestionEndpoint=...` | ✅ Backend |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | (vide en prod, sauf si collecteur externe) | ❌ |
| `OTEL_SERVICE_NAME` | `infraflowsculptor-api` | Recommandé |

---

## 11. Plan d'implémentation

### Phase 1 — Backend (priorité haute)

| # | Tâche | Impact |
|---|-------|--------|
| 1 | Ajouter `Azure.Monitor.OpenTelemetry.AspNetCore` dans `Directory.Packages.props` | NuGet |
| 2 | Ajouter `UseAzureMonitor()` conditionnel dans `AddObservability()` | Config |
| 3 | Ajouter EF Core instrumentation (`AddEntityFrameworkCoreInstrumentation()`) | Visibilité SQL |
| 4 | Ajouter `LoggingBehavior` dans le pipeline MediatR | Visibilité handlers |
| 5 | Créer `ApplicationMetrics` avec les métriques métier | Dashboards |
| 6 | Ajouter health check PostgreSQL | Résilience |
| 7 | Commencer à migrer les logs vers `[LoggerMessage]` source gen | Performance |

### Phase 2 — Frontend (priorité haute)

| # | Tâche | Impact |
|---|-------|--------|
| 1 | Installer `@microsoft/applicationinsights-web` + Angular plugin | NPM |
| 2 | Créer `ApplicationInsightsService` | Initialisation |
| 3 | Ajouter `appInsightsConnectionString` dans `EnvironmentInterface` | Config |
| 4 | Brancher dans `app.config.ts` (condition production) | Activation |
| 5 | Ajouter `ErrorHandler` global qui reporte à App Insights | Fiabilité |
| 6 | Conserver `TelemetryService` OTel pour le mode Aspire local | Dev only |

### Phase 3 — Aspire & Infrastructure (priorité moyenne)

| # | Tâche | Impact |
|---|-------|--------|
| 1 | Ajouter `Aspire.Hosting.Azure.ApplicationInsights` dans AppHost | Provisioning |
| 2 | Wirer la ressource App Insights avec `WithReference` | Injection auto |
| 3 | Configurer les alertes Azure Monitor | Production-ready |
| 4 | Créer un Workbook personnalisé pour les métriques métier | Monitoring |

### Phase 4 — Maturité (priorité basse)

| # | Tâche | Impact |
|---|-------|--------|
| 1 | Click Analytics plugin frontend | UX analytics |
| 2 | Sampling adaptatif configuré | Coût maîtrisé |
| 3 | Export logs vers Log Analytics pour requêtes Kusto | Investigation |
| 4 | Dashboards Azure avec métriques métier | Visibilité business |
| 5 | Chaos engineering avec health checks | Résilience |

---

## 12. Références

### Documentation officielle

- [Azure Monitor OpenTelemetry Distro for .NET](https://learn.microsoft.com/dotnet/api/overview/azure/monitor.opentelemetry.aspnetcore-readme)
- [Enable OpenTelemetry with Application Insights](https://learn.microsoft.com/azure/azure-monitor/app/opentelemetry-enable)
- [Application Insights JavaScript SDK](https://learn.microsoft.com/azure/azure-monitor/app/javascript-sdk)
- [Angular Plugin for App Insights](https://learn.microsoft.com/azure/azure-monitor/app/javascript-framework-extensions?tabs=angular)
- [High-performance logging in .NET](https://learn.microsoft.com/dotnet/core/extensions/logging/high-performance-logging)
- [Compile-time logging source generation](https://learn.microsoft.com/dotnet/core/extensions/logging/source-generation)
- [Aspire Azure Application Insights integration](https://aspire.dev/docs/azure-application-insights)
- [Best practices: Observability agent](https://learn.microsoft.com/azure/azure-monitor/aiops/observability-agent-best-practices)

### Patterns de référence

- [Reliable Web App pattern for .NET](https://learn.microsoft.com/azure/architecture/web-apps/guides/enterprise-app-patterns/reliable-web-app/dotnet/guidance)
- [Modern Web App pattern for .NET](https://learn.microsoft.com/azure/architecture/web-apps/guides/enterprise-app-patterns/modern-web-app/dotnet/guidance)

### Règles de code à activer

- `CA1848` — Use the LoggerMessage delegates
- `CA1873` — Avoid potentially expensive logging

---

## Glossaire

| Terme | Définition |
|-------|-----------|
| **OTLP** | OpenTelemetry Protocol — protocole standard d'export de telemetry |
| **Distro** | Distribution = bundle pré-configuré d'OpenTelemetry avec exporters |
| **Span** | Unité de travail dans une trace distribuée |
| **Activity** | Équivalent .NET d'un span OpenTelemetry |
| **ActivitySource** | Factory de spans/activities dans .NET |
| **Meter** | Container de métriques dans .NET |
| **W3C TraceContext** | Standard de propagation des IDs de trace via HTTP headers |
| **Sampling** | Réduction du volume de telemetry en ne gardant qu'un échantillon |
| **Live Metrics** | Flux temps réel de métriques dans Application Insights |
| **Application Map** | Visualisation des dépendances entre services dans App Insights |
| **Kusto (KQL)** | Langage de requête pour explorer les logs dans Log Analytics |
