import {
  BicepFileNode,
  BicepFileType,
  BicepFolderNode,
  BicepTreeNode,
} from '../../shared/components/bicep-file-panel/bicep-file-panel.component';

const COMMON_ROOT_KEY = 'Common';
const COMMON_PREFIX = 'Common/';
const COMMON_MODULES_PREFIX = 'modules/';
const AZURE_DEVOPS_ROOT_KEY = '.azuredevops';
const AZURE_DEVOPS_PREFIX = '.azuredevops/';
const SHARED_BICEP_BADGE_KEY = 'PROJECT_DETAIL.BICEP.SHARED';
const TYPES_FILE_NAME = 'types.bicep';
const FUNCTIONS_FILE_NAME = 'functions.bicep';
const CONSTANTS_FILE_NAME = 'constants.bicep';
const MAIN_BICEP_FILE_NAME = 'main.bicep';
const PARAMS_FILE_SUFFIX = '.bicepparam';
const ROLE_ASSIGNMENTS_FILE_SUFFIX = '.roleassignments.module.bicep';

interface FolderNodeOptions {
  parentFolderKey?: string;
  folderIcon?: BicepFolderNode['folderIcon'];
  badge?: BicepFolderNode['badge'];
  badgeVariant?: BicepFolderNode['badgeVariant'];
}

export function buildProjectBicepNodes(
  commonFileUris: Record<string, string>,
  configFileUris: Record<string, Record<string, string>>,
): BicepTreeNode[] {
  const nodes: BicepTreeNode[] = [];

  appendCommonBicepNodes(nodes, commonFileUris);
  appendConfigBicepNodes(nodes, configFileUris);

  return nodes;
}

export function buildAzureDevOpsNodes(
  commonFileUris: Record<string, string>,
  configFileUris: Record<string, Record<string, string>>,
): BicepTreeNode[] {
  const nodes: BicepTreeNode[] = [];
  const commonEntries = Object.keys(commonFileUris);
  const configEntries = Object.entries(configFileUris);

  if (commonEntries.length === 0 && configEntries.length === 0) {
    return nodes;
  }

  ensureFolderNode(nodes, AZURE_DEVOPS_ROOT_KEY, `${AZURE_DEVOPS_ROOT_KEY}/`, 0, { folderIcon: 'folder_shared' });

  for (const filePath of commonEntries) {
    const relativePath = stripPrefix(filePath, AZURE_DEVOPS_PREFIX);
    if (!relativePath) {
      continue;
    }

    appendHierarchicalFileNode(nodes, AZURE_DEVOPS_ROOT_KEY, filePath, relativePath, resolveGenericFileType);
  }

  for (const [configName, files] of configEntries) {
    for (const filePath of Object.keys(files)) {
      const backendPath = normalizeAzureDevOpsConfigPath(configName, filePath);
      const relativePath = stripPrefix(backendPath, AZURE_DEVOPS_PREFIX);
      if (!relativePath) {
        continue;
      }

      appendHierarchicalFileNode(nodes, AZURE_DEVOPS_ROOT_KEY, backendPath, relativePath, resolveGenericFileType);
    }
  }

  return nodes;
}

function appendCommonBicepNodes(
  nodes: BicepTreeNode[],
  commonFileUris: Record<string, string>,
): void {
  const commonEntries = Object.keys(commonFileUris)
    .map((filePath) => stripPrefix(filePath, COMMON_PREFIX))
    .filter((relativePath) => relativePath.length > 0);

  if (commonEntries.length === 0) {
    return;
  }

  ensureFolderNode(
    nodes,
    COMMON_ROOT_KEY,
    `${COMMON_ROOT_KEY}/`,
    0,
    {
      folderIcon: 'folder_shared',
      badge: SHARED_BICEP_BADGE_KEY,
      badgeVariant: 'types',
    },
  );

  const topLevelEntries = commonEntries.filter((relativePath) => !relativePath.startsWith(COMMON_MODULES_PREFIX));
  const moduleEntries = commonEntries.filter((relativePath) => relativePath.startsWith(COMMON_MODULES_PREFIX));

  for (const relativePath of topLevelEntries) {
    appendHierarchicalFileNode(
      nodes,
      COMMON_ROOT_KEY,
      `${COMMON_ROOT_KEY}/${relativePath}`,
      relativePath,
      resolveCommonBicepFileType,
    );
  }

  for (const relativePath of moduleEntries) {
    appendHierarchicalFileNode(
      nodes,
      COMMON_ROOT_KEY,
      `${COMMON_ROOT_KEY}/${relativePath}`,
      relativePath,
      resolveCommonBicepFileType,
    );
  }
}

function appendConfigBicepNodes(
  nodes: BicepTreeNode[],
  configFileUris: Record<string, Record<string, string>>,
): void {
  for (const [configName, files] of Object.entries(configFileUris)) {
    ensureFolderNode(nodes, configName, `${configName}/`, 0);

    for (const fileName of Object.keys(files)) {
      const backendPath = fileName.startsWith(`${configName}/`)
        ? fileName
        : `${configName}/${fileName}`;
      const relativePath = stripPrefix(backendPath, `${configName}/`);
      if (!relativePath) {
        continue;
      }

      appendHierarchicalFileNode(
        nodes,
        configName,
        backendPath,
        relativePath,
        resolveConfigBicepFileType,
      );
    }
  }
}

function ensureFolderNode(
  nodes: BicepTreeNode[],
  key: string,
  name: string,
  depth: number,
  options: FolderNodeOptions = {},
): void {
  if (nodes.some((node) => node.kind === 'folder' && node.key === key)) {
    return;
  }

  const {
    parentFolderKey,
    folderIcon = 'folder',
    badge,
    badgeVariant,
  } = options;

  nodes.push({
    kind: 'folder',
    key,
    name,
    badge,
    badgeVariant,
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
    ensureFolderNode(nodes, folderKey, `${parts[index]}/`, index + 1, { parentFolderKey: parentKey });
    parentKey = folderKey;
  }

  const displayName = parts.at(-1);
  if (!displayName) {
    return;
  }

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

function stripPrefix(path: string, prefix: string): string {
  return path.startsWith(prefix)
    ? path.slice(prefix.length)
    : path;
}

function normalizeAzureDevOpsConfigPath(configName: string, filePath: string): string {
  if (filePath.startsWith(AZURE_DEVOPS_PREFIX)) {
    return filePath;
  }

  if (filePath.startsWith(`${configName}/`)) {
    return filePath;
  }

  return `${configName}/${filePath}`;
}

function resolveCommonBicepFileType(entryPath: string, displayName: string): BicepFileType {
  if (entryPath.startsWith(COMMON_MODULES_PREFIX)) {
    return resolveModuleBicepFileType(displayName);
  }

  if (displayName === TYPES_FILE_NAME) {
    return 'types';
  }

  if (displayName === FUNCTIONS_FILE_NAME) {
    return 'functions';
  }

  if (displayName === CONSTANTS_FILE_NAME) {
    return 'constants';
  }

  return 'generic';
}

function resolveModuleBicepFileType(displayName: string): BicepFileType {
  if (displayName === TYPES_FILE_NAME) {
    return 'types';
  }

  if (displayName.endsWith(ROLE_ASSIGNMENTS_FILE_SUFFIX)) {
    return 'role-assignments';
  }

  return 'module-type';
}

function resolveConfigBicepFileType(entryPath: string, displayName: string): BicepFileType {
  if (displayName === MAIN_BICEP_FILE_NAME && !entryPath.includes('/')) {
    return 'entry-point';
  }

  if (displayName.endsWith(PARAMS_FILE_SUFFIX)) {
    return 'params';
  }

  if (displayName.endsWith(ROLE_ASSIGNMENTS_FILE_SUFFIX)) {
    return 'role-assignments';
  }

  return 'generic';
}

function resolveGenericFileType(): BicepFileType {
  return 'generic';
}