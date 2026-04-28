import { Component, inject, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

import {
  DsButtonComponent,
  DsTextFieldComponent,
  DsAlertComponent,
  DsDatePickerComponent,
} from '../../../shared/components/ds';
import { PersonalAccessTokenService } from '../../../shared/services/personal-access-token.service';

@Component({
  selector: 'app-create-pat-dialog',
  standalone: true,
  imports: [
    TranslateModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    DsButtonComponent,
    DsTextFieldComponent,
    DsDatePickerComponent,
    DsAlertComponent,
  ],
  templateUrl: './create-pat-dialog.component.html',
  styleUrl: './create-pat-dialog.component.scss',
})
export class CreatePatDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<CreatePatDialogComponent>);
  private readonly patService = inject(PersonalAccessTokenService);

  protected readonly tokenName = signal('');
  protected readonly expiresAt = signal('');
  protected readonly isSubmitting = signal(false);
  protected readonly errorKey = signal('');

  protected readonly createdToken = signal('');
  protected readonly tokenCreated = signal(false);
  protected readonly copied = signal(false);

  protected async onCreate(): Promise<void> {
    if (!this.tokenName().trim() || this.isSubmitting()) {
      return;
    }

    this.isSubmitting.set(true);
    this.errorKey.set('');

    try {
      const expiryValue = this.expiresAt().trim();
      const result = await this.patService.create({
        name: this.tokenName().trim(),
        expiresAt: expiryValue ? new Date(expiryValue).toISOString() : null,
      });

      this.createdToken.set(result.plainTextToken);
      this.tokenCreated.set(true);
    } catch {
      this.errorKey.set('SETTINGS.CREATE_DIALOG.ERROR');
    } finally {
      this.isSubmitting.set(false);
    }
  }

  protected async copyToken(): Promise<void> {
    try {
      await navigator.clipboard.writeText(this.createdToken());
      this.copied.set(true);
    } catch {
      // Fallback: select text for manual copy
    }
  }

  protected onDone(): void {
    this.dialogRef.close(true);
  }

  protected onCancel(): void {
    this.dialogRef.close(false);
  }
}
