import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';

import { EnvironmentStepComponent } from './environment-step.component';
import { EMPTY_DRAFT } from '../create-project-wizard.types';

const ENVIRONMENT_REQUIRED_TOKENS = [
  'var(--ifs-text-primary)',
  'var(--ifs-text-secondary)',
  'var(--ifs-focus-ring)',
];

const ENVIRONMENT_LEGACY_TOKENS = [
  'var(--text-primary',
  'var(--text-secondary',
  'var(--color-primary',
];

function getCompiledStyles(componentType: unknown): string {
  return (componentType as { ɵcmp: { styles: string[] } }).ɵcmp.styles.join('\n');
}

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

  it('Given_compiledEnvironmentStepStyles_When_checked_Then_designSystemTokensAreUsedForReadableTextAndFocusStates', () => {
    const compiledStyles = getCompiledStyles(EnvironmentStepComponent);

    for (const token of ENVIRONMENT_REQUIRED_TOKENS) {
      expect(compiledStyles).toContain(token);
    }

    for (const token of ENVIRONMENT_LEGACY_TOKENS) {
      expect(compiledStyles).not.toContain(token);
    }
  });
});
