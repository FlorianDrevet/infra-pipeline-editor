import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';

import { AppSettingService } from '../../../shared/services/app-setting.service';
import { EditStaticAppSettingDialogComponent, EditStaticAppSettingDialogData } from './edit-static-app-setting-dialog.component';

describe('EditStaticAppSettingDialogComponent', () => {
  let fixture: ComponentFixture<EditStaticAppSettingDialogComponent>;
  let component: EditStaticAppSettingDialogComponent;

  const dialogData: EditStaticAppSettingDialogData = {
    resourceId: 'res-1',
    appSettingId: 'setting-1',
    currentName: 'MY_SETTING',
    currentEnvironmentValues: { dev: 'val1' },
    environments: [{ name: 'dev' }],
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditStaticAppSettingDialogComponent, TranslateModule.forRoot()],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: dialogData },
        {
          provide: MatDialogRef,
          useValue: jasmine.createSpyObj<MatDialogRef<EditStaticAppSettingDialogComponent>>('MatDialogRef', ['close']),
        },
        {
          provide: AppSettingService,
          useValue: jasmine.createSpyObj<AppSettingService>('AppSettingService', ['update']),
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(EditStaticAppSettingDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
