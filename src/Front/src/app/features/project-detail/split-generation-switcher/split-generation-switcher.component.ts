import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  output,
} from '@angular/core';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';
import { DsButtonComponent, DsPanelActionButtonComponent } from '../../../shared/components/ds';
import { BootstrapSetupGuideComponent } from '../bootstrap-setup-guide/bootstrap-setup-guide.component';
import {
  GenerateProjectBicepResponse,
  GenerateProjectBootstrapPipelineResponse,
  GenerateProjectPipelineResponse,
} from '../../../shared/interfaces/project.interface';
import {
  BicepFileNode,
  BicepFileType,
  BicepFilePanelComponent,
  BicepFolderNode,
  BicepTreeNode,
} from '../../../shared/components/bicep-file-panel/bicep-file-panel.component';

/**
 * Two-level switcher rendered when project.layoutPreset === 'SplitInfraCode'.
 * Outer tabs: Infra / Code. Inner sub-tabs: Bicep, Pipeline, Bootstrap (Infra) /
 * Pipeline, Bootstrap (Code).
 */
@Component({
  selector: 'app-split-generation-switcher',
  standalone: true,
  imports: [
    TranslateModule,
    MatChipsModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTabsModule,
    MatTooltipModule,
    DsButtonComponent,
    DsPanelActionButtonComponent,
    BicepFilePanelComponent,
    BootstrapSetupGuideComponent,
  ],
  templateUrl: './split-generation-switcher.component.html',
  styleUrl: './split-generation-switcher.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SplitGenerationSwitcherComponent {
  readonly bicepResult = input<GenerateProjectBicepResponse | null>(null);
  readonly pipelineResult = input<GenerateProjectPipelineResponse | null>(null);
  readonly bootstrapResult = input<GenerateProjectBootstrapPipelineResponse | null>(null);

  readonly isGeneratingBicep = input<boolean>(false);
  readonly isGeneratingPipeline = input<boolean>(false);
  readonly isGeneratingBootstrap = input<boolean>(false);

  readonly bicepGenerationError = input<string | null>(null);
  readonly pipelineGenerationError = input<string | null>(null);
  readonly bootstrapGenerationError = input<string | null>(null);
  readonly collapsed = input<boolean>(false);

  readonly loadBicepFile = input<(filePath: string) => Promise<string>>(() => Promise.resolve(''));
  readonly loadPipelineFile = input<(filePath: string) => Promise<string>>(() => Promise.resolve(''));
  readonly loadBootstrapFile = input<(filePath: string) => Promise<string>>(() => Promise.resolve(''));

  readonly retryBicep = output<void>();
  readonly retryPipeline = output<void>();
  readonly retryBootstrap = output<void>();
  readonly pushInfra = output<void>();
  readonly pushCode = output<void>();
  readonly downloadInfraZip = output<void>();
  readonly downloadCodeZip = output<void>();
  readonly toggleCollapsed = output<void>();

  readonly isDownloadingInfraZip = input<boolean>(false);
  readonly isDownloadingCodeZip = input<boolean>(false);

  // ─── File counts (for badges) ───
  protected readonly infraFileCount = computed(() => {
    const p = this.pipelineResult();
    const b = this.bicepResult();
    const bs = this.bootstrapResult();
    let total = 0;
    if (b) {
      total += Object.keys(b.commonFileUris ?? {}).length;
      for (const cfg of Object.values(b.configFileUris ?? {})) {
        total += Object.keys(cfg).length;
      }
    }
    if (p) {
      total += Object.keys(p.infraCommonFileUris ?? {}).length;
      for (const cfg of Object.values(p.infraConfigFileUris ?? {})) {
        total += Object.keys(cfg).length;
      }
    }
    if (bs) {
      total += Object.keys(bs.infraFileUris ?? {}).length;
    }
    return total;
  });

  protected readonly codeFileCount = computed(() => {
    const p = this.pipelineResult();
    const bs = this.bootstrapResult();
    let total = Object.keys(p?.appCommonFileUris ?? {}).length;
    for (const cfg of Object.values(p?.appConfigFileUris ?? {})) {
      total += Object.keys(cfg).length;
    }
    total += Object.keys(bs?.appFileUris ?? {}).length;
    return total;
  });

  // ─── Bicep tree (infra-only) ───
  protected readonly bicepNodes = computed<BicepTreeNode[]>(() => {
    const result = this.bicepResult();
    if (!result) return [];
    return buildBicepNodes(result.commonFileUris, result.configFileUris);
  });

  // ─── Pipeline trees (infra/app split) ───
  protected readonly infraPipelineNodes = computed<BicepTreeNode[]>(() => {
    const result = this.pipelineResult();
    if (!result) return [];
    return buildPipelineNodes(result.infraCommonFileUris, result.infraConfigFileUris);
  });

  protected readonly appPipelineNodes = computed<BicepTreeNode[]>(() => {
    const result = this.pipelineResult();
    if (!result) return [];
    return buildPipelineNodes(result.appCommonFileUris, result.appConfigFileUris);
  });

  // ─── Bootstrap trees (infra/app split) ───
  protected readonly infraBootstrapNodes = computed<BicepTreeNode[]>(() => {
    const result = this.bootstrapResult();
    if (!result) return [];
    return buildBootstrapNodes(result.infraFileUris, 'infra');
  });

  protected readonly appBootstrapNodes = computed<BicepTreeNode[]>(() => {
    const result = this.bootstrapResult();
    if (!result) return [];
    return buildBootstrapNodes(result.appFileUris, 'app');
  });

  protected readonly hasInfraPipeline = computed(() => this.infraPipelineNodes().length > 0);
  protected readonly hasAppPipeline = computed(() => this.appPipelineNodes().length > 0);
  protected readonly hasInfraBootstrap = computed(() => this.infraBootstrapNodes().length > 0);
  protected readonly canPushInfra = computed(() => this.bicepNodes().length > 0
    && this.hasInfraPipeline()
    && this.hasInfraBootstrap()
    && !this.isGeneratingBicep()
    && !this.isGeneratingPipeline()
    && !this.isGeneratingBootstrap());
  protected readonly canPushCode = computed(() => this.hasAppPipeline()
    && this.appBootstrapNodes().length > 0
    && !this.isGeneratingPipeline()
    && !this.isGeneratingBootstrap());
  protected readonly canDownloadInfraZip = computed(() => this.bicepNodes().length > 0
    && this.hasInfraPipeline()
    && this.hasInfraBootstrap()
    && !this.isGeneratingBicep()
    && !this.isGeneratingPipeline()
    && !this.isGeneratingBootstrap()
    && !this.isDownloadingInfraZip());
  protected readonly canDownloadCodeZip = computed(() => this.hasAppPipeline()
    && this.appBootstrapNodes().length > 0
    && !this.isGeneratingPipeline()
    && !this.isGeneratingBootstrap()
    && !this.isDownloadingCodeZip());

  protected onRetryBicep(): void { this.retryBicep.emit(); }
  protected onRetryPipeline(): void { this.retryPipeline.emit(); }
  protected onRetryBootstrap(): void { this.retryBootstrap.emit(); }
  protected onPushInfra(): void {
    if (!this.canPushInfra()) return;
    this.pushInfra.emit();
  }

  protected onPushCode(): void {
    if (!this.canPushCode()) return;
    this.pushCode.emit();
  }

  protected onDownloadInfraZip(): void {
    if (!this.canDownloadInfraZip()) return;
    this.downloadInfraZip.emit();
  }

  protected onDownloadCodeZip(): void {
    if (!this.canDownloadCodeZip()) return;
    this.downloadCodeZip.emit();
  }

  protected onToggleCollapsed(): void {
    this.toggleCollapsed.emit();
  }
}

// ─── Tree builders (extracted from project-detail.component.ts logic) ───

function buildBicepNodes(
  commonFileUris: Record<string, string>,
  configFileUris: Record<string, Record<string, string>>,
): BicepTreeNode[] {
  const nodes: BicepTreeNode[] = [];

  const commonEntries = Object.keys(commonFileUris ?? {});
  if (commonEntries.length > 0) {
    ensureFolderNode(nodes, 'Common', 'Common/', 0, undefined, 'folder_shared');

    for (const filePath of commonEntries) {
      const relativePath = filePath.startsWith('Common/')
        ? filePath.slice('Common/'.length)
        : filePath;

      if (!relativePath) {
        continue;
      }

      appendHierarchicalFileNode(
        nodes,
        'Common',
        filePath,
        relativePath,
        (entryPath, displayName) => {
          if (entryPath.startsWith('modules/')) {
            return displayName === 'types.bicep'
              ? 'types'
              : displayName.endsWith('.roleassignments.module.bicep')
                ? 'role-assignments'
                : 'module-type';
          }

          return displayName === 'types.bicep'
            ? 'types'
            : displayName === 'functions.bicep'
              ? 'functions'
              : displayName === 'constants.bicep'
                ? 'constants'
                : 'generic';
        },
      );
    }
  }

  for (const [configName, files] of Object.entries(configFileUris ?? {})) {
    ensureFolderNode(nodes, configName, `${configName}/`, 0);

    for (const fileName of Object.keys(files)) {
      const backendPath = fileName.startsWith(`${configName}/`)
        ? fileName
        : `${configName}/${fileName}`;
      const relativePath = backendPath.startsWith(`${configName}/`)
        ? backendPath.slice(configName.length + 1)
        : backendPath;

      if (!relativePath) {
        continue;
      }

      appendHierarchicalFileNode(
        nodes,
        configName,
        backendPath,
        relativePath,
        (entryPath, displayName) => {
          if (displayName === 'main.bicep' && !entryPath.includes('/')) {
            return 'entry-point';
          }

          return displayName.endsWith('.bicepparam')
            ? 'params'
            : displayName.endsWith('.roleassignments.module.bicep')
              ? 'role-assignments'
              : 'generic';
        },
      );
    }
  }

  return nodes;
}

function buildPipelineNodes(
  commonFileUris: Record<string, string>,
  configFileUris: Record<string, Record<string, string>>,
): BicepTreeNode[] {
  const nodes: BicepTreeNode[] = [];

  const commonEntries = Object.keys(commonFileUris ?? {});
  const configEntries = Object.entries(configFileUris ?? {});

  if (commonEntries.length === 0 && configEntries.length === 0) {
    return nodes;
  }

  ensureFolderNode(nodes, '.azuredevops', '.azuredevops/', 0, undefined, 'folder_shared');

  for (const filePath of commonEntries) {
    const relativePath = filePath.startsWith('.azuredevops/')
      ? filePath.slice('.azuredevops/'.length)
      : filePath;

    if (!relativePath) {
      continue;
    }

    appendHierarchicalFileNode(nodes, '.azuredevops', filePath, relativePath, () => 'generic');
  }

  for (const [configName, files] of configEntries) {
    for (const filePath of Object.keys(files)) {
      const backendPath = filePath.startsWith('.azuredevops/')
        ? filePath
        : filePath.startsWith(`${configName}/`)
          ? filePath
          : `${configName}/${filePath}`;
      const relativePath = backendPath.startsWith('.azuredevops/')
        ? backendPath.slice('.azuredevops/'.length)
        : backendPath;

      if (!relativePath) {
        continue;
      }

      appendHierarchicalFileNode(nodes, '.azuredevops', backendPath, relativePath, () => 'generic');
    }
  }

  return nodes;
}

function buildBootstrapNodes(
  fileUris: Record<string, string>,
  bucketPrefix?: string,
): BicepTreeNode[] {
  const nodes: BicepTreeNode[] = [];

  for (const filePath of Object.keys(fileUris ?? {})) {
    appendBucketedFileNode(nodes, filePath, bucketPrefix ? `${bucketPrefix}/${filePath}` : filePath);
  }

  return nodes;
}

function ensureFolderNode(
  nodes: BicepTreeNode[],
  key: string,
  name: string,
  depth: number,
  parentFolderKey?: string,
  folderIcon: BicepFolderNode['folderIcon'] = 'folder',
): void {
  if (nodes.some((node) => node.kind === 'folder' && node.key === key)) {
    return;
  }

  nodes.push({
    kind: 'folder',
    key,
    name,
    folderIcon,
    depth,
    parentFolderKey,
  } satisfies BicepFolderNode);
}

function appendHierarchicalFileNode(
  nodes: BicepTreeNode[],
  rootKey: string,
  backendPath: string,
  relativePath: string,
  resolveType: (entryPath: string, displayName: string) => BicepFileType,
): void {
  const parts = relativePath.split('/').filter((part) => part.length > 0);
  if (parts.length === 0) {
    return;
  }

  let parentKey = rootKey;
  for (let index = 0; index < parts.length - 1; index++) {
    const folderKey = `${rootKey}/${parts.slice(0, index + 1).join('/')}`;
    ensureFolderNode(nodes, folderKey, `${parts[index]}/`, index + 1, parentKey);
    parentKey = folderKey;
  }

  const displayName = parts[parts.length - 1];
  nodes.push({
    kind: 'file',
    path: backendPath,
    displayName,
    type: resolveType(relativePath, displayName),
    uri: backendPath,
    depth: parts.length,
    parentFolderKey: parentKey,
  } satisfies BicepFileNode);
}

function appendBucketedFileNode(
  nodes: BicepTreeNode[],
  relativePath: string,
  uri: string,
): void {
  const parts = relativePath.split('/').filter((part) => part.length > 0);
  if (parts.length === 0) {
    return;
  }

  let parentKey = '';
  for (let index = 0; index < parts.length - 1; index++) {
    const folderKey = parts.slice(0, index + 1).join('/');
    ensureFolderNode(nodes, folderKey, `${parts[index]}/`, index, parentKey || undefined);
    parentKey = folderKey;
  }

  const displayName = parts[parts.length - 1];
  nodes.push({
    kind: 'file',
    path: relativePath,
    displayName,
    type: 'generic',
    uri,
    depth: parts.length - 1,
    parentFolderKey: parentKey,
  } satisfies BicepFileNode);
}
