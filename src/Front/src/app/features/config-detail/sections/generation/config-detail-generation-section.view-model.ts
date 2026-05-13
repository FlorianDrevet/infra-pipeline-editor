import { GenerateBicepResponse } from '../../../../shared/interfaces/bicep-generator.interface';
import { GeneratePipelineResponse } from '../../../../shared/interfaces/pipeline-generator.interface';
import { BicepTreeNode } from '../../../../shared/components/bicep-file-panel/bicep-file-panel.component';

export interface ConfigDetailGenerationSectionViewModel {
  showPanel: boolean;
  isCollapsed: boolean;
  bicepLoading: boolean;
  bicepDownloading: boolean;
  bicepResult: GenerateBicepResponse | null;
  bicepErrorKey: string;
  configBicepNodes: BicepTreeNode[];
  loadConfigBicepFile: (filePath: string) => Promise<string>;
  pipelineLoading: boolean;
  pipelineDownloading: boolean;
  pipelineResult: GeneratePipelineResponse | null;
  pipelineErrorKey: string;
  configPipelineNodes: BicepTreeNode[];
  loadConfigPipelineFile: (filePath: string) => Promise<string>;
  onClosePanel: () => void;
  onTogglePanelCollapsed: () => void;
  onDownloadBicepFiles: () => void;
  onGenerateBicep: () => void;
  onDownloadPipelineFiles: () => void;
  onGeneratePipeline: () => void;
}