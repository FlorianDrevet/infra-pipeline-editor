import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';

import { RepositoriesStepComponent } from './repositories-step.component';
import { EMPTY_DRAFT } from '../create-project-wizard.types';

describe('RepositoriesStepComponent', () => {
  let fixture: ComponentFixture<RepositoriesStepComponent>;
  let component: RepositoriesStepComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RepositoriesStepComponent, TranslateModule.forRoot()],
    }).compileComponents();

    fixture = TestBed.createComponent(RepositoriesStepComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('draft', { ...EMPTY_DRAFT });
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('does not render the removed repository label field', () => {
    fixture.componentRef.setInput('draft', {
      ...EMPTY_DRAFT,
      layoutPreset: 'AllInOne',
      repositories: [
        {
          contentKinds: ['Infrastructure', 'ApplicationCode'],
          providerType: '',
          repositoryUrl: '',
          defaultBranch: '',
        },
      ],
    });
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent ?? '';

    expect(text).not.toContain('PROJECT_CREATE.STEP.REPOSITORIES.REMOVED_REPOSITORY_LABEL');
  });
});
