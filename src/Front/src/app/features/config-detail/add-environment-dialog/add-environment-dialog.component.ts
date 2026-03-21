import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatOptionModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { TranslateModule } from '@ngx-translate/core';
import {
  EnvironmentDefinitionResponse,
  InfrastructureConfigResponse,
} from '../../../shared/interfaces/infra-config.interface';
import { InfraConfigService } from '../../../shared/services/infra-config.service';
import { LOCATION_OPTIONS } from '../enums/location.enum';

export interface AddEnvironmentDialogData {
  configId: string;
  existing?: EnvironmentDefinitionResponse;
}

@Component({
  selector: 'app-add-environment-dialog',
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
    MatSlideToggleModule,
    ReactiveFormsModule,
    TranslateModule,
  ],
  templateUrl: './add-environment-dialog.component.html',
  styleUrl: './add-environment-dialog.component.scss',
})
export class AddEnvironmentDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<AddEnvironmentDialogComponent>);
  private readonly data: AddEnvironmentDialogData = inject(MAT_DIALOG_DATA);
  private readonly infraConfigService = inject(InfraConfigService);
  private readonly fb = inject(FormBuilder);

  protected readonly isEditMode = !!this.data.existing;
  protected readonly isSubmitting = signal(false);
  protected readonly errorKey = signal('');
  protected readonly locationOptions = LOCATION_OPTIONS;

  protected readonly form = this.fb.group({
    name: [this.data.existing?.name ?? '', [Validators.required, Validators.minLength(1), Validators.maxLength(100)]],
    location: [this.data.existing?.location ?? '', [Validators.required]],
    tenantId: [this.data.existing?.tenantId ?? '', [Validators.required]],
    subscriptionId: [this.data.existing?.subscriptionId ?? '', [Validators.required]],
    prefix: [this.data.existing?.prefix ?? ''],
    suffix: [this.data.existing?.suffix ?? ''],
    order: [this.data.existing?.order ?? 0],
    requiresApproval: [this.data.existing?.requiresApproval ?? false],
  });

  protected onCancel(): void {
    this.dialogRef.close();
  }

  protected async onSubmit(): Promise<void> {
    if (this.form.invalid) return;

    this.isSubmitting.set(true);
    this.errorKey.set('');

    const values = this.form.getRawValue();
    const payload = {
      name: values.name!,
      location: values.location!,
      tenantId: values.tenantId!,
      subscriptionId: values.subscriptionId!,
      prefix: values.prefix || undefined,
      suffix: values.suffix || undefined,
      order: values.order ?? 0,
      requiresApproval: values.requiresApproval ?? false,
    };

    try {
      let result: InfrastructureConfigResponse;
      if (this.isEditMode) {
        result = await this.infraConfigService.updateEnvironment(
          this.data.configId,
          this.data.existing!.id,
          payload
        );
      } else {
        result = await this.infraConfigService.addEnvironment(this.data.configId, payload);
      }
      this.dialogRef.close(result);
    } catch {
      this.errorKey.set(
        this.isEditMode
          ? 'CONFIG_DETAIL.ENVIRONMENTS.EDIT_DIALOG_ERROR'
          : 'CONFIG_DETAIL.ENVIRONMENTS.ADD_DIALOG_ERROR'
      );
    } finally {
      this.isSubmitting.set(false);
    }
  }
}
