import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';

import { AppConfigurationKeyService } from '../services/app-configuration-key.service';
import { AppSettingService } from '../../../shared/services/app-setting.service';
import { RoleAssignmentService } from '../../../shared/services/role-assignment.service';
import { ProjectService } from '../../../shared/services/project.service';
import { AddAppConfigKeyDialogComponent, AddAppConfigKeyDialogData } from './add-app-config-key-dialog.component';

describe('AddAppConfigKeyDialogComponent', () => {
  let fixture: ComponentFixture<AddAppConfigKeyDialogComponent>;
  let component: AddAppConfigKeyDialogComponent;

  const dialogData: AddAppConfigKeyDialogData = {
    appConfigurationId: 'cfg-1',
    siblingResources: [],
    environments: [{ name: 'dev' }],
    projectId: 'proj-1',
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddAppConfigKeyDialogComponent, TranslateModule.forRoot()],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: dialogData },
        {
          provide: MatDialogRef,
          useValue: jasmine.createSpyObj<MatDialogRef<AddAppConfigKeyDialogComponent>>('MatDialogRef', ['close']),
        },
        {
          provide: AppConfigurationKeyService,
          useValue: jasmine.createSpyObj<AppConfigurationKeyService>('AppConfigurationKeyService', ['list', 'add', 'remove']),
        },
        {
          provide: AppSettingService,
          useValue: jasmine.createSpyObj<AppSettingService>('AppSettingService', ['getAvailableOutputs']),
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

    fixture = TestBed.createComponent(AddAppConfigKeyDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
