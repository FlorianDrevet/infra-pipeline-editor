import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { TranslateModule } from '@ngx-translate/core';
import { AddConfigDialogComponent, AddConfigDialogData } from './add-config-dialog.component';
import { InfraConfigService } from '../../../shared/services/infra-config.service';

describe('AddConfigDialogComponent', () => {
  let fixture: ComponentFixture<AddConfigDialogComponent>;
  let component: AddConfigDialogComponent;

  const mockData: AddConfigDialogData = { projectId: 'proj-1' };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddConfigDialogComponent, TranslateModule.forRoot()],
      providers: [
        provideNoopAnimations(),
        { provide: MatDialogRef, useValue: jasmine.createSpyObj('MatDialogRef', ['close']) },
        { provide: MAT_DIALOG_DATA, useValue: mockData },
        { provide: InfraConfigService, useValue: jasmine.createSpyObj('InfraConfigService', ['create']) },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AddConfigDialogComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
