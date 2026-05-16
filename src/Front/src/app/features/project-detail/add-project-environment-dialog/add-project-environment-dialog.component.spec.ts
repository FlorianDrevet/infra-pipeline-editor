import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';

import {
  AddProjectEnvironmentDialogComponent,
  AddProjectEnvironmentDialogData,
} from './add-project-environment-dialog.component';
import { ProjectService } from '../../../shared/services/project.service';

describe('AddProjectEnvironmentDialogComponent', () => {
  let fixture: ComponentFixture<AddProjectEnvironmentDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddProjectEnvironmentDialogComponent, TranslateModule.forRoot()],
      providers: [
        {
          provide: MAT_DIALOG_DATA,
          useValue: {
            projectId: 'project-1',
            allEnvironments: [],
          } satisfies AddProjectEnvironmentDialogData,
        },
        {
          provide: MatDialogRef,
          useValue: jasmine.createSpyObj<MatDialogRef<AddProjectEnvironmentDialogComponent>>('MatDialogRef', ['close']),
        },
        {
          provide: ProjectService,
          useValue: jasmine.createSpyObj<ProjectService>('ProjectService', ['addEnvironment', 'updateEnvironment']),
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AddProjectEnvironmentDialogComponent);
    fixture.detectChanges();
  });

  it('renders a scrollable dialog shell with compact equal-width footer actions', () => {
    const host = fixture.nativeElement as HTMLElement;

    expect(host.querySelector('.dialog-shell')).not.toBeNull();
    expect(host.querySelector('mat-dialog-content.dialog-scroll-content')).not.toBeNull();
    expect(host.querySelector('mat-dialog-actions.dialog-actions.dialog-actions--compact')).not.toBeNull();
    expect(host.querySelector('button.dialog-action.dialog-action--cancel')).not.toBeNull();
    expect(host.querySelector('app-ds-button.dialog-action.dialog-action--submit')).not.toBeNull();
  });

  it('renders the design-system toggle instead of a material slide toggle', () => {
    const host = fixture.nativeElement as HTMLElement;

    expect(host.querySelectorAll('app-ds-toggle').length).toBe(1);
    expect(host.querySelector('mat-slide-toggle')).toBeNull();
  });
});