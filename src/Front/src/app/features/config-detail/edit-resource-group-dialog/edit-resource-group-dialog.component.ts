import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { DsButtonComponent, DsSelectComponent, DsTextFieldComponent } from '../../../shared/components/ds';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';
import { ResourceGroupResponse } from '../../../shared/interfaces/resource-group.interface';
import { ResourceGroupService } from '../../../shared/services/resource-group.service';
import { LOCATION_OPTIONS } from '../enums/location.enum';

export interface EditResourceGroupDialogData {
  resourceGroup: ResourceGroupResponse;
}

@Component({
  selector: 'app-edit-resource-group-dialog',
  standalone: true,
  imports: [
    MatDialogModule,
    MatIconModule,
    ReactiveFormsModule,
    TranslateModule,
    DsButtonComponent,
    DsSelectComponent,
    DsTextFieldComponent,
  ],
  templateUrl: './edit-resource-group-dialog.component.html',
  styleUrl: './edit-resource-group-dialog.component.scss',
})
export class EditResourceGroupDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<EditResourceGroupDialogComponent>);
  private readonly data: EditResourceGroupDialogData = inject(MAT_DIALOG_DATA);
  private readonly resourceGroupService = inject(ResourceGroupService);
  private readonly fb = inject(FormBuilder);

  protected readonly isSubmitting = signal(false);
  protected readonly errorKey = signal('');
  protected readonly locationOptions = LOCATION_OPTIONS;

  protected readonly form = this.fb.group({
    name: [this.data.resourceGroup.name, [Validators.required, Validators.minLength(1), Validators.maxLength(80)]],
    location: [this.data.resourceGroup.location, [Validators.required]],
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
      const result: ResourceGroupResponse = await this.resourceGroupService.update(
        this.data.resourceGroup.id,
        {
          name: values.name!,
          location: values.location!,
        }
      );
      this.dialogRef.close(result);
    } catch {
      this.errorKey.set('CONFIG_DETAIL.RESOURCE_GROUPS.EDIT_DIALOG_ERROR');
    } finally {
      this.isSubmitting.set(false);
    }
  }
}
