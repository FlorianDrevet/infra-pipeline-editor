import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';

import { ProjectDetailNamingSectionComponent } from './project-detail-naming-section.component';

describe('ProjectDetailNamingSectionComponent', () => {
  let fixture: ComponentFixture<ProjectDetailNamingSectionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProjectDetailNamingSectionComponent, TranslateModule.forRoot()],
    }).compileComponents();

    fixture = TestBed.createComponent(ProjectDetailNamingSectionComponent);
    fixture.componentRef.setInput('project', null);
    fixture.componentRef.setInput('canWrite', false);
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(fixture.componentInstance).toBeTruthy();
  });
});
