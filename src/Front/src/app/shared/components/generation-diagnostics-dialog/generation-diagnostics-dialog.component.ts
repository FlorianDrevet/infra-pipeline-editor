import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ResourceDiagnosticResponse } from '../../interfaces/bicep-generator.interface';
import { RESOURCE_TYPE_ABBREVIATIONS } from '../../../features/config-detail/enums/resource-type.enum';

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

export interface GenerationDiagnosticsDialogData {
  configDiagnostics: ConfigDiagnosticGroup[];
  missingEnvConfigs?: ConfigMissingEnvGroup[];
}

@Component({
  selector: 'app-generation-diagnostics-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatDialogModule,
    MatIconModule,
    MatTooltipModule,
    TranslateModule,
  ],
  templateUrl: './generation-diagnostics-dialog.component.html',
  styleUrl: './generation-diagnostics-dialog.component.scss',
})
export class GenerationDiagnosticsDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<GenerationDiagnosticsDialogComponent>);
  private readonly router = inject(Router);
  private readonly translate = inject(TranslateService);
  protected readonly data: GenerationDiagnosticsDialogData = inject(MAT_DIALOG_DATA);

  protected readonly totalDiagnostics = this.data.configDiagnostics.reduce(
    (sum, g) => sum + g.diagnostics.length, 0,
  );

  protected readonly totalMissingEnvIssues = (this.data.missingEnvConfigs ?? []).reduce(
    (sum, g) => sum + g.resources.length, 0,
  );

  protected readonly totalIssues = this.totalDiagnostics + this.totalMissingEnvIssues;

  protected readonly hasDiagnostics = this.totalDiagnostics > 0;
  protected readonly hasMissingEnvs = this.totalMissingEnvIssues > 0;

  protected readonly isMultiConfig = (() => {
    const configIds = new Set<string>();
    for (const g of this.data.configDiagnostics) configIds.add(g.configId);
    for (const g of this.data.missingEnvConfigs ?? []) configIds.add(g.configId);
    return configIds.size > 1;
  })();

  protected getSeverityIcon(severity: string): string {
    return severity?.toLowerCase() === 'warning' ? 'warning' : 'gpp_bad';
  }

  protected getSeverityClass(severity: string): string {
    return severity?.toLowerCase() === 'warning' ? 'warning' : 'error';
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

  protected onContinue(): void {
    this.dialogRef.close(true);
  }

  protected onCancel(): void {
    this.dialogRef.close(false);
  }
}
