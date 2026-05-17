import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';

import { AddCustomDomainDialogComponent, AddCustomDomainDialogData } from './add-custom-domain-dialog.component';

describe('AddCustomDomainDialogComponent', () => {
  let fixture: ComponentFixture<AddCustomDomainDialogComponent>;
  let component: AddCustomDomainDialogComponent;

  const dialogData: AddCustomDomainDialogData = {
    environments: [{ name: 'dev', shortName: 'dev', order: 0 } as any],
    existingDomains: [],
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddCustomDomainDialogComponent, TranslateModule.forRoot()],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: dialogData },
        {
          provide: MatDialogRef,
          useValue: jasmine.createSpyObj<MatDialogRef<AddCustomDomainDialogComponent>>('MatDialogRef', ['close']),
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AddCustomDomainDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
