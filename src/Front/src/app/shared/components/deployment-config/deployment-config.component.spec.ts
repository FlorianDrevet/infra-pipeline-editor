import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { MatDialog } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';

import { DeploymentConfigComponent } from './deployment-config.component';

describe('DeploymentConfigComponent', () => {
  let fixture: ComponentFixture<DeploymentConfigComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DeploymentConfigComponent, TranslateModule.forRoot()],
      providers: [
        {
          provide: MatDialog,
          useValue: jasmine.createSpyObj<MatDialog>('MatDialog', ['open']),
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(DeploymentConfigComponent);
    fixture.componentRef.setInput('mode', 'container-only');
    fixture.componentRef.setInput('containerRegistryId', 'acr-1');
    fixture.componentRef.setInput('availableContainerRegistries', [{ id: 'acr-1', name: 'Infra ACR' }]);
    fixture.componentRef.setInput('dockerImageName', 'ifs/frontend');

    await flushComponent();
  });

  it('shows the warning hint only while the docker image is not validated', async () => {
    fixture.componentRef.setInput('dockerImageValidated', false);
    await flushComponent();

    expect(getValidationHint()?.textContent).toContain('RESOURCE_EDIT.DOCKER_IMAGE_VALIDATION.HINT_NOT_VALIDATED');

    fixture.componentRef.setInput('dockerImageValidated', true);
    await flushComponent();

    expect(queryValidationHint()).toBeNull();
    expect(getValidateButton().classList).toContain('docker-image-field__validate-btn--validated');
  });

  it('shows UAI select for ACR pull when managed identity mode with registry selected and state is ok', async () => {
    fixture.componentRef.setInput('showAcrPullIdentitySelector', true);
    fixture.componentRef.setInput('containerRegistryId', 'acr-1');
    fixture.componentRef.setInput('acrAuthMode', 'ManagedIdentity');
    fixture.componentRef.setInput('acrUaiState', 'ok');
    fixture.componentRef.setInput('uaiOptions', [
      { value: 'uai-1', label: 'UAI One' },
      { value: 'uai-2', label: 'UAI Two' },
    ]);
    await flushComponent();

    const selectLabel = fixture.nativeElement.querySelector('.acr-identity-card__section-label') as HTMLElement | null;
    expect(selectLabel).toBeTruthy();
    expect(selectLabel?.textContent).toContain('RESOURCE_EDIT.FIELDS.ACR_IDENTITY_LABEL');
  });

  it('hides UAI select for ACR pull by default', async () => {
    fixture.componentRef.setInput('containerRegistryId', 'acr-1');
    fixture.componentRef.setInput('acrAuthMode', 'ManagedIdentity');
    fixture.componentRef.setInput('acrUaiState', 'ok');
    fixture.componentRef.setInput('uaiOptions', [
      { value: 'uai-1', label: 'UAI One' },
    ]);
    await flushComponent();

    expect(fixture.nativeElement.querySelector('.acr-identity-card')).toBeNull();
  });

  it('emits acrSelectedUaiIdChange when UAI select changes', async () => {
    fixture.componentRef.setInput('showAcrPullIdentitySelector', true);
    fixture.componentRef.setInput('containerRegistryId', 'acr-1');
    fixture.componentRef.setInput('acrAuthMode', 'ManagedIdentity');
    fixture.componentRef.setInput('acrUaiState', 'no-uai');
    fixture.componentRef.setInput('uaiOptions', [
      { value: 'uai-1', label: 'UAI One' },
    ]);
    await flushComponent();

    const emitSpy = spyOn(fixture.componentInstance.acrSelectedUaiIdChange, 'emit');
    const dsSelect = fixture.debugElement.query(By.css('.acr-identity-card app-ds-select'));
    const trigger = dsSelect.nativeElement.querySelector('.ds-select__trigger') as HTMLButtonElement;

    trigger.click();
    await flushComponent();

    const option = document.body.querySelector('.ds-select__option') as HTMLButtonElement | null;
    expect(option).toBeTruthy();
    option?.click();
    await flushComponent();

    expect(emitSpy).toHaveBeenCalledOnceWith('uai-1');
  });

  async function flushComponent(): Promise<void> {
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
  }

  function getValidationHint(): HTMLElement | null {
    return fixture.nativeElement.querySelector('.docker-image-validation-hint') as HTMLElement | null;
  }

  function queryValidationHint(): HTMLElement | null {
    return fixture.nativeElement.querySelector('.docker-image-validation-hint') as HTMLElement | null;
  }

  function getValidateButton(): HTMLButtonElement {
    return fixture.nativeElement.querySelector('.docker-image-field__validate-btn') as HTMLButtonElement;
  }
});