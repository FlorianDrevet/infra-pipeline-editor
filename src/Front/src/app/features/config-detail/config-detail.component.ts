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
import {
  InfrastructureConfigResponse,
  MemberResponse,
  UserResponse,
} from '../../shared/interfaces/infra-config.interface';
import { ResourceGroupResponse, AzureResourceResponse } from '../../shared/interfaces/resource-group.interface';
import { InfraConfigService } from '../../shared/services/infra-config.service';
import { AuthenticationService } from '../../shared/services/authentication.service';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { AddMemberDialogComponent, AddMemberDialogData } from './add-member-dialog/add-member-dialog.component';
import { AddEnvironmentDialogComponent, AddEnvironmentDialogData } from './add-environment-dialog/add-environment-dialog.component';
import { AddResourceGroupDialogComponent, AddResourceGroupDialogData } from './add-resource-group-dialog/add-resource-group-dialog.component';
import { AddResourceDialogComponent, AddResourceDialogData } from './add-resource-dialog/add-resource-dialog.component';
import { ResourceGroupService } from '../../shared/services/resource-group.service';
import { RESOURCE_TYPE_ICONS } from './enums/resource-type.enum';
import { EnvironmentDefinitionResponse } from '../../shared/interfaces/infra-config.interface';

const ROLES = ['Owner', 'Contributor', 'Reader'] as const;
const ROLE_ORDER: Record<string, number> = { Owner: 0, Contributor: 1, Reader: 2 };
const ROLE_ICONS: Record<string, string> = { Owner: 'shield', Contributor: 'edit', Reader: 'visibility' };

@Component({
  selector: 'app-config-detail',
  standalone: true,
  imports: [
    TranslateModule,
    RouterLink,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatTabsModule,
    MatTooltipModule,
  ],
  templateUrl: './config-detail.component.html',
  styleUrl: './config-detail.component.scss',
})
export class ConfigDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly infraConfigService = inject(InfraConfigService);
  private readonly resourceGroupService = inject(ResourceGroupService);
  private readonly authService = inject(AuthenticationService);
  private readonly dialog = inject(MatDialog);

  protected readonly config = signal<InfrastructureConfigResponse | null>(null);
  protected readonly resourceGroups = signal<ResourceGroupResponse[]>([]);
  protected readonly availableUsers = signal<UserResponse[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly loadError = signal('');
  protected readonly memberActionId = signal<string | null>(null);
  protected readonly memberErrorKey = signal('');
  protected readonly envActionId = signal<string | null>(null);
  protected readonly envErrorKey = signal('');
  protected readonly rgErrorKey = signal('');
  protected readonly expandedRgId = signal<string | null>(null);
  protected readonly rgResources = signal<{ [rgId: string]: AzureResourceResponse[] | undefined }>({});
  protected readonly rgResourcesLoading = signal<string | null>(null);
  protected readonly resourceTypeIcons = RESOURCE_TYPE_ICONS;
  protected readonly roles = ROLES;

  protected readonly membersByRole = computed(() => {
    const members = this.config()?.members ?? [];
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
    const members = this.config()?.members ?? [];
    const me = members.find((m) => m.entraId === oid);
    return me?.role === 'Owner';
  });

  protected readonly canWrite = computed(() => {
    const oid = this.authService.getMsalAccount?.localAccountId;
    if (!oid) return false;
    const members = this.config()?.members ?? [];
    const me = members.find((m) => m.entraId === oid);
    return me?.role === 'Owner' || me?.role === 'Contributor';
  });

  protected readonly sortedEnvironments = computed(() => {
    const envs = this.config()?.environmentDefinitions ?? [];
    return [...envs].sort((a, b) => a.order - b.order);
  });

  async ngOnInit(): Promise<void> {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.loadError.set('CONFIG_DETAIL.ERROR.NO_ID');
      return;
    }
    await this.loadConfig(id);
  }

  private async loadConfig(id: string): Promise<void> {
    this.isLoading.set(true);
    this.loadError.set('');

    try {
      const [config, resourceGroups, users] = await Promise.all([
        this.infraConfigService.getById(id),
        this.infraConfigService.getResourceGroups(id),
        this.infraConfigService.getUsers(),
      ]);
      this.config.set(config);
      this.resourceGroups.set(resourceGroups);
      this.availableUsers.set(users);
    } catch {
      this.loadError.set('CONFIG_DETAIL.ERROR.LOAD_FAILED');
    } finally {
      this.isLoading.set(false);
    }
  }

  protected async onRoleChange(member: MemberResponse, newRole: string): Promise<void> {
    if (newRole === member.role) return;

    const configId = this.config()?.id;
    if (!configId) return;

    this.memberActionId.set(member.id);
    this.memberErrorKey.set('');

    try {
      await this.infraConfigService.updateMemberRole(configId, member.userId, {
        newRole,
      });
      const refreshed = await this.infraConfigService.getById(configId);
      this.config.set(refreshed);
    } catch {
      this.memberErrorKey.set('CONFIG_DETAIL.MEMBERS.ROLE_CHANGE_ERROR');
    } finally {
      this.memberActionId.set(null);
    }
  }

  protected openRemoveDialog(member: MemberResponse): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        titleKey: 'CONFIG_DETAIL.MEMBERS.REMOVE_CONFIRM_TITLE',
        messageKey: 'CONFIG_DETAIL.MEMBERS.REMOVE_CONFIRM_MESSAGE',
        messageParams: { name: `${member.firstName} ${member.lastName}` },
        confirmKey: 'CONFIG_DETAIL.MEMBERS.REMOVE_CONFIRM_YES',
        cancelKey: 'CONFIG_DETAIL.MEMBERS.REMOVE_CONFIRM_CANCEL',
      } satisfies ConfirmDialogData,
      width: '400px',
    });

    dialogRef.afterClosed().subscribe(async (confirmed: boolean) => {
      if (!confirmed) return;
      await this.removeMember(member);
    });
  }

  private async removeMember(member: MemberResponse): Promise<void> {
    const configId = this.config()?.id;
    if (!configId) return;

    this.memberActionId.set(member.id);
    this.memberErrorKey.set('');

    try {
      await this.infraConfigService.removeMember(configId, member.userId);
      const refreshed = await this.infraConfigService.getById(configId);
      this.config.set(refreshed);
    } catch {
      this.memberErrorKey.set('CONFIG_DETAIL.MEMBERS.REMOVE_ERROR');
    } finally {
      this.memberActionId.set(null);
    }
  }

  protected openAddMemberDialog(): void {
    const currentConfig = this.config();
    if (!currentConfig) return;

    const dialogRef = this.dialog.open(AddMemberDialogComponent, {
      data: {
        configId: currentConfig.id,
        existingUserIds: currentConfig.members.map((m) => m.userId),
        availableUsers: this.availableUsers(),
      } satisfies AddMemberDialogData,
      width: '440px',
    });

    dialogRef.afterClosed().subscribe(async (result: InfrastructureConfigResponse | null) => {
      if (result) {
        const refreshed = await this.infraConfigService.getById(currentConfig.id);
        this.config.set(refreshed);
      }
    });
  }

  protected openAddResourceGroupDialog(): void {
    const currentConfig = this.config();
    if (!currentConfig) return;

    const dialogRef = this.dialog.open(AddResourceGroupDialogComponent, {
      data: {
        infraConfigId: currentConfig.id,
      } satisfies AddResourceGroupDialogData,
      width: '440px',
    });

    dialogRef.afterClosed().subscribe(async (result: import('../../shared/interfaces/resource-group.interface').ResourceGroupResponse | null) => {
      if (result) {
        try {
          const resourceGroups = await this.infraConfigService.getResourceGroups(currentConfig.id);
          this.resourceGroups.set(resourceGroups);
        } catch {
          this.rgErrorKey.set('CONFIG_DETAIL.RESOURCE_GROUPS.REFRESH_ERROR');
        }
      }
    });
  }

  protected async toggleRgExpand(rgId: string): Promise<void> {
    if (this.expandedRgId() === rgId) {
      this.expandedRgId.set(null);
      return;
    }
    this.expandedRgId.set(rgId);
    await this.loadRgResources(rgId);
  }

  private async loadRgResources(rgId: string): Promise<void> {
    if (this.rgResources()[rgId]) return;
    this.rgResourcesLoading.set(rgId);
    try {
      const resources = await this.resourceGroupService.getResources(rgId);
      this.rgResources.update((prev) => ({ ...prev, [rgId]: resources }));
    } catch {
      this.rgResources.update((prev) => ({ ...prev, [rgId]: [] }));
    } finally {
      this.rgResourcesLoading.set(null);
    }
  }

  protected openAddResourceDialog(rgId: string): void {
    const rg = this.resourceGroups().find((r) => r.id === rgId);
    const dialogRef = this.dialog.open(AddResourceDialogComponent, {
      data: { resourceGroupId: rgId, location: rg?.location ?? '' } satisfies AddResourceDialogData,
      width: '560px',
    });

    dialogRef.afterClosed().subscribe(async (created: boolean) => {
      if (created) {
        this.rgResources.update((prev) => {
          const updated = { ...prev };
          delete updated[rgId];
          return updated;
        });
        await this.loadRgResources(rgId);
      }
    });
  }

  protected openAddEnvironmentDialog(existing?: EnvironmentDefinitionResponse): void {
    const currentConfig = this.config();
    if (!currentConfig) return;

    const dialogRef = this.dialog.open(AddEnvironmentDialogComponent, {
      data: {
        configId: currentConfig.id,
        existing,
      } satisfies AddEnvironmentDialogData,
      width: '520px',
    });

    dialogRef.afterClosed().subscribe(async (result: InfrastructureConfigResponse | null) => {
      if (result) {
        const refreshed = await this.infraConfigService.getById(currentConfig.id);
        this.config.set(refreshed);
      }
    });
  }

  protected openRemoveEnvironmentDialog(env: EnvironmentDefinitionResponse): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        titleKey: 'CONFIG_DETAIL.ENVIRONMENTS.REMOVE_CONFIRM_TITLE',
        messageKey: 'CONFIG_DETAIL.ENVIRONMENTS.REMOVE_CONFIRM_MESSAGE',
        messageParams: { name: env.name },
        confirmKey: 'CONFIG_DETAIL.ENVIRONMENTS.REMOVE_CONFIRM_YES',
        cancelKey: 'CONFIG_DETAIL.ENVIRONMENTS.REMOVE_CONFIRM_CANCEL',
      } satisfies ConfirmDialogData,
      width: '400px',
    });

    dialogRef.afterClosed().subscribe(async (confirmed: boolean) => {
      if (!confirmed) return;
      await this.removeEnvironment(env);
    });
  }

  private async removeEnvironment(env: EnvironmentDefinitionResponse): Promise<void> {
    const configId = this.config()?.id;
    if (!configId) return;

    this.envActionId.set(env.id);
    this.envErrorKey.set('');

    try {
      await this.infraConfigService.removeEnvironment(configId, env.id);
      const refreshed = await this.infraConfigService.getById(configId);
      this.config.set(refreshed);
    } catch {
      this.envErrorKey.set('CONFIG_DETAIL.ENVIRONMENTS.REMOVE_ERROR');
    } finally {
      this.envActionId.set(null);
    }
  }
}
