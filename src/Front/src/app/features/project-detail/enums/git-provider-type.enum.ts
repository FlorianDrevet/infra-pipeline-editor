export enum GitProviderTypeEnum {
  GitHub = 'GitHub',
  AzureDevOps = 'AzureDevOps',
}

export const GIT_PROVIDER_TYPE_OPTIONS = Object.entries(GitProviderTypeEnum).map(
  ([key, value]) => ({
    label: key,
    value,
  })
);
