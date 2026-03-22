import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';
import { ProjectResponse, ProjectMemberResponse } from '../../shared/interfaces/project.interface';
import { InfrastructureConfigResponse } from '../../shared/interfaces/infra-config.interface';
import { UserResponse } from '../../shared/interfaces/infra-config.interface';
import { ProjectService } from '../../shared/services/project.service';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { AddProjectMemberDialogComponent, AddProjectMemberDialogData } from './add-project-member-dialog/add-project-member-dialog.component';
import { AddConfigDialogComponent, AddConfigDialogData } from './add-config-dialog/add-config-dialog.component';
import { FormsModule } from '@angular/forms';

const ROLES = ['Owner', 'Contributor', 'Reader'] as const;
const ROLE_ORDER: Record<string, number> = { Owner: 0, Contributor: 1, Reader: 2 };
const ROLE_ICONS: Record<string, string> = { Owner: 'shield', Contributor: 'edit', Reader: 'visibility' };

@Component({
  selector: 'app-project-detail',
  standalone: true,
  imports: [
    TranslateModule,
    RouterLink,
    FormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatTabsModule,
    MatTooltipModule,
  ],
  templateUrl: './project-detail.component.html',
  styleUrl: './project-detail.component.scss',
})
export class ProjectDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly projectService = inject(ProjectService);
  private readonly authService = inject(AuthenticationService);
  private readonly dialog = inject(MatDialog);

  protected readonly project = signal<ProjectResponse | null>(null);
  protected readonly configs = signal<InfrastructureConfigResponse[]>([]);
  protected readonly availableUsers = signal<UserResponse[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly loadError = signal('');
  protected readonly memberActionId = signal<string | null>(null);
  protected readonly memberErrorKey = signal('');
  protected readonly configErrorKey = signal('');
  protected readonly roles = ROLES;

  protected readonly membersByRole = computed(() => {
    const members = this.project()?.members ?? [];
    return ROLES
      .map((role) => ({
        role,
        icon: ROLE_ICONS[role],
        members: members.filter((m) => m.role === role),
      }))
      .filter((group) => group.members.length > 0);
  });

  protected readonly isOwner = computed(() => {
    const oid = this.authService.getMsalAccount?.localAccountId;
    if (!oid) return false;
    const members = this.project()?.members ?? [];
    const me = members.find((m) => m.entraId === oid);
    return me?.role === 'Owner';
  });

  protected readonly canWrite = computed(() => {
    const oid = this.authService.getMsalAccount?.localAccountId;
    if (!oid) return false;
    const members = this.project()?.members ?? [];
    const me = members.find((m) => m.entraId === oid);
    return me?.role === 'Owner' || me?.role === 'Contributor';
  });

  async ngOnInit(): Promise<void> {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.loadError.set('PROJECT_DETAIL.ERROR.NO_ID');
      return;
    }
    await this.loadProject(id);
  }

  private async loadProject(id: string): Promise<void> {
    this.isLoading.set(true);
    this.loadError.set('');

    try {
      const [project, configs, users] = await Promise.all([
        this.projectService.getProject(id),
        this.projectService.getProjectConfigs(id),
        this.projectService.getUsers(),
      ]);
      this.project.set(project);
      this.configs.set(configs);
      this.availableUsers.set(users);
    } catch {
      this.loadError.set('PROJECT_DETAIL.ERROR.LOAD_FAILED');
    } finally {
      this.isLoading.set(false);
    }
  }

  // ─── Members ───

  protected async onRoleChange(member: ProjectMemberResponse, newRole: string): Promise<void> {
    if (newRole === member.role) return;

    const projectId = this.project()?.id;
    if (!projectId) return;

    this.memberActionId.set(member.id);
    this.memberErrorKey.set('');

    try {
      const updated = await this.projectService.updateMemberRole(projectId, member.userId, { newRole });
      this.project.set(updated);
    } catch {
      this.memberErrorKey.set('PROJECT_DETAIL.MEMBERS.ROLE_CHANGE_ERROR');
    } finally {
      this.memberActionId.set(null);
    }
  }

  protected openAddMemberDialog(): void {
    const projectId = this.project()?.id;
    if (!projectId) return;

    const data: AddProjectMemberDialogData = {
      projectId,
      existingUserIds: (this.project()?.members ?? []).map((m) => m.userId),
      availableUsers: this.availableUsers(),
    };

    const dialogRef = this.dialog.open(AddProjectMemberDialogComponent, {
      width: '480px',
      data,
    });

    dialogRef.afterClosed().subscribe((result?: ProjectResponse) => {
      if (result) {
        this.project.set(result);
      }
    });
  }

  protected openRemoveMemberDialog(member: ProjectMemberResponse): void {
    const projectId = this.project()?.id;
    if (!projectId) return;

    const data: ConfirmDialogData = {
      titleKey: 'PROJECT_DETAIL.MEMBERS.REMOVE_CONFIRM_TITLE',
      messageKey: 'PROJECT_DETAIL.MEMBERS.REMOVE_CONFIRM_MESSAGE',
      messageParams: { name: `${member.firstName} ${member.lastName}` },
      confirmKey: 'PROJECT_DETAIL.MEMBERS.REMOVE_CONFIRM_YES',
      cancelKey: 'PROJECT_DETAIL.MEMBERS.REMOVE_CONFIRM_CANCEL',
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, { width: '400px', data });

    dialogRef.afterClosed().subscribe(async (confirmed?: boolean) => {
      if (!confirmed) return;

      this.memberActionId.set(member.id);
      this.memberErrorKey.set('');

      try {
        await this.projectService.removeMember(projectId, member.userId);
        this.project.update((p) => {
          if (!p) return p;
          return { ...p, members: p.members.filter((m) => m.id !== member.id) };
        });
      } catch {
        this.memberErrorKey.set('PROJECT_DETAIL.MEMBERS.REMOVE_ERROR');
      } finally {
        this.memberActionId.set(null);
      }
    });
  }

  // ─── Configs ───

  protected openAddConfigDialog(): void {
    const projectId = this.project()?.id;
    if (!projectId) return;

    const data: AddConfigDialogData = { projectId };

    const dialogRef = this.dialog.open(AddConfigDialogComponent, {
      width: '480px',
      data,
    });

    dialogRef.afterClosed().subscribe((result?: InfrastructureConfigResponse) => {
      if (result) {
        this.configs.update((configs) => [result, ...configs]);
      }
    });
  }
}
