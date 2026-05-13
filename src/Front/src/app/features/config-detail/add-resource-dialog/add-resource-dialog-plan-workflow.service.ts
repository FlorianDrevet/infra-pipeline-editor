import { inject, Injectable } from '@angular/core';

import { ResourceTypeEnum } from '../enums/resource-type.enum';
import { ProjectResourceResponse } from '../../../shared/interfaces/cross-config-reference.interface';
import { AzureResourceResponse } from '../../../shared/interfaces/resource-group.interface';
import { AppServicePlanService } from '../../../shared/services/app-service-plan.service';
import { ContainerAppEnvironmentService } from '../../../shared/services/container-app-environment.service';
import { InfraConfigService } from '../../../shared/services/infra-config.service';
import { LogAnalyticsWorkspaceService } from '../../../shared/services/log-analytics-workspace.service';
import { ProjectService } from '../../../shared/services/project.service';
import { ResourceGroupService } from '../../../shared/services/resource-group.service';
import { SqlServerService } from '../../../shared/services/sql-server.service';
import {
  AddResourceEnvironmentDefinition,
} from './add-resource-dialog-environment-settings.helper';
import { resolveAddResourceParentResourceDescriptor } from './add-resource-dialog-parent-resource.helper';

interface AddResourceDialogCreatePlanData {
  readonly name: string;
  readonly location: string;
  readonly osType: string;
}

interface AddResourceDialogCreateSqlServerPlanData {
  readonly name: string;
  readonly location: string;
  readonly version: string;
  readonly administratorLogin: string;
}

interface AddResourceDialogPlanSelectionResult {
  readonly existingPlans: AzureResourceResponse[];
  readonly crossConfigPlans: ProjectResourceResponse[];
  readonly availableContainerRegistries: AzureResourceResponse[];
}

interface AddResourceDialogCreatedPlan {
  readonly id: string;
  readonly name: string;
}

@Injectable()
export class AddResourceDialogPlanWorkflowService {
  private readonly appServicePlanService = inject(AppServicePlanService);
  private readonly containerAppEnvironmentService = inject(ContainerAppEnvironmentService);
  private readonly infraConfigService = inject(InfraConfigService);
  private readonly logAnalyticsWorkspaceService = inject(LogAnalyticsWorkspaceService);
  private readonly projectService = inject(ProjectService);
  private readonly resourceGroupService = inject(ResourceGroupService);
  private readonly sqlServerService = inject(SqlServerService);

  async loadAvailableContainerRegistries(configId: string, projectId: string): Promise<AzureResourceResponse[]> {
    try {
      return await this.loadContainerRegistries(configId, projectId);
    } catch {
      return [];
    }
  }

  async loadPlanSelection(
    selectedType: ResourceTypeEnum | null,
    configId: string,
    projectId: string,
  ): Promise<AddResourceDialogPlanSelectionResult> {
    try {
      const descriptor = resolveAddResourceParentResourceDescriptor(selectedType);
      const [resourceGroups, projectResources] = await Promise.all([
        this.infraConfigService.getResourceGroups(configId),
        this.projectService.getProjectResources(projectId),
      ]);

      const allResourceArrays = await Promise.all(resourceGroups.map((resourceGroup) => this.resourceGroupService.getResources(resourceGroup.id)));
      const allResources = allResourceArrays.flat();
      const localAcrs = allResources.filter((resource) => resource.resourceType === ResourceTypeEnum.ContainerRegistry);

      return {
        existingPlans: allResources.filter((resource) => resource.resourceType === descriptor.filterType),
        crossConfigPlans: projectResources.filter(
          (resource) => resource.resourceType === descriptor.filterType && resource.configId !== configId,
        ),
        availableContainerRegistries: this.mergeContainerRegistries(localAcrs, projectResources, configId),
      };
    } catch {
      return {
        existingPlans: [],
        crossConfigPlans: [],
        availableContainerRegistries: [],
      };
    }
  }

  async ensureCrossConfigPlanReference(configId: string, resourceId: string): Promise<void> {
    try {
      const references = await this.infraConfigService.getCrossConfigReferences(configId);
      const alreadyReferenced = references.some((reference) => reference.targetResourceId === resourceId);
      if (!alreadyReferenced) {
        await this.infraConfigService.addCrossConfigReference(configId, { targetResourceId: resourceId });
      }
    } catch {
      // Non-blocking: the dialog can proceed with the selected reference even if it already exists.
    }
  }

  async createPlan(command: {
    readonly selectedType: ResourceTypeEnum | null;
    readonly resourceGroupId: string;
    readonly planData: AddResourceDialogCreatePlanData;
    readonly sqlServerData: AddResourceDialogCreateSqlServerPlanData;
    readonly environments: readonly AddResourceEnvironmentDefinition[];
  }): Promise<AddResourceDialogCreatedPlan> {
    const { selectedType, resourceGroupId, planData, sqlServerData, environments } = command;

    if (selectedType === ResourceTypeEnum.ContainerApp) {
      const created = await this.containerAppEnvironmentService.create({
        resourceGroupId,
        name: planData.name,
        location: planData.location,
        environmentSettings: environments.map((environment) => ({
          environmentName: environment.name,
          sku: 'Consumption',
        })),
      });
      return { id: created.id, name: created.name };
    }

    if (selectedType === ResourceTypeEnum.ApplicationInsights || selectedType === ResourceTypeEnum.ContainerAppEnvironment) {
      const created = await this.logAnalyticsWorkspaceService.create({
        resourceGroupId,
        name: planData.name,
        location: planData.location,
        environmentSettings: environments.map((environment) => ({
          environmentName: environment.name,
          sku: 'PerGB2018',
        })),
      });
      return { id: created.id, name: created.name };
    }

    if (selectedType === ResourceTypeEnum.SqlDatabase) {
      const created = await this.sqlServerService.create({
        resourceGroupId,
        name: sqlServerData.name,
        location: sqlServerData.location,
        version: sqlServerData.version,
        administratorLogin: sqlServerData.administratorLogin,
        environmentSettings: environments.map((environment) => ({
          environmentName: environment.name,
          minimalTlsVersion: '1.2',
        })),
      });
      return { id: created.id, name: created.name };
    }

    const created = await this.appServicePlanService.create({
      resourceGroupId,
      name: planData.name,
      location: planData.location,
      osType: planData.osType,
      environmentSettings: environments.map((environment) => ({
        environmentName: environment.name,
        sku: 'B1',
        capacity: 1,
      })),
    });
    return { id: created.id, name: created.name };
  }

  private async loadContainerRegistries(configId: string, projectId: string): Promise<AzureResourceResponse[]> {
    const resourceGroups = await this.infraConfigService.getResourceGroups(configId);
    const allResourceArrays = await Promise.all(resourceGroups.map((resourceGroup) => this.resourceGroupService.getResources(resourceGroup.id)));
    const localResources = allResourceArrays.flat();
    const localContainerRegistries = localResources.filter((resource) => resource.resourceType === ResourceTypeEnum.ContainerRegistry);
    const projectResources = await this.projectService.getProjectResources(projectId);

    return this.mergeContainerRegistries(localContainerRegistries, projectResources, configId);
  }

  private mergeContainerRegistries(
    localContainerRegistries: AzureResourceResponse[],
    projectResources: ProjectResourceResponse[],
    configId: string,
  ): AzureResourceResponse[] {
    const localIds = new Set(localContainerRegistries.map((resource) => resource.id));
    const crossConfigContainerRegistries = projectResources
      .filter(
        (resource) => resource.resourceType === ResourceTypeEnum.ContainerRegistry
          && resource.configId !== configId
          && !localIds.has(resource.resourceId),
      )
      .map((resource) => ({
        id: resource.resourceId,
        resourceType: resource.resourceType,
        name: `${resource.resourceName} (${resource.configName})`,
        location: '',
      } satisfies AzureResourceResponse));

    return [...localContainerRegistries, ...crossConfigContainerRegistries];
  }
}