import { Component, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateModule } from '@ngx-translate/core';
import { StorageAccountService } from '../../../shared/services/storage-account.service';
import { StorageAccountResponse } from '../../../shared/interfaces/storage-account.interface';

export interface AddStorageServiceDialogData {
  storageAccountId: string;
  existingBlobNames: string[];
  existingQueueNames: string[];
  existingTableNames: string[];
}

export interface AddStorageServiceDialogResult {
  type: 'BlobContainer' | 'Queue' | 'Table';
  storageAccountResponse: StorageAccountResponse;
}

type StorageServiceType = 'BlobContainer' | 'Queue' | 'Table';

const PUBLIC_ACCESS_OPTIONS = [
  { label: 'RESOURCE_EDIT.STORAGE_SERVICES.PUBLIC_ACCESS_NONE', value: 'None' },
  { label: 'RESOURCE_EDIT.STORAGE_SERVICES.PUBLIC_ACCESS_BLOB', value: 'Blob' },
  { label: 'RESOURCE_EDIT.STORAGE_SERVICES.PUBLIC_ACCESS_CONTAINER', value: 'Container' },
];

@Component({
  selector: 'app-add-storage-service-dialog',
  standalone: true,
  imports: [
    TranslateModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatSelectModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './add-storage-service-dialog.component.html',
  styleUrl: './add-storage-service-dialog.component.scss',
})
export class AddStorageServiceDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<AddStorageServiceDialogComponent>);
  private readonly data = inject<AddStorageServiceDialogData>(MAT_DIALOG_DATA);
  private readonly storageAccountService = inject(StorageAccountService);

  protected readonly step = signal<'type' | 'config'>('type');
  protected readonly selectedType = signal<StorageServiceType | null>(null);
  protected readonly isSubmitting = signal(false);
  protected readonly errorKey = signal('');
  protected readonly publicAccessOptions = PUBLIC_ACCESS_OPTIONS;

  protected readonly form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(63)]],
    publicAccess: ['None'],
  });

  protected selectType(type: StorageServiceType): void {
    this.selectedType.set(type);
    this.step.set('config');
    this.form.reset({ name: '', publicAccess: 'None' });
    this.form.get('name')!.clearValidators();
    this.form.get('name')!.setValidators([
      Validators.required,
      Validators.maxLength(63),
      this.uniqueNameValidator(type),
    ]);
    this.form.get('name')!.updateValueAndValidity();
    this.errorKey.set('');
  }

  protected goBack(): void {
    this.step.set('type');
    this.selectedType.set(null);
    this.errorKey.set('');
  }

  protected async onSubmit(): Promise<void> {
    if (this.form.invalid || this.isSubmitting()) return;

    this.isSubmitting.set(true);
    this.errorKey.set('');

    const { name, publicAccess } = this.form.getRawValue();
    const type = this.selectedType()!;

    try {
      let response: StorageAccountResponse;
      switch (type) {
        case 'BlobContainer':
          response = await this.storageAccountService.addBlobContainer(this.data.storageAccountId, {
            name: name!,
            publicAccess: publicAccess ?? 'None',
          });
          break;
        case 'Queue':
          response = await this.storageAccountService.addQueue(this.data.storageAccountId, {
            name: name!,
          });
          break;
        case 'Table':
          response = await this.storageAccountService.addTable(this.data.storageAccountId, {
            name: name!,
          });
          break;
      }
      this.dialogRef.close({ type, storageAccountResponse: response } satisfies AddStorageServiceDialogResult);
    } catch {
      this.errorKey.set('RESOURCE_EDIT.STORAGE_SERVICES.ACTION_ERROR');
    } finally {
      this.isSubmitting.set(false);
    }
  }

  private uniqueNameValidator(type: StorageServiceType) {
    return (control: AbstractControl): ValidationErrors | null => {
      const name = control.value?.trim().toLowerCase();
      if (!name) return null;
      let existingNames: string[];
      switch (type) {
        case 'BlobContainer': existingNames = this.data.existingBlobNames; break;
        case 'Queue': existingNames = this.data.existingQueueNames; break;
        case 'Table': existingNames = this.data.existingTableNames; break;
      }
      const exists = existingNames.some(n => n.toLowerCase() === name);
      return exists ? { duplicateName: true } : null;
    };
  }

  protected onCancel(): void {
    this.dialogRef.close();
  }
}
