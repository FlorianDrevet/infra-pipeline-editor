import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';

import { RoleAssignmentImpactDialogComponent, RoleAssignmentImpactDialogData } from './role-assignment-impact-dialog.component';

describe('RoleAssignmentImpactDialogComponent', () => {
  let fixture: ComponentFixture<RoleAssignmentImpactDialogComponent>;
  let component: RoleAssignmentImpactDialogComponent;

  const dialogData: RoleAssignmentImpactDialogData = {
    roleName: 'Contributor',
    targetResourceName: 'my-kv',
    impactResult: {
      hasImpact: false,
      impacts: [],
    },
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RoleAssignmentImpactDialogComponent, TranslateModule.forRoot()],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: dialogData },
        {
          provide: MatDialogRef,
          useValue: jasmine.createSpyObj<MatDialogRef<RoleAssignmentImpactDialogComponent>>('MatDialogRef', ['close']),
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(RoleAssignmentImpactDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
