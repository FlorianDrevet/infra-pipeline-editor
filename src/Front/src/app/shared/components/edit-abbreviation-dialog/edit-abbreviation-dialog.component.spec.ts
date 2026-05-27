import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';
import { EditAbbreviationDialogComponent, EditAbbreviationDialogData } from './edit-abbreviation-dialog.component';

describe('EditAbbreviationDialogComponent', () => {
  let fixture: ComponentFixture<EditAbbreviationDialogComponent>;
  let component: EditAbbreviationDialogComponent;

  const mockData: EditAbbreviationDialogData = {
    resourceType: 'Storage Account',
    defaultAbbreviation: 'st',
    currentAbbreviation: 'st',
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditAbbreviationDialogComponent, TranslateModule.forRoot()],
      providers: [
        { provide: MatDialogRef, useValue: jasmine.createSpyObj('MatDialogRef', ['close']) },
        { provide: MAT_DIALOG_DATA, useValue: mockData },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(EditAbbreviationDialogComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
