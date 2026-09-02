import { Component, inject } from '@angular/core';

import { Router } from '@angular/router';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ResourceDiagnosticResponse } from '../../interfaces/bicep-generator.interface';
import { PendingCustomDomainIssue } from '../../interfaces/pending-custom-domain-issue.interface';
import { RESOURCE_TYPE_ABBREVIATIONS } from '../../resource-metadata/resource-type.metadata';
import { DsButtonComponent, DsIconButtonComponent } from '../ds';

const ARM_TYPE_TO_FRIENDLY: Record<string, string> = {
  'Microsoft.KeyVault/vaults': 'KeyVault',
  'Microsoft.Cache/Redis': 'RedisCache',
  'Microsoft.Storage/storageAccounts': 'StorageAccount',
  'Microsoft.Web/serverfarms': 'AppServicePlan',
  'Microsoft.Web/sites': 'WebApp',
  'Microsoft.Web/sites/functionapp': 'FunctionApp',
  'Microsoft.ManagedIdentity/userAssignedIdentities': 'UserAssignedIdentity',
  'Microsoft.AppConfiguration/configurationStores': 'AppConfiguration',
  'Microsoft.App/managedEnvironments': 'ContainerAppEnvironment',
  'Microsoft.App/containerApps': 'ContainerApp',
  'Microsoft.OperationalInsights/workspaces': 'LogAnalyticsWorkspace',
  'Microsoft.Insights/components': 'ApplicationInsights',
  'Microsoft.DocumentDB/databaseAccounts': 'CosmosDb',
  'Microsoft.Sql/servers': 'SqlServer',
  'Microsoft.Sql/servers/databases': 'SqlDatabase',
  'Microsoft.ServiceBus/namespaces': 'ServiceBusNamespace',
  'Microsoft.ContainerRegistry/registries': 'ContainerRegistry',
  'Microsoft.EventHub/namespaces': 'EventHubNamespace',
};

const WarningSeverity = 'warning';
const DockerImageNotValidatedRuleCode = 'DOCKER_IMAGE_NOT_VALIDATED';

export interface ConfigDiagnosticGroup {
  configId: string;
  configName: string;
  diagnostics: ResourceDiagnosticResponse[];
}

export interface MissingEnvResource {
  resourceId: string;
  resourceName: string;
  resourceType: string;
  missingEnvironments: string[];
}

export interface ConfigMissingEnvGroup {
  configId: string;
  configName: string;
  resources: MissingEnvResource[];
}

export interface ConfigPendingCustomDomainGroup {
  configId: string;
  configName: string;
  domains: PendingCustomDomainIssue[];
}

export interface PendingDockerImageIssue {
  resourceId: string;
  resourceName: string;
  resourceType: string;
  dockerImageName: string;
}

export interface ConfigPendingDockerImageGroup {
  configId: string;
  configName: string;
  resources: PendingDockerImageIssue[];
}

interface ConfigPendingDockerImageDialogGroup {
  configId: string;
  configName: string;
  diagnosticIssues: ResourceDiagnosticResponse[];
  resources: PendingDockerImageIssue[];
}

export interface EnvironmentConfigIssue {
  environmentName: string;
  missingSubscriptionId: boolean;
  missingAzureConnection: boolean;
}

export interface GenerationDiagnosticsDialogData {
  configDiagnostics: ConfigDiagnosticGroup[];
  missingEnvConfigs?: ConfigMissingEnvGroup[];
  pendingCustomDomainConfigs?: ConfigPendingCustomDomainGroup[];
  pendingDockerImageConfigs?: ConfigPendingDockerImageGroup[];
  incompleteEnvironmentConfigs?: EnvironmentConfigIssue[];
}

@Component({
  selector: 'app-generation-diagnostics-dialog',
  standalone: true,
  imports: [
    MatDialogModule,
    MatIconModule,
    TranslateModule,
    DsButtonComponent,
    DsIconButtonComponent
],
  templateUrl: './generation-diagnostics-dialog.component.html',
  styleUrl: './generation-diagnostics-dialog.component.scss',
})
export class GenerationDiagnosticsDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<GenerationDiagnosticsDialogComponent>);
  private readonly router = inject(Router);
  private readonly translate = inject(TranslateService);
  protected readonly data: GenerationDiagnosticsDialogData = inject(MAT_DIALOG_DATA);

  protected readonly regularDiagnosticConfigs: ConfigDiagnosticGroup[] = this.data.configDiagnostics
    .map((group) => ({
      ...group,
      diagnostics: group.diagnostics.filter((diagnostic) => diagnostic.ruleCode !== DockerImageNotValidatedRuleCode),
    }))
    .filter((group) => group.diagnostics.length > 0);

  protected readonly pendingDockerImageDialogGroups: ConfigPendingDockerImageDialogGroup[] = (() => {
    const groups = new Map<string, ConfigPendingDockerImageDialogGroup>();
    const explicitDockerImageIssueKeys = new Set<string>();

    const getOrCreateGroup = (configId: string, configName: string): ConfigPendingDockerImageDialogGroup => {
      const existingGroup = groups.get(configId);
      if (existingGroup) {
        return existingGroup;
      }

      const createdGroup: ConfigPendingDockerImageDialogGroup = {
        configId,
        configName,
        diagnosticIssues: [],
        resources: [],
      };
      groups.set(configId, createdGroup);
      return createdGroup;
    };

    for (const group of this.data.pendingDockerImageConfigs ?? []) {
      const targetGroup = getOrCreateGroup(group.configId, group.configName);
      for (const resource of group.resources) {
        const issueKey = `${group.configId}:${resource.resourceId}`;
        if (explicitDockerImageIssueKeys.has(issueKey)) {
          continue;
        }

        explicitDockerImageIssueKeys.add(issueKey);
        targetGroup.resources.push(resource);
      }
    }

    for (const group of this.data.configDiagnostics) {
      const dockerDiagnostics = group.diagnostics.filter((diagnostic) => {
        if (diagnostic.ruleCode !== DockerImageNotValidatedRuleCode) {
          return false;
        }

        const issueKey = `${group.configId}:${diagnostic.resourceId}`;
        return !explicitDockerImageIssueKeys.has(issueKey);
      });

      if (dockerDiagnostics.length === 0) {
        continue;
      }

      const targetGroup = getOrCreateGroup(group.configId, group.configName);
      targetGroup.diagnosticIssues.push(...dockerDiagnostics);
    }

    return Array.from(groups.values())
      .filter((group) => group.diagnosticIssues.length > 0 || group.resources.length > 0);
  })();

  protected readonly totalDiagnostics = this.regularDiagnosticConfigs.reduce(
    (sum, g) => sum + g.diagnostics.length, 0,
  );

  protected readonly totalMissingEnvIssues = (this.data.missingEnvConfigs ?? []).reduce(
    (sum, g) => sum + g.resources.length, 0,
  );

  protected readonly totalPendingCustomDomainIssues = (this.data.pendingCustomDomainConfigs ?? []).reduce(
    (sum, g) => sum + g.domains.length, 0,
  );

  protected readonly totalPendingDockerImageIssues = this.pendingDockerImageDialogGroups.reduce(
    (sum, g) => sum + g.resources.length + g.diagnosticIssues.length, 0,
  );

  protected readonly incompleteEnvironments = this.data.incompleteEnvironmentConfigs ?? [];
  protected readonly totalIncompleteEnvironments = this.incompleteEnvironments.length;
  protected readonly hasIncompleteEnvironments = this.totalIncompleteEnvironments > 0;

  protected readonly totalIssues = this.totalDiagnostics + this.totalMissingEnvIssues + this.totalPendingCustomDomainIssues + this.totalPendingDockerImageIssues + this.totalIncompleteEnvironments;

  protected readonly hasDiagnostics = this.totalDiagnostics > 0;
  protected readonly hasMissingEnvs = this.totalMissingEnvIssues > 0;
  protected readonly hasPendingCustomDomains = this.totalPendingCustomDomainIssues > 0;
  protected readonly hasPendingDockerImages = this.totalPendingDockerImageIssues > 0;

  protected readonly hasErrorDiagnostics = this.regularDiagnosticConfigs.some((group) =>
    group.diagnostics.some((diagnostic) => diagnostic.severity?.toLowerCase() !== WarningSeverity),
  );

  protected readonly dialogTitleIcon = this.hasErrorDiagnostics ? 'gpp_bad' : 'warning_amber';

  protected readonly isMultiConfig = (() => {
    const configIds = new Set<string>();
    for (const g of this.data.configDiagnostics) configIds.add(g.configId);
    for (const g of this.data.missingEnvConfigs ?? []) configIds.add(g.configId);
    for (const g of this.data.pendingCustomDomainConfigs ?? []) configIds.add(g.configId);
    for (const g of this.data.pendingDockerImageConfigs ?? []) configIds.add(g.configId);
    return configIds.size > 1;
  })();

  protected getSeverityIcon(severity: string): string {
    return severity?.toLowerCase() === WarningSeverity ? 'warning' : 'gpp_bad';
  }

  protected getSeverityClass(severity: string): string {
    return severity?.toLowerCase() === WarningSeverity ? 'warning' : 'error';
  }

  protected getResourceTypeAbbr(resourceType: string): string {
    return RESOURCE_TYPE_ABBREVIATIONS[resourceType] ?? resourceType;
  }

  protected getTranslatedMessage(diagnostic: ResourceDiagnosticResponse): string {
    const key = `CONFIG_DETAIL.DIAGNOSTICS.${diagnostic.ruleCode}`;
    const params = { target: diagnostic.targetResourceName };
    const translated = this.translate.instant(key, params);

    if (translated === key) {
      return this.translate.instant('CONFIG_DETAIL.DIAGNOSTICS.UNKNOWN_RULE', {
        ruleCode: diagnostic.ruleCode,
        target: diagnostic.targetResourceName,
      });
    }

    return translated;
  }

  protected navigateToResource(configId: string, diag: ResourceDiagnosticResponse): void {
    this.dialogRef.close(false);
    const friendlyType = ARM_TYPE_TO_FRIENDLY[diag.resourceType] ?? diag.resourceType;
    this.router.navigate(['/config', configId, 'resource', friendlyType, diag.resourceId]);
  }

  protected navigateToMissingEnvResource(configId: string, resource: MissingEnvResource): void {
    this.dialogRef.close(false);
    const friendlyType = ARM_TYPE_TO_FRIENDLY[resource.resourceType] ?? resource.resourceType;
    this.router.navigate(['/config', configId, 'resource', friendlyType, resource.resourceId]);
  }

  protected navigateToPendingCustomDomainResource(configId: string, issue: PendingCustomDomainIssue): void {
    this.dialogRef.close(false);
    const friendlyType = ARM_TYPE_TO_FRIENDLY[issue.resourceType] ?? issue.resourceType;
    this.router.navigate(['/config', configId, 'resource', friendlyType, issue.resourceId]);
  }

  protected navigateToPendingDockerImageResource(configId: string, issue: PendingDockerImageIssue): void {
    this.dialogRef.close(false);
    const friendlyType = ARM_TYPE_TO_FRIENDLY[issue.resourceType] ?? issue.resourceType;
    this.router.navigate(['/config', configId, 'resource', friendlyType, issue.resourceId]);
  }

  protected getIncompleteEnvMessage(issue: EnvironmentConfigIssue): string {
    if (issue.missingSubscriptionId && issue.missingAzureConnection) {
      return this.translate.instant('GENERATION_DIAGNOSTICS.INCOMPLETE_ENV_BOTH');
    }
    if (issue.missingSubscriptionId) {
      return this.translate.instant('GENERATION_DIAGNOSTICS.INCOMPLETE_ENV_SUBSCRIPTION');
    }
    return this.translate.instant('GENERATION_DIAGNOSTICS.INCOMPLETE_ENV_CONNECTION');
  }

  protected onContinue(): void {
    this.dialogRef.close(true);
  }

  protected onCancel(): void {
    this.dialogRef.close(false);
  }
}
