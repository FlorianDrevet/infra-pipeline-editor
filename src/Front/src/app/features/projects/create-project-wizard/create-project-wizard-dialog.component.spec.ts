import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateModule } from '@ngx-translate/core';

import { ProjectService } from '../../../shared/services/project.service';
import { CreateProjectWizardDraftService } from './create-project-wizard-draft.service';
import { CreateProjectWizardDialogComponent } from './create-project-wizard-dialog.component';

describe('CreateProjectWizardDialogComponent', () => {
  let fixture: ComponentFixture<CreateProjectWizardDialogComponent>;
  let component: CreateProjectWizardDialogComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreateProjectWizardDialogComponent, TranslateModule.forRoot()],
      providers: [
        {
          provide: MatDialogRef,
          useValue: jasmine.createSpyObj<MatDialogRef<CreateProjectWizardDialogComponent>>('MatDialogRef', ['close'], {
            disableClose: false,
          }),
        },
        {
          provide: CreateProjectWizardDraftService,
          useValue: jasmine.createSpyObj<CreateProjectWizardDraftService>('CreateProjectWizardDraftService', [
            'load',
            'save',
            'clear',
          ]),
        },
        {
          provide: ProjectService,
          useValue: jasmine.createSpyObj<ProjectService>('ProjectService', ['createProjectWithSetup']),
        },
        {
          provide: MatSnackBar,
          useValue: jasmine.createSpyObj<MatSnackBar>('MatSnackBar', ['open']),
        },
      ],
    }).compileComponents();

    const draftService = TestBed.inject(CreateProjectWizardDraftService) as jasmine.SpyObj<CreateProjectWizardDraftService>;
    draftService.load.and.returnValue(null);

    fixture = TestBed.createComponent(CreateProjectWizardDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
