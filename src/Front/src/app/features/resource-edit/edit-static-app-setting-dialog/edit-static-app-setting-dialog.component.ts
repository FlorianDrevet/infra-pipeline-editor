import { Component, inject, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { DsButtonComponent, DsTextFieldComponent } from '../../../shared/components/ds';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import axios from 'axios';
import { AppSettingService } from '../../../shared/services/app-setting.service';
import { AppSettingResponse } from '../../../shared/interfaces/app-setting.interface';

export interface EditStaticAppSettingDialogData {
  resourceId: string;
  appSettingId: string;
  currentName: string;
  currentEnvironmentValues: Record<string, string>;
  environments: { name: string }[];
}

@Component({
  selector: 'app-edit-static-app-setting-dialog',
  standalone: true,
  imports: [
    TranslateModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
      DsButtonComponent,
      DsTextFieldComponent,
  ],
  templateUrl: './edit-static-app-setting-dialog.component.html',
  styleUrl: './edit-static-app-setting-dialog.component.scss',
})
export class EditStaticAppSettingDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<EditStaticAppSettingDialogComponent>);
  protected readonly data: EditStaticAppSettingDialogData = inject(MAT_DIALOG_DATA);
  private readonly appSettingService = inject(AppSettingService);

  protected readonly settingName = signal(this.data.currentName);
  protected readonly environmentValues = signal<Record<string, string>>({ ...this.data.currentEnvironmentValues });

  protected readonly isSubmitting = signal(false);
  protected readonly errorKey = signal('');

  protected updateEnvironmentValue(envName: string, value: string): void {
    this.environmentValues.update(vals => ({ ...vals, [envName]: value }));
  }

  protected async onSave(): Promise<void> {
    this.isSubmitting.set(true);
    this.errorKey.set('');

    try {
      const result = await this.appSettingService.update(
        this.data.resourceId,
        this.data.appSettingId,
        {
          name: this.settingName(),
          environmentValues: this.environmentValues(),
        }
      );
      this.dialogRef.close(result);
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.status === 409) {
        this.errorKey.set('RESOURCE_EDIT.EDIT_APP_SETTING_DIALOG.ERROR_DUPLICATE');
      } else {
        this.errorKey.set('RESOURCE_EDIT.EDIT_APP_SETTING_DIALOG.ERROR');
      }
    } finally {
      this.isSubmitting.set(false);
    }
  }

  protected onCancel(): void {
    this.dialogRef.close();
  }
}
