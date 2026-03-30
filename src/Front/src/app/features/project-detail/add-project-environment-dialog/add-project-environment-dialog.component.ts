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
import { EnvironmentDefinitionResponse } from '../../../shared/interfaces/infra-config.interface';
import { ProjectService } from '../../../shared/services/project.service';
import { LOCATION_OPTIONS } from '../../../shared/enums/location.enum';

export interface AddProjectEnvironmentDialogData {
  projectId: string;
  existing?: EnvironmentDefinitionResponse;
  allEnvironments: EnvironmentDefinitionResponse[];
}

@Component({
  selector: 'app-add-project-environment-dialog',
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
  templateUrl: './add-project-environment-dialog.component.html',
  styleUrl: './add-project-environment-dialog.component.scss',
})
export class AddProjectEnvironmentDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<AddProjectEnvironmentDialogComponent>);
  private readonly data: AddProjectEnvironmentDialogData = inject(MAT_DIALOG_DATA);
  private readonly projectService = inject(ProjectService);
  private readonly fb = inject(FormBuilder);

  protected readonly isEditMode = !!this.data.existing;
  protected readonly isSubmitting = signal(false);
  protected readonly errorKey = signal('');
  protected readonly locationOptions = LOCATION_OPTIONS;

  private readonly otherEnvironments = this.data.allEnvironments
    .filter((e) => e.id !== this.data.existing?.id)
    .sort((a, b) => a.order - b.order);

  protected readonly minOrder = 1;
  protected readonly maxOrder = this.otherEnvironments.length + 1;

  private computeInitialPosition(): number {
    if (!this.data.existing) return this.maxOrder;
    const existingOrder = this.data.existing.order;
    return this.otherEnvironments.filter((e) => e.order < existingOrder).length + 1;
  }

  private readonly initialPosition = this.computeInitialPosition();

  protected readonly currentOrder = signal(this.initialPosition);
  protected readonly currentName = signal(this.data.existing?.name ?? '');

  protected readonly timelineItems = computed(() => {
    const position = this.currentOrder();
    const currentName = this.currentName() || '?';
    const insertIdx = position - 1;

    const items: { name: string; isCurrent: boolean }[] = [];

    for (let i = 0; i < this.otherEnvironments.length; i++) {
      if (i === insertIdx) {
        items.push({ name: currentName, isCurrent: true });
      }
      items.push({ name: this.otherEnvironments[i].name, isCurrent: false });
    }

    if (insertIdx >= this.otherEnvironments.length) {
      items.push({ name: currentName, isCurrent: true });
    }

    return items;
  });

  protected readonly form = this.fb.group({
    name: [this.data.existing?.name ?? '', [Validators.required, Validators.minLength(1), Validators.maxLength(100)]],
    location: [this.data.existing?.location ?? '', [Validators.required]],
    subscriptionId: [this.data.existing?.subscriptionId ?? '', [Validators.required]],
    azureResourceManagerConnection: [this.data.existing?.azureResourceManagerConnection ?? ''],
    shortName: [this.data.existing?.shortName ?? ''],
    prefix: [this.data.existing?.prefix ?? ''],
    suffix: [this.data.existing?.suffix ?? ''],
    requiresApproval: [this.data.existing?.requiresApproval ?? false],
  });

  private readonly _syncName = this.form.controls.name.valueChanges.subscribe((v) => this.currentName.set(v ?? ''));

  protected moveOrder(delta: number): void {
    const next = this.currentOrder() + delta;
    if (next >= this.minOrder && next <= this.maxOrder) {
      this.currentOrder.set(next);
    }
  }

  private computeTargetOrder(): number {
    const position = this.currentOrder();
    const others = this.otherEnvironments;

    if (this.isEditMode && position === this.initialPosition) {
      return this.data.existing!.order;
    }

    if (!this.isEditMode) {
      if (position <= others.length) {
        return others[position - 1].order;
      }
      return others.length === 0 ? 1 : others[others.length - 1].order + 1;
    }

    const movingRight = position > this.initialPosition;

    if (movingRight) {
      const idx = position - 2;
      return idx < others.length ? others[idx].order : others[others.length - 1].order;
    }

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
      subscriptionId: values.subscriptionId!,
      shortName: values.shortName || undefined,
      prefix: values.prefix || undefined,
      suffix: values.suffix || undefined,
      order: this.computeTargetOrder(),
      requiresApproval: values.requiresApproval ?? false,
      azureResourceManagerConnection: values.azureResourceManagerConnection || undefined,
    };

    try {
      if (this.isEditMode) {
        await this.projectService.updateEnvironment(
          this.data.projectId,
          this.data.existing!.id,
          payload
        );
      } else {
        await this.projectService.addEnvironment(this.data.projectId, payload);
      }
      this.dialogRef.close(true);
    } catch {
      this.errorKey.set(
        this.isEditMode
          ? 'PROJECT_DETAIL.ENVIRONMENTS.EDIT_DIALOG_ERROR'
          : 'PROJECT_DETAIL.ENVIRONMENTS.ADD_DIALOG_ERROR'
      );
    } finally {
      this.isSubmitting.set(false);
    }
  }
}
