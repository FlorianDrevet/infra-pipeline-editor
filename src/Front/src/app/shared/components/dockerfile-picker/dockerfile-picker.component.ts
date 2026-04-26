import { ChangeDetectionStrategy, Component, computed, inject, input, output, signal } from '@angular/core';
import { CdkConnectedOverlay, CdkOverlayOrigin, ConnectedPosition } from '@angular/cdk/overlay';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateModule } from '@ngx-translate/core';
import { ProjectService } from '../../services/project.service';
import { GitBranchResponse, GitFileResponse } from '../../interfaces/project.interface';

@Component({
  selector: 'app-dockerfile-picker',
  standalone: true,
  imports: [
    CdkConnectedOverlay,
    CdkOverlayOrigin,
    MatIconModule,
    MatProgressSpinnerModule,
    TranslateModule,
  ],
  templateUrl: './dockerfile-picker.component.html',
  styleUrl: './dockerfile-picker.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DockerfilePickerComponent {
  private readonly projectService = inject(ProjectService);

  // Inputs
  readonly projectId = input.required<string>();
  readonly configId = input<string | undefined>(undefined);
  readonly disabled = input(false);

  // Output
  readonly pathSelected = output<string>();

  // State
  protected readonly isOpen = signal(false);
  protected readonly branches = signal<GitBranchResponse[]>([]);
  protected readonly files = signal<GitFileResponse[]>([]);
  protected readonly selectedBranch = signal<string | null>(null);
  protected readonly loadingBranches = signal(false);
  protected readonly loadingFiles = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly hasFiles = computed(() => this.files().length > 0);

  protected readonly overlayPositions: ConnectedPosition[] = [
    { originX: 'end', originY: 'bottom', overlayX: 'end', overlayY: 'top', offsetY: 4 },
    { originX: 'end', originY: 'top', overlayX: 'end', overlayY: 'bottom', offsetY: -4 },
  ];

  async toggle(): Promise<void> {
    if (this.disabled()) return;
    if (this.isOpen()) {
      this.close();
      return;
    }
    this.isOpen.set(true);
    this.error.set(null);
    await this.loadBranches();
  }

  close(): void {
    this.isOpen.set(false);
  }

  protected async loadBranches(): Promise<void> {
    this.loadingBranches.set(true);
    try {
      const branches = await this.projectService.listCodeBranches(
        this.projectId(),
        this.configId()
      );
      this.branches.set(branches);
      if (branches.length > 0 && !this.selectedBranch()) {
        await this.selectBranch(branches[0].name);
      }
    } catch {
      this.error.set('RESOURCE_EDIT.APP_PIPELINE.NO_REPO_CONFIGURED');
      this.branches.set([]);
    } finally {
      this.loadingBranches.set(false);
    }
  }

  protected async selectBranch(branchName: string): Promise<void> {
    this.selectedBranch.set(branchName);
    this.loadingFiles.set(true);
    this.files.set([]);
    try {
      const files = await this.projectService.searchCodeFiles(
        this.projectId(),
        branchName,
        'Dockerfile',
        this.configId()
      );
      this.files.set(files);
    } catch {
      this.files.set([]);
    } finally {
      this.loadingFiles.set(false);
    }
  }

  protected onBranchChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.selectBranch(select.value);
  }

  protected selectFile(file: GitFileResponse): void {
    this.pathSelected.emit(file.path);
    this.close();
  }
}
