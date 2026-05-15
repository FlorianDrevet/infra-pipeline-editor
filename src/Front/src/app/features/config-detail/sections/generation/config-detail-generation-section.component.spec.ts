import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';

import { ConfigDetailGenerationSectionComponent } from './config-detail-generation-section.component';

describe('ConfigDetailGenerationSectionComponent', () => {
  let fixture: ComponentFixture<ConfigDetailGenerationSectionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConfigDetailGenerationSectionComponent, TranslateModule.forRoot()],
    }).compileComponents();

    fixture = TestBed.createComponent(ConfigDetailGenerationSectionComponent);
  });

  it('delegates bicep downloads to the parent callback when artifacts are available', () => {
    const onDownloadBicepFiles = jasmine.createSpy('onDownloadBicepFiles');

    fixture.componentRef.setInput('viewModel', createViewModel({
      showPanel: true,
      bicepResult: { warnings: [] },
      onDownloadBicepFiles,
    }));
    fixture.detectChanges();

    getButton('.config-detail-generation__download-bicep').click();

    expect(onDownloadBicepFiles).toHaveBeenCalledOnceWith();
  });

  it('delegates bicep retries to the parent callback when generation fails', () => {
    const onGenerateBicep = jasmine.createSpy('onGenerateBicep');

    fixture.componentRef.setInput('viewModel', createViewModel({
      showPanel: true,
      bicepErrorKey: 'CONFIG_DETAIL.BICEP.GENERATE_ERROR',
      onGenerateBicep,
    }));
    fixture.detectChanges();

    getButton('.config-detail-generation__retry-bicep').click();

    expect(onGenerateBicep).toHaveBeenCalledOnceWith();
  });

  function getButton(selector: string): HTMLButtonElement {
    return fixture.nativeElement.querySelector(selector) as HTMLButtonElement;
  }
});

function createViewModel(overrides: Partial<Record<string, unknown>> = {}): Record<string, unknown> {
  return {
    showPanel: false,
    isCollapsed: false,
    bicepLoading: false,
    bicepDownloading: false,
    bicepResult: null,
    bicepErrorKey: '',
    configBicepNodes: [],
    pipelineLoading: false,
    pipelineDownloading: false,
    pipelineResult: null,
    pipelineErrorKey: '',
    configPipelineNodes: [],
    loadConfigBicepFile: async () => '',
    loadConfigPipelineFile: async () => '',
    onClosePanel: () => undefined,
    onTogglePanelCollapsed: () => undefined,
    onDownloadBicepFiles: () => undefined,
    onGenerateBicep: () => undefined,
    onDownloadPipelineFiles: () => undefined,
    onGeneratePipeline: () => undefined,
    ...overrides,
  };
}