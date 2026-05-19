import { TestBed } from '@angular/core/testing';

import { MethodEnum } from '../enums/method.enum';
import { ProjectResponse } from '../interfaces/project.interface';
import { AxiosService } from './axios.service';
import { ProjectService } from './project.service';

const PROJECT_ID = 'project-1';
const REPOSITORY_ID = 'repo-1';
const PROJECT_URL = `/projects/${PROJECT_ID}`;
const LATEST_GENERATION_URL = `${PROJECT_URL}/latest-generation`;
const PROJECT_REPOSITORY_PAT_URL = `${PROJECT_URL}/repositories/${REPOSITORY_ID}/git-pat`;
const PROJECT_REPOSITORY_TEST_URL = `${PROJECT_URL}/repositories/${REPOSITORY_ID}/test-connection`;
const PROJECT_REPOSITORY_VERIFY_URL = `${PROJECT_URL}/repositories/verify`;
const EXISTING_PROJECT_REPOSITORY_VERIFY_URL = `${PROJECT_URL}/repositories/${REPOSITORY_ID}/verify`;

interface ProjectMutationCase {
  readonly description: string;
  readonly run: (service: ProjectService) => Promise<unknown>;
}

describe('ProjectService', () => {
  let service: ProjectService;
  let axiosServiceSpy: jasmine.SpyObj<AxiosService>;

  beforeEach(() => {
    axiosServiceSpy = jasmine.createSpyObj<AxiosService>('AxiosService', ['request$']);

    TestBed.configureTestingModule({
      providers: [
        ProjectService,
        { provide: AxiosService, useValue: axiosServiceSpy },
      ],
    });

    service = TestBed.inject(ProjectService);
  });

  const projectMutationCases: readonly ProjectMutationCase[] = [
    {
      description: 'adding a member',
      run: (currentService) => currentService.addMember(PROJECT_ID, { userId: 'user-1', role: 'Owner' }),
    },
    {
      description: 'updating a member role',
      run: (currentService) => currentService.updateMemberRole(PROJECT_ID, 'user-1', { newRole: 'Reader' }),
    },
    {
      description: 'removing a member',
      run: (currentService) => currentService.removeMember(PROJECT_ID, 'user-1'),
    },
    {
      description: 'adding an environment',
      run: (currentService) => currentService.addEnvironment(PROJECT_ID, {
        name: 'Development',
        location: 'westeurope',
        subscriptionId: 'subscription-1',
      }),
    },
    {
      description: 'updating an environment',
      run: (currentService) => currentService.updateEnvironment(PROJECT_ID, 'env-1', {
        name: 'Development',
        location: 'westeurope',
        subscriptionId: 'subscription-1',
      }),
    },
    {
      description: 'removing an environment',
      run: (currentService) => currentService.removeEnvironment(PROJECT_ID, 'env-1'),
    },
    {
      description: 'setting the default naming template',
      run: (currentService) => currentService.setDefaultNamingTemplate(PROJECT_ID, { template: '{project}-{env}' }),
    },
    {
      description: 'setting a resource naming template',
      run: (currentService) => currentService.setResourceNamingTemplate(PROJECT_ID, 'StorageAccount', { template: '{project}sa' }),
    },
    {
      description: 'removing a resource naming template',
      run: (currentService) => currentService.removeResourceNamingTemplate(PROJECT_ID, 'StorageAccount'),
    },
    {
      description: 'setting a resource abbreviation',
      run: (currentService) => currentService.setResourceAbbreviation(PROJECT_ID, 'StorageAccount', { abbreviation: 'st' }),
    },
    {
      description: 'removing a resource abbreviation',
      run: (currentService) => currentService.removeResourceAbbreviation(PROJECT_ID, 'StorageAccount'),
    },
    {
      description: 'setting tags',
      run: (currentService) => currentService.setTags(PROJECT_ID, {
        tags: [{ name: 'environment', value: 'dev' }],
      }),
    },
    {
      description: 'setting the agent pool',
      run: (currentService) => currentService.setAgentPool(PROJECT_ID, { agentPoolName: 'pool-1' }),
    },
    {
      description: 'adding a repository',
      run: (currentService) => currentService.addRepository(PROJECT_ID, {
        contentKinds: ['Infrastructure'],
        providerType: 'GitHub',
        repositoryUrl: 'https://github.com/org/infra',
        defaultBranch: 'main',
        personalAccessToken: 'token',
      }),
    },
    {
      description: 'updating a repository',
      run: (currentService) => currentService.updateRepository(PROJECT_ID, 'repo-1', {
        contentKinds: ['Infrastructure'],
        providerType: 'GitHub',
        repositoryUrl: 'https://github.com/org/infra',
        defaultBranch: 'main',
        personalAccessToken: 'token',
      }),
    },
    {
      description: 'removing a repository',
      run: (currentService) => currentService.removeRepository(PROJECT_ID, 'repo-1'),
    },
    {
      description: 'changing the layout preset',
      run: (currentService) => currentService.setLayoutPreset(PROJECT_ID, 'SplitInfraCode'),
    },
  ];

  for (const mutationCase of projectMutationCases) {
    it(`invalidates the cached project after ${mutationCase.description}`, async () => {
      await expectProjectReloadAfterMutation(mutationCase.run);
    });
  }

  it('returns null when the latest generation endpoint responds with 404', async () => {
    const notFoundError = createAxiosError(404);
    axiosServiceSpy.request$.and.returnValue(Promise.reject(notFoundError));

    const result = await service.getProjectLatestGeneration(PROJECT_ID);

    expect(result).toBeNull();
  });

  it('rethrows latest generation errors when the endpoint does not respond with 404', async () => {
    const serverError = createAxiosError(500);
    axiosServiceSpy.request$.and.returnValue(Promise.reject(serverError));

    try {
      await service.getProjectLatestGeneration(PROJECT_ID);
      fail('Expected getProjectLatestGeneration to reject for non-404 errors.');
    } catch (error) {
      expect(error).toBe(serverError);
    }
  });

  it('stores the repository PAT through the repository-scoped endpoint', async () => {
    const request = {
      personalAccessToken: 'ghp_project_shared_token',
    };
    axiosServiceSpy.request$.and.resolveTo(undefined);

    await service.setGitPat(PROJECT_ID, REPOSITORY_ID, request);

    expect(axiosServiceSpy.request$).toHaveBeenCalledOnceWith(
      MethodEnum.PUT,
      PROJECT_REPOSITORY_PAT_URL,
      request,
    );
  });

  it('tests a specific repository connection through the repository-scoped endpoint', async () => {
    const response = {
      success: true,
      repositoryFullName: 'example/repo-1',
      defaultBranch: 'main',
      errorMessage: null,
    };
    axiosServiceSpy.request$.and.resolveTo(response);

    const result = await service.testRepositoryConnection(PROJECT_ID, REPOSITORY_ID);

    expect(result).toEqual(response);
    expect(axiosServiceSpy.request$).toHaveBeenCalledOnceWith(
      MethodEnum.POST,
      PROJECT_REPOSITORY_TEST_URL,
    );
  });

  it('verifies a new repository connection through the stateless project endpoint', async () => {
    const request = {
      providerType: 'GitHub',
      repositoryUrl: 'https://github.com/org/infra',
      personalAccessToken: 'token',
    };
    const response = {
      owner: 'org',
      repositoryName: 'infra',
      branches: [{ name: 'main', isProtected: true }],
      defaultBranchCandidate: 'main',
    };
    axiosServiceSpy.request$.and.resolveTo(response);

    const result = await service.verifyRepositoryConnection(PROJECT_ID, request);

    expect(result).toEqual(response);
    expect(axiosServiceSpy.request$).toHaveBeenCalledOnceWith(
      MethodEnum.POST,
      PROJECT_REPOSITORY_VERIFY_URL,
      request,
    );
  });

  it('verifies an existing repository connection through the repository-scoped endpoint', async () => {
    const request = {
      providerType: 'GitHub',
      repositoryUrl: 'https://github.com/org/infra',
    };
    const response = {
      owner: 'org',
      repositoryName: 'infra',
      branches: [{ name: 'main', isProtected: false }],
      defaultBranchCandidate: 'main',
    };
    axiosServiceSpy.request$.and.resolveTo(response);

    const result = await service.verifyRepositoryConnection(PROJECT_ID, request, REPOSITORY_ID);

    expect(result).toEqual(response);
    expect(axiosServiceSpy.request$).toHaveBeenCalledOnceWith(
      MethodEnum.POST,
      EXISTING_PROJECT_REPOSITORY_VERIFY_URL,
      request,
    );
  });

  async function expectProjectReloadAfterMutation(
    runMutation: (currentService: ProjectService) => Promise<unknown>,
  ): Promise<void> {
    const initialProject = createProjectResponse('AllInOne');
    const refreshedProject = createProjectResponse('SplitInfraCode');
    let currentProject = initialProject;

    axiosServiceSpy.request$.and.callFake(<T>(method: MethodEnum, url: string): Promise<T> => {
      if (method === MethodEnum.GET && url === PROJECT_URL) {
        return Promise.resolve(currentProject as T);
      }

      if (method === MethodEnum.GET && url === LATEST_GENERATION_URL) {
        return Promise.resolve(null as T);
      }

      return Promise.resolve({} as T);
    });

    const firstLoad = await service.getProject(PROJECT_ID);
    const cachedLoad = await service.getProject(PROJECT_ID);

    expect(firstLoad).toEqual(initialProject);
    expect(cachedLoad).toEqual(initialProject);
    expect(countProjectGetCalls()).toBe(1);

    await runMutation(service);
    currentProject = refreshedProject;

    const refreshedLoad = await service.getProject(PROJECT_ID);

    expect(refreshedLoad).toEqual(refreshedProject);
    expect(countProjectGetCalls()).toBe(2);
  }

  function countProjectGetCalls(): number {
    return axiosServiceSpy.request$.calls
      .allArgs()
      .filter(([method, url]) => method === MethodEnum.GET && url === PROJECT_URL)
      .length;
  }
});

function createProjectResponse(layoutPreset: string): ProjectResponse {
  return {
    id: PROJECT_ID,
    name: 'Project 1',
    members: [],
    environmentDefinitions: [],
    defaultNamingTemplate: null,
    resourceNamingTemplates: [],
    resourceAbbreviations: [],
    tags: [],
    agentPoolName: null,
    repositories: [],
    layoutPreset,
    usedResourceTypes: [],
  };
}

function createAxiosError(status: number): Error & {
  readonly isAxiosError: boolean;
  readonly response: {
    readonly status: number;
  };
} {
  const error: Error & {
    readonly isAxiosError: boolean;
    readonly response: {
      readonly status: number;
    };
  } = {
    name: 'AxiosError',
    message: `Request failed with status code ${status}`,
    isAxiosError: true,
    response: {
      status,
    },
  };

  return error;
}