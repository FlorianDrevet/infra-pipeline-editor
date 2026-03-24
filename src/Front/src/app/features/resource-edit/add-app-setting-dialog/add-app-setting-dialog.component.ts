import { Component, inject, signal, computed } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatRadioModule } from '@angular/material/radio';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { AzureResourceResponse } from '../../../shared/interfaces/resource-group.interface';
import { AppSettingService } from '../../../shared/services/app-setting.service';
import { AppSettingResponse, OutputDefinitionResponse } from '../../../shared/interfaces/app-setting.interface';
import { RESOURCE_TYPE_ICONS } from '../../config-detail/enums/resource-type.enum';

export interface AddAppSettingDialogData {
  resourceId: string;
  currentResourceName: string;
  siblingResources: AzureResourceResponse[];
}

@Component({
  selector: 'app-add-app-setting-dialog',
  standalone: true,
  imports: [
    TranslateModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatRadioModule,
    MatSelectModule,
    MatFormFieldModule,
    MatTooltipModule,
  ],
  templateUrl: './add-app-setting-dialog.component.html',
  styleUrl: './add-app-setting-dialog.component.scss',
})
export class AddAppSettingDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<AddAppSettingDialogComponent>);
  private readonly data: AddAppSettingDialogData = inject(MAT_DIALOG_DATA);
  private readonly appSettingService = inject(AppSettingService);

  protected readonly resourceTypeIcons = RESOURCE_TYPE_ICONS;

  // ─── Mode: static value or resource output ───
  protected readonly mode = signal<'static' | 'output'>('output');

  // ─── Step management ───
  protected readonly step = signal<1 | 2 | 3>(1);

  // ─── Step 1 — Source selection (output mode) ───
  protected readonly sourceResources = computed(() => this.data.siblingResources);
  protected readonly selectedSource = signal<AzureResourceResponse | null>(null);

  // ─── Step 2 — Output selection ───
  protected readonly availableOutputs = signal<OutputDefinitionResponse[]>([]);
  protected readonly outputsLoading = signal(false);
  protected readonly selectedOutput = signal<OutputDefinitionResponse | null>(null);

  // ─── Step 3 / Static — Name configuration ───
  protected readonly settingName = signal('');
  protected readonly staticValue = signal('');

  // ─── Submit state ───
  protected readonly isSubmitting = signal(false);
  protected readonly errorKey = signal('');

  protected readonly suggestedName = computed(() => {
    const source = this.selectedSource();
    const output = this.selectedOutput();
    if (!source || !output) return '';
    const prefix = source.name.toUpperCase().replace(/[^A-Z0-9]/g, '_');
    const suffix = output.name.toUpperCase().replace(/[^A-Z0-9]/g, '_');
    return `${prefix}__${suffix}`;
  });

  protected readonly canSubmit = computed(() => {
    const name = this.settingName().trim();
    if (!name || this.isSubmitting()) return false;
    if (this.mode() === 'static') return true;
    return !!this.selectedSource() && !!this.selectedOutput();
  });

  protected onModeChange(value: 'static' | 'output'): void {
    this.mode.set(value);
    this.step.set(1);
    this.selectedSource.set(null);
    this.selectedOutput.set(null);
    this.settingName.set('');
    this.staticValue.set('');
    this.errorKey.set('');
  }

  protected selectSource(resource: AzureResourceResponse): void {
    this.selectedSource.set(resource);
    this.goToStep2();
  }

  protected async goToStep2(): Promise<void> {
    this.step.set(2);
    this.errorKey.set('');
    this.selectedOutput.set(null);

    const source = this.selectedSource();
    if (!source) return;

    this.outputsLoading.set(true);
    try {
      const response = await this.appSettingService.getAvailableOutputs(source.id);
      this.availableOutputs.set(response.outputs);
    } catch {
      this.availableOutputs.set([]);
    } finally {
      this.outputsLoading.set(false);
    }
  }

  protected selectOutput(output: OutputDefinitionResponse): void {
    this.selectedOutput.set(output);
    this.settingName.set(this.suggestedName());
    this.step.set(3);
  }

  protected goBack(): void {
    if (this.step() === 3) {
      this.step.set(2);
    } else if (this.step() === 2) {
      this.step.set(1);
    }
    this.errorKey.set('');
  }

  protected async onSubmit(): Promise<void> {
    const name = this.settingName().trim();
    if (!name || this.isSubmitting()) return;

    this.isSubmitting.set(true);
    this.errorKey.set('');

    try {
      const request = this.mode() === 'static'
        ? { name, staticValue: this.staticValue() }
        : {
            name,
            sourceResourceId: this.selectedSource()!.id,
            sourceOutputName: this.selectedOutput()!.name,
          };

      const result = await this.appSettingService.add(this.data.resourceId, request);
      this.dialogRef.close(result);
    } catch {
      this.errorKey.set('RESOURCE_EDIT.ADD_APP_SETTING_DIALOG.ERROR');
    } finally {
      this.isSubmitting.set(false);
    }
  }

  protected onCancel(): void {
    this.dialogRef.close(undefined);
  }
}
