import { ChangeDetectionStrategy, Component, computed, inject, input, output, signal } from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CdkConnectedOverlay, CdkOverlayOrigin, ConnectedPosition } from '@angular/cdk/overlay';
import { MatIconModule } from '@angular/material/icon';
import { DsSpinnerComponent } from '../ds/ds-spinner/ds-spinner.component';
import { TranslateModule } from '@ngx-translate/core';
import { DsSelectComponent, DsSelectOption } from '../ds/ds-select/ds-select.component';
import { ProjectService } from '../../services/project.service';
import { GitBranchResponse, GitFileResponse } from '../../interfaces/project.interface';

export interface DirectoryTreeNode {
  name: string;
  path: string;
  children: DirectoryTreeNode[];
  expanded: boolean;
}

@Component({
  selector: 'app-build-context-picker',
  standalone: true,
  imports: [
    NgTemplateOutlet,
    FormsModule,
    CdkConnectedOverlay,
    CdkOverlayOrigin,
    MatIconModule,
    DsSpinnerComponent,
    TranslateModule,
    DsSelectComponent,
  ],
  templateUrl: './build-context-picker.component.html',
  styleUrl: './build-context-picker.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BuildContextPickerComponent {
  private readonly projectService = inject(ProjectService);

  readonly projectId = input<string>('');
  readonly configId = input<string | undefined>(undefined);
  readonly disabled = input(false);

  protected readonly isDisabled = computed(() => this.disabled() || !this.projectId());

  readonly pathSelected = output<string>();

  protected readonly isOpen = signal(false);
  protected readonly branches = signal<GitBranchResponse[]>([]);
  protected readonly treeRoots = signal<DirectoryTreeNode[]>([]);
  protected readonly selectedBranch = signal<string | null>(null);
  protected readonly loadingBranches = signal(false);
  protected readonly loadingDirectories = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly hasDirectories = computed(() => this.treeRoots().length > 0);

  protected readonly branchSelectOptions = computed<DsSelectOption[]>(() =>
    this.branches().map((b) => ({ value: b.name, label: b.name, icon: b.isProtected ? 'lock' : 'account_tree' }))
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
    this.loadingDirectories.set(true);
    this.treeRoots.set([]);
    try {
      const dirs = await this.projectService.searchCodeDirectories(
        this.projectId(),
        branchName,
        undefined,
        this.configId()
      );
      this.treeRoots.set(this.buildTree(dirs));
    } catch {
      this.treeRoots.set([]);
    } finally {
      this.loadingDirectories.set(false);
    }
  }

  protected onBranchSelect(value: string | number | null): void {
    if (value != null) {
      this.selectBranch(String(value));
    }
  }

  protected toggleNode(node: DirectoryTreeNode, event: Event): void {
    event.stopPropagation();
    node.expanded = !node.expanded;
    // Trigger change detection by replacing the array
    this.treeRoots.set([...this.treeRoots()]);
  }

  protected selectNode(node: DirectoryTreeNode): void {
    this.pathSelected.emit(node.path);
    this.close();
  }

  protected selectRoot(): void {
    this.pathSelected.emit('.');
    this.close();
  }

  private buildTree(dirs: GitFileResponse[]): DirectoryTreeNode[] {
    const nodeMap = new Map<string, DirectoryTreeNode>();

    // Sort by path to ensure parents are processed before children
    const sorted = [...dirs].sort((a, b) => a.path.localeCompare(b.path));

    for (const dir of sorted) {
      const node: DirectoryTreeNode = {
        name: dir.name,
        path: dir.path,
        children: [],
        expanded: false,
      };
      nodeMap.set(dir.path, node);
    }

    const roots: DirectoryTreeNode[] = [];

    for (const dir of sorted) {
      const node = nodeMap.get(dir.path)!;
      const lastSlash = dir.path.lastIndexOf('/');
      if (lastSlash === -1) {
        roots.push(node);
      } else {
        const parentPath = dir.path.substring(0, lastSlash);
        const parent = nodeMap.get(parentPath);
        if (parent) {
          parent.children.push(node);
        } else {
          roots.push(node);
        }
      }
    }

    return roots;
  }
}
