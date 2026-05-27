import { ChangeDetectionStrategy, Component, inject, input, OnInit, signal } from '@angular/core';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { DsSpinnerComponent } from '../../../shared/components/ds/ds-spinner/ds-spinner.component';
import { DsButtonComponent } from '../../../shared/components/ds/ds-button/ds-button.component';
import { DsIconButtonComponent } from '../../../shared/components/ds/ds-icon-button/ds-icon-button.component';
import { TranslateModule } from '@ngx-translate/core';

import { ProjectResponse, ProjectPipelineVariableGroupResponse } from '../../../shared/interfaces/project.interface';
import { ProjectService } from '../../../shared/services/project.service';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { AddVariableGroupDialogComponent } from '../../config-detail/add-variable-group-dialog/add-variable-group-dialog.component';

@Component({
  selector: 'app-project-detail-variable-groups-section',
  standalone: true,
  imports: [
    MatDialogModule,
    MatIconModule,
    DsSpinnerComponent,
    DsButtonComponent,
    DsIconButtonComponent,
    TranslateModule,
  ],
  templateUrl: './project-detail-variable-groups-section.component.html',
  styleUrl: './project-detail-variable-groups-section.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProjectDetailVariableGroupsSectionComponent implements OnInit {
  private readonly projectService = inject(ProjectService);
  private readonly dialog = inject(MatDialog);

  readonly project = input.required<ProjectResponse | null>();
  readonly canWrite = input.required<boolean>();

  protected readonly variableGroups = signal<ProjectPipelineVariableGroupResponse[]>([]);
  protected readonly vgLoading = signal(false);
  protected readonly vgErrorKey = signal('');
  protected readonly vgLoaded = signal(false);

  ngOnInit(): void {
    this.loadVariableGroups().catch(() => {});
  }

  protected async loadVariableGroups(): Promise<void> {
    const project = this.project();
    if (!project) return;

    this.vgLoading.set(true);
    this.vgErrorKey.set('');
    try {
      const groups = await this.projectService.getPipelineVariableGroups(project.id);
      this.variableGroups.set(groups.map(g => ({ ...g, variables: g.variables ?? [] })));
      this.vgLoaded.set(true);
    } catch {
      this.vgErrorKey.set('PROJECT_DETAIL.PIPELINE_VARIABLES.ERROR_ADD_GROUP');
    } finally {
      this.vgLoading.set(false);
    }
  }

  protected openAddVariableGroupDialog(): void {
    const dialogRef = this.dialog.open(AddVariableGroupDialogComponent, {
      width: '420px',
    });

    dialogRef.afterClosed().subscribe(async (groupName?: string) => {
      if (!groupName) return;
      const project = this.project();
      if (!project) return;

      this.vgErrorKey.set('');
      try {
        const newGroup = await this.projectService.addPipelineVariableGroup(project.id, { groupName });
        this.variableGroups.update(groups => [...groups, { ...newGroup, variables: newGroup.variables ?? [] }]);
      } catch {
        this.vgErrorKey.set('PROJECT_DETAIL.PIPELINE_VARIABLES.ERROR_ADD_GROUP');
      }
    });
  }

  protected openRemoveVariableGroupDialog(group: ProjectPipelineVariableGroupResponse): void {
    const data: ConfirmDialogData = {
      titleKey: 'PROJECT_DETAIL.PIPELINE_VARIABLES.REMOVE_GROUP',
      messageKey: 'PROJECT_DETAIL.PIPELINE_VARIABLES.CONFIRM_DELETE_GROUP',
      confirmKey: 'PROJECT_DETAIL.PIPELINE_VARIABLES.REMOVE_GROUP',
      cancelKey: 'CONFIG_DETAIL.PIPELINE_VARIABLES.DIALOG_CANCEL',
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, { width: '400px', data });

    dialogRef.afterClosed().subscribe(async (confirmed?: boolean) => {
      if (!confirmed) return;
      const project = this.project();
      if (!project) return;

      this.vgErrorKey.set('');
      try {
        await this.projectService.removePipelineVariableGroup(project.id, group.id);
        this.variableGroups.update(groups => groups.filter(g => g.id !== group.id));
      } catch {
        this.vgErrorKey.set('PROJECT_DETAIL.PIPELINE_VARIABLES.ERROR_REMOVE_GROUP');
      }
    });
  }
}
