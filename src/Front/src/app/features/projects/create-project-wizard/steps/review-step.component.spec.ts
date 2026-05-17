import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';

import { ReviewStepComponent } from './review-step.component';
import { EMPTY_DRAFT } from '../create-project-wizard.types';

describe('ReviewStepComponent', () => {
  let fixture: ComponentFixture<ReviewStepComponent>;
  let component: ReviewStepComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReviewStepComponent, TranslateModule.forRoot()],
    }).compileComponents();

    fixture = TestBed.createComponent(ReviewStepComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('draft', { ...EMPTY_DRAFT });
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
