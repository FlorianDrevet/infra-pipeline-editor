import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { DsSpinnerComponent } from '../../../shared/components/ds/ds-spinner/ds-spinner.component';
import { DsButtonComponent, DsIconButtonComponent } from '../../../shared/components/ds';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';

import { EnvironmentDefinitionResponse, TagResponse } from '../../../shared/interfaces/infra-config.interface';

const EMPTY_VALUE = '—';
const ENVIRONMENT_GROUP_KINDS = {
  facts: 'facts',
  tags: 'tags',
} as const;
const BOOLEAN_TRANSLATION_KEYS = {
  yes: 'PROJECT_DETAIL.BOOLEAN.YES',
  no: 'PROJECT_DETAIL.BOOLEAN.NO',
} as const;
const ENVIRONMENT_TRANSLATION_KEYS = {
  approvalRequired: 'PROJECT_DETAIL.ENVIRONMENTS.META.APPROVAL_REQUIRED',
  armConnection: 'PROJECT_DETAIL.ENVIRONMENTS.ARM_CONNECTION',
  governanceDescription: 'PROJECT_DETAIL.ENVIRONMENTS.SECTIONS.GOVERNANCE.DESCRIPTION',
  governanceTitle: 'PROJECT_DETAIL.ENVIRONMENTS.SECTIONS.GOVERNANCE.TITLE',
  namingDescription: 'PROJECT_DETAIL.ENVIRONMENTS.SECTIONS.NAMING.DESCRIPTION',
  namingTitle: 'PROJECT_DETAIL.ENVIRONMENTS.SECTIONS.NAMING.TITLE',
  noTags: 'PROJECT_DETAIL.ENVIRONMENTS.NO_TAGS',
  notConfigured: 'PROJECT_DETAIL.ENVIRONMENTS.NOT_CONFIGURED',
  order: 'PROJECT_DETAIL.ENVIRONMENTS.ORDER',
  orderMeta: 'PROJECT_DETAIL.ENVIRONMENTS.META.ORDER',
  overviewDescription: 'PROJECT_DETAIL.ENVIRONMENTS.SECTIONS.OVERVIEW.DESCRIPTION',
  overviewTitle: 'PROJECT_DETAIL.ENVIRONMENTS.SECTIONS.OVERVIEW.TITLE',
  standardRelease: 'PROJECT_DETAIL.ENVIRONMENTS.META.STANDARD_RELEASE',
  tagsDescription: 'PROJECT_DETAIL.ENVIRONMENTS.SECTIONS.TAGS.DESCRIPTION',
  tagsTitle: 'PROJECT_DETAIL.ENVIRONMENTS.SECTIONS.TAGS.TITLE',
} as const;

type EnvironmentGroupKind = (typeof ENVIRONMENT_GROUP_KINDS)[keyof typeof ENVIRONMENT_GROUP_KINDS];

interface EnvironmentFact {
  labelKey: string;
  value: string;
  valueKey?: string;
  monospace?: boolean;
}

interface EnvironmentFactsGroup {
  descriptionKey: string;
  facts: readonly EnvironmentFact[];
  key: string;
  kind: typeof ENVIRONMENT_GROUP_KINDS.facts;
  titleKey: string;
}

interface EnvironmentTagsGroup {
  descriptionKey: string;
  key: string;
  kind: typeof ENVIRONMENT_GROUP_KINDS.tags;
  tags: readonly TagResponse[];
  titleKey: string;
}

type EnvironmentGroup = EnvironmentFactsGroup | EnvironmentTagsGroup;

interface EnvironmentPanelViewModel {
  approvalMetaKey: string;
  environment: EnvironmentDefinitionResponse;
  groups: readonly EnvironmentGroup[];
}

@Component({
  selector: 'app-project-detail-environments-section',
  standalone: true,
  imports: [MatIconModule, DsSpinnerComponent, DsButtonComponent, DsIconButtonComponent, MatTooltipModule, TranslateModule],
  templateUrl: './project-detail-environments-section.component.html',
  styleUrl: './project-detail-environments-section.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProjectDetailEnvironmentsSectionComponent {
  readonly environments = input.required<readonly EnvironmentDefinitionResponse[]>();
  readonly canWrite = input.required<boolean>();
  readonly actionId = input<string | null>(null);
  readonly errorKey = input('');

  readonly addEnvironment = output<void>();
  readonly editEnvironment = output<EnvironmentDefinitionResponse>();
  readonly removeEnvironment = output<EnvironmentDefinitionResponse>();

  protected readonly groupKinds = ENVIRONMENT_GROUP_KINDS;
  protected readonly environmentPanels = computed<readonly EnvironmentPanelViewModel[]>(() =>
    this.environments().map((environment) => ({
      environment,
      approvalMetaKey: environment.requiresApproval
        ? ENVIRONMENT_TRANSLATION_KEYS.approvalRequired
        : ENVIRONMENT_TRANSLATION_KEYS.standardRelease,
      groups: buildEnvironmentGroups(environment),
    })),
  );

  protected onAddEnvironment(): void {
    this.addEnvironment.emit();
  }

  protected onEditEnvironment(environment: EnvironmentDefinitionResponse): void {
    this.editEnvironment.emit(environment);
  }

  protected onRemoveEnvironment(environment: EnvironmentDefinitionResponse): void {
    this.removeEnvironment.emit(environment);
  }
}

function buildEnvironmentGroups(environment: EnvironmentDefinitionResponse): readonly EnvironmentGroup[] {
  return [
    {
      key: 'overview',
      kind: ENVIRONMENT_GROUP_KINDS.facts,
      titleKey: ENVIRONMENT_TRANSLATION_KEYS.overviewTitle,
      descriptionKey: ENVIRONMENT_TRANSLATION_KEYS.overviewDescription,
      facts: [
        createFact('PROJECT_DETAIL.ENVIRONMENTS.LOCATION', environment.location),
        createFact('PROJECT_DETAIL.ENVIRONMENTS.SHORT_NAME', environment.shortName),
        createFact('PROJECT_DETAIL.ENVIRONMENTS.SUBSCRIPTION_ID', environment.subscriptionId, { monospace: true }),
      ],
    },
    {
      key: 'naming',
      kind: ENVIRONMENT_GROUP_KINDS.facts,
      titleKey: ENVIRONMENT_TRANSLATION_KEYS.namingTitle,
      descriptionKey: ENVIRONMENT_TRANSLATION_KEYS.namingDescription,
      facts: [
        createFact('PROJECT_DETAIL.ENVIRONMENTS.PREFIX', environment.prefix),
        createFact('PROJECT_DETAIL.ENVIRONMENTS.SUFFIX', environment.suffix),
      ],
    },
    {
      key: 'governance',
      kind: ENVIRONMENT_GROUP_KINDS.facts,
      titleKey: ENVIRONMENT_TRANSLATION_KEYS.governanceTitle,
      descriptionKey: ENVIRONMENT_TRANSLATION_KEYS.governanceDescription,
      facts: [
        createFact(ENVIRONMENT_TRANSLATION_KEYS.order, environment.order.toString()),
        createFact('PROJECT_DETAIL.ENVIRONMENTS.REQUIRES_APPROVAL', EMPTY_VALUE, {
          valueKey: environment.requiresApproval ? BOOLEAN_TRANSLATION_KEYS.yes : BOOLEAN_TRANSLATION_KEYS.no,
        }),
        createFact(ENVIRONMENT_TRANSLATION_KEYS.armConnection, environment.azureResourceManagerConnection?.trim() ?? EMPTY_VALUE, {
          valueKey: environment.azureResourceManagerConnection?.trim() ? undefined : ENVIRONMENT_TRANSLATION_KEYS.notConfigured,
        }),
      ],
    },
    {
      key: 'tags',
      kind: ENVIRONMENT_GROUP_KINDS.tags,
      titleKey: ENVIRONMENT_TRANSLATION_KEYS.tagsTitle,
      descriptionKey: ENVIRONMENT_TRANSLATION_KEYS.tagsDescription,
      tags: environment.tags,
    },
  ];
}

function createFact(
  labelKey: string,
  rawValue: string,
  options?: { monospace?: boolean; valueKey?: string },
): EnvironmentFact {
  return {
    labelKey,
    value: rawValue || EMPTY_VALUE,
    valueKey: options?.valueKey,
    monospace: options?.monospace,
  };
}