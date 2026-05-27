import {
  afterNextRender,
  Component,
  effect,
  ElementRef,
  inject,
  Injector,
  input,
  runInInjectionContext,
  signal,
  viewChild,
  viewChildren,
} from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';
import { BicepHighlightPipe } from '../../pipes/bicep-highlight.pipe';
import { DsSpinnerComponent } from '../ds/ds-spinner/ds-spinner.component';
import { UserPreferencesService } from '../../services/user-preferences.service';

const FILE_PATH_DATASET_KEY = 'filePath';
const VIEWER_SCROLL_OPTIONS: ScrollIntoViewOptions = {
  behavior: 'smooth',
  block: 'start',
  inline: 'nearest',
};
const ACTIVE_FILE_SCROLL_OPTIONS: ScrollIntoViewOptions = {
  behavior: 'smooth',
  block: 'center',
  inline: 'nearest',
};

// ─── Public types ──────────────────────────────────────────────────────────────

const BICEP_FILE_TYPES = [
  'types',
  'functions',
  'constants',
  'entry-point',
  'params',
  'role-assignments',
  'module-type',
  'generic',
] as const;

export type BicepFileType = (typeof BICEP_FILE_TYPES)[number];

export interface BicepFolderNode {
  kind: 'folder';
  /** Unique key used for expand/collapse, e.g. 'modules', 'modules/KeyVault', 'Common'. */
  key: string;
  /** Display label, e.g. 'modules/' or 'KeyVault/'. */
  name: string;
  /** Optional pre-translated badge text or i18n key. */
  badge?: string;
  /** Badge colour variant. */
  badgeVariant?: 'types' | 'params' | 'functions' | 'constants';
  /** Folder icon name. Use 'folder_shared' for the shared Common/ folder. */
  folderIcon: 'folder_shared' | 'folder';
  /** Nesting level (0 = top-level). Drives left-padding. */
  depth: number;
  /** Key of the parent folder. Undefined = top-level. */
  parentFolderKey?: string;
}

export interface BicepFileNode {
  kind: 'file';
  /** Unique path used as the viewer state key, e.g. 'main.bicep' or 'Common/types.bicep'. */
  path: string;
  /** File name shown in the tree, e.g. 'main.bicep'. */
  displayName: string;
  /** Semantic file type used to derive icon and badge. */
  type: BicepFileType;
  /** URI passed to the loadFile function when the file is opened. */
  uri: string;
  /** Nesting level (0 = top-level, 1 = inside a folder, …). */
  depth: number;
  /** Key of the parent folder. Empty string = direct child of root. */
  parentFolderKey: string;
}

export type BicepTreeNode = BicepFolderNode | BicepFileNode;

// ─── Component ─────────────────────────────────────────────────────────────────

@Component({
  selector: 'app-bicep-file-panel',
  standalone: true,
  imports: [MatIconModule, TranslateModule, BicepHighlightPipe, DsSpinnerComponent],
  templateUrl: './bicep-file-panel.component.html',
  styleUrl: './bicep-file-panel.component.scss',
})
export class BicepFilePanelComponent {
  private readonly injector = inject(Injector);
  private readonly userPreferencesService = inject(UserPreferencesService);
  private loadRequestSequence = 0;

  /** Flat, ordered list of tree nodes. Parent folders must precede their children. */
  readonly nodes = input.required<BicepTreeNode[]>();
  /** Text displayed in the terminal window-chrome title bar. */
  readonly barTitle = input<string>('bicep-generator');
  /** Line shown below the prompt when generation is done. */
  readonly terminalDoneText = input<string>('');
  /** Text shown in the viewer while a file is loading. */
  readonly fileLoadingText = input<string>('');
  /** Text shown in the viewer when a file fails to load. */
  readonly fileErrorText = input<string>('');
  /** Removes the outer shell when the panel is rendered inside another workspace container. */
  readonly embedded = input(false);
  /**
   * Function used to load file content.
   * Config-detail uses the Bicep API; project-detail uses native fetch() on SAS URLs.
   */
  readonly loadFile = input.required<(uri: string) => Promise<string>>();

  protected readonly expandedFolders = signal<Set<string>>(new Set());
  protected readonly viewerFile = signal<string | null>(null);
  protected readonly viewerContent = signal<string | null>(null);
  protected readonly viewerLoading = signal(false);
  protected readonly bicepViewerTheme = this.userPreferencesService.bicepViewerTheme;
  protected readonly viewerSectionRef = viewChild<ElementRef<HTMLElement>>('viewerSectionRef');
  protected readonly fileItemRefs = viewChildren<ElementRef<HTMLButtonElement>>('fileItemRef');

  constructor() {
    // Auto-expand ALL folders when a fresh result arrives; reset state when cleared.
    effect(
      () => {
        const nodes = this.nodes();
        if (nodes.length === 0) {
          this.expandedFolders.set(new Set());
          this.viewerFile.set(null);
          this.viewerContent.set(null);
          return;
        }
        const allFolderKeys = new Set(
          nodes
            .filter((n): n is BicepFolderNode => n.kind === 'folder')
            .map(n => n.key),
        );
        this.expandedFolders.set(allFolderKeys);
      },
    );
  }

  /**
   * Returns true when the node should be rendered.
   * A node is visible when its entire ancestor-folder chain is expanded.
   */
  protected isNodeVisible(node: BicepTreeNode): boolean {
    const parentKey =
      node.kind === 'folder' ? (node.parentFolderKey ?? '') : node.parentFolderKey;
    if (!parentKey) return true;
    const expanded = this.expandedFolders();
    const parts = parentKey.split('/');
    for (let i = 1; i <= parts.length; i++) {
      if (!expanded.has(parts.slice(0, i).join('/'))) return false;
    }
    return true;
  }

  protected toggleFolder(key: string): void {
    this.expandedFolders.update(set => {
      const next = new Set(set);
      if (next.has(key)) {
        next.delete(key);
      } else {
        next.add(key);
      }
      return next;
    });
  }

  protected isFolderExpanded(key: string): boolean {
    return this.expandedFolders().has(key);
  }

  protected async openFile(path: string, uri: string): Promise<void> {
    if (this.viewerFile() === path) {
      this.cancelActiveLoad();
      this.viewerFile.set(null);
      this.viewerContent.set(null);
      this.viewerLoading.set(false);
      return;
    }

    const loadRequestId = ++this.loadRequestSequence;
    this.viewerFile.set(path);
    this.viewerContent.set(null);
    this.viewerLoading.set(true);
    this.scrollViewerIntoView();

    try {
      const content = await this.loadFile()(uri);
      if (loadRequestId !== this.loadRequestSequence) {
        return;
      }

      this.viewerContent.set(content);
    } catch {
      if (loadRequestId !== this.loadRequestSequence) {
        return;
      }

      this.viewerContent.set(null);
    } finally {
      if (loadRequestId === this.loadRequestSequence) {
        this.viewerLoading.set(false);
      }
    }
  }

  protected closeViewer(): void {
    this.cancelActiveLoad();
    this.viewerFile.set(null);
    this.viewerContent.set(null);
    this.viewerLoading.set(false);
  }

  protected scrollToCurrentFile(): void {
    const fileNode = this.getFileNode(this.viewerFile());

    if (!fileNode) {
      return;
    }

    this.expandFolderPath(fileNode.parentFolderKey);
    this.runAfterNextRender(() => {
      this.getFileItemElement(fileNode.path)?.scrollIntoView(ACTIVE_FILE_SCROLL_OPTIONS);
    });
  }

  protected getFileIcon(type: BicepFileType): string {
    switch (type) {
      case 'types': return 'data_object';
      case 'functions': return 'functions';
      case 'constants':
      case 'role-assignments': return 'shield';
      case 'entry-point': return 'description';
      case 'params': return 'tune';
      case 'module-type': return 'memory';
      default: return 'description';
    }
  }

  /** Returns { key, variant } for the badge, or null when no badge should be shown. */
  protected getFileBadge(type: BicepFileType): { key: string; variant: string } | null {
    switch (type) {
      case 'types':
        return { key: 'CONFIG_DETAIL.BICEP.TYPES', variant: 'types' };
      case 'functions':
        return { key: 'CONFIG_DETAIL.BICEP.FUNCTIONS', variant: 'functions' };
      case 'constants':
      case 'role-assignments':
        return { key: 'CONFIG_DETAIL.BICEP.CONSTANTS', variant: 'constants' };
      case 'entry-point':
        return { key: 'CONFIG_DETAIL.BICEP.ENTRY_POINT', variant: '' };
      case 'params':
        return { key: 'CONFIG_DETAIL.BICEP.PARAMETERS', variant: 'params' };
      case 'module-type':
        return { key: 'CONFIG_DETAIL.BICEP.MODULE', variant: 'module' };
      default:
        return null;
    }
  }

  /** Builds the full CSS class string for a badge given an optional variant suffix. */
  protected badgeClass(variant?: string): string {
    return variant ? `bicep-tree__badge bicep-tree__badge--${variant}` : 'bicep-tree__badge';
  }

  /** Computes padding-left for file nodes based on nesting depth. */
  protected getFilePaddingLeft(depth: number): string {
    return depth === 0 ? '0.75rem' : `calc(0.75rem + ${depth} * 1.45rem)`;
  }

  /** Computes padding-left for folder nodes based on nesting depth. */
  protected getFolderPaddingLeft(depth: number): string {
    return depth === 0 ? '0.75rem' : `calc(0.75rem + ${depth} * 1rem)`;
  }

  private scrollViewerIntoView(): void {
    this.runAfterNextRender(() => {
      this.viewerSectionRef()?.nativeElement.scrollIntoView(VIEWER_SCROLL_OPTIONS);
    });
  }

  private cancelActiveLoad(): void {
    this.loadRequestSequence += 1;
  }

  private runAfterNextRender(callback: () => void): void {
    runInInjectionContext(this.injector, () => {
      afterNextRender(callback);
    });
  }

  private getFileItemElement(path: string | null): HTMLButtonElement | null {
    if (!path) {
      return null;
    }

    return this.fileItemRefs()
      .find(fileItemRef => fileItemRef.nativeElement.dataset[FILE_PATH_DATASET_KEY] === path)
      ?.nativeElement ?? null;
  }

  private getFileNode(path: string | null): BicepFileNode | null {
    if (!path) {
      return null;
    }

    return this.nodes().find(
      (node): node is BicepFileNode => node.kind === 'file' && node.path === path,
    ) ?? null;
  }

  private expandFolderPath(parentFolderKey: string): void {
    if (!parentFolderKey) {
      return;
    }

    this.expandedFolders.update(currentFolders => {
      const expandedFolders = new Set(currentFolders);
      const pathSegments = parentFolderKey.split('/');

      for (let index = 1; index <= pathSegments.length; index++) {
        expandedFolders.add(pathSegments.slice(0, index).join('/'));
      }

      return expandedFolders;
    });
  }
}
