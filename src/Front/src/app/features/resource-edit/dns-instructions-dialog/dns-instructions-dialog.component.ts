import { Component, inject, OnInit, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { CustomDomainResponse, DnsInstructionsResponse, DnsInstructionStepResponse } from '../../../shared/interfaces/custom-domain.interface';
import { CustomDomainService } from '../../../shared/services/custom-domain.service';

export interface DnsInstructionsDialogData {
  resourceId: string;
  domain: CustomDomainResponse;
}

@Component({
  selector: 'app-dns-instructions-dialog',
  standalone: true,
  imports: [
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
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
}
