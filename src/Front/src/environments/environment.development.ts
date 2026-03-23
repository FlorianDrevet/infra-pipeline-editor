import {EnvironmentInterface} from "../app/shared/interfaces/environment.interface";

export const environment : EnvironmentInterface =
  {
    production: false,
    api_url: "http://localhost:8080",
    msalConfig: {
      clientId: '24c34231-a984-43b3-8ac3-9278ebd067ef',
      authority: 'https://login.microsoftonline.com/cc625709-6696-4cf6-a330-7baf406f6a99',
      redirectUri: 'http://localhost:4200',
      apiScopes: ['api://4f7f2dbd-8a11-42c4-bedc-acbb127e9394/Configuration.Write'],
    },
  };
