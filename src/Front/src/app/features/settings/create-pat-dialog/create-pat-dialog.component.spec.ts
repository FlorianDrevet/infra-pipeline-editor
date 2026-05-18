import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialogRef } from '@angular/material/dialog';
import { By } from '@angular/platform-browser';
import { TranslateModule } from '@ngx-translate/core';

import { DsButtonComponent, DsDatePickerComponent } from '../../../shared/components/ds';
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

  it('uses a dedicated token toolbar and a filled copy action in the success state', () => {
    const component = fixture.componentInstance as CreatePatDialogComponent & {
      createdToken: { set(value: string): void };
      tokenCreated: { set(value: boolean): void };
    };

    component.createdToken.set('ifs_pat_live_123456789');
    component.tokenCreated.set(true);
    fixture.detectChanges();

    const toolbar = fixture.nativeElement.querySelector('.create-pat-dialog__token-toolbar');
    const copyButton = fixture.debugElement
      .queryAll(By.directive(DsButtonComponent))
      .map(debugElement => debugElement.componentInstance as DsButtonComponent)
      .find(button => button.icon() === 'content_copy');

    expect(toolbar).not.toBeNull();
    expect(copyButton).toBeDefined();
    expect(copyButton?.variant()).toBe('subtle');
  });
});