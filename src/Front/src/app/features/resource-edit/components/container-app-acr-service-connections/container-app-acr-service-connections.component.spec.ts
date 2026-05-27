import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormBuilder } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

import { ResourceEditEnvironmentFormEntry } from '../../helpers/resource-edit-environment-settings.helpers';
import { ContainerAppAcrServiceConnectionsComponent } from './container-app-acr-service-connections.component';

const CONTAINER_REGISTRY_SERVICE_CONNECTION_CONTROL = 'containerRegistryServiceConnection';
const DEVELOPMENT_ENVIRONMENT_NAME = 'Development';
const PRODUCTION_ENVIRONMENT_NAME = 'Production';
const DEVELOPMENT_SERVICE_CONNECTION_NAME = 'acr-dev-docker';
const PRODUCTION_SERVICE_CONNECTION_NAME = 'acr-prod-docker';
const SHARED_SERVICE_CONNECTION_NAME = 'acr-shared-docker';

describe('ContainerAppAcrServiceConnectionsComponent', () => {
  let fixture: ComponentFixture<ContainerAppAcrServiceConnectionsComponent>;
  const fb = new FormBuilder();

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ContainerAppAcrServiceConnectionsComponent, TranslateModule.forRoot()],
    }).compileComponents();
  });

  it('Given identical persisted values When rendering Then shows one field per environment and no shared mode control', () => {
    fixture = createComponent([
      createEnvironmentFormEntry(DEVELOPMENT_ENVIRONMENT_NAME, SHARED_SERVICE_CONNECTION_NAME),
      createEnvironmentFormEntry(PRODUCTION_ENVIRONMENT_NAME, SHARED_SERVICE_CONNECTION_NAME),
    ]);

    expect(queryModeControl()).toBeNull();
    expect(queryTextFields().length).toBe(2);
  });

  it('Given empty persisted values When rendering Then still shows one field per environment', () => {
    const envForms = [
      createEnvironmentFormEntry(DEVELOPMENT_ENVIRONMENT_NAME, null),
      createEnvironmentFormEntry(PRODUCTION_ENVIRONMENT_NAME, null),
    ];

    fixture = createComponent(envForms);

    expect(queryTextFields().length).toBe(2);
  });

  it('Given different persisted values When rendering Then preserves one editable field per environment', () => {
    const envForms = [
      createEnvironmentFormEntry(DEVELOPMENT_ENVIRONMENT_NAME, DEVELOPMENT_SERVICE_CONNECTION_NAME),
      createEnvironmentFormEntry(PRODUCTION_ENVIRONMENT_NAME, PRODUCTION_SERVICE_CONNECTION_NAME),
    ];

    fixture = createComponent(envForms);

    expect(queryTextFields().length).toBe(2);
  });

  it('Given empty persisted values When changing one environment field Then does not copy the value to other environments', () => {
    const envForms = [
      createEnvironmentFormEntry(DEVELOPMENT_ENVIRONMENT_NAME, null),
      createEnvironmentFormEntry(PRODUCTION_ENVIRONMENT_NAME, null),
    ];

    fixture = createComponent(envForms);

    const inputs = queryInputs();
    inputs[0].value = DEVELOPMENT_SERVICE_CONNECTION_NAME;
    inputs[0].dispatchEvent(new Event('input'));
    fixture.detectChanges();

    expect(envForms[0].form.get(CONTAINER_REGISTRY_SERVICE_CONNECTION_CONTROL)?.value).toBe(DEVELOPMENT_SERVICE_CONNECTION_NAME);
    expect(envForms[1].form.get(CONTAINER_REGISTRY_SERVICE_CONNECTION_CONTROL)?.value).toBeNull();
    expect(envForms[0].form.dirty).toBeTrue();
    expect(envForms[1].form.dirty).toBeFalse();
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

  function queryInputs(): NodeListOf<HTMLInputElement> {
    return fixture.nativeElement.querySelectorAll('input');
  }

  function queryModeControl(): Element | null {
    return fixture.nativeElement.querySelector('app-ds-segmented-control');
  }
});