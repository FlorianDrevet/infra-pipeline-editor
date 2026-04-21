export type NameAvailabilityStatus = 'available' | 'unavailable' | 'unknown' | 'invalid';

export interface EnvironmentNameAvailabilityResponseItem {
  environmentName: string;
  environmentShortName: string;
  subscriptionId: string;
  generatedName: string;
  appliedTemplate: string;
  status: NameAvailabilityStatus;
  reason?: string | null;
  message?: string | null;
}

export interface CheckResourceNameAvailabilityResponse {
  resourceType: string;
  rawName: string;
  supported: boolean;
  environments: EnvironmentNameAvailabilityResponseItem[];
}

export interface CheckResourceNameAvailabilityRequest {
  projectId: string;
  configId?: string | null;
  name: string;
}
