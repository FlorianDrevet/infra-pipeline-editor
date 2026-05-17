import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';

import { ProjectDetailTagsSectionComponent } from './project-detail-tags-section.component';

describe('ProjectDetailTagsSectionComponent', () => {
  let fixture: ComponentFixture<ProjectDetailTagsSectionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProjectDetailTagsSectionComponent, TranslateModule.forRoot()],
    }).compileComponents();

    fixture = TestBed.createComponent(ProjectDetailTagsSectionComponent);
    fixture.componentRef.setInput('project', null);
    fixture.componentRef.setInput('canWrite', false);
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(fixture.componentInstance).toBeTruthy();
  });
});
