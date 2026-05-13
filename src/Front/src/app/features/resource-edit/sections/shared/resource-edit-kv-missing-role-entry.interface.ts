export interface ResourceEditKvMissingRoleEntry {
  keyVaultResourceId: string;
  keyVaultName: string;
  missingRoleName: string | null;
  missingRoleDefinitionId: string | null;
  affectedSettingsCount: number;
  checking: boolean;
  assigning: boolean;
  selectedIdentityType: 'UserAssigned' | 'SystemAssigned';
  selectedUaiId: string | null;
}