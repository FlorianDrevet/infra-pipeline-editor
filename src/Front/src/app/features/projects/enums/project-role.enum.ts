export enum ProjectRoleEnum {
  Owner = 'Owner',
  Contributor = 'Contributor',
  Reader = 'Reader',
}

export const PROJECT_ROLE_OPTIONS = Object.entries(ProjectRoleEnum).map(([key, value]) => ({
  label: key,
  value,
}));

export const PROJECT_ROLE_ICONS: Record<string, string> = {
  Owner: 'shield',
  Contributor: 'edit',
  Reader: 'visibility',
};
