import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { TranslateModule } from '@ngx-translate/core';

import { RepositoryDialogComponent, RepositoryDialogData } from './repository-dialog.component';
import { ProjectService } from '../../../../shared/services/project.service';
import { ProjectRepositoryResponse } from '../../../../shared/interfaces/project-repository.interface';

describe('RepositoryDialogComponent', () => {
  let fixture: ComponentFixture<RepositoryDialogComponent>;
  let component: RepositoryDialogComponent;
  let dialogData: RepositoryDialogData;

  beforeEach(async () => {
    dialogData = {
      projectId: 'proj-1',
      mode: 'create',
    };

    await TestBed.configureTestingModule({
      imports: [RepositoryDialogComponent, TranslateModule.forRoot()],
      providers: [
        provideNoopAnimations(),
        { provide: MatDialogRef, useValue: jasmine.createSpyObj('MatDialogRef', ['close']) },
        { provide: MAT_DIALOG_DATA, useFactory: () => dialogData },
        { provide: ProjectService, useValue: jasmine.createSpyObj('ProjectService', ['addRepository', 'updateRepository']) },
      ],
    }).compileComponents();
  });

  it('should create', () => {
    createComponent();

    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('renders locked content kinds as tags in the dialog title for slotted layouts', () => {
    dialogData = {
      projectId: 'proj-1',
      mode: 'edit',
      existing: createRepositoryResponse('repo-1', ['Infrastructure', 'ApplicationCode']),
      lockedKinds: ['Infrastructure', 'ApplicationCode'],
    };

    createComponent();

    const dialogText = fixture.nativeElement.textContent ?? '';

    expect(fixture.nativeElement.querySelector('.repo-dialog-title__tag')).not.toBeNull();
    expect(fixture.nativeElement.querySelectorAll('mat-checkbox').length).toBe(0);
    expect(dialogText).toContain('PROJECT_DETAIL.LAYOUT.CONTENT_KIND.Infrastructure');
    expect(dialogText).toContain('PROJECT_DETAIL.LAYOUT.CONTENT_KIND.ApplicationCode');
  });

  it('keeps editable content kind checkboxes when the slot is not locked', () => {
    createComponent();

    expect(fixture.nativeElement.querySelector('.locked-kinds-summary')).toBeNull();
    expect(fixture.nativeElement.querySelectorAll('mat-checkbox').length).toBe(2);
  });

  function createComponent(): void {
    fixture = TestBed.createComponent(RepositoryDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }
});

function createRepositoryResponse(
  id: string,
  contentKinds: ProjectRepositoryResponse['contentKinds'],
): ProjectRepositoryResponse {
  return {
    id,
    alias: `alias-${id}`,
    providerType: 'GitHub',
    repositoryUrl: `https://github.com/example/${id}`,
    owner: 'example',
    repositoryName: id,
    defaultBranch: 'main',
    contentKinds,
  };
}
