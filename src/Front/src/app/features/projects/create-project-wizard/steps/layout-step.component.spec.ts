import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';

import { LayoutStepComponent } from './layout-step.component';
import { EMPTY_DRAFT } from '../create-project-wizard.types';

describe('LayoutStepComponent', () => {
  let fixture: ComponentFixture<LayoutStepComponent>;
  let component: LayoutStepComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LayoutStepComponent, TranslateModule.forRoot()],
    }).compileComponents();

    fixture = TestBed.createComponent(LayoutStepComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('draft', { ...EMPTY_DRAFT });
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
