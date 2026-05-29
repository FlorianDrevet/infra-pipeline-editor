import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';

import { DsButtonComponent } from '../../../../../shared/components/ds/ds-button/ds-button.component';

@Component({
  selector: 'app-dns-help-dialog',
  standalone: true,
  imports: [DsButtonComponent, MatDialogModule, MatIconModule, TranslateModule],
  templateUrl: './dns-help-dialog.component.html',
  styleUrl: './dns-help-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DnsHelpDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<DnsHelpDialogComponent>);

  protected close(): void {
    this.dialogRef.close();
  }
}
