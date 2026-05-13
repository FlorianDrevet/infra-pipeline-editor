import { GenerateBicepResponse } from '../../../shared/interfaces/bicep-generator.interface';
import { GeneratePipelineResponse } from '../../../shared/interfaces/pipeline-generator.interface';
import {
  BicepFileNode,
  BicepFolderNode,
  BicepTreeNode,
} from '../../../shared/components/bicep-file-panel/bicep-file-panel.component';

interface BicepModuleFileEntry {
  path: string;
  displayName: string;
  uri: string;
}

interface BicepModuleFolderEntry {
  name: string;
  files: BicepModuleFileEntry[];
}

export function buildConfigBicepNodes(result: GenerateBicepResponse): BicepTreeNode[] {
  return [
    ...buildRootBicepNodes(result),
    ...buildParameterBicepNodes(result),
    ...buildModuleBicepNodes(result),
  ];
}

export function buildConfigPipelineNodes(result: GeneratePipelineResponse): BicepTreeNode[] {
  const nodes: BicepTreeNode[] = [];
  const folderMap = new Map<string, { name: string; files: Array<{ path: string; displayName: string; uri: string }> }>();

  for (const [filePath, uri] of Object.entries(result.fileUris)) {
    const parts = filePath.split('/');
    if (parts.length >= 2) {
      const folderName = parts.slice(0, -1).join('/');
      const displayName = parts.at(-1) ?? filePath;
      const existingFolder = folderMap.get(folderName);

      if (existingFolder) {
        existingFolder.files.push({ path: filePath, displayName, uri });
        continue;
      }

      folderMap.set(folderName, { name: folderName, files: [{ path: filePath, displayName, uri }] });
      continue;
    }

    nodes.push({
      kind: 'file',
      path: filePath,
      displayName: filePath,
      type: 'generic',
      uri,
      depth: 0,
      parentFolderKey: '',
    } satisfies BicepFileNode);
  }

  for (const [folderKey, folder] of folderMap) {
    nodes.push({
      kind: 'folder',
      key: folderKey,
      name: `${folder.name}/`,
      folderIcon: 'folder',
      depth: 0,
    } satisfies BicepFolderNode);

    for (const file of folder.files) {
      nodes.push({
        kind: 'file',
        path: file.path,
        displayName: file.displayName,
        type: 'generic',
        uri: file.uri,
        depth: 1,
        parentFolderKey: folderKey,
      } satisfies BicepFileNode);
    }
  }

  return nodes;
}

function buildRootBicepNodes(result: GenerateBicepResponse): BicepTreeNode[] {
  return [
    { kind: 'file', path: 'types.bicep', displayName: 'types.bicep', type: 'types', uri: 'types.bicep', depth: 0, parentFolderKey: '' } satisfies BicepFileNode,
    { kind: 'file', path: 'functions.bicep', displayName: 'functions.bicep', type: 'functions', uri: 'functions.bicep', depth: 0, parentFolderKey: '' } satisfies BicepFileNode,
    ...(result.constantsBicepUri
      ? [{ kind: 'file', path: 'constants.bicep', displayName: 'constants.bicep', type: 'constants', uri: 'constants.bicep', depth: 0, parentFolderKey: '' } satisfies BicepFileNode]
      : []),
    { kind: 'file', path: 'main.bicep', displayName: 'main.bicep', type: 'entry-point', uri: 'main.bicep', depth: 0, parentFolderKey: '' } satisfies BicepFileNode,
  ];
}

function buildParameterBicepNodes(result: GenerateBicepResponse): BicepTreeNode[] {
  const parameterEntries = Object.entries(result.parameterFileUris ?? {});
  if (parameterEntries.length === 0) {
    return [];
  }

  return [
    { kind: 'folder', key: 'parameters', name: 'parameters/', folderIcon: 'folder', depth: 0 } satisfies BicepFolderNode,
    ...parameterEntries.map(([name, uri]) => ({
      kind: 'file',
      path: name,
      displayName: name.split('/').at(-1) ?? name,
      type: 'params',
      uri,
      depth: 1,
      parentFolderKey: 'parameters',
    } satisfies BicepFileNode)),
  ];
}

function buildModuleBicepNodes(result: GenerateBicepResponse): BicepTreeNode[] {
  const folderMap = buildModuleFolderMap(result.moduleUris);
  if (folderMap.size === 0) {
    return [];
  }

  return [
    { kind: 'folder', key: 'modules', name: 'modules/', folderIcon: 'folder', depth: 0 } satisfies BicepFolderNode,
    ...Array.from(folderMap.entries()).flatMap(([folderKey, folder]) => [
      { kind: 'folder', key: folderKey, name: `${folder.name}/`, folderIcon: 'folder', depth: 1, parentFolderKey: 'modules' } satisfies BicepFolderNode,
      ...folder.files.map((file) => createModuleBicepFileNode(folderKey, file)),
    ]),
  ];
}

function buildModuleFolderMap(moduleUris?: Record<string, string>): Map<string, BicepModuleFolderEntry> {
  const folderMap = new Map<string, BicepModuleFolderEntry>();
  if (!moduleUris) {
    return folderMap;
  }

  for (const [filePath, uri] of Object.entries(moduleUris)) {
    const parts = filePath.split('/');
    if (parts.length < 3) {
      continue;
    }

    const folderName = parts[1];
    const folderKey = `modules/${folderName}`;
    const file: BicepModuleFileEntry = {
      path: filePath,
      displayName: parts[2],
      uri,
    };

    const existingFolder = folderMap.get(folderKey);
    if (existingFolder) {
      existingFolder.files.push(file);
      continue;
    }

    folderMap.set(folderKey, { name: folderName, files: [file] });
  }

  return folderMap;
}

function createModuleBicepFileNode(folderKey: string, file: BicepModuleFileEntry): BicepFileNode {
  return {
    kind: 'file',
    path: file.path,
    displayName: file.displayName,
    type: getModuleBicepFileType(file.displayName),
    uri: file.uri,
    depth: 2,
    parentFolderKey: folderKey,
  } satisfies BicepFileNode;
}

function getModuleBicepFileType(displayName: string): BicepFileNode['type'] {
  if (displayName === 'types.bicep') {
    return 'types';
  }

  if (displayName.endsWith('.roleassignments.module.bicep')) {
    return 'role-assignments';
  }

  return 'module-type';
}