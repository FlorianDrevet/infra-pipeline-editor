export const PROJECT_DETAIL_ROUTE_TABS = {
  environments: 'environments',
  naming: 'naming',
  members: 'members',
  variables: 'variables',
  settings: 'settings',
  repositories: 'repositories',
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
    case PROJECT_DETAIL_ROUTE_TABS.members:
      return 3;
    case PROJECT_DETAIL_ROUTE_TABS.variables:
      return 4;
    case PROJECT_DETAIL_ROUTE_TABS.settings:
      return 5;
    case PROJECT_DETAIL_ROUTE_TABS.repositories:
      return 6;
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
      return PROJECT_DETAIL_ROUTE_TABS.members;
    case 4:
      return PROJECT_DETAIL_ROUTE_TABS.variables;
    case 5:
      return PROJECT_DETAIL_ROUTE_TABS.settings;
    case 6:
      return PROJECT_DETAIL_ROUTE_TABS.repositories;
    default:
      return null;
  }
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