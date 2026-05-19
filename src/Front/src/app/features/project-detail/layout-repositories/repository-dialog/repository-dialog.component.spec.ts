import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';

import { RepositoryDialogComponent, RepositoryDialogData } from './repository-dialog.component';
import { ProjectService } from '../../../../shared/services/project.service';
import { ProjectRepositoryResponse } from '../../../../shared/interfaces/project-repository.interface';
import { RepositoryFormGroup } from '../../../../shared/utils/repository-dialog.utils';

interface RepositoryDialogComponentTestApi {
  readonly form: RepositoryFormGroup;
  readonly branchOptions: () => readonly { value: string; label: string }[];
  readonly canSave: () => boolean;
  readonly canVerify: () => boolean;
  verifyConnection(): Promise<void>;
  onSubmit(): Promise<void>;
}

describe('RepositoryDialogComponent', () => {
  let fixture: ComponentFixture<RepositoryDialogComponent>;
  let component: RepositoryDialogComponent;
  let dialogData: RepositoryDialogData;
  let projectServiceSpy: jasmine.SpyObj<ProjectService>;

  beforeEach(async () => {
    dialogData = {
      projectId: 'proj-1',
      mode: 'create',
    };

    projectServiceSpy = jasmine.createSpyObj<ProjectService>('ProjectService', [
      'addRepository',
      'updateRepository',
      'verifyRepositoryConnection',
    ]);
    projectServiceSpy.verifyRepositoryConnection.and.resolveTo({
      owner: 'example',
      repositoryName: 'repo-1',
      branches: [
        { name: 'main', isProtected: true },
        { name: 'develop', isProtected: false },
      ],
      defaultBranchCandidate: 'main',
    });
    projectServiceSpy.addRepository.and.resolveTo({ id: 'repo-1' });
    projectServiceSpy.updateRepository.and.resolveTo();

    await TestBed.configureTestingModule({
      imports: [RepositoryDialogComponent, TranslateModule.forRoot()],
      providers: [
        { provide: MatDialogRef, useValue: jasmine.createSpyObj('MatDialogRef', ['close']) },
        { provide: MAT_DIALOG_DATA, useFactory: () => dialogData },
        { provide: ProjectService, useValue: projectServiceSpy },
      ],
    }).compileComponents();
  });

  it('should create', () => {
    createComponent();

    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('renders locked content kinds as tags in the dialog title for slotted layouts', () => {
    dialogData = {
      projectId: 'proj-1',
      mode: 'edit',
      existing: createRepositoryResponse('repo-1', ['Infrastructure', 'ApplicationCode']),
      lockedKinds: ['Infrastructure', 'ApplicationCode'],
    };

    createComponent();

    const dialogText = fixture.nativeElement.textContent ?? '';

    expect(fixture.nativeElement.querySelector('.repo-dialog-title__tag')).not.toBeNull();
    expect(fixture.nativeElement.querySelectorAll('mat-checkbox').length).toBe(0);
    expect(dialogText).toContain('PROJECT_DETAIL.LAYOUT.CONTENT_KIND.Infrastructure');
    expect(dialogText).toContain('PROJECT_DETAIL.LAYOUT.CONTENT_KIND.ApplicationCode');
  });

  it('does not render the removed repository label field', () => {
    createComponent();

    const dialogText = fixture.nativeElement.textContent ?? '';

    expect(dialogText).not.toContain('PROJECT_DETAIL.LAYOUT.FORM.REMOVED_REPOSITORY_LABEL');
  });

  it('keeps save disabled until connection verification succeeds and the branch is verified', async () => {
    createComponent();
    const api = getComponentTestApi();

    api.form.controls.repositoryUrl.setValue('https://github.com/example/repo-1');
    api.form.controls.personalAccessToken.setValue('token');
    api.form.controls.contentKinds.controls[0].setValue(true);
    fixture.detectChanges();

    expect(api.canVerify()).toBeTrue();
    expect(api.canSave()).toBeFalse();
    expect(api.form.controls.defaultBranch.disabled).toBeTrue();

    await api.verifyConnection();
    fixture.detectChanges();

    expect(api.form.controls.defaultBranch.enabled).toBeTrue();
    expect(api.form.controls.defaultBranch.value).toBe('main');
    expect(api.branchOptions().map((option) => option.label)).toEqual(['main']);
    expect(api.canSave()).toBeTrue();

    api.form.controls.defaultBranch.setValue('feature/not-verified');
    fixture.detectChanges();

    expect(api.canSave()).toBeFalse();
  });

  it('sends the PAT inside the create request after a successful verification', async () => {
    createComponent();
    const api = getComponentTestApi();

    api.form.controls.repositoryUrl.setValue('https://github.com/example/repo-1');
    api.form.controls.personalAccessToken.setValue('token');
    api.form.controls.contentKinds.controls[0].setValue(true);
    await api.verifyConnection();
    await api.onSubmit();

    expect(projectServiceSpy.addRepository).toHaveBeenCalledOnceWith('proj-1', {
      providerType: 'AzureDevOps',
      repositoryUrl: 'https://github.com/example/repo-1',
      defaultBranch: 'main',
      personalAccessToken: 'token',
      contentKinds: ['Infrastructure'],
    });
  });

  it('uses the stored PAT for edit verification when the PAT input is blank', async () => {
    dialogData = {
      projectId: 'proj-1',
      mode: 'edit',
      existing: createRepositoryResponse('repo-1', ['Infrastructure']),
      lockedKinds: ['Infrastructure'],
    };
    createComponent();
    const api = getComponentTestApi();

    await api.verifyConnection();

    expect(projectServiceSpy.verifyRepositoryConnection).toHaveBeenCalledOnceWith('proj-1', {
      providerType: 'GitHub',
      repositoryUrl: 'https://github.com/example/repo-1',
    }, 'repo-1');
  });

  it('keeps editable content kind checkboxes when the slot is not locked', () => {
    createComponent();

    expect(fixture.nativeElement.querySelector('.locked-kinds-summary')).toBeNull();
    expect(fixture.nativeElement.querySelectorAll('mat-checkbox').length).toBe(2);
  });

  function createComponent(): void {
    fixture = TestBed.createComponent(RepositoryDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }

  function getComponentTestApi(): RepositoryDialogComponentTestApi {
    return component as unknown as RepositoryDialogComponentTestApi;
  }
});

function createRepositoryResponse(
  id: string,
  contentKinds: ProjectRepositoryResponse['contentKinds'],
): ProjectRepositoryResponse {
  return {
    id,
    providerType: 'GitHub',
    repositoryUrl: `https://github.com/example/${id}`,
    owner: 'example',
    repositoryName: id,
    defaultBranch: 'main',
    isConfigured: true,
    contentKinds,
  };
}
