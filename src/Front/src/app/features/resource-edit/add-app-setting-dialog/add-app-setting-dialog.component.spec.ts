import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';

import { AppSettingService } from '../../../shared/services/app-setting.service';
import { RoleAssignmentService } from '../../../shared/services/role-assignment.service';
import { ProjectService } from '../../../shared/services/project.service';
import { AddAppSettingDialogComponent, AddAppSettingDialogData } from './add-app-setting-dialog.component';

describe('AddAppSettingDialogComponent', () => {
  let fixture: ComponentFixture<AddAppSettingDialogComponent>;
  let component: AddAppSettingDialogComponent;

  const dialogData: AddAppSettingDialogData = {
    resourceId: 'res-1',
    currentResourceName: 'my-app',
    siblingResources: [],
    environments: [{ name: 'dev' }],
    projectId: 'proj-1',
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddAppSettingDialogComponent, TranslateModule.forRoot()],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: dialogData },
        {
          provide: MatDialogRef,
          useValue: jasmine.createSpyObj<MatDialogRef<AddAppSettingDialogComponent>>('MatDialogRef', ['close']),
        },
        {
          provide: AppSettingService,
          useValue: jasmine.createSpyObj<AppSettingService>('AppSettingService', ['add', 'getAvailableOutputs']),
        },
        {
          provide: RoleAssignmentService,
          useValue: jasmine.createSpyObj<RoleAssignmentService>('RoleAssignmentService', ['getByResourceId', 'add']),
        },
        {
          provide: ProjectService,
          useValue: jasmine.createSpyObj<ProjectService>('ProjectService', ['getPipelineVariableGroups']),
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AddAppSettingDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
