import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { TranslateModule } from '@ngx-translate/core';
import { DockerfilePickerComponent } from './dockerfile-picker.component';
import { ProjectService } from '../../services/project.service';
import { GitBranchResponse } from '../../interfaces/project.interface';

describe('DockerfilePickerComponent', () => {
  let fixture: ComponentFixture<DockerfilePickerComponent>;
  let component: DockerfilePickerComponent;
  let projectService: jasmine.SpyObj<ProjectService>;

  const branches: GitBranchResponse[] = [
    { name: 'main', isProtected: true },
    { name: 'develop', isProtected: false },
  ];

  beforeEach(async () => {
    projectService = jasmine.createSpyObj<ProjectService>('ProjectService', [
      'listCodeBranches',
      'searchCodeFiles',
    ]);
    projectService.listCodeBranches.and.resolveTo(branches);
    projectService.searchCodeFiles.and.resolveTo([]);

    await TestBed.configureTestingModule({
      imports: [DockerfilePickerComponent, NoopAnimationsModule, TranslateModule.forRoot()],
      providers: [
        {
          provide: ProjectService,
          useValue: projectService,
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(DockerfilePickerComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should use the design-system autocomplete for branch selection', async () => {
    fixture.componentRef.setInput('projectId', 'project-id');
    fixture.detectChanges();

    const trigger = fixture.nativeElement.querySelector('.picker-trigger') as HTMLButtonElement;
    trigger.click();
    await fixture.whenStable();
    fixture.detectChanges();

    const overlayContainer = document.querySelector('.cdk-overlay-container');
    expect(overlayContainer?.querySelector('app-ds-autocomplete')).not.toBeNull();
    expect(overlayContainer?.querySelector('app-ds-select')).toBeNull();
    expect(overlayContainer?.querySelector('select')).toBeNull();
  });
});
