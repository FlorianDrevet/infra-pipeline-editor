import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';

import { AppSettingService } from '../../../shared/services/app-setting.service';
import { ProjectService } from '../../../shared/services/project.service';
import { ImportAppSettingsDialogComponent, ImportAppSettingsDialogData } from './import-app-settings-dialog.component';

describe('ImportAppSettingsDialogComponent', () => {
  let fixture: ComponentFixture<ImportAppSettingsDialogComponent>;
  let component: ImportAppSettingsDialogComponent;

  const dialogData: ImportAppSettingsDialogData = {
    resourceId: 'res-1',
    currentResourceName: 'my-app',
    siblingResources: [],
    environments: [{ name: 'dev' }],
    projectId: 'proj-1',
    existingSettingNames: [],
    resourceType: 'WebApp',
    deploymentMode: null,
    runtimeStack: null,
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ImportAppSettingsDialogComponent, TranslateModule.forRoot()],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: dialogData },
        {
          provide: MatDialogRef,
          useValue: jasmine.createSpyObj<MatDialogRef<ImportAppSettingsDialogComponent>>('MatDialogRef', ['close']),
        },
        {
          provide: AppSettingService,
          useValue: jasmine.createSpyObj<AppSettingService>('AppSettingService', ['add', 'getAvailableOutputs']),
        },
        {
          provide: ProjectService,
          useValue: jasmine.createSpyObj<ProjectService>('ProjectService', ['getPipelineVariableGroups']),
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ImportAppSettingsDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
