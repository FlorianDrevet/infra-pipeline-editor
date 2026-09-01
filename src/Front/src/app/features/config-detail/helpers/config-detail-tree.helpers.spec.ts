import { BicepFileNode, BicepFolderNode } from '../../../shared/components/bicep-file-panel/bicep-file-panel.component';
import { GenerateBicepResponse } from '../../../shared/interfaces/bicep-generator.interface';
import { GeneratePipelineResponse } from '../../../shared/interfaces/pipeline-generator.interface';
import { GenerateBootstrapResponse } from '../../../shared/interfaces/bootstrap-generator.interface';
import { buildConfigBicepNodes, buildConfigBootstrapNodes, buildConfigPipelineNodes } from './config-detail-tree.helpers';

describe('config detail tree helpers', () => {
  it('builds config bicep nodes with grouped parameter and module folders', () => {
    const nodes = buildConfigBicepNodes(createBicepResponse());

    expectFile(nodes, 'types.bicep');
    expectFile(nodes, 'functions.bicep');
    expectFile(nodes, 'constants.bicep');
    expectFile(nodes, 'main.bicep');

    const parametersFolder = expectFolder(nodes, 'parameters');
    expect(parametersFolder.depth).toBe(0);
    expectFile(nodes, 'parameters/dev.bicepparam');
    expectFile(nodes, 'parameters/prod.bicepparam');

    const modulesFolder = expectFolder(nodes, 'modules');
    expect(modulesFolder.depth).toBe(0);

    const storageFolder = expectFolder(nodes, 'modules/storage');
    expect(storageFolder.parentFolderKey).toBe('modules');
    expectFile(nodes, 'modules/storage/storage.module.bicep');

    const roleAssignmentsFile = expectFile(nodes, 'modules/webapp/webapp.roleassignments.module.bicep');
    expect(roleAssignmentsFile.type).toBe('role-assignments');
  });

  it('builds config pipeline nodes with root files and nested folders', () => {
    const nodes = buildConfigPipelineNodes(createPipelineResponse());

    expectFile(nodes, 'azure-pipelines.yml');

    const pipelinesFolder = expectFolder(nodes, '.azuredevops/pipelines');
    expect(pipelinesFolder.depth).toBe(0);

    const deployFile = expectFile(nodes, '.azuredevops/pipelines/deploy.yml');
    expect(deployFile.parentFolderKey).toBe('.azuredevops/pipelines');
    expect(deployFile.depth).toBe(1);
  });

  it('builds config bootstrap nodes as a flat file list with no folder grouping', () => {
    const nodes = buildConfigBootstrapNodes(createBootstrapResponse());

    expect(nodes.every((node) => node.kind === 'file')).toBeTrue();

    const bootstrapFile = expectFile(nodes, 'bootstrap.pipeline.yml');
    expect(bootstrapFile.depth).toBe(0);
    expect(bootstrapFile.parentFolderKey).toBe('');
    expect(bootstrapFile.displayName).toBe('bootstrap.pipeline.yml');
  });
});

function createBicepResponse(): GenerateBicepResponse {
  return {
    mainBicepUri: 'main.bicep',
    constantsBicepUri: 'constants.bicep',
    parameterFileUris: {
      'parameters/dev.bicepparam': 'parameters/dev.bicepparam',
      'parameters/prod.bicepparam': 'parameters/prod.bicepparam',
    },
    moduleUris: {
      'modules/storage/storage.module.bicep': 'modules/storage/storage.module.bicep',
      'modules/webapp/webapp.roleassignments.module.bicep': 'modules/webapp/webapp.roleassignments.module.bicep',
    },
  };
}

function createPipelineResponse(): GeneratePipelineResponse {
  return {
    fileUris: {
      'azure-pipelines.yml': 'azure-pipelines.yml',
      '.azuredevops/pipelines/deploy.yml': '.azuredevops/pipelines/deploy.yml',
    },
  };
}

function createBootstrapResponse(): GenerateBootstrapResponse {
  return {
    fileUris: {
      'bootstrap.pipeline.yml': 'bootstrap.pipeline.yml',
    },
  };
}

function expectFolder(nodes: Array<BicepFolderNode | BicepFileNode>, key: string): BicepFolderNode {
  const folder = nodes.find((node): node is BicepFolderNode => node.kind === 'folder' && node.key === key);
  expect(folder).withContext(`Expected folder node ${key} in ${JSON.stringify(nodes.map(toNodeIdentifier))}`).toBeDefined();
  return folder!;
}

function expectFile(nodes: Array<BicepFolderNode | BicepFileNode>, path: string): BicepFileNode {
  const file = nodes.find((node): node is BicepFileNode => node.kind === 'file' && node.path === path);
  expect(file).withContext(`Expected file node ${path} in ${JSON.stringify(nodes.map(toNodeIdentifier))}`).toBeDefined();
  return file!;
}

function toNodeIdentifier(node: BicepFolderNode | BicepFileNode): string {
  return node.kind === 'folder'
    ? `folder:${node.key}`
    : `file:${node.path}`;
}