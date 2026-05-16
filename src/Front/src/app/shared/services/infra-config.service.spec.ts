import { TestBed } from '@angular/core/testing';

import { MethodEnum } from '../enums/method.enum';
import { InfrastructureConfigResponse } from '../interfaces/infra-config.interface';
import { AxiosService } from './axios.service';
import { InfraConfigService } from './infra-config.service';

const CONFIG_ID = 'config-1';
const CONFIG_URL = `/infra-config/${CONFIG_ID}`;

interface InfraConfigMutationCase {
  readonly description: string;
  readonly run: (service: InfraConfigService) => Promise<unknown>;
}

describe('InfraConfigService', () => {
  let service: InfraConfigService;
  let axiosServiceSpy: jasmine.SpyObj<AxiosService>;

  beforeEach(() => {
    axiosServiceSpy = jasmine.createSpyObj<AxiosService>('AxiosService', ['request$']);

    TestBed.configureTestingModule({
      providers: [
        InfraConfigService,
        { provide: AxiosService, useValue: axiosServiceSpy },
      ],
    });

    service = TestBed.inject(InfraConfigService);
  });

  const configMutationCases: readonly InfraConfigMutationCase[] = [
    {
      description: 'setting the default naming template',
      run: (currentService) => currentService.setDefaultNamingTemplate(CONFIG_ID, { template: '{config}-{env}' }),
    },
    {
      description: 'setting a resource naming template',
      run: (currentService) => currentService.setResourceNamingTemplate(CONFIG_ID, 'StorageAccount', { template: '{config}sa' }),
    },
    {
      description: 'removing a resource naming template',
      run: (currentService) => currentService.removeResourceNamingTemplate(CONFIG_ID, 'StorageAccount'),
    },
    {
      description: 'setting a resource abbreviation override',
      run: (currentService) => currentService.setResourceAbbreviationOverride(CONFIG_ID, 'StorageAccount', { abbreviation: 'st' }),
    },
    {
      description: 'removing a resource abbreviation override',
      run: (currentService) => currentService.removeResourceAbbreviationOverride(CONFIG_ID, 'StorageAccount'),
    },
    {
      description: 'changing inheritance',
      run: (currentService) => currentService.setInheritance(CONFIG_ID, { useProjectNamingConventions: true }),
    },
    {
      description: 'adding a cross-config reference',
      run: (currentService) => currentService.addCrossConfigReference(CONFIG_ID, { targetResourceId: 'resource-2' }),
    },
    {
      description: 'removing a cross-config reference',
      run: (currentService) => currentService.removeCrossConfigReference(CONFIG_ID, 'reference-1'),
    },
    {
      description: 'setting tags',
      run: (currentService) => currentService.setTags(CONFIG_ID, {
        tags: [{ name: 'environment', value: 'dev' }],
      }),
    },
  ];

  for (const mutationCase of configMutationCases) {
    it(`invalidates the cached config after ${mutationCase.description}`, async () => {
      await expectConfigReloadAfterMutation(mutationCase.run);
    });
  }

  async function expectConfigReloadAfterMutation(
    runMutation: (currentService: InfraConfigService) => Promise<unknown>,
  ): Promise<void> {
    const initialConfig = createInfraConfigResponse('Config 1');
    const refreshedConfig = createInfraConfigResponse('Config 1 Updated');
    let currentConfig = initialConfig;

    axiosServiceSpy.request$.and.callFake(<T>(method: MethodEnum, url: string): Promise<T> => {
      if (method === MethodEnum.GET && url === CONFIG_URL) {
        return Promise.resolve(currentConfig as T);
      }

      return Promise.resolve({} as T);
    });

    const firstLoad = await service.getById(CONFIG_ID);
    const cachedLoad = await service.getById(CONFIG_ID);

    expect(firstLoad).toEqual(initialConfig);
    expect(cachedLoad).toEqual(initialConfig);
    expect(countConfigGetCalls()).toBe(1);

    await runMutation(service);
    currentConfig = refreshedConfig;

    const refreshedLoad = await service.getById(CONFIG_ID);

    expect(refreshedLoad).toEqual(refreshedConfig);
    expect(countConfigGetCalls()).toBe(2);
  }

  function countConfigGetCalls(): number {
    return axiosServiceSpy.request$.calls
      .allArgs()
      .filter(([method, url]) => method === MethodEnum.GET && url === CONFIG_URL)
      .length;
  }
});

function createInfraConfigResponse(name: string): InfrastructureConfigResponse {
  return {
    id: CONFIG_ID,
    name,
    defaultNamingTemplate: null,
    projectId: 'project-1',
    useProjectNamingConventions: true,
    resourceNamingTemplates: [],
    resourceAbbreviationOverrides: [],
    resourceGroupCount: 0,
    resourceCount: 0,
    crossConfigReferenceCount: 0,
    appPipelineMode: 'Combined',
    tags: [],
    layoutMode: null,
    repositories: [],
  };
}