import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MatDialog } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';

import { DeploymentConfigComponent } from './deployment-config.component';

describe('DeploymentConfigComponent', () => {
  let fixture: ComponentFixture<DeploymentConfigComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DeploymentConfigComponent, TranslateModule.forRoot(), NoopAnimationsModule],
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