import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { TranslateModule } from '@ngx-translate/core';
import { AddStorageServiceDialogComponent, AddStorageServiceDialogData } from './add-storage-service-dialog.component';
import { StorageAccountService } from '../../../shared/services/storage-account.service';

describe('AddStorageServiceDialogComponent', () => {
  let fixture: ComponentFixture<AddStorageServiceDialogComponent>;
  let component: AddStorageServiceDialogComponent;

  const mockData: AddStorageServiceDialogData = {
    storageAccountId: 'sa-1',
    existingBlobNames: [],
    existingQueueNames: [],
    existingTableNames: [],
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddStorageServiceDialogComponent, TranslateModule.forRoot()],
      providers: [
        provideNoopAnimations(),
        { provide: MatDialogRef, useValue: jasmine.createSpyObj('MatDialogRef', ['close']) },
        { provide: MAT_DIALOG_DATA, useValue: mockData },
        {
          provide: StorageAccountService,
          useValue: jasmine.createSpyObj('StorageAccountService', ['addBlobContainer', 'addQueue', 'addTable']),
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AddStorageServiceDialogComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
