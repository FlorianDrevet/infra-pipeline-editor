import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import {
  GenerationDiagnosticsDialogComponent,
  GenerationDiagnosticsDialogData,
} from './generation-diagnostics-dialog.component';

describe('GenerationDiagnosticsDialogComponent', () => {
  let fixture: ComponentFixture<GenerationDiagnosticsDialogComponent>;

  const dialogData: GenerationDiagnosticsDialogData = {
    configDiagnostics: [
      {
        configId: 'config-1',
        configName: 'Config Demo',
        diagnostics: [
          {
            resourceId: 'resource-1',
            resourceName: 'ifs-frontend',
            resourceType: 'Microsoft.App/containerApps',
            severity: 'Warning',
            ruleCode: 'DOCKER_IMAGE_NOT_VALIDATED',
            targetResourceName: 'ifs-frontend',
          },
        ],
      },
    ],
    missingEnvConfigs: [
      {
        configId: 'config-1',
        configName: 'Config Demo',
        resources: [
          {
            resourceId: 'resource-2',
            resourceName: 'ifs-api',
            resourceType: 'Microsoft.App/containerApps',
            missingEnvironments: ['Development'],
          },
        ],
      },
    ],
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GenerationDiagnosticsDialogComponent, TranslateModule.forRoot()],
      providers: [
        provideNoopAnimations(),
        provideRouter([]),
        {
          provide: MAT_DIALOG_DATA,
          useValue: dialogData,
        },
        {
          provide: MatDialogRef,
          useValue: jasmine.createSpyObj<MatDialogRef<GenerationDiagnosticsDialogComponent>>('MatDialogRef', ['close']),
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(GenerationDiagnosticsDialogComponent);
    fixture.detectChanges();
  });

  it('moves Docker image validation warnings out of the RBAC section and into the Docker image section', () => {
    const contentText = fixture.nativeElement.textContent as string;

    expect(contentText).not.toContain('GENERATION_DIAGNOSTICS.RBAC_SECTION_TITLE');
    expect(contentText).toContain('GENERATION_DIAGNOSTICS.PENDING_DOCKER_IMAGES_TITLE');

    expect(getSectionIcons().map((icon) => icon.textContent?.trim())).toContain('inventory_2');
  });

  it('uses warning iconography for a dialog that only contains warnings', () => {
    const titleIcon = fixture.nativeElement.querySelector('.diagnostics-dialog__title-icon') as HTMLElement | null;
    const count = fixture.nativeElement.querySelector('.diagnostics-dialog__count') as HTMLElement | null;

    expect(titleIcon?.textContent?.trim()).toBe('warning_amber');
    expect(count?.classList.contains('diagnostics-dialog__count--warning')).toBeTrue();
  });

  it('renders each visible warning category inside its own section card', () => {
    const sectionCards = Array.from(
      fixture.nativeElement.querySelectorAll('.diagnostics-dialog__section-card'),
    ) as HTMLElement[];

    expect(sectionCards.length).toBe(2);
    expect(sectionCards.every((sectionCard) => sectionCard.classList.contains('diagnostics-dialog__section-card--warning'))).toBeTrue();
  });

  function getSectionIcons(): HTMLElement[] {
    return Array.from(fixture.nativeElement.querySelectorAll('.diagnostics-dialog__section-header-icon')) as HTMLElement[];
  }
});