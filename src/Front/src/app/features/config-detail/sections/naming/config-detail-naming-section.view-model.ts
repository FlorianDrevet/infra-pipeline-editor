import { InfrastructureConfigResponse, ResourceNamingTemplateResponse } from '../../../../shared/interfaces/infra-config.interface';
import { ProjectResponse } from '../../../../shared/interfaces/project.interface';

export interface ConfigDetailNamingSectionViewModel {
  config: InfrastructureConfigResponse;
  project: ProjectResponse | null;
  canWrite: boolean;
  useProjectNamingConventions: boolean;
  inheritanceLoading: boolean;
  namingActionKey: string | null;
  namingErrorKey: string;
  canAddResourceNamingTemplate: boolean;
  resourceTypeIcons: Readonly<Record<string, string>>;
  abbreviationDisplayItems: ReadonlyArray<{
    resourceType: string;
    defaultAbbreviation: string;
    effectiveAbbreviation: string;
    isCustomized: boolean;
  }>;
  isNamingActionActive: (actionKey: string) => boolean;
  isResourceNamingTemplateBusy: (resourceType: string) => boolean;
  isAbbreviationBusy: (resourceType: string) => boolean;
  onToggleInheritanceNaming: (useProject: boolean) => void | Promise<void>;
  onOpenDefaultNamingTemplateDialog: () => void;
  onOpenResourceNamingTemplateDialog: (existing?: ResourceNamingTemplateResponse) => void;
  onOpenRemoveResourceNamingTemplateDialog: (template: ResourceNamingTemplateResponse) => void;
  onOpenEditAbbreviationDialog: (item: { resourceType: string; defaultAbbreviation: string; effectiveAbbreviation: string }) => void;
  onOpenResetAbbreviationDialog: (item: { resourceType: string; defaultAbbreviation: string }) => void;
}