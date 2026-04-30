import { BicepFileNode, BicepFolderNode } from '../../shared/components/bicep-file-panel/bicep-file-panel.component';

import { buildAzureDevOpsNodes, buildProjectBicepNodes } from './project-detail-tree.helpers';

function expectFolder(nodes: Array<BicepFolderNode | BicepFileNode>, key: string): BicepFolderNode {
  const folder = nodes.find((node): node is BicepFolderNode => node.kind === 'folder' && node.key === key);

  expect(folder).withContext(`Expected folder ${key} to exist.`).toBeDefined();

  return folder!;
}

function expectFile(nodes: Array<BicepFolderNode | BicepFileNode>, path: string): BicepFileNode {
  const file = nodes.find((node): node is BicepFileNode => node.kind === 'file' && node.path === path);

  expect(file).withContext(`Expected file ${path} to exist.`).toBeDefined();

  return file!;
}

describe('project-detail tree helpers', () => {
  it('builds common and config bicep nodes with the expected hierarchy and file types', () => {
    const nodes = buildProjectBicepNodes(
      {
        'Common/types.bicep': 'ignored-common-types-uri',
        'Common/modules/KeyVault/types.bicep': 'ignored-module-types-uri',
        'Common/modules/KeyVault/main.roleassignments.module.bicep': 'ignored-module-role-uri',
      },
      {
        dev: {
          'main.bicep': 'ignored-main-uri',
          'parameters/main.dev.bicepparam': 'ignored-params-uri',
        },
      },
    );

    const commonFolder = expectFolder(nodes, 'Common');
    const moduleFolder = expectFolder(nodes, 'Common/modules');
    const keyVaultFolder = expectFolder(nodes, 'Common/modules/KeyVault');
    const configFolder = expectFolder(nodes, 'dev');
    const configParametersFolder = expectFolder(nodes, 'dev/parameters');
    const commonTypesFile = expectFile(nodes, 'Common/types.bicep');
    const moduleTypesFile = expectFile(nodes, 'Common/modules/KeyVault/types.bicep');
    const moduleRoleAssignmentsFile = expectFile(nodes, 'Common/modules/KeyVault/main.roleassignments.module.bicep');
    const mainFile = expectFile(nodes, 'dev/main.bicep');
    const paramsFile = expectFile(nodes, 'dev/parameters/main.dev.bicepparam');

    expect(commonFolder.folderIcon).toBe('folder_shared');
    expect(commonFolder.badge).toBe('PROJECT_DETAIL.BICEP.SHARED');
    expect(moduleFolder.parentFolderKey).toBe('Common');
    expect(keyVaultFolder.parentFolderKey).toBe('Common/modules');
    expect(configFolder.parentFolderKey).toBeUndefined();
    expect(configParametersFolder.parentFolderKey).toBe('dev');

    expect(commonTypesFile.displayName).toBe('types.bicep');
    expect(commonTypesFile.type).toBe('types');
    expect(commonTypesFile.uri).toBe('Common/types.bicep');
    expect(moduleTypesFile.type).toBe('types');
    expect(moduleRoleAssignmentsFile.type).toBe('role-assignments');
    expect(moduleRoleAssignmentsFile.parentFolderKey).toBe('Common/modules/KeyVault');
    expect(mainFile.type).toBe('entry-point');
    expect(mainFile.parentFolderKey).toBe('dev');
    expect(paramsFile.type).toBe('params');
    expect(paramsFile.parentFolderKey).toBe('dev/parameters');
  });

  it('builds Azure DevOps nodes under the shared root and normalizes config-relative paths', () => {
    const nodes = buildAzureDevOpsNodes(
      {
        '.azuredevops/common/build.yml': 'ignored-common-uri',
      },
      {
        dev: {
          'pipelines/dev.yml': 'ignored-config-uri',
        },
      },
    );

    const rootFolder = expectFolder(nodes, '.azuredevops');
    const commonFolder = expectFolder(nodes, '.azuredevops/common');
    const configFolder = expectFolder(nodes, '.azuredevops/dev');
    const pipelinesFolder = expectFolder(nodes, '.azuredevops/dev/pipelines');
    const sharedFile = expectFile(nodes, '.azuredevops/common/build.yml');
    const configFile = expectFile(nodes, 'dev/pipelines/dev.yml');

    expect(rootFolder.folderIcon).toBe('folder_shared');
    expect(commonFolder.parentFolderKey).toBe('.azuredevops');
    expect(configFolder.parentFolderKey).toBe('.azuredevops');
    expect(pipelinesFolder.parentFolderKey).toBe('.azuredevops/dev');
    expect(sharedFile.displayName).toBe('build.yml');
    expect(sharedFile.parentFolderKey).toBe('.azuredevops/common');
    expect(sharedFile.uri).toBe('.azuredevops/common/build.yml');
    expect(configFile.displayName).toBe('dev.yml');
    expect(configFile.parentFolderKey).toBe('.azuredevops/dev/pipelines');
    expect(configFile.uri).toBe('dev/pipelines/dev.yml');
  });
});