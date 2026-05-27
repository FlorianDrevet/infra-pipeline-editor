import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';

import { ProjectDetailVariableGroupsSectionComponent } from './project-detail-variable-groups-section.component';

describe('ProjectDetailVariableGroupsSectionComponent', () => {
  let fixture: ComponentFixture<ProjectDetailVariableGroupsSectionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProjectDetailVariableGroupsSectionComponent, TranslateModule.forRoot()],
    }).compileComponents();

    fixture = TestBed.createComponent(ProjectDetailVariableGroupsSectionComponent);
    fixture.componentRef.setInput('project', null);
    fixture.componentRef.setInput('canWrite', false);
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(fixture.componentInstance).toBeTruthy();
  });
});
