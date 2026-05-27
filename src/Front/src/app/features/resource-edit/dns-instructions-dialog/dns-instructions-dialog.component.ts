import { Component, inject, OnInit, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { DsSpinnerComponent } from '../../../shared/components/ds/ds-spinner/ds-spinner.component';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { DsAlertComponent } from '../../../shared/components/ds';
import { CustomDomainResponse, DnsInstructionsResponse, DnsInstructionStepResponse } from '../../../shared/interfaces/custom-domain.interface';
import { CustomDomainService } from '../../../shared/services/custom-domain.service';

const ContainerAppResourceType = 'ContainerApp';
const WebAppResourceType = 'WebApp';
const FunctionAppResourceType = 'FunctionApp';
const CnameRecordType = 'CNAME';
const TxtRecordType = 'TXT';
const DnsDialogStepTranslationKeyPrefix = 'RESOURCE_EDIT.CUSTOM_DOMAINS.DNS_DIALOG_STEPS';

export interface DnsInstructionsDialogData {
  resourceId: string;
  resourceType: string;
  domain: CustomDomainResponse;
}

@Component({
  selector: 'app-dns-instructions-dialog',
  standalone: true,
  imports: [
    MatDialogModule,
    MatButtonModule,
    DsAlertComponent,
    MatIconModule,
    DsSpinnerComponent,
    MatSnackBarModule,
    TranslateModule,
  ],
  templateUrl: './dns-instructions-dialog.component.html',
  styleUrl: './dns-instructions-dialog.component.scss',
})
export class DnsInstructionsDialogComponent implements OnInit {
  private readonly data: DnsInstructionsDialogData = inject(MAT_DIALOG_DATA);
  private readonly customDomainService = inject(CustomDomainService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translate = inject(TranslateService);

  protected readonly isLoading = signal(true);
  protected readonly instructions = signal<DnsInstructionsResponse | null>(null);
  protected readonly errorKey = signal('');

  async ngOnInit(): Promise<void> {
    try {
      const result = await this.customDomainService.getDnsInstructions(
        this.data.resourceId,
        this.data.domain.id,
      );
      this.instructions.set(result);
    } catch {
      this.errorKey.set('RESOURCE_EDIT.CUSTOM_DOMAINS.LOAD_ERROR');
    } finally {
      this.isLoading.set(false);
    }
  }

  protected async copyToClipboard(value: string): Promise<void> {
    try {
      await navigator.clipboard.writeText(value);
      this.snackBar.open(
        this.translate.instant('RESOURCE_EDIT.CUSTOM_DOMAINS.DNS_DIALOG_COPIED'),
        undefined,
        { duration: 2000 },
      );
    } catch {
      // Clipboard may be unavailable in non-secure contexts
    }
  }

  protected hasRecord(step: DnsInstructionStepResponse): boolean {
    return step.recordType !== null && step.recordName !== null && step.recordValue !== null;
  }

  protected getStepTitle(step: DnsInstructionStepResponse): string {
    const translationKey = this.getStepTitleTranslationKey(step);
    if (translationKey === null) {
      return step.title;
    }

    return this.translateOrFallback(translationKey, {}, step.title);
  }

  protected getStepDescription(step: DnsInstructionStepResponse): string {
    const translationKey = this.getStepDescriptionTranslationKey(step);
    if (translationKey === null) {
      return step.description;
    }

    return this.translateOrFallback(
      translationKey,
      {
        domainName: this.data.domain.domainName,
        recordName: step.recordName ?? '',
        recordValue: step.recordValue ?? '',
      },
      step.description,
    );
  }

  private getStepTitleTranslationKey(step: DnsInstructionStepResponse): string | null {
    if (step.recordType === CnameRecordType) {
      return `${DnsDialogStepTranslationKeyPrefix}.CNAME_TITLE`;
    }

    if (step.recordType === TxtRecordType) {
      return `${DnsDialogStepTranslationKeyPrefix}.TXT_TITLE`;
    }

    if (this.isValidationStep(step) && this.isSupportedResourceType()) {
      return `${DnsDialogStepTranslationKeyPrefix}.VALIDATE_TITLE`;
    }

    return null;
  }

  private getStepDescriptionTranslationKey(step: DnsInstructionStepResponse): string | null {
    if (step.recordType === CnameRecordType) {
      if (this.data.resourceType === ContainerAppResourceType) {
        return `${DnsDialogStepTranslationKeyPrefix}.CONTAINER_APP_CNAME_DESC`;
      }

      if (this.isWebLikeResourceType()) {
        return `${DnsDialogStepTranslationKeyPrefix}.WEB_LIKE_CNAME_DESC`;
      }
    }

    if (step.recordType === TxtRecordType) {
      if (this.data.resourceType === ContainerAppResourceType) {
        return `${DnsDialogStepTranslationKeyPrefix}.CONTAINER_APP_TXT_DESC`;
      }

      if (this.isWebLikeResourceType()) {
        return `${DnsDialogStepTranslationKeyPrefix}.WEB_LIKE_TXT_DESC`;
      }
    }

    if (this.isValidationStep(step) && this.isSupportedResourceType()) {
      return `${DnsDialogStepTranslationKeyPrefix}.VALIDATE_DESC`;
    }

    return null;
  }

  private isSupportedResourceType(): boolean {
    return this.data.resourceType === ContainerAppResourceType
      || this.data.resourceType === WebAppResourceType
      || this.data.resourceType === FunctionAppResourceType;
  }

  private isWebLikeResourceType(): boolean {
    return this.data.resourceType === WebAppResourceType
      || this.data.resourceType === FunctionAppResourceType;
  }

  private isValidationStep(step: DnsInstructionStepResponse): boolean {
    return step.recordType === null && step.recordName === null && step.recordValue === null;
  }

  private translateOrFallback(
    key: string,
    params: Record<string, string>,
    fallback: string,
  ): string {
    const translated = this.translate.instant(key, params);
    return translated === key ? fallback : translated;
  }
}
