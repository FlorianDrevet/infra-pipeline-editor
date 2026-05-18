import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { TranslateModule } from '@ngx-translate/core';
import { PushToGitDialogComponent, PushToGitDialogData } from './push-to-git-dialog.component';
import { DsAutocompleteComponent } from '../../../shared/components/ds';
import { BicepGeneratorService } from '../../../shared/services/bicep-generator.service';
import { PipelineGeneratorService } from '../../../shared/services/pipeline-generator.service';
import { ProjectService } from '../../../shared/services/project.service';

describe('PushToGitDialogComponent', () => {
  let fixture: ComponentFixture<PushToGitDialogComponent>;
  let component: PushToGitDialogComponent;

  const mockData: PushToGitDialogData = {
    configId: 'cfg-1',
    projectId: 'proj-1',
  };

  const mockProjectService = jasmine.createSpyObj('ProjectService', ['listBranches']);

  beforeEach(async () => {
    mockProjectService.listBranches.and.returnValue(Promise.resolve([]));

    await TestBed.configureTestingModule({
      imports: [PushToGitDialogComponent, TranslateModule.forRoot()],
      providers: [
        provideNoopAnimations(),
        { provide: MatDialogRef, useValue: jasmine.createSpyObj('MatDialogRef', ['close']) },
        { provide: MAT_DIALOG_DATA, useValue: mockData },
        { provide: BicepGeneratorService, useValue: jasmine.createSpyObj('BicepGeneratorService', ['pushToGit']) },
        { provide: PipelineGeneratorService, useValue: jasmine.createSpyObj('PipelineGeneratorService', ['pushToGit']) },
        { provide: ProjectService, useValue: mockProjectService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PushToGitDialogComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('renders the shared design-system autocomplete for branch selection', () => {
    fixture.detectChanges();

    const autocomplete = fixture.debugElement.query(By.directive(DsAutocompleteComponent));

    expect(autocomplete).not.toBeNull();
  });
});
