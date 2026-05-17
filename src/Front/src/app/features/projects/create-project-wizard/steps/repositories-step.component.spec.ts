import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';

import { RepositoriesStepComponent } from './repositories-step.component';
import { EMPTY_DRAFT } from '../create-project-wizard.types';

describe('RepositoriesStepComponent', () => {
  let fixture: ComponentFixture<RepositoriesStepComponent>;
  let component: RepositoriesStepComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RepositoriesStepComponent, TranslateModule.forRoot()],
    }).compileComponents();

    fixture = TestBed.createComponent(RepositoriesStepComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('draft', { ...EMPTY_DRAFT });
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
