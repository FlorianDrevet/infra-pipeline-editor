import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';

import { IdentityStepComponent } from './identity-step.component';
import { EMPTY_DRAFT } from '../create-project-wizard.types';

describe('IdentityStepComponent', () => {
  let fixture: ComponentFixture<IdentityStepComponent>;
  let component: IdentityStepComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [IdentityStepComponent, TranslateModule.forRoot()],
    }).compileComponents();

    fixture = TestBed.createComponent(IdentityStepComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('draft', { ...EMPTY_DRAFT });
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
