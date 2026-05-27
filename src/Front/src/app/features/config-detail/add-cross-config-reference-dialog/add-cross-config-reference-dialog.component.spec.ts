import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { TranslateModule } from '@ngx-translate/core';
import { AddCrossConfigReferenceDialogComponent, AddCrossConfigReferenceDialogData } from './add-cross-config-reference-dialog.component';
import { ProjectService } from '../../../shared/services/project.service';

describe('AddCrossConfigReferenceDialogComponent', () => {
  let fixture: ComponentFixture<AddCrossConfigReferenceDialogComponent>;
  let component: AddCrossConfigReferenceDialogComponent;

  const mockData: AddCrossConfigReferenceDialogData = {
    configId: 'cfg-1',
    projectId: 'proj-1',
    existingReferenceResourceIds: [],
  };

  const mockProjectService = jasmine.createSpyObj('ProjectService', ['getProjectResources']);

  beforeEach(async () => {
    mockProjectService.getProjectResources.and.returnValue(Promise.resolve([]));

    await TestBed.configureTestingModule({
      imports: [AddCrossConfigReferenceDialogComponent, TranslateModule.forRoot()],
      providers: [
        provideNoopAnimations(),
        { provide: MatDialogRef, useValue: jasmine.createSpyObj('MatDialogRef', ['close']) },
        { provide: MAT_DIALOG_DATA, useValue: mockData },
        { provide: ProjectService, useValue: mockProjectService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AddCrossConfigReferenceDialogComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
