import { computed, inject, signal, Signal } from '@angular/core';
import { saveAs } from 'file-saver';

import { BicepTreeNode } from '../../../../shared/components/bicep-file-panel/bicep-file-panel.component';
import { InfrastructureConfigResponse } from '../../../../shared/interfaces/infra-config.interface';
import { BicepGeneratorService } from '../../../../shared/services/bicep-generator.service';
import { PipelineGeneratorService } from '../../../../shared/services/pipeline-generator.service';
import { buildConfigBicepNodes, buildConfigPipelineNodes } from '../../helpers/config-detail-tree.helpers';
import { ConfigDetailGenerationSectionViewModel } from './config-detail-generation-section.view-model';

export interface ConfigDetailGenerationSectionController {
  readonly generateAllLoading: Signal<boolean>;
  readonly validatingDiagnostics: Signal<boolean>;
  readonly viewModel: Signal<ConfigDetailGenerationSectionViewModel | null>;

  reset(): void;
  generateAll(): Promise<void>;
}

interface ConfigDetailGenerationSectionControllerDependencies {
  getConfig(): InfrastructureConfigResponse | null;
  isProjectMultiRepo(): boolean;
  showDiagnosticsDialog(): Promise<boolean>;
}

export function createConfigDetailGenerationSectionController(
  dependencies: ConfigDetailGenerationSectionControllerDependencies,
): ConfigDetailGenerationSectionController {
  const bicepService = inject(BicepGeneratorService);
  const pipelineService = inject(PipelineGeneratorService);

  const validatingDiagnostics = signal(false);
  const bicepLoading = signal(false);
  const bicepResult = signal<Awaited<ReturnType<BicepGeneratorService['generate']>> | null>(null);
  const bicepErrorKey = signal('');
  const bicepPanelOpen = signal(false);
  const bicepDownloading = signal(false);

  const pipelineLoading = signal(false);
  const pipelineResult = signal<Awaited<ReturnType<PipelineGeneratorService['generate']>> | null>(null);
  const pipelineErrorKey = signal('');
  const pipelinePanelOpen = signal(false);
  const pipelineDownloading = signal(false);

  const generationPanelCollapsed = signal(false);

  const configBicepNodes = computed<BicepTreeNode[]>(() => {
    const result = bicepResult();
    return result ? buildConfigBicepNodes(result) : [];
  });

  const loadConfigBicepFile = (filePath: string): Promise<string> => {
    const configId = dependencies.getConfig()?.id ?? '';
    return bicepService.getFileContent(configId, filePath);
  };

  const configPipelineNodes = computed<BicepTreeNode[]>(() => {
    const result = pipelineResult();
    return result ? buildConfigPipelineNodes(result) : [];
  });

  const loadConfigPipelineFile = (filePath: string): Promise<string> => {
    const configId = dependencies.getConfig()?.id ?? '';
    return pipelineService.getFileContent(configId, filePath);
  };

  const generateAllLoading = computed(
    () => validatingDiagnostics() || bicepLoading() || pipelineLoading(),
  );

  const generationPanelOpen = computed(
    () => bicepPanelOpen() || pipelinePanelOpen() || bicepLoading() || pipelineLoading(),
  );

  const closeBicepPanel = (): void => {
    bicepPanelOpen.set(false);
    bicepResult.set(null);
    bicepErrorKey.set('');
  };

  const closePipelinePanel = (): void => {
    pipelinePanelOpen.set(false);
    pipelineResult.set(null);
    pipelineErrorKey.set('');
  };

  const closeGenerationPanel = (): void => {
    closeBicepPanel();
    closePipelinePanel();
    generationPanelCollapsed.set(false);
  };

  const toggleGenerationPanelCollapsed = (): void => {
    generationPanelCollapsed.update((collapsed) => !collapsed);
  };

  const doGenerateBicep = async (): Promise<void> => {
    const configId = dependencies.getConfig()?.id;
    if (!configId || bicepLoading()) {
      return;
    }

    bicepLoading.set(true);
    bicepErrorKey.set('');
    bicepResult.set(null);
    generationPanelCollapsed.set(false);
    bicepPanelOpen.set(true);

    try {
      bicepResult.set(await bicepService.generate({ infrastructureConfigId: configId }));
    } catch (error: unknown) {
      const axios = await import('axios');
      if (axios.isAxiosError(error)) {
        const status = error.response?.status;
        if (status === 401 || status === 403) {
          bicepErrorKey.set('CONFIG_DETAIL.BICEP.GENERATE_AUTH_ERROR');
        } else {
          bicepErrorKey.set('CONFIG_DETAIL.BICEP.GENERATE_ERROR');
        }
      } else if (error instanceof Error && error.message.includes('access token')) {
        bicepErrorKey.set('CONFIG_DETAIL.BICEP.GENERATE_AUTH_ERROR');
      } else {
        bicepErrorKey.set('CONFIG_DETAIL.BICEP.GENERATE_ERROR');
      }
    } finally {
      bicepLoading.set(false);
    }
  };

  const generateBicep = async (): Promise<void> => {
    const configId = dependencies.getConfig()?.id;
    if (!configId || bicepLoading()) {
      return;
    }

    validatingDiagnostics.set(true);
    try {
      const shouldContinue = await dependencies.showDiagnosticsDialog();
      if (!shouldContinue) {
        return;
      }
    } finally {
      validatingDiagnostics.set(false);
    }

    await doGenerateBicep();
  };

  const downloadBicepFiles = async (): Promise<void> => {
    const result = bicepResult();
    if (!result || bicepDownloading()) {
      return;
    }

    bicepDownloading.set(true);
    try {
      const config = dependencies.getConfig();
      if (!config) {
        return;
      }

      const blob = await bicepService.downloadZip(config.id);
      saveAs(blob, `${config.name ?? 'bicep'}-bicep.zip`);
    } finally {
      bicepDownloading.set(false);
    }
  };

  const doGeneratePipeline = async (): Promise<void> => {
    const configId = dependencies.getConfig()?.id;
    if (!configId || pipelineLoading()) {
      return;
    }

    pipelineLoading.set(true);
    pipelineErrorKey.set('');
    pipelineResult.set(null);
    generationPanelCollapsed.set(false);
    pipelinePanelOpen.set(true);

    try {
      pipelineResult.set(await pipelineService.generate({ infrastructureConfigId: configId }));
    } catch (error: unknown) {
      const axios = await import('axios');
      if (axios.isAxiosError(error)) {
        const status = error.response?.status;
        if (status === 401 || status === 403) {
          pipelineErrorKey.set('CONFIG_DETAIL.PIPELINE.GENERATE_AUTH_ERROR');
        } else {
          pipelineErrorKey.set('CONFIG_DETAIL.PIPELINE.GENERATE_ERROR');
        }
      } else {
        pipelineErrorKey.set('CONFIG_DETAIL.PIPELINE.GENERATE_ERROR');
      }
    } finally {
      pipelineLoading.set(false);
    }
  };

  const generatePipeline = async (): Promise<void> => {
    const configId = dependencies.getConfig()?.id;
    if (!configId || pipelineLoading()) {
      return;
    }

    validatingDiagnostics.set(true);
    try {
      const shouldContinue = await dependencies.showDiagnosticsDialog();
      if (!shouldContinue) {
        return;
      }
    } finally {
      validatingDiagnostics.set(false);
    }

    await doGeneratePipeline();
  };

  const downloadPipelineFiles = async (): Promise<void> => {
    const result = pipelineResult();
    if (!result || pipelineDownloading()) {
      return;
    }

    pipelineDownloading.set(true);
    try {
      const config = dependencies.getConfig();
      if (!config) {
        return;
      }

      const blob = await pipelineService.downloadZip(config.id);
      saveAs(blob, `${config.name ?? 'pipeline'}-pipeline.zip`);
    } finally {
      pipelineDownloading.set(false);
    }
  };

  const generateAll = async (): Promise<void> => {
    const configId = dependencies.getConfig()?.id;
    if (!configId || generateAllLoading()) {
      return;
    }

    validatingDiagnostics.set(true);
    try {
      const shouldContinue = await dependencies.showDiagnosticsDialog();
      if (!shouldContinue) {
        return;
      }
    } finally {
      validatingDiagnostics.set(false);
    }

    await Promise.all([doGenerateBicep(), doGeneratePipeline()]);
  };

  const viewModel = computed<ConfigDetailGenerationSectionViewModel | null>(() => {
    if (!dependencies.getConfig()) {
      return null;
    }

    return {
      showPanel: dependencies.isProjectMultiRepo() && generationPanelOpen(),
      isCollapsed: generationPanelCollapsed(),
      bicepLoading: bicepLoading(),
      bicepDownloading: bicepDownloading(),
      bicepResult: bicepResult(),
      bicepErrorKey: bicepErrorKey(),
      configBicepNodes: configBicepNodes(),
      loadConfigBicepFile,
      pipelineLoading: pipelineLoading(),
      pipelineDownloading: pipelineDownloading(),
      pipelineResult: pipelineResult(),
      pipelineErrorKey: pipelineErrorKey(),
      configPipelineNodes: configPipelineNodes(),
      loadConfigPipelineFile,
      onClosePanel: closeGenerationPanel,
      onTogglePanelCollapsed: toggleGenerationPanelCollapsed,
      onDownloadBicepFiles: () => void downloadBicepFiles(),
      onGenerateBicep: () => void generateBicep(),
      onDownloadPipelineFiles: () => void downloadPipelineFiles(),
      onGeneratePipeline: () => void generatePipeline(),
    };
  });

  const reset = (): void => {
    validatingDiagnostics.set(false);
    bicepLoading.set(false);
    bicepResult.set(null);
    bicepErrorKey.set('');
    bicepPanelOpen.set(false);
    bicepDownloading.set(false);
    pipelineLoading.set(false);
    pipelineResult.set(null);
    pipelineErrorKey.set('');
    pipelinePanelOpen.set(false);
    pipelineDownloading.set(false);
    generationPanelCollapsed.set(false);
  };

  return {
    generateAllLoading,
    validatingDiagnostics,
    viewModel,
    reset,
    generateAll,
  };
}