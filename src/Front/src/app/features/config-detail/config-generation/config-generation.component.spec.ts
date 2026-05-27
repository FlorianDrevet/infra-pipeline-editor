import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, ParamMap, Router, convertToParamMap } from '@angular/router';

import { InfrastructureConfigResponse } from '../../../shared/interfaces/infra-config.interface';
import { InfraConfigService } from '../../../shared/services/infra-config.service';
import { ConfigGenerationComponent } from './config-generation.component';

describe('ConfigGenerationComponent', () => {
  let fixture: ComponentFixture<ConfigGenerationComponent>;
  let infraConfigServiceSpy: jasmine.SpyObj<Pick<InfraConfigService, 'getById'>>;
  let routerSpy: jasmine.SpyObj<Pick<Router, 'navigate'>>;

  beforeEach(async () => {
    infraConfigServiceSpy = jasmine.createSpyObj<Pick<InfraConfigService, 'getById'>>('InfraConfigService', ['getById']);
    routerSpy = jasmine.createSpyObj<Pick<Router, 'navigate'>>('Router', ['navigate']);
    routerSpy.navigate.and.returnValue(Promise.resolve(true));

    await TestBed.configureTestingModule({
      imports: [ConfigGenerationComponent],
      providers: [
        {
          provide: ActivatedRoute,
          useValue: createActivatedRouteStub('config-456'),
        },
        {
          provide: InfraConfigService,
          useValue: infraConfigServiceSpy,
        },
        {
          provide: Router,
          useValue: routerSpy,
        },
      ],
    }).compileComponents();
  });

  it('Given_ConfigRoute_When_ProjectIdIsResolved_Then_NavigatesToProjectGenerationPage', async () => {
    infraConfigServiceSpy.getById.and.resolveTo(createInfrastructureConfigResponse({ projectId: 'project-123' }));

    fixture = TestBed.createComponent(ConfigGenerationComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(infraConfigServiceSpy.getById).toHaveBeenCalledOnceWith('config-456');
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/projects', 'project-123', 'generate']);
  });

  it('Given_ConfigRoute_When_ProjectIdCannotBeResolved_Then_FallsBackToConfigDetail', async () => {
    infraConfigServiceSpy.getById.and.rejectWith(new Error('lookup failed'));

    fixture = TestBed.createComponent(ConfigGenerationComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(routerSpy.navigate).toHaveBeenCalledWith(['/config', 'config-456']);
  });

  it('Given_ConfigRoute_When_ConfigHasNoProjectId_Then_FallsBackToConfigDetail', async () => {
    infraConfigServiceSpy.getById.and.resolveTo(createInfrastructureConfigResponse({ projectId: '' }));

    fixture = TestBed.createComponent(ConfigGenerationComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(routerSpy.navigate).toHaveBeenCalledWith(['/config', 'config-456']);
  });

  it('Given_MissingConfigRouteId_When_Initialized_Then_FallsBackToHome', async () => {
    TestBed.resetTestingModule();
    infraConfigServiceSpy = jasmine.createSpyObj<Pick<InfraConfigService, 'getById'>>('InfraConfigService', ['getById']);
    routerSpy = jasmine.createSpyObj<Pick<Router, 'navigate'>>('Router', ['navigate']);
    routerSpy.navigate.and.returnValue(Promise.resolve(true));

    await TestBed.configureTestingModule({
      imports: [ConfigGenerationComponent],
      providers: [
        {
          provide: ActivatedRoute,
          useValue: createActivatedRouteStub(null),
        },
        {
          provide: InfraConfigService,
          useValue: infraConfigServiceSpy,
        },
        {
          provide: Router,
          useValue: routerSpy,
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ConfigGenerationComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(infraConfigServiceSpy.getById).not.toHaveBeenCalled();
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/']);
  });
});

function createActivatedRouteStub(id: string | null): ActivatedRoute {
  return {
    snapshot: {
      paramMap: createParamMap(id),
    } as ActivatedRoute['snapshot'],
  } as unknown as ActivatedRoute;
}

function createParamMap(id: string | null): ParamMap {
  return convertToParamMap(id ? { id } : {});
}

function createInfrastructureConfigResponse(
  overrides: Partial<InfrastructureConfigResponse>
): InfrastructureConfigResponse {
  return {
    id: 'config-456',
    name: 'Config 456',
    defaultNamingTemplate: null,
    projectId: 'project-123',
    useProjectNamingConventions: false,
    resourceNamingTemplates: [],
    resourceAbbreviationOverrides: [],
    resourceGroupCount: 0,
    resourceCount: 0,
    crossConfigReferenceCount: 0,
    appPipelineMode: 'single',
    tags: [],
    ...overrides,
  };
}