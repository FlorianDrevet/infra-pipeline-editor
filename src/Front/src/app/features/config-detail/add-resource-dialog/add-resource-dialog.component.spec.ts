import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { TranslateModule } from '@ngx-translate/core';
import { AddResourceDialogComponent, AddResourceDialogData } from './add-resource-dialog.component';
import { NameAvailabilityService } from '../../../shared/services/name-availability.service';
import { AddResourceDialogPlanWorkflowService } from './add-resource-dialog-plan-workflow.service';
import { AddResourceDialogResourceSubmitterService } from './add-resource-dialog-resource-submitter.service';

describe('AddResourceDialogComponent', () => {
  let fixture: ComponentFixture<AddResourceDialogComponent>;
  let component: AddResourceDialogComponent;

  const mockData: AddResourceDialogData = {
    resourceGroupId: 'rg-1',
    configId: 'cfg-1',
    projectId: 'proj-1',
    location: 'westeurope',
    environments: [{ name: 'Development' }],
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddResourceDialogComponent, TranslateModule.forRoot()],
      providers: [
        provideNoopAnimations(),
        { provide: MatDialogRef, useValue: jasmine.createSpyObj('MatDialogRef', ['close']) },
        { provide: MAT_DIALOG_DATA, useValue: mockData },
        {
          provide: NameAvailabilityService,
          useValue: jasmine.createSpyObj('NameAvailabilityService', ['checkAvailability']),
        },
        {
          provide: AddResourceDialogPlanWorkflowService,
          useValue: jasmine.createSpyObj('AddResourceDialogPlanWorkflowService', ['selectExistingPlan', 'createNewPlan']),
        },
        {
          provide: AddResourceDialogResourceSubmitterService,
          useValue: jasmine.createSpyObj('AddResourceDialogResourceSubmitterService', ['submit']),
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AddResourceDialogComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
