import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';
import { CascadeDeleteDialogComponent, CascadeDeleteDialogData } from './cascade-delete-dialog.component';

describe('CascadeDeleteDialogComponent', () => {
  let fixture: ComponentFixture<CascadeDeleteDialogComponent>;
  let component: CascadeDeleteDialogComponent;

  const mockData: CascadeDeleteDialogData = {
    titleKey: 'TITLE',
    messageKey: 'MESSAGE',
    dependentsHeaderKey: 'HEADER',
    confirmKey: 'CONFIRM',
    cancelKey: 'CANCEL',
    dependents: [],
    resourceTypeIcons: {},
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CascadeDeleteDialogComponent, TranslateModule.forRoot()],
      providers: [
        { provide: MatDialogRef, useValue: jasmine.createSpyObj('MatDialogRef', ['close']) },
        { provide: MAT_DIALOG_DATA, useValue: mockData },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(CascadeDeleteDialogComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
