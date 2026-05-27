import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';

import { ProjectService } from '../../../../shared/services/project.service';
import {
  ProjectGitPatDialogComponent,
  ProjectGitPatDialogData,
} from './project-git-pat-dialog.component';

describe('ProjectGitPatDialogComponent', () => {
  let fixture: ComponentFixture<ProjectGitPatDialogComponent>;
  let dialogRefSpy: jasmine.SpyObj<MatDialogRef<ProjectGitPatDialogComponent>>;
  let projectServiceSpy: jasmine.SpyObj<ProjectService>;
  let dialogData: ProjectGitPatDialogData;

  beforeEach(async () => {
    dialogRefSpy = jasmine.createSpyObj<MatDialogRef<ProjectGitPatDialogComponent>>('MatDialogRef', ['close']);
    projectServiceSpy = jasmine.createSpyObj<ProjectService>('ProjectService', ['setGitPat']);
    projectServiceSpy.setGitPat.and.resolveTo();
    dialogData = {
      projectId: 'project-1',
      repositoryId: 'repo-1',
      providerTypes: ['GitHub', 'AzureDevOps'],
    };

    await TestBed.configureTestingModule({
      imports: [ProjectGitPatDialogComponent, TranslateModule.forRoot()],
      providers: [
        { provide: MatDialogRef, useValue: dialogRefSpy },
        { provide: MAT_DIALOG_DATA, useFactory: () => dialogData },
        { provide: ProjectService, useValue: projectServiceSpy },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ProjectGitPatDialogComponent);
    fixture.detectChanges();
  });

  it('submits the shared PAT through the project-level endpoint', async () => {
    const component = fixture.componentInstance as ProjectGitPatDialogComponent & {
      form: {
        controls: {
          personalAccessToken: { setValue(value: string): void };
        };
      };
      onSubmit(): Promise<void>;
    };

    component.form.controls.personalAccessToken.setValue('ghp_shared_project_token');

    await component.onSubmit();

    expect(projectServiceSpy.setGitPat).toHaveBeenCalledOnceWith('project-1', 'repo-1', {
      personalAccessToken: 'ghp_shared_project_token',
    });
    expect(dialogRefSpy.close).toHaveBeenCalledOnceWith(true);
  });

  it('shows the generic save error when the backend returns a technical Key Vault storage failure', async () => {
    const component = fixture.componentInstance as ProjectGitPatDialogComponent & {
      form: {
        controls: {
          personalAccessToken: { setValue(value: string): void };
        };
      };
      onSubmit(): Promise<void>;
    };

    const apiError = Object.assign(new Error('Request failed with status code 400'), {
      response: {
        data: {
          errors: [
            {
              code: 'GitRepository.SecretStorageFailed',
              description: 'The application is not allowed to write secrets to the configured Key Vault.',
            },
          ],
        },
      },
    });
    projectServiceSpy.setGitPat.and.returnValue(Promise.reject(apiError));

    component.form.controls.personalAccessToken.setValue('ghp_repo_token');

    await component.onSubmit();
    fixture.detectChanges();

    expect(dialogRefSpy.close).not.toHaveBeenCalledWith(true);
    expect(fixture.nativeElement.textContent).toContain('PROJECT_DETAIL.LAYOUT.AUTH.SAVE_ERROR');
    expect(fixture.nativeElement.textContent).not.toContain(
      'The application is not allowed to write secrets to the configured Key Vault.',
    );
  });

  it('reuses the existing provider help keys for supported providers', () => {
    const dialogText = fixture.nativeElement.textContent ?? '';

    expect(dialogText).toContain('PROJECT_DETAIL.GIT_CONFIG.FORM.PAT_HELP_GITHUB');
    expect(dialogText).toContain('PROJECT_DETAIL.GIT_CONFIG.FORM.PAT_HELP_AZUREDEVOPS');
  });
});