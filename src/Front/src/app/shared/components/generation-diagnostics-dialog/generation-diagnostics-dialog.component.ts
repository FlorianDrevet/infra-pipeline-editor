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

export interface ConfigDiagnosticGroup {
  configId: string;
  configName: string;
  diagnostics: ResourceDiagnosticResponse[];
}

export interface GenerationDiagnosticsDialogData {
  configDiagnostics: ConfigDiagnosticGroup[];
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

  protected readonly totalIssues = this.data.configDiagnostics.reduce(
    (sum, g) => sum + g.diagnostics.length, 0,
  );

  protected readonly isMultiConfig = this.data.configDiagnostics.length > 1;

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

  protected navigateToResource(configId: string, resourceId: string): void {
    this.dialogRef.close(false);
    this.router.navigate(['/configs', configId], {
      queryParams: { highlightResource: resourceId },
    });
  }

  protected onContinue(): void {
    this.dialogRef.close(true);
  }

  protected onCancel(): void {
    this.dialogRef.close(false);
  }
}
