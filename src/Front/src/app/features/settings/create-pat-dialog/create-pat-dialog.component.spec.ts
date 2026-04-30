import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialogRef } from '@angular/material/dialog';
import { By } from '@angular/platform-browser';
import { TranslateModule } from '@ngx-translate/core';

import { DsDatePickerComponent } from '../../../shared/components/ds';
import { PersonalAccessTokenService } from '../../../shared/services/personal-access-token.service';
import { CreatePatDialogComponent } from './create-pat-dialog.component';

describe('CreatePatDialogComponent', () => {
  let fixture: ComponentFixture<CreatePatDialogComponent>;

  beforeEach(async () => {
    jasmine.clock().install();
    jasmine.clock().mockDate(new Date(2026, 4, 2));

    await TestBed.configureTestingModule({
      imports: [CreatePatDialogComponent, TranslateModule.forRoot()],
      providers: [
        {
          provide: MatDialogRef,
          useValue: jasmine.createSpyObj<MatDialogRef<CreatePatDialogComponent>>('MatDialogRef', ['close']),
        },
        {
          provide: PersonalAccessTokenService,
          useValue: jasmine.createSpyObj<PersonalAccessTokenService>('PersonalAccessTokenService', ['create']),
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(CreatePatDialogComponent);
    fixture.detectChanges();
  });

  afterEach(() => {
    jasmine.clock().uninstall();
  });

  it('passes dynamic min and max expiry bounds to the shared date picker', () => {
    const datePicker = fixture.debugElement.query(By.directive(DsDatePickerComponent))
      .componentInstance as DsDatePickerComponent;

    expect(datePicker.min()).toBe('2026-05-02');
    expect(datePicker.max()).toBe('2027-05-02');
  });
});