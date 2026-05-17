import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';
import { DockerfilePickerComponent } from './dockerfile-picker.component';
import { ProjectService } from '../../services/project.service';

describe('DockerfilePickerComponent', () => {
  let fixture: ComponentFixture<DockerfilePickerComponent>;
  let component: DockerfilePickerComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DockerfilePickerComponent, TranslateModule.forRoot()],
      providers: [
        {
          provide: ProjectService,
          useValue: jasmine.createSpyObj('ProjectService', [
            'listCodeBranches',
            'searchCodeFiles',
          ]),
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
});
