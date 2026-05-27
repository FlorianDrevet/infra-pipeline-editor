import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { TranslateModule } from '@ngx-translate/core';
import { InfraConfigRepositoryDialogComponent, InfraConfigRepositoryDialogData } from './infra-config-repository-dialog.component';
import { ProjectService } from '../../../shared/services/project.service';

describe('InfraConfigRepositoryDialogComponent', () => {
  let fixture: ComponentFixture<InfraConfigRepositoryDialogComponent>;
  let component: InfraConfigRepositoryDialogComponent;

  const mockData: InfraConfigRepositoryDialogData = {
    projectId: 'proj-1',
    configId: 'cfg-1',
    mode: 'create',
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InfraConfigRepositoryDialogComponent, TranslateModule.forRoot()],
      providers: [
        provideNoopAnimations(),
        { provide: MatDialogRef, useValue: jasmine.createSpyObj('MatDialogRef', ['close']) },
        { provide: MAT_DIALOG_DATA, useValue: mockData },
        { provide: ProjectService, useValue: jasmine.createSpyObj('ProjectService', ['addConfigRepository', 'updateConfigRepository']) },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(InfraConfigRepositoryDialogComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
