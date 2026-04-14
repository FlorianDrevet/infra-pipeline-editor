import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';
import { RoleAssignmentImpactResponse } from '../../../shared/interfaces/role-assignment.interface';

export interface RoleAssignmentImpactDialogData {
  roleName: string;
  targetResourceName: string;
  impactResult: RoleAssignmentImpactResponse;
}

@Component({
  selector: 'app-role-assignment-impact-dialog',
  standalone: true,
  imports: [
    TranslateModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
  ],
  templateUrl: './role-assignment-impact-dialog.component.html',
  styleUrl: './role-assignment-impact-dialog.component.scss',
})
export class RoleAssignmentImpactDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<RoleAssignmentImpactDialogComponent>);
  private readonly data = inject<RoleAssignmentImpactDialogData>(MAT_DIALOG_DATA);

  protected readonly filteredImpacts = this.data.impactResult.impacts.filter(
    i => i.impactType !== 'LastRoleToTarget'
  );

  protected readonly roleName = this.data.roleName;
  protected readonly targetResourceName = this.data.targetResourceName;

  protected severityIcon(severity: string): string {
    return severity === 'Critical' ? 'error' : 'warning';
  }

  protected confirm(): void {
    this.dialogRef.close(true);
  }

  protected cancel(): void {
    this.dialogRef.close(false);
  }
}
