export const PROJECT_DETAIL_ROUTE_TABS = {
  environments: 'environments',
  naming: 'naming',
  tags: 'tags',
  variables: 'variables',
} as const;

export type ProjectDetailRouteTab = (typeof PROJECT_DETAIL_ROUTE_TABS)[keyof typeof PROJECT_DETAIL_ROUTE_TABS];

export const CONFIG_DETAIL_ROUTE_TABS = {
  tags: 'tags',
  naming: 'naming',
  crossConfigRefs: 'cross-config-refs',
  variables: 'variables',
  git: 'git',
} as const;

export type ConfigDetailRouteTab = (typeof CONFIG_DETAIL_ROUTE_TABS)[keyof typeof CONFIG_DETAIL_ROUTE_TABS];

export function getProjectDetailTabIndex(tab: string | null): number {
  switch (tab) {
    case PROJECT_DETAIL_ROUTE_TABS.environments:
      return 1;
    case PROJECT_DETAIL_ROUTE_TABS.naming:
      return 2;
    case PROJECT_DETAIL_ROUTE_TABS.tags:
      return 3;
    case PROJECT_DETAIL_ROUTE_TABS.variables:
      return 4;
    default:
      return 0;
  }
}

export function getProjectDetailTabQuery(index: number): ProjectDetailRouteTab | null {
  switch (index) {
    case 1:
      return PROJECT_DETAIL_ROUTE_TABS.environments;
    case 2:
      return PROJECT_DETAIL_ROUTE_TABS.naming;
    case 3:
      return PROJECT_DETAIL_ROUTE_TABS.tags;
    case 4:
      return PROJECT_DETAIL_ROUTE_TABS.variables;
    default:
      return null;
  }
}
export function isProjectDetailTab(tab: string | null): tab is ProjectDetailRouteTab {
  return tab !== null && getProjectDetailTabQuery(getProjectDetailTabIndex(tab)) === tab;
}

export function getConfigDetailTabIndex(tab: string | null): number {
  switch (tab) {
    case CONFIG_DETAIL_ROUTE_TABS.tags:
      return 1;
    case CONFIG_DETAIL_ROUTE_TABS.naming:
      return 2;
    case CONFIG_DETAIL_ROUTE_TABS.crossConfigRefs:
      return 3;
    case CONFIG_DETAIL_ROUTE_TABS.variables:
      return 4;
    case CONFIG_DETAIL_ROUTE_TABS.git:
      return 5;
    default:
      return 0;
  }
}

export function getConfigDetailTabQuery(index: number): ConfigDetailRouteTab | null {
  switch (index) {
    case 1:
      return CONFIG_DETAIL_ROUTE_TABS.tags;
    case 2:
      return CONFIG_DETAIL_ROUTE_TABS.naming;
    case 3:
      return CONFIG_DETAIL_ROUTE_TABS.crossConfigRefs;
    case 4:
      return CONFIG_DETAIL_ROUTE_TABS.variables;
    case 5:
      return CONFIG_DETAIL_ROUTE_TABS.git;
    default:
      return null;
  }
}

/**
 * Tab identifiers used by ds-tabs for config-detail. The first entry is the
 * default tab when no `?tab=` query parameter is present.
 */
export const CONFIG_DETAIL_TAB_IDS = [
  'resource-groups',
  'tags',
  'naming',
  'cross-config-refs',
  'variables',
  'git',
] as const;

export type ConfigDetailTabId = (typeof CONFIG_DETAIL_TAB_IDS)[number];

export function getConfigDetailTabIdFromQuery(tab: string | null): ConfigDetailTabId {
  return CONFIG_DETAIL_TAB_IDS[getConfigDetailTabIndex(tab)];
}

export function getConfigDetailQueryFromTabId(tabId: ConfigDetailTabId): ConfigDetailRouteTab | null {
  return getConfigDetailTabQuery(CONFIG_DETAIL_TAB_IDS.indexOf(tabId));
}

/**
 * Project detail tab identifiers used by ds-tabs. First entry is the default
 * tab when no `?tab=` query parameter is present.
 */
export const PROJECT_DETAIL_TAB_IDS = [
  'overview',
  'environments',
  'naming',
  'tags',
  'variables',
] as const;

export type ProjectDetailTabId = (typeof PROJECT_DETAIL_TAB_IDS)[number];

export function getProjectDetailTabIdFromQuery(tab: string | null): ProjectDetailTabId {
  return PROJECT_DETAIL_TAB_IDS[getProjectDetailTabIndex(tab)];
}

export function getProjectDetailQueryFromTabId(tabId: ProjectDetailTabId): ProjectDetailRouteTab | null {
  return getProjectDetailTabQuery(PROJECT_DETAIL_TAB_IDS.indexOf(tabId));
}