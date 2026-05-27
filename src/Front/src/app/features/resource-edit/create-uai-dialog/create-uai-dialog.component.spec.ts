import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';

import { UserAssignedIdentityService } from '../../../shared/services/user-assigned-identity.service';
import { CreateUaiDialogComponent, CreateUaiDialogData } from './create-uai-dialog.component';

describe('CreateUaiDialogComponent', () => {
  let fixture: ComponentFixture<CreateUaiDialogComponent>;
  let component: CreateUaiDialogComponent;

  const dialogData: CreateUaiDialogData = {
    resourceGroupId: 'rg-1',
    location: 'WestEurope',
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreateUaiDialogComponent, TranslateModule.forRoot()],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: dialogData },
        {
          provide: MatDialogRef,
          useValue: jasmine.createSpyObj<MatDialogRef<CreateUaiDialogComponent>>('MatDialogRef', ['close']),
        },
        {
          provide: UserAssignedIdentityService,
          useValue: jasmine.createSpyObj<UserAssignedIdentityService>('UserAssignedIdentityService', ['create']),
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(CreateUaiDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
