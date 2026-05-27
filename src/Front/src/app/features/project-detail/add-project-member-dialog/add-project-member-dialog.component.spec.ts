import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { TranslateModule } from '@ngx-translate/core';
import { AddProjectMemberDialogComponent, AddProjectMemberDialogData } from './add-project-member-dialog.component';
import { DsAutocompleteComponent } from '../../../shared/components/ds';
import { ProjectService } from '../../../shared/services/project.service';

describe('AddProjectMemberDialogComponent', () => {
  let fixture: ComponentFixture<AddProjectMemberDialogComponent>;
  let component: AddProjectMemberDialogComponent;

  const mockData: AddProjectMemberDialogData = {
    projectId: 'proj-1',
    existingUserIds: [],
    availableUsers: [],
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddProjectMemberDialogComponent, TranslateModule.forRoot()],
      providers: [
        provideNoopAnimations(),
        { provide: MatDialogRef, useValue: jasmine.createSpyObj('MatDialogRef', ['close']) },
        { provide: MAT_DIALOG_DATA, useValue: mockData },
        { provide: ProjectService, useValue: jasmine.createSpyObj('ProjectService', ['addMember']) },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AddProjectMemberDialogComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('renders the shared design-system autocomplete for user search', () => {
    fixture.detectChanges();

    const autocomplete = fixture.debugElement.query(By.directive(DsAutocompleteComponent));

    expect(autocomplete).not.toBeNull();
  });
});
