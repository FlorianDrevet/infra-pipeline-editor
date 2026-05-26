import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateModule } from '@ngx-translate/core';

import { ProjectService } from '../../../shared/services/project.service';
import { CreateProjectWizardDraftService } from './create-project-wizard-draft.service';
import { CreateProjectWizardDialogComponent } from './create-project-wizard-dialog.component';
import { IdentityStepComponent } from './steps/identity-step.component';
import { RepositoriesStepComponent } from './steps/repositories-step.component';

const DIALOG_REQUIRED_TOKENS = [
  'var(--ifs-border-subtle)',
  'var(--ifs-text-primary)',
  'var(--ifs-focus-ring)',
];

const DIALOG_LEGACY_TOKENS = [
  'var(--border-color',
  'var(--text-primary',
];

const STEP_REQUIRED_TOKENS = [
  'var(--ifs-text-primary)',
  'var(--ifs-text-secondary)',
];

const STEP_LEGACY_TOKENS = [
  'var(--text-primary',
  'var(--text-secondary',
];

function getCompiledStyles(componentType: unknown): string {
  return (componentType as { ɵcmp: { styles: string[] } }).ɵcmp.styles.join('\n');
}

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

  it('Given_compiledDialogStyles_When_checked_Then_designSystemTokensAreUsedForHeaderAndStepperStates', () => {
    const compiledStyles = getCompiledStyles(CreateProjectWizardDialogComponent);

    for (const token of DIALOG_REQUIRED_TOKENS) {
      expect(compiledStyles).toContain(token);
    }

    for (const token of DIALOG_LEGACY_TOKENS) {
      expect(compiledStyles).not.toContain(token);
    }
  });

  it('Given_compiledIdentityAndRepositoryStepStyles_When_checked_Then_designSystemTokensAreUsedForReadableWizardText', () => {
    const stepStyles = [
      getCompiledStyles(IdentityStepComponent),
      getCompiledStyles(RepositoriesStepComponent),
    ];

    for (const compiledStyles of stepStyles) {
      for (const token of STEP_REQUIRED_TOKENS) {
        expect(compiledStyles).toContain(token);
      }

      for (const token of STEP_LEGACY_TOKENS) {
        expect(compiledStyles).not.toContain(token);
      }
    }
  });
});
