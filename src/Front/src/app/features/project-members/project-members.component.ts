import { Component, OnDestroy, OnInit, computed, effect, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { DsSpinnerComponent } from '../../shared/components/ds/ds-spinner/ds-spinner.component';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';

import { ProjectResponse, ProjectMemberResponse } from '../../shared/interfaces/project.interface';
import { UserResponse } from '../../shared/interfaces/infra-config.interface';
import { ProjectService } from '../../shared/services/project.service';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { PageContextService } from '../../shared/services/page-context.service';
import { SidebarContextService } from '../../core/layouts/sidebar/sidebar-context.service';
import { DsButtonComponent, DsSelectComponent, DsSelectOption, DsTextFieldComponent } from '../../shared/components/ds';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import {
  AddProjectMemberDialogComponent,
  AddProjectMemberDialogData,
} from '../project-detail/add-project-member-dialog/add-project-member-dialog.component';

const ROLES = ['Owner', 'Contributor', 'Reader'] as const;
const ROLE_ICONS: Record<string, string> = { Owner: 'shield', Contributor: 'edit', Reader: 'visibility' };
const ROLE_DESCRIPTIONS: Record<string, string> = {
  Owner: 'PROJECT_MEMBERS.ROLE_DESC_OWNER',
  Contributor: 'PROJECT_MEMBERS.ROLE_DESC_CONTRIBUTOR',
  Reader: 'PROJECT_MEMBERS.ROLE_DESC_READER',
};

@Component({
  selector: 'app-project-members',
  standalone: true,
  imports: [
    TranslateModule,
    RouterLink,
    FormsModule,
    MatIconModule,
    DsSpinnerComponent,
    MatTooltipModule,
    DsButtonComponent,
    DsSelectComponent,
    DsTextFieldComponent,
  ],
  templateUrl: './project-members.component.html',
  styleUrl: './project-members.component.scss',
})
export class ProjectMembersComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly projectService = inject(ProjectService);
  private readonly authService = inject(AuthenticationService);
  private readonly dialog = inject(MatDialog);
  private readonly translate = inject(TranslateService);
  private readonly pageContextService = inject(PageContextService);
  private readonly sidebarContextService = inject(SidebarContextService);

  protected readonly project = signal<ProjectResponse | null>(null);
  protected readonly availableUsers = signal<UserResponse[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly loadError = signal('');
  protected readonly memberActionId = signal<string | null>(null);
  protected readonly memberErrorKey = signal('');
  protected readonly searchQuery = signal('');

  protected readonly roleDsOptions: DsSelectOption[] = ROLES.map((role) => ({
    value: role,
    label: this.translate.instant('PROJECT_MEMBERS.ROLE_' + role.toUpperCase()),
  }));

  private readonly breadcrumbEffect = effect(() => {
    const project = this.project();
    const projectsLabel = this.translate.instant('NAV.BREADCRUMB.PROJECTS') as string;
    const membersLabel = this.translate.instant('PROJECT_MEMBERS.BREADCRUMB') as string;
    const segments = project
      ? [
          { label: projectsLabel, routerLink: '/' },
          { label: project.name, routerLink: `/projects/${project.id}` },
          { label: membersLabel },
        ]
      : [{ label: projectsLabel, routerLink: '/' }];
    this.pageContextService.setBreadcrumb(segments);
  });

  protected readonly isOwner = computed(() => {
    const oid = this.authService.getMsalAccount?.localAccountId;
    if (!oid) return false;
    const members = this.project()?.members ?? [];
    const me = members.find((m) => m.entraId === oid);
    return me?.role === 'Owner';
  });

  protected readonly filteredMembersByRole = computed(() => {
    const members = this.project()?.members ?? [];
    const query = this.searchQuery().toLowerCase().trim();

    const filtered = query
      ? members.filter(
          (m) =>
            m.firstName.toLowerCase().includes(query) ||
            m.lastName.toLowerCase().includes(query)
        )
      : members;

    return ROLES.map((role) => ({
      role,
      icon: ROLE_ICONS[role],
      descriptionKey: ROLE_DESCRIPTIONS[role],
      members: filtered.filter((m) => m.role === role),
    })).filter((group) => group.members.length > 0);
  });

  protected readonly totalMembers = computed(() => this.project()?.members.length ?? 0);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.loadError.set('PROJECT_MEMBERS.ERROR.NO_ID');
      return;
    }
    void this.loadProject(id);
  }

  ngOnDestroy(): void {
    this.pageContextService.clear();
  }

  private async loadProject(id: string): Promise<void> {
    this.isLoading.set(true);
    this.loadError.set('');

    try {
      const [project, users] = await Promise.all([
        this.projectService.getProject(id),
        this.projectService.getUsers(),
      ]);
      this.project.set(project);
      this.availableUsers.set(users);
      this.sidebarContextService.setProjectContext(project.id, project.name);
    } catch {
      this.loadError.set('PROJECT_MEMBERS.ERROR.LOAD_FAILED');
    } finally {
      this.isLoading.set(false);
    }
  }

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
      this.memberErrorKey.set('PROJECT_MEMBERS.ROLE_CHANGE_ERROR');
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
      titleKey: 'PROJECT_MEMBERS.REMOVE_CONFIRM_TITLE',
      messageKey: 'PROJECT_MEMBERS.REMOVE_CONFIRM_MESSAGE',
      messageParams: { name: `${member.firstName} ${member.lastName}` },
      confirmKey: 'PROJECT_MEMBERS.REMOVE_CONFIRM_YES',
      cancelKey: 'PROJECT_MEMBERS.REMOVE_CONFIRM_CANCEL',
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
        this.memberErrorKey.set('PROJECT_MEMBERS.REMOVE_ERROR');
      } finally {
        this.memberActionId.set(null);
      }
    });
  }
}
