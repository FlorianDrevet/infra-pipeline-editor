import { Component, inject, signal } from '@angular/core';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { InfraConfigService } from '../../../shared/services/infra-config.service';

@Component({
  selector: 'app-create-config-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule],
  templateUrl: './create-config-dialog.component.html',
})
export class CreateConfigDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<CreateConfigDialogComponent>);
  private readonly infraConfigService = inject(InfraConfigService);

  protected configName = signal('');
  protected isLoading = signal(false);

  protected submit(): void {
    if (!this.configName() || this.isLoading()) return;
    void this.doSubmit();
  }

  private async doSubmit(): Promise<void> {
    try {
      this.isLoading.set(true);
      const result = await this.infraConfigService.create({ name: this.configName() });
      this.dialogRef.close(result);
    } catch {
      this.isLoading.set(false);
    }
  }
}
