import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { TranslateModule } from '@ngx-translate/core';
import { InfraConfigRepositoryDialogComponent, InfraConfigRepositoryDialogData } from './infra-config-repository-dialog.component';
import { ProjectService } from '../../../shared/services/project.service';
import { InfraConfigRepositoryResponse } from '../../../shared/interfaces/infra-config-repository.interface';
import { RepositoryFormGroup } from '../../../shared/utils/repository-dialog.utils';

interface InfraConfigRepositoryDialogComponentTestApi {
  readonly form: RepositoryFormGroup;
  onSubmit(): Promise<void>;
}

describe('InfraConfigRepositoryDialogComponent', () => {
  let fixture: ComponentFixture<InfraConfigRepositoryDialogComponent>;
  let component: InfraConfigRepositoryDialogComponent;
  let dialogData: InfraConfigRepositoryDialogData;
  let projectServiceSpy: jasmine.SpyObj<ProjectService>;

  beforeEach(async () => {
    dialogData = {
      projectId: 'proj-1',
      configId: 'cfg-1',
      mode: 'create',
    };

    projectServiceSpy = jasmine.createSpyObj<ProjectService>('ProjectService', [
      'addConfigRepository',
      'updateConfigRepository',
    ]);
    projectServiceSpy.addConfigRepository.and.resolveTo({ id: 'repo-1' });
    projectServiceSpy.updateConfigRepository.and.resolveTo();

    await TestBed.configureTestingModule({
      imports: [InfraConfigRepositoryDialogComponent, TranslateModule.forRoot()],
      providers: [
        provideNoopAnimations(),
        { provide: MatDialogRef, useValue: jasmine.createSpyObj('MatDialogRef', ['close']) },
        { provide: MAT_DIALOG_DATA, useFactory: () => dialogData },
        { provide: ProjectService, useValue: projectServiceSpy },
      ],
    }).compileComponents();
  });

  it('should create', () => {
    createComponent();
    expect(component).toBeTruthy();
  });

  it('sends the PAT inside the create request', async () => {
    createComponent();
    const api = getComponentTestApi();

    api.form.controls.providerType.setValue('AzureDevOps');
    api.form.controls.repositoryUrl.setValue('https://dev.azure.com/example/repo-1');
    api.form.controls.defaultBranch.setValue('main');
    api.form.controls.personalAccessToken.setValue('token');
    api.form.controls.contentKinds.controls[0].setValue(true);

    await api.onSubmit();

    expect(projectServiceSpy.addConfigRepository).toHaveBeenCalledOnceWith('proj-1', 'cfg-1', {
      providerType: 'AzureDevOps',
      repositoryUrl: 'https://dev.azure.com/example/repo-1',
      defaultBranch: 'main',
      personalAccessToken: 'token',
      contentKinds: ['Infrastructure'],
    });
  });

  it('sends the PAT inside the update request when a new token is provided', async () => {
    dialogData = {
      projectId: 'proj-1',
      configId: 'cfg-1',
      mode: 'edit',
      existing: createRepositoryResponse('repo-1', ['Infrastructure']),
    };
    createComponent();
    const api = getComponentTestApi();

    api.form.controls.personalAccessToken.setValue('new-token');

    await api.onSubmit();

    expect(projectServiceSpy.updateConfigRepository).toHaveBeenCalledOnceWith('proj-1', 'cfg-1', 'repo-1', {
      providerType: 'AzureDevOps',
      repositoryUrl: 'https://dev.azure.com/example/repo-1',
      defaultBranch: 'main',
      personalAccessToken: 'new-token',
      contentKinds: ['Infrastructure'],
    });
  });

  it('omits the PAT from the update request when the field is left blank', async () => {
    dialogData = {
      projectId: 'proj-1',
      configId: 'cfg-1',
      mode: 'edit',
      existing: createRepositoryResponse('repo-1', ['Infrastructure']),
    };
    createComponent();
    const api = getComponentTestApi();

    await api.onSubmit();

    expect(projectServiceSpy.updateConfigRepository).toHaveBeenCalledOnceWith('proj-1', 'cfg-1', 'repo-1', {
      providerType: 'AzureDevOps',
      repositoryUrl: 'https://dev.azure.com/example/repo-1',
      defaultBranch: 'main',
      contentKinds: ['Infrastructure'],
    });
  });

  function createComponent(): void {
    fixture = TestBed.createComponent(InfraConfigRepositoryDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }

  function getComponentTestApi(): InfraConfigRepositoryDialogComponentTestApi {
    return component as unknown as InfraConfigRepositoryDialogComponentTestApi;
  }
});

function createRepositoryResponse(
  id: string,
  contentKinds: InfraConfigRepositoryResponse['contentKinds'],
): InfraConfigRepositoryResponse {
  return {
    id,
    providerType: 'AzureDevOps',
    repositoryUrl: 'https://dev.azure.com/example/repo-1',
    owner: 'example',
    repositoryName: id,
    defaultBranch: 'main',
    contentKinds,
  };
}
