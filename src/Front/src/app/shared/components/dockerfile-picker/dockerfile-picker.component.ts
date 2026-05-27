import { ChangeDetectionStrategy, Component, computed, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CdkConnectedOverlay, CdkOverlayOrigin, ConnectedPosition } from '@angular/cdk/overlay';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateModule } from '@ngx-translate/core';
import { DsAutocompleteComponent, DsAutocompleteOption } from '../ds/ds-autocomplete/ds-autocomplete.component';
import { ProjectService } from '../../services/project.service';
import { GitBranchResponse, GitFileResponse } from '../../interfaces/project.interface';

@Component({
  selector: 'app-dockerfile-picker',
  standalone: true,
  imports: [
    FormsModule,
    CdkConnectedOverlay,
    CdkOverlayOrigin,
    MatIconModule,
    MatProgressSpinnerModule,
    TranslateModule,
    DsAutocompleteComponent,
  ],
  templateUrl: './dockerfile-picker.component.html',
  styleUrl: './dockerfile-picker.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DockerfilePickerComponent {
  private readonly projectService = inject(ProjectService);

  // Inputs
  readonly projectId = input<string>('');
  readonly configId = input<string | undefined>(undefined);
  readonly disabled = input(false);

  protected readonly isDisabled = computed(() => this.disabled() || !this.projectId());

  // Output
  readonly pathSelected = output<string>();

  // State
  protected readonly isOpen = signal(false);
  protected readonly branches = signal<GitBranchResponse[]>([]);
  protected readonly files = signal<GitFileResponse[]>([]);
  protected readonly selectedBranch = signal<string | null>(null);
  protected readonly branchQuery = signal('');
  protected readonly loadingBranches = signal(false);
  protected readonly loadingFiles = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly hasFiles = computed(() => this.files().length > 0);

  protected readonly branchOptions = computed<ReadonlyArray<DsAutocompleteOption<string>>>(() =>
    this.branches()
      .filter((branch) => {
        const query = this.branchQuery().trim().toLocaleLowerCase();

        return !query || branch.name.toLocaleLowerCase().includes(query);
      })
      .map((branch) => ({
        value: branch.name,
        label: branch.name,
        icon: branch.isProtected ? 'lock' : 'account_tree',
      }))
  );

  protected readonly overlayPositions: ConnectedPosition[] = [
    { originX: 'end', originY: 'bottom', overlayX: 'end', overlayY: 'top', offsetY: 4 },
    { originX: 'end', originY: 'top', overlayX: 'end', overlayY: 'bottom', offsetY: -4 },
  ];

  async toggle(): Promise<void> {
    if (this.isDisabled()) return;
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

  protected onBranchSelected(option: DsAutocompleteOption<unknown>): void {
    this.selectBranch(String(option.value));
  }

  protected onBranchSearchChanged(query: string): void {
    this.branchQuery.set(query);
  }

  protected selectFile(file: GitFileResponse): void {
    this.pathSelected.emit(file.path);
    this.close();
  }
}
