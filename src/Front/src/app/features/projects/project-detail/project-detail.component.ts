import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { LowerCasePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { ProjectResponse, ProjectMemberResponse } from '../../../shared/interfaces/project.interface';
import { InfrastructureConfigResponse } from '../../../shared/interfaces/infra-config.interface';
import { ProjectService } from '../../../shared/services/project.service';
import { InfraConfigService } from '../../../shared/services/infra-config.service';
import { AuthenticationService } from '../../../shared/services/authentication.service';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { AddProjectMemberDialogComponent, AddProjectMemberDialogData } from '../add-member-dialog/add-member-dialog.component';
import { PROJECT_ROLE_ICONS } from '../enums/project-role.enum';

const ROLES = ['Owner', 'Contributor', 'Reader'] as const;
const ROLE_ORDER: Record<string, number> = { Owner: 0, Contributor: 1, Reader: 2 };

@Component({
  selector: 'app-project-detail',
  standalone: true,
  imports: [
    LowerCasePipe,
    TranslateModule,
    RouterLink,
    FormsModule,
    MatButtonModule,
    MatDialogModule,
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
  private readonly infraConfigService = inject(InfraConfigService);
  private readonly authService = inject(AuthenticationService);
  private readonly dialog = inject(MatDialog);

  protected readonly project = signal<ProjectResponse | null>(null);
  protected readonly configurations = signal<InfrastructureConfigResponse[]>([]);
  protected readonly allConfigs = signal<InfrastructureConfigResponse[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly loadErrorKey = signal('');
  protected readonly memberActionId = signal<string | null>(null);
  protected readonly memberErrorKey = signal('');
  protected readonly configErrorKey = signal('');
  protected readonly selectedConfigId = signal('');
  protected readonly roles = ROLES;

  protected readonly sortedMembers = computed(() => {
    const p = this.project();
    if (!p) return [];
    return [...p.members].sort(
      (a, b) => (ROLE_ORDER[a.role] ?? 9) - (ROLE_ORDER[b.role] ?? 9)
    );
  });

  protected readonly canManage = computed(() => {
    const myRole = this.getMyRole();
    return myRole === 'Owner' || myRole === 'Contributor';
  });

  protected readonly isOwner = computed(() => this.getMyRole() === 'Owner');

  protected readonly availableConfigs = computed(() => {
    const projectConfigs = this.configurations();
    const all = this.allConfigs();
    const usedIds = new Set(projectConfigs.map((c) => c.id));
    return all.filter((c) => !usedIds.has(c.id));
  });

  public async ngOnInit(): Promise<void> {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.loadErrorKey.set('PROJECTS.ERROR.NO_ID');
      return;
    }
    await this.loadProject(id);
  }

  protected getRoleIcon(role: string): string {
    return PROJECT_ROLE_ICONS[role] ?? 'person';
  }

  protected getMyRole(): string | null {
    const p = this.project();
    if (!p) return null;
    const entraId = this.authService.getMsalAccount?.localAccountId;
    if (!entraId) return null;
    const member = p.members.find((m) => m.entraId === entraId);
    return member?.role ?? null;
  }

  protected async onRoleChange(member: ProjectMemberResponse, newRole: string): Promise<void> {
    const p = this.project();
    if (!p) return;

    this.memberActionId.set(member.userId);
    this.memberErrorKey.set('');

    try {
      const updated = await this.projectService.updateMemberRole(p.id, member.userId, {
        newRole,
      });
      this.project.set(updated);
    } catch {
      this.memberErrorKey.set('PROJECTS.MEMBERS.ROLE_CHANGE_ERROR');
    } finally {
      this.memberActionId.set(null);
    }
  }

  protected openAddMemberDialog(): void {
    const p = this.project();
    if (!p) return;

    const dialogRef = this.dialog.open(AddProjectMemberDialogComponent, {
      width: '440px',
      data: {
        projectId: p.id,
        existingMembers: p.members,
      } satisfies AddProjectMemberDialogData,
    });

    dialogRef.afterClosed().subscribe((result?: ProjectResponse) => {
      if (result) {
        this.project.set(result);
      }
    });
  }

  protected confirmRemoveMember(member: ProjectMemberResponse): void {
    const p = this.project();
    if (!p) return;

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        titleKey: 'PROJECTS.MEMBERS.REMOVE_CONFIRM_TITLE',
        messageKey: 'PROJECTS.MEMBERS.REMOVE_CONFIRM_MSG',
        messageParams: { name: `${member.firstName} ${member.lastName}` },
        confirmKey: 'PROJECTS.MEMBERS.REMOVE_CONFIRM_YES',
        cancelKey: 'PROJECTS.MEMBERS.REMOVE_CONFIRM_CANCEL',
      } satisfies ConfirmDialogData,
    });

    dialogRef.afterClosed().subscribe(async (confirmed?: boolean) => {
      if (!confirmed) return;

      this.memberActionId.set(member.userId);
      this.memberErrorKey.set('');

      try {
        await this.projectService.removeMember(p.id, member.userId);
        this.project.update((proj) =>
          proj
            ? { ...proj, members: proj.members.filter((m) => m.userId !== member.userId) }
            : proj
        );
      } catch {
        this.memberErrorKey.set('PROJECTS.MEMBERS.REMOVE_ERROR');
      } finally {
        this.memberActionId.set(null);
      }
    });
  }

  protected async addConfiguration(): Promise<void> {
    const p = this.project();
    const configId = this.selectedConfigId();
    if (!p || !configId) return;

    this.configErrorKey.set('');

    try {
      await this.projectService.addConfiguration(p.id, { configId });
      this.selectedConfigId.set('');
      await this.loadConfigurations(p.id);
    } catch {
      this.configErrorKey.set('PROJECTS.CONFIGS.ADD_ERROR');
    }
  }

  protected async removeConfiguration(configId: string): Promise<void> {
    const p = this.project();
    if (!p) return;

    this.configErrorKey.set('');

    try {
      await this.projectService.removeConfiguration(p.id, configId);
      this.configurations.update((list) => list.filter((c) => c.id !== configId));
    } catch {
      this.configErrorKey.set('PROJECTS.CONFIGS.REMOVE_ERROR');
    }
  }

  protected onConfigSelect(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.selectedConfigId.set(select.value);
  }

  private async loadProject(id: string): Promise<void> {
    this.isLoading.set(true);
    this.loadErrorKey.set('');

    try {
      const [project, configs] = await Promise.all([
        this.projectService.getById(id),
        this.projectService.getConfigurations(id),
      ]);
      this.project.set(project);
      this.configurations.set(configs);
      await this.loadAllConfigs();
    } catch {
      this.loadErrorKey.set('PROJECTS.ERROR.LOAD_FAILED');
    } finally {
      this.isLoading.set(false);
    }
  }

  private async loadConfigurations(projectId: string): Promise<void> {
    try {
      const configs = await this.projectService.getConfigurations(projectId);
      this.configurations.set(configs);
    } catch {
      this.configErrorKey.set('PROJECTS.CONFIGS.REFRESH_ERROR');
    }
  }

  private async loadAllConfigs(): Promise<void> {
    try {
      const all = await this.infraConfigService.getAll();
      this.allConfigs.set(all);
    } catch {
      // non-blocking
    }
  }
}
