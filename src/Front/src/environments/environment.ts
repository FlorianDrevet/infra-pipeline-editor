import {EnvironmentInterface} from "../app/shared/interfaces/environment.interface";

export const environment : EnvironmentInterface =
  {
    production: true,
    api_url: '',
    msalConfig: {
      clientId: '24c34231-a984-43b3-8ac3-9278ebd067ef',
      authority: 'https://login.microsoftonline.com/common',
      redirectUri: '/',
    },
  };
