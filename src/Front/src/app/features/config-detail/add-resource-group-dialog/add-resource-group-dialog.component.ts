import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { DsButtonComponent } from '../../../shared/components/ds';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatOptionModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { TranslateModule } from '@ngx-translate/core';
import { ResourceGroupResponse } from '../../../shared/interfaces/resource-group.interface';
import { ResourceGroupService } from '../../../shared/services/resource-group.service';
import { LOCATION_OPTIONS } from '../enums/location.enum';

export interface AddResourceGroupDialogData {
  infraConfigId: string;
}

@Component({
  selector: 'app-add-resource-group-dialog',
  standalone: true,
  imports: [
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatOptionModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    ReactiveFormsModule,
    TranslateModule,
      DsButtonComponent,
  ],
  templateUrl: './add-resource-group-dialog.component.html',
  styleUrl: './add-resource-group-dialog.component.scss',
})
export class AddResourceGroupDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<AddResourceGroupDialogComponent>);
  private readonly data: AddResourceGroupDialogData = inject(MAT_DIALOG_DATA);
  private readonly resourceGroupService = inject(ResourceGroupService);
  private readonly fb = inject(FormBuilder);

  protected readonly isSubmitting = signal(false);
  protected readonly errorKey = signal('');
  protected readonly locationOptions = LOCATION_OPTIONS;

  protected readonly form = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(1), Validators.maxLength(80)]],
    location: ['', [Validators.required]],
  });

  protected onCancel(): void {
    this.dialogRef.close();
  }

  protected async onSubmit(): Promise<void> {
    if (this.form.invalid) return;

    this.isSubmitting.set(true);
    this.errorKey.set('');

    const values = this.form.getRawValue();

    try {
      const result: ResourceGroupResponse = await this.resourceGroupService.create({
        infraConfigId: this.data.infraConfigId,
        name: values.name!,
        location: values.location!,
      });
      this.dialogRef.close(result);
    } catch {
      this.errorKey.set('CONFIG_DETAIL.RESOURCE_GROUPS.ADD_DIALOG_ERROR');
    } finally {
      this.isSubmitting.set(false);
    }
  }
}
