import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';
import {
  InfrastructureConfigResponse,
  MemberResponse,
  UserResponse,
} from '../../shared/interfaces/infra-config.interface';
import { ResourceGroupResponse } from '../../shared/interfaces/resource-group.interface';
import { InfraConfigService } from '../../shared/services/infra-config.service';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { AddMemberDialogComponent, AddMemberDialogData } from './add-member-dialog/add-member-dialog.component';

const ROLES = ['Owner', 'Contributor', 'Reader'] as const;

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
    MatTooltipModule,
  ],
  templateUrl: './config-detail.component.html',
  styleUrl: './config-detail.component.scss',
})
export class ConfigDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly infraConfigService = inject(InfraConfigService);
  private readonly dialog = inject(MatDialog);

  protected readonly config = signal<InfrastructureConfigResponse | null>(null);
  protected readonly resourceGroups = signal<ResourceGroupResponse[]>([]);
  protected readonly availableUsers = signal<UserResponse[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly loadError = signal('');
  protected readonly memberActionId = signal<string | null>(null);
  protected readonly memberErrorKey = signal('');
  protected readonly roles = ROLES;

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
      const updated = await this.infraConfigService.updateMemberRole(configId, member.userId, {
        newRole,
      });
      this.config.set(updated);
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

    dialogRef.afterClosed().subscribe((result: InfrastructureConfigResponse | null) => {
      if (result) {
        this.config.set(result);
      }
    });
  }
}
