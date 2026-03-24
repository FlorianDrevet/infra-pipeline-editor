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

  /** Order bounds use a 1-based position range and include the final slot after the last environment. */
  protected readonly minOrder = 1;
  protected readonly maxOrder = this.otherEnvironments.length + 1;

  /**
   * Convert the existing backend order into a 1-based position among other environments.
   * Position = how many other envs come before + 1.
   */
  private computeInitialPosition(): number {
    if (!this.data.existing) return this.maxOrder;
    const existingOrder = this.data.existing.order;
    return this.otherEnvironments.filter((e) => e.order < existingOrder).length + 1;
  }

  /** The initial position (used to detect "no actual move" on submit). */
  private readonly initialPosition = this.computeInitialPosition();

  /** Reactive signal tracking the current 1-based position. */
  protected readonly currentOrder = signal(this.initialPosition);

  /** Reactive signal tracking the name typed in the form. */
  protected readonly currentName = signal(this.data.existing?.name ?? '');

  /** Computed timeline that inserts the current environment at the right position among others. */
  protected readonly timelineItems = computed(() => {
    const position = this.currentOrder(); // 1-based
    const currentName = this.currentName() || '?';
    const insertIdx = position - 1; // 0-based index

    const items: { name: string; isCurrent: boolean }[] = [];

    for (let i = 0; i < this.otherEnvironments.length; i++) {
      if (i === insertIdx) {
        items.push({ name: currentName, isCurrent: true });
      }
      items.push({ name: this.otherEnvironments[i].name, isCurrent: false });
    }

    // If position is after all others, append at the end
    if (insertIdx >= this.otherEnvironments.length) {
      items.push({ name: currentName, isCurrent: true });
    }

    return items;
  });

  protected readonly form = this.fb.group({
    name: [this.data.existing?.name ?? '', [Validators.required, Validators.minLength(1), Validators.maxLength(100)]],
    location: [this.data.existing?.location ?? '', [Validators.required]],
    tenantId: [this.data.existing?.tenantId ?? '', [Validators.required]],
    subscriptionId: [this.data.existing?.subscriptionId ?? '', [Validators.required]],
    shortName: [this.data.existing?.shortName ?? ''],
    prefix: [this.data.existing?.prefix ?? ''],
    suffix: [this.data.existing?.suffix ?? ''],
    requiresApproval: [this.data.existing?.requiresApproval ?? false],
  });

  private readonly _syncName = this.form.controls.name.valueChanges.subscribe((v) => this.currentName.set(v ?? ''));

  /** Move the current environment position by the given delta (-1 = left, +1 = right). */
  protected moveOrder(delta: number): void {
    const next = this.currentOrder() + delta;
    if (next >= this.minOrder && next <= this.maxOrder) {
      this.currentOrder.set(next);
    }
  }

  /**
   * Convert the visual position (1-based) back to a backend order value
   * that will produce the correct result when processed by the domain's
   * ShiftOrdersUp (add) / ReorderEnvironments (update) logic.
   *
   * Key insight: the backend ReorderEnvironments works by shifting a range:
   *   - Moving left  (newOrder < oldOrder): shifts [newOrder, oldOrder) UP by 1
   *   - Moving right (newOrder > oldOrder): shifts (oldOrder, newOrder] DOWN by 1
   *
   * So for "move left"  we send the order of the env AT the target slot (others[pos-1]).
   * For "move right" we send the order of the env we JUMP OVER   (others[pos-2]).
   */
  private computeTargetOrder(): number {
    const position = this.currentOrder();
    const others = this.otherEnvironments;

    // No move in edit mode → keep original order unchanged
    if (this.isEditMode && position === this.initialPosition) {
      return this.data.existing!.order;
    }

    // Create mode: send the order of the env at the target slot; ShiftOrdersUp makes room
    if (!this.isEditMode) {
      if (position <= others.length) {
        return others[position - 1].order;
      }
      return others.length === 0 ? 1 : others[others.length - 1].order + 1;
    }

    // Edit mode — direction matters
    const movingRight = position > this.initialPosition;

    if (movingRight) {
      // Send the order of the env just before the target slot (the one we jump over)
      const idx = position - 2;
      return idx < others.length ? others[idx].order : others[others.length - 1].order;
    }

    // Moving left: send the order of the env at the target slot
    return others[position - 1].order;
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
      shortName: values.shortName || undefined,
      prefix: values.prefix || undefined,
      suffix: values.suffix || undefined,
      order: this.computeTargetOrder(),
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
