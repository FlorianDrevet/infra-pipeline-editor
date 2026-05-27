import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';

import { EnvironmentDefinitionResponse } from '../../../shared/interfaces/infra-config.interface';
import { ProjectDetailEnvironmentsSectionComponent } from './project-detail-environments-section.component';

describe('ProjectDetailEnvironmentsSectionComponent', () => {
  let fixture: ComponentFixture<ProjectDetailEnvironmentsSectionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProjectDetailEnvironmentsSectionComponent, TranslateModule.forRoot()],
    }).compileComponents();

    fixture = TestBed.createComponent(ProjectDetailEnvironmentsSectionComponent);
  });

  it('renders environments as structured sections with grouped facts', () => {
    fixture.componentRef.setInput('canWrite', true);
    fixture.componentRef.setInput('actionId', null);
    fixture.componentRef.setInput('errorKey', '');
    fixture.componentRef.setInput('environments', [createEnvironment()]);
    fixture.detectChanges();

    expect(getText('.environment-panel__title')).toContain('Development');
    expect(fixture.nativeElement.querySelector('.env-card')).toBeNull();
    expect(fixture.nativeElement.querySelectorAll('.environment-group').length).toBe(4);
    expect(getText('.environment-fact__value--mono')).toContain('830d');
  });

  it('renders a denser environment header summary and split tag chips for readability', () => {
    fixture.componentRef.setInput('canWrite', true);
    fixture.componentRef.setInput('actionId', null);
    fixture.componentRef.setInput('errorKey', '');
    fixture.componentRef.setInput('environments', [createEnvironment()]);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelectorAll('.environment-panel__summary-item').length).toBe(2);
    expect(getText('.environment-panel__subtitle-value')).toContain('830d');
    expect(getText('.environment-tag__key')).toContain('environment');
    expect(getText('.environment-tag__value')).toContain('dev');
  });

  it('wraps the environment header content in an inset hero container to keep meta and title away from the panel border', () => {
    fixture.componentRef.setInput('canWrite', true);
    fixture.componentRef.setInput('actionId', null);
    fixture.componentRef.setInput('errorKey', '');
    fixture.componentRef.setInput('environments', [createEnvironment()]);
    fixture.detectChanges();

    const hero = fixture.nativeElement.querySelector('.environment-panel__hero') as HTMLElement | null;

    expect(hero).withContext('the header should render an inner inset surface').not.toBeNull();
    expect(hero?.querySelector('.environment-panel__title')?.textContent).toContain('Development');
  });

  it('emits add, edit, and remove actions', () => {
    const environment = createEnvironment();
    fixture.componentRef.setInput('canWrite', true);
    fixture.componentRef.setInput('actionId', null);
    fixture.componentRef.setInput('errorKey', '');
    fixture.componentRef.setInput('environments', [environment]);

    const addSpy = spyOn(fixture.componentInstance.addEnvironment, 'emit');
    const editSpy = spyOn(fixture.componentInstance.editEnvironment, 'emit');
    const removeSpy = spyOn(fixture.componentInstance.removeEnvironment, 'emit');

    fixture.detectChanges();

    getButton('.environment-toolbar__add-button').click();
    getButton('.environment-panel__action--edit').click();
    getButton('.environment-panel__action--delete').click();

    expect(addSpy).toHaveBeenCalledOnceWith();
    expect(editSpy).toHaveBeenCalledOnceWith(environment);
    expect(removeSpy).toHaveBeenCalledOnceWith(environment);
  });

  it('shows the empty state when no environments exist', () => {
    fixture.componentRef.setInput('canWrite', false);
    fixture.componentRef.setInput('actionId', null);
    fixture.componentRef.setInput('errorKey', '');
    fixture.componentRef.setInput('environments', []);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.empty-state')).not.toBeNull();
  });

  function getButton(selector: string): HTMLButtonElement {
    return fixture.nativeElement.querySelector(selector) as HTMLButtonElement;
  }

  function getText(selector: string): string {
    return (fixture.nativeElement.querySelector(selector) as HTMLElement).textContent ?? '';
  }
});

function createEnvironment(): EnvironmentDefinitionResponse {
  return {
    id: 'env-1',
    name: 'Development',
    shortName: 'dev',
    prefix: 'dev-',
    suffix: '-dev',
    location: 'FranceCentral',
    subscriptionId: '830d0222-c10f-40e7-8eb9-72f37ae7e283',
    order: 1,
    requiresApproval: false,
    azureResourceManagerConnection: null,
    tags: [{ name: 'environment', value: 'dev' }],
  };
}