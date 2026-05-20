import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormBuilder } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

import { ResourceEditEnvironmentFormEntry } from '../../helpers/resource-edit-environment-settings.helpers';
import { ContainerAppAcrServiceConnectionsComponent } from './container-app-acr-service-connections.component';

const CONTAINER_REGISTRY_SERVICE_CONNECTION_CONTROL = 'containerRegistryServiceConnection';
const DEVELOPMENT_ENVIRONMENT_NAME = 'Development';
const PRODUCTION_ENVIRONMENT_NAME = 'Production';
const SHARED_SERVICE_CONNECTION_NAME = 'acr-shared-docker';
const DEVELOPMENT_SERVICE_CONNECTION_NAME = 'acr-dev-docker';
const PRODUCTION_SERVICE_CONNECTION_NAME = 'acr-prod-docker';
const EMPTY_SERVICE_CONNECTION_NAME = '';
const SHARED_MODE_BUTTON_INDEX = 0;
const PER_ENVIRONMENT_MODE_BUTTON_INDEX = 1;

describe('ContainerAppAcrServiceConnectionsComponent', () => {
  let fixture: ComponentFixture<ContainerAppAcrServiceConnectionsComponent>;
  const fb = new FormBuilder();

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ContainerAppAcrServiceConnectionsComponent, TranslateModule.forRoot()],
    }).compileComponents();
  });

  it('infers shared mode when all non-empty service connection names are identical', () => {
    fixture = createComponent([
      createEnvironmentFormEntry(DEVELOPMENT_ENVIRONMENT_NAME, SHARED_SERVICE_CONNECTION_NAME),
      createEnvironmentFormEntry(PRODUCTION_ENVIRONMENT_NAME, SHARED_SERVICE_CONNECTION_NAME),
    ]);

    expect(queryTextFields().length).toBe(1);
  });

  it('infers per-environment mode when service connection names differ', () => {
    fixture = createComponent([
      createEnvironmentFormEntry(DEVELOPMENT_ENVIRONMENT_NAME, DEVELOPMENT_SERVICE_CONNECTION_NAME),
      createEnvironmentFormEntry(PRODUCTION_ENVIRONMENT_NAME, PRODUCTION_SERVICE_CONNECTION_NAME),
    ]);

    expect(queryTextFields().length).toBe(2);
  });

  it('copies shared service connection changes to every environment form', () => {
    const envForms = [
      createEnvironmentFormEntry(DEVELOPMENT_ENVIRONMENT_NAME, null),
      createEnvironmentFormEntry(PRODUCTION_ENVIRONMENT_NAME, null),
    ];
    fixture = createComponent(envForms);

    const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;
    input.value = SHARED_SERVICE_CONNECTION_NAME;
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    expect(envForms[0].form.get(CONTAINER_REGISTRY_SERVICE_CONNECTION_CONTROL)?.value).toBe(SHARED_SERVICE_CONNECTION_NAME);
    expect(envForms[1].form.get(CONTAINER_REGISTRY_SERVICE_CONNECTION_CONTROL)?.value).toBe(SHARED_SERVICE_CONNECTION_NAME);
    expect(envForms[0].form.dirty).toBeTrue();
    expect(envForms[1].form.dirty).toBeTrue();
  });

  it('Given per-environment values differ When switching to shared mode Then copies the first non-empty value to every environment form', () => {
    const envForms = [
      createEnvironmentFormEntry(DEVELOPMENT_ENVIRONMENT_NAME, DEVELOPMENT_SERVICE_CONNECTION_NAME),
      createEnvironmentFormEntry(PRODUCTION_ENVIRONMENT_NAME, PRODUCTION_SERVICE_CONNECTION_NAME),
    ];
    fixture = createComponent(envForms);

    queryModeButtons()[SHARED_MODE_BUTTON_INDEX].click();
    fixture.detectChanges();

    expect(envForms[0].form.get(CONTAINER_REGISTRY_SERVICE_CONNECTION_CONTROL)?.value).toBe(DEVELOPMENT_SERVICE_CONNECTION_NAME);
    expect(envForms[1].form.get(CONTAINER_REGISTRY_SERVICE_CONNECTION_CONTROL)?.value).toBe(DEVELOPMENT_SERVICE_CONNECTION_NAME);
    expect(envForms[0].form.dirty).toBeTrue();
    expect(envForms[1].form.dirty).toBeTrue();
  });

  it('Given all values are empty When switching from per-environment mode to shared mode Then keeps the shared value empty', () => {
    const envForms = [
      createEnvironmentFormEntry(DEVELOPMENT_ENVIRONMENT_NAME, null),
      createEnvironmentFormEntry(PRODUCTION_ENVIRONMENT_NAME, null),
    ];
    fixture = createComponent(envForms);

    queryModeButtons()[PER_ENVIRONMENT_MODE_BUTTON_INDEX].click();
    fixture.detectChanges();
    queryModeButtons()[SHARED_MODE_BUTTON_INDEX].click();
    fixture.detectChanges();

    const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;
    expect(input.value).toBe(EMPTY_SERVICE_CONNECTION_NAME);
  });

  function createComponent(envForms: ReadonlyArray<ResourceEditEnvironmentFormEntry>): ComponentFixture<ContainerAppAcrServiceConnectionsComponent> {
    const componentFixture = TestBed.createComponent(ContainerAppAcrServiceConnectionsComponent);
    componentFixture.componentRef.setInput('envForms', envForms);
    componentFixture.detectChanges();
    return componentFixture;
  }

  function createEnvironmentFormEntry(envName: string, serviceConnection: string | null): ResourceEditEnvironmentFormEntry {
    return {
      envName,
      form: fb.group({
        [CONTAINER_REGISTRY_SERVICE_CONNECTION_CONTROL]: [serviceConnection],
      }),
    };
  }

  function queryTextFields(): NodeListOf<Element> {
    return fixture.nativeElement.querySelectorAll('app-ds-text-field');
  }

  function queryModeButtons(): NodeListOf<HTMLButtonElement> {
    return fixture.nativeElement.querySelectorAll('app-ds-segmented-control button');
  }
});