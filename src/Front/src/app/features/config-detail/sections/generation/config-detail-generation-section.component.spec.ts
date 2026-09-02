import { ComponentFixture, TestBed } from '@angular/core/testing';
import { WritableSignal } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';

import { ConfigDetailGenerationSectionComponent } from './config-detail-generation-section.component';

interface ConfigDetailGenerationSectionComponentTestApi {
  activeGenerationTabId: WritableSignal<string | null>;
}

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

  it('delegates bootstrap downloads to the parent callback when artifacts are available', () => {
    const onDownloadBootstrapFiles = jasmine.createSpy('onDownloadBootstrapFiles');

    fixture.componentRef.setInput('viewModel', createViewModel({
      showPanel: true,
      bootstrapResult: { fileUris: { 'bootstrap.pipeline.yml': 'https://example.test/bootstrap.pipeline.yml' } },
      onDownloadBootstrapFiles,
    }));
    switchToBootstrapTab();
    fixture.detectChanges();

    getButton('.config-detail-generation__download-bootstrap').click();

    expect(onDownloadBootstrapFiles).toHaveBeenCalledOnceWith();
  });

  it('delegates bootstrap retries to the parent callback when generation fails', () => {
    const onGenerateBootstrap = jasmine.createSpy('onGenerateBootstrap');

    fixture.componentRef.setInput('viewModel', createViewModel({
      showPanel: true,
      bootstrapErrorKey: 'CONFIG_DETAIL.BOOTSTRAP.GENERATE_ERROR',
      onGenerateBootstrap,
    }));
    switchToBootstrapTab();
    fixture.detectChanges();

    getButton('.config-detail-generation__retry-bootstrap').click();

    expect(onGenerateBootstrap).toHaveBeenCalledOnceWith();
  });

  it('delegates the dedicated bootstrap push-to-git button to the parent callback', () => {
    const onPushBootstrapToGit = jasmine.createSpy('onPushBootstrapToGit');

    fixture.componentRef.setInput('viewModel', createViewModel({
      showPanel: true,
      onPushBootstrapToGit,
    }));
    switchToBootstrapTab();
    fixture.detectChanges();

    getButton('.config-detail-generation__push-bootstrap').click();

    expect(onPushBootstrapToGit).toHaveBeenCalledOnceWith();
  });

  // app-ds-button renders its own inner <button>; the class we assign in the
  // template lands on the <app-ds-button> host element, not on that native
  // button, so tests must reach one level deeper to click the real element
  // carrying the (click) listener.
  function getButton(selector: string): HTMLButtonElement {
    return fixture.nativeElement.querySelector(`${selector} button`) as HTMLButtonElement;
  }

  function switchToBootstrapTab(): void {
    const testApi = fixture.componentInstance as unknown as ConfigDetailGenerationSectionComponentTestApi;
    testApi.activeGenerationTabId.set('bootstrap');
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
    bootstrapLoading: false,
    bootstrapDownloading: false,
    bootstrapResult: null,
    bootstrapErrorKey: '',
    configBootstrapNodes: [],
    loadConfigBicepFile: async () => '',
    loadConfigPipelineFile: async () => '',
    loadConfigBootstrapFile: async () => '',
    onClosePanel: () => undefined,
    onTogglePanelCollapsed: () => undefined,
    onDownloadBicepFiles: () => undefined,
    onGenerateBicep: () => undefined,
    onDownloadPipelineFiles: () => undefined,
    onGeneratePipeline: () => undefined,
    onDownloadBootstrapFiles: () => undefined,
    onGenerateBootstrap: () => undefined,
    onPushBootstrapToGit: () => undefined,
    ...overrides,
  };
}