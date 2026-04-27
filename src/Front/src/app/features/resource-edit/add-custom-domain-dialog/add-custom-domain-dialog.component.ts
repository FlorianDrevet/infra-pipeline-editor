import { Component, inject, signal, computed } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { DsButtonComponent, DsSelectComponent, DsTextFieldComponent, type DsSelectOption } from '../../../shared/components/ds';
import { MatIconModule } from '@angular/material/icon';
import { MatRadioModule } from '@angular/material/radio';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { EnvironmentDefinitionResponse } from '../../../shared/interfaces/infra-config.interface';
import { CustomDomainResponse, AddCustomDomainRequest } from '../../../shared/interfaces/custom-domain.interface';

export interface AddCustomDomainDialogData {
  environments: EnvironmentDefinitionResponse[];
  existingDomains: CustomDomainResponse[];
  preselectedEnvironment?: string;
}

@Component({
  selector: 'app-add-custom-domain-dialog',
  standalone: true,
  imports: [
    TranslateModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatRadioModule,
      DsButtonComponent,
      DsSelectComponent,
      DsTextFieldComponent,
  ],
  templateUrl: './add-custom-domain-dialog.component.html',
  styleUrl: './add-custom-domain-dialog.component.scss',
})
export class AddCustomDomainDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<AddCustomDomainDialogComponent>);
  protected readonly data: AddCustomDomainDialogData = inject(MAT_DIALOG_DATA);

  protected readonly environmentName = signal(this.data.preselectedEnvironment ?? '');
  protected readonly domainName = signal('');
  protected readonly bindingType = signal('SniEnabled');

  protected readonly environmentOptions = computed<DsSelectOption[]>(() =>
    this.data.environments.map((env) => ({ value: env.name, label: env.name })),
  );

  private readonly fqdnPattern = /^(?!-)[a-zA-Z0-9-]{1,63}(?<!-)(\.[a-zA-Z0-9-]{1,63})*\.[a-zA-Z]{2,}$/;

  protected readonly isDomainValid = computed(() => {
    const domain = this.domainName().trim();
    return domain.length > 0 && domain.length <= 253 && this.fqdnPattern.test(domain);
  });

  protected readonly isDuplicate = computed(() => {
    const domain = this.domainName().trim().toLowerCase();
    const env = this.environmentName();
    return this.data.existingDomains.some(
      d => d.domainName.toLowerCase() === domain && d.environmentName === env
    );
  });

  protected readonly isFormValid = computed(() =>
    this.environmentName() !== '' && this.isDomainValid() && !this.isDuplicate()
  );

  protected onConfirm(): void {
    if (!this.isFormValid()) return;
    const result: AddCustomDomainRequest = {
      environmentName: this.environmentName(),
      domainName: this.domainName().trim(),
      bindingType: this.bindingType(),
    };
    this.dialogRef.close(result);
  }

  protected onCancel(): void {
    this.dialogRef.close(null);
  }
}
