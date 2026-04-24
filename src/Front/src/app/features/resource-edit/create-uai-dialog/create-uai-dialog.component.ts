import { Component, inject, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { DsButtonComponent } from '../../../shared/components/ds';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import axios from 'axios';
import { UserAssignedIdentityService } from '../../../shared/services/user-assigned-identity.service';
import { UserAssignedIdentityResponse } from '../../../shared/interfaces/user-assigned-identity.interface';

export interface CreateUaiDialogData {
  resourceGroupId: string;
  location: string;
}

@Component({
  selector: 'app-create-uai-dialog',
  standalone: true,
  imports: [
    TranslateModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatFormFieldModule,
      DsButtonComponent,
  ],
  templateUrl: './create-uai-dialog.component.html',
  styleUrl: './create-uai-dialog.component.scss',
})
export class CreateUaiDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<CreateUaiDialogComponent>);
  protected readonly data: CreateUaiDialogData = inject(MAT_DIALOG_DATA);
  private readonly uaiService = inject(UserAssignedIdentityService);

  protected readonly name = signal('');
  protected readonly isSubmitting = signal(false);
  protected readonly errorKey = signal('');

  protected async onCreate(): Promise<void> {
    this.isSubmitting.set(true);
    this.errorKey.set('');

    try {
      const result = await this.uaiService.create({
        resourceGroupId: this.data.resourceGroupId,
        name: this.name(),
        location: this.data.location,
      });
      this.dialogRef.close(result);
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.status === 409) {
        this.errorKey.set('RESOURCE_EDIT.CREATE_UAI_DIALOG.ERROR_CONFLICT');
      } else {
        this.errorKey.set('RESOURCE_EDIT.CREATE_UAI_DIALOG.ERROR');
      }
    } finally {
      this.isSubmitting.set(false);
    }
  }

  protected onCancel(): void {
    this.dialogRef.close();
  }
}
