import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';

import { EnvironmentStepComponent } from './environment-step.component';
import { EMPTY_DRAFT } from '../create-project-wizard.types';

describe('EnvironmentStepComponent', () => {
  let fixture: ComponentFixture<EnvironmentStepComponent>;
  let component: EnvironmentStepComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EnvironmentStepComponent, TranslateModule.forRoot()],
    }).compileComponents();

    fixture = TestBed.createComponent(EnvironmentStepComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('draft', { ...EMPTY_DRAFT });
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
