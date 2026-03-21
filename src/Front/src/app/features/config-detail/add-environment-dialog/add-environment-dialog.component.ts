import { Component, computed, inject, signal } from '@angular/core';
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
  allEnvironments: EnvironmentDefinitionResponse[];
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

  /** Other environments (excluding the one being edited). */
  private readonly otherEnvironments = this.data.allEnvironments
    .filter((e) => e.id !== this.data.existing?.id)
    .sort((a, b) => a.order - b.order);

  /** Order bounds: in create mode min = 0, max = count; in edit mode same logic excluding self. */
  protected readonly minOrder = 0;
  protected readonly maxOrder = this.otherEnvironments.length;

  /** Reactive signal tracking the current order value from the form. */
  protected readonly currentOrder = signal(this.data.existing?.order ?? this.otherEnvironments.length);

  /** Computed timeline that inserts the current environment at the right position among others. */
  protected readonly timelineItems = computed(() => {
    const order = this.currentOrder();
    const currentName = this.data.existing?.name ?? '?';

    // Build sorted list of other envs as timeline items
    const others: { name: string; order: number; isCurrent: boolean }[] = this.otherEnvironments.map((e) => ({
      name: e.name,
      order: e.order,
      isCurrent: false,
    }));

    // Insert the current env at the position indicated by the order value
    const insertIdx = others.findIndex((item) => item.order >= order);
    const current = { name: currentName, order, isCurrent: true };

    if (insertIdx === -1) {
      others.push(current);
    } else {
      others.splice(insertIdx, 0, current);
    }

    return others;
  });

  protected readonly form = this.fb.group({
    name: [this.data.existing?.name ?? '', [Validators.required, Validators.minLength(1), Validators.maxLength(100)]],
    location: [this.data.existing?.location ?? '', [Validators.required]],
    tenantId: [this.data.existing?.tenantId ?? '', [Validators.required]],
    subscriptionId: [this.data.existing?.subscriptionId ?? '', [Validators.required]],
    prefix: [this.data.existing?.prefix ?? ''],
    suffix: [this.data.existing?.suffix ?? ''],
    requiresApproval: [this.data.existing?.requiresApproval ?? false],
  });

  /** Move the current environment position by the given delta (-1 = left, +1 = right). */
  protected moveOrder(delta: number): void {
    const next = this.currentOrder() + delta;
    if (next >= this.minOrder && next <= this.maxOrder) {
      this.currentOrder.set(next);
    }
  }

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
      order: this.currentOrder(),
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
