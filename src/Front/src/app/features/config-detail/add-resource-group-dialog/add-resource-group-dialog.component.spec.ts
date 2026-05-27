import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { TranslateModule } from '@ngx-translate/core';
import { AddResourceGroupDialogComponent, AddResourceGroupDialogData } from './add-resource-group-dialog.component';
import { ResourceGroupService } from '../../../shared/services/resource-group.service';

describe('AddResourceGroupDialogComponent', () => {
  let fixture: ComponentFixture<AddResourceGroupDialogComponent>;
  let component: AddResourceGroupDialogComponent;

  const mockData: AddResourceGroupDialogData = { infraConfigId: 'cfg-1' };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddResourceGroupDialogComponent, TranslateModule.forRoot()],
      providers: [
        provideNoopAnimations(),
        { provide: MatDialogRef, useValue: jasmine.createSpyObj('MatDialogRef', ['close']) },
        { provide: MAT_DIALOG_DATA, useValue: mockData },
        { provide: ResourceGroupService, useValue: jasmine.createSpyObj('ResourceGroupService', ['create']) },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AddResourceGroupDialogComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
