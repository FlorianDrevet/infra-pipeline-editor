import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateModule } from '@ngx-translate/core';
import { RoleAssignmentService } from '../../../shared/services/role-assignment.service';
import { RoleAssignmentImpactResponse } from '../../../shared/interfaces/role-assignment.interface';

export interface RoleAssignmentImpactDialogData {
  resourceId: string;
  roleAssignmentId: string;
  roleName: string;
  targetResourceName: string;
}

@Component({
  selector: 'app-role-assignment-impact-dialog',
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './role-assignment-impact-dialog.component.html',
  styleUrl: './role-assignment-impact-dialog.component.scss',
})
export class RoleAssignmentImpactDialogComponent implements OnInit {
  private readonly dialogRef = inject(MatDialogRef<RoleAssignmentImpactDialogComponent>);
  private readonly data = inject<RoleAssignmentImpactDialogData>(MAT_DIALOG_DATA);
  private readonly roleAssignmentService = inject(RoleAssignmentService);

  protected loading = signal(true);
  protected impactResult = signal<RoleAssignmentImpactResponse | null>(null);
  protected error = signal(false);

  protected filteredImpacts = computed(() => {
    const result = this.impactResult();
    if (!result?.impacts) return [];
    return result.impacts.filter(i => i.impactType !== 'LastRoleToTarget');
  });

  protected hasFilteredImpact = computed(() => this.filteredImpacts().length > 0);

  protected readonly roleName = this.data.roleName;
  protected readonly targetResourceName = this.data.targetResourceName;

  async ngOnInit(): Promise<void> {
    try {
      const result = await this.roleAssignmentService.analyzeImpact(
        this.data.resourceId,
        this.data.roleAssignmentId
      );
      this.impactResult.set(result);
    } catch {
      this.error.set(true);
    } finally {
      this.loading.set(false);
    }
  }

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
