import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialogRef } from '@angular/material/dialog';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { TranslateModule } from '@ngx-translate/core';
import { AddVariableGroupDialogComponent } from './add-variable-group-dialog.component';

describe('AddVariableGroupDialogComponent', () => {
  let fixture: ComponentFixture<AddVariableGroupDialogComponent>;
  let component: AddVariableGroupDialogComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddVariableGroupDialogComponent, TranslateModule.forRoot()],
      providers: [
        provideNoopAnimations(),
        { provide: MatDialogRef, useValue: jasmine.createSpyObj('MatDialogRef', ['close']) },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AddVariableGroupDialogComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
