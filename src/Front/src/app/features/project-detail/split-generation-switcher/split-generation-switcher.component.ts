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
 * Outer tabs: Infra / Code. Inner sub-tabs: Bicep, Pipeline, Bootstrap (Infra) / Pipeline (Code).
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
    BicepFilePanelComponent,
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

  readonly loadBicepFile = input<(filePath: string) => Promise<string>>(() => Promise.resolve(''));
  readonly loadPipelineFile = input<(filePath: string) => Promise<string>>(() => Promise.resolve(''));
  readonly loadBootstrapFile = input<(filePath: string) => Promise<string>>(() => Promise.resolve(''));

  readonly retryBicep = output<void>();
  readonly retryPipeline = output<void>();
  readonly retryBootstrap = output<void>();

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
      total += Object.keys(bs.fileUris ?? {}).length;
    }
    return total;
  });

  protected readonly codeFileCount = computed(() => {
    const p = this.pipelineResult();
    if (!p) return 0;
    let total = Object.keys(p.appCommonFileUris ?? {}).length;
    for (const cfg of Object.values(p.appConfigFileUris ?? {})) {
      total += Object.keys(cfg).length;
    }
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

  // ─── Bootstrap tree (infra-only) ───
  protected readonly bootstrapNodes = computed<BicepTreeNode[]>(() => {
    const result = this.bootstrapResult();
    if (!result) return [];
    const nodes: BicepTreeNode[] = [];
    for (const fileName of Object.keys(result.fileUris)) {
      nodes.push({
        kind: 'file',
        path: fileName,
        displayName: fileName,
        type: 'generic',
        uri: fileName,
        depth: 0,
        parentFolderKey: '',
      } satisfies BicepFileNode);
    }
    return nodes;
  });

  protected readonly hasInfraPipeline = computed(() => this.infraPipelineNodes().length > 0);
  protected readonly hasAppPipeline = computed(() => this.appPipelineNodes().length > 0);

  protected onRetryBicep(): void { this.retryBicep.emit(); }
  protected onRetryPipeline(): void { this.retryPipeline.emit(); }
  protected onRetryBootstrap(): void { this.retryBootstrap.emit(); }
}

// ─── Tree builders (extracted from project-detail.component.ts logic) ───

function buildBicepNodes(
  commonFileUris: Record<string, string>,
  configFileUris: Record<string, Record<string, string>>,
): BicepTreeNode[] {
  const nodes: BicepTreeNode[] = [];

  // Backend (SplitInfraCode) emits shared files at the repository root: keys are e.g.
  // 'types.bicep', 'functions.bicep', 'modules/<ResourceType>/<file>.bicep'.
  // Legacy 'Common/' prefix is stripped defensively to stay forward-compatible.
  const commonEntries = Object.entries(commonFileUris ?? {})
    .map(([key, uri]) => [key.startsWith('Common/') ? key.slice('Common/'.length) : key, uri] as [string, string]);

  // Top-level shared files (types/functions/constants) at depth 0.
  for (const [key] of commonEntries) {
    if (key.startsWith('modules/')) continue;
    const type: BicepFileType =
      key === 'types.bicep' ? 'types'
      : key === 'functions.bicep' ? 'functions'
      : key === 'constants.bicep' ? 'constants'
      : 'generic';
    nodes.push({ kind: 'file', path: key, displayName: key, type, uri: key, depth: 0, parentFolderKey: '' } satisfies BicepFileNode);
  }

  // Shared modules/ folder at depth 0 (no Common/ wrapper).
  const moduleEntries = commonEntries.filter(([key]) => key.startsWith('modules/'));
  if (moduleEntries.length > 0) {
    nodes.push({ kind: 'folder', key: 'modules', name: 'modules/', folderIcon: 'folder', depth: 0 } satisfies BicepFolderNode);
    const folderMap = new Map<string, { name: string; files: Array<{ path: string; displayName: string }> }>();
    for (const [filePath] of moduleEntries) {
      const parts = filePath.split('/');
      if (parts.length < 3) continue;
      const fn = parts[1];
      const folderKey = `modules/${fn}`;
      if (!folderMap.has(folderKey)) folderMap.set(folderKey, { name: fn, files: [] });
      folderMap.get(folderKey)!.files.push({ path: filePath, displayName: parts[2] });
    }
    for (const [folderKey, folder] of folderMap) {
      nodes.push({ kind: 'folder', key: folderKey, name: `${folder.name}/`, folderIcon: 'folder', depth: 1, parentFolderKey: 'modules' } satisfies BicepFolderNode);
      for (const file of folder.files) {
        const type: BicepFileType =
          file.displayName === 'types.bicep' ? 'types'
          : file.displayName.endsWith('.roleassignments.module.bicep') ? 'role-assignments'
          : 'module-type';
        nodes.push({ kind: 'file', path: file.path, displayName: file.displayName, type, uri: file.path, depth: 2, parentFolderKey: folderKey } satisfies BicepFileNode);
      }
    }
  }

  for (const [configName, files] of Object.entries(configFileUris ?? {})) {
    nodes.push({ kind: 'folder', key: configName, name: `${configName}/`, folderIcon: 'folder', depth: 0 } satisfies BicepFolderNode);
    for (const fileName of Object.keys(files)) {
      const parts = fileName.split('/');
      if (parts.length > 1) {
        let parentKey = configName;
        for (let i = 0; i < parts.length - 1; i++) {
          const folderKey = `${configName}/${parts.slice(0, i + 1).join('/')}`;
          if (!nodes.some(n => n.kind === 'folder' && n.key === folderKey)) {
            nodes.push({ kind: 'folder', key: folderKey, name: `${parts[i]}/`, folderIcon: 'folder', depth: i + 1, parentFolderKey: parentKey } satisfies BicepFolderNode);
          }
          parentKey = folderKey;
        }
        const displayName = parts[parts.length - 1];
        const type: BicepFileType =
          displayName.endsWith('.bicepparam') ? 'params'
          : displayName.endsWith('.roleassignments.module.bicep') ? 'role-assignments'
          : 'generic';
        nodes.push({ kind: 'file', path: `${configName}/${fileName}`, displayName, type, uri: `${configName}/${fileName}`, depth: parts.length, parentFolderKey: parentKey } satisfies BicepFileNode);
      } else {
        const type: BicepFileType =
          fileName === 'main.bicep' ? 'entry-point'
          : fileName.endsWith('.bicepparam') ? 'params'
          : fileName.endsWith('.roleassignments.module.bicep') ? 'role-assignments'
          : 'generic';
        nodes.push({ kind: 'file', path: `${configName}/${fileName}`, displayName: fileName, type, uri: `${configName}/${fileName}`, depth: 1, parentFolderKey: configName } satisfies BicepFileNode);
      }
    }
  }

  return nodes;
}

function buildPipelineNodes(
  commonFileUris: Record<string, string>,
  configFileUris: Record<string, Record<string, string>>,
): BicepTreeNode[] {
  const nodes: BicepTreeNode[] = [];

  if (commonFileUris && Object.keys(commonFileUris).length > 0) {
    nodes.push({ kind: 'folder', key: '.azuredevops', name: '.azuredevops/', folderIcon: 'folder_shared', depth: 0 } satisfies BicepFolderNode);
    const subfolderMap = new Map<string, Array<{ path: string; displayName: string }>>();
    for (const key of Object.keys(commonFileUris)) {
      const relativePath = key.startsWith('.azuredevops/') ? key.slice('.azuredevops/'.length) : key;
      const parts = relativePath.split('/');
      if (parts.length >= 3) {
        const subfolderKey = parts.slice(0, parts.length - 1).join('/');
        if (!subfolderMap.has(subfolderKey)) subfolderMap.set(subfolderKey, []);
        subfolderMap.get(subfolderKey)!.push({ path: key, displayName: parts[parts.length - 1] });
      } else if (parts.length === 2) {
        const subfolderKey = parts[0];
        if (!subfolderMap.has(subfolderKey)) subfolderMap.set(subfolderKey, []);
        subfolderMap.get(subfolderKey)!.push({ path: key, displayName: parts[1] });
      } else {
        nodes.push({ kind: 'file', path: key, displayName: parts[0], type: 'generic', uri: key, depth: 1, parentFolderKey: '.azuredevops' } satisfies BicepFileNode);
      }
    }
    for (const [subfolderPath, files] of subfolderMap) {
      const subParts = subfolderPath.split('/');
      let parentKey = '.azuredevops';
      for (let i = 0; i < subParts.length; i++) {
        const folderKey = `.azuredevops/${subParts.slice(0, i + 1).join('/')}`;
        if (!nodes.some(n => n.kind === 'folder' && n.key === folderKey)) {
          nodes.push({ kind: 'folder', key: folderKey, name: `${subParts[i]}/`, folderIcon: 'folder', depth: i + 1, parentFolderKey: parentKey } satisfies BicepFolderNode);
        }
        parentKey = folderKey;
      }
      for (const file of files) {
        nodes.push({ kind: 'file', path: file.path, displayName: file.displayName, type: 'generic', uri: file.path, depth: subParts.length + 1, parentFolderKey: parentKey } satisfies BicepFileNode);
      }
    }
  }

  for (const [configName, files] of Object.entries(configFileUris ?? {})) {
    nodes.push({ kind: 'folder', key: configName, name: `${configName}/`, folderIcon: 'folder', depth: 0 } satisfies BicepFolderNode);
    for (const fileName of Object.keys(files)) {
      const parts = fileName.split('/');
      if (parts.length > 1) {
        let parentKey = configName;
        for (let i = 0; i < parts.length - 1; i++) {
          const folderKey = `${configName}/${parts.slice(0, i + 1).join('/')}`;
          if (!nodes.some(n => n.kind === 'folder' && n.key === folderKey)) {
            nodes.push({ kind: 'folder', key: folderKey, name: `${parts[i]}/`, folderIcon: 'folder', depth: i + 1, parentFolderKey: parentKey } satisfies BicepFolderNode);
          }
          parentKey = folderKey;
        }
        nodes.push({ kind: 'file', path: `${configName}/${fileName}`, displayName: parts[parts.length - 1], type: 'generic', uri: `${configName}/${fileName}`, depth: parts.length, parentFolderKey: parentKey } satisfies BicepFileNode);
      } else {
        nodes.push({ kind: 'file', path: `${configName}/${fileName}`, displayName: fileName, type: 'generic', uri: `${configName}/${fileName}`, depth: 1, parentFolderKey: configName } satisfies BicepFileNode);
      }
    }
  }

  return nodes;
}
