import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { TranslateModule } from '@ngx-translate/core';
import { RepositoryDialogComponent, RepositoryDialogData } from './repository-dialog.component';
import { ProjectService } from '../../../../shared/services/project.service';

describe('RepositoryDialogComponent', () => {
  let fixture: ComponentFixture<RepositoryDialogComponent>;
  let component: RepositoryDialogComponent;

  const mockData: RepositoryDialogData = {
    projectId: 'proj-1',
    mode: 'create',
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RepositoryDialogComponent, TranslateModule.forRoot()],
      providers: [
        provideNoopAnimations(),
        { provide: MatDialogRef, useValue: jasmine.createSpyObj('MatDialogRef', ['close']) },
        { provide: MAT_DIALOG_DATA, useValue: mockData },
        { provide: ProjectService, useValue: jasmine.createSpyObj('ProjectService', ['addRepository', 'updateRepository']) },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(RepositoryDialogComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
