export interface MsalConfigInterface {
  clientId: string;
  authority: string;
  redirectUri: string;
}

export interface EnvironmentInterface {
  production: boolean;
  api_url: string;
  msalConfig: MsalConfigInterface;
}
