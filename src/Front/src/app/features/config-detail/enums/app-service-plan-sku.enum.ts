export enum AppServicePlanSkuEnum {
  F1 = 'F1',
  B1 = 'B1',
  B2 = 'B2',
  B3 = 'B3',
  S1 = 'S1',
  S2 = 'S2',
  S3 = 'S3',
  P1v3 = 'P1v3',
  P2v3 = 'P2v3',
  P3v3 = 'P3v3',
}

export const APP_SERVICE_PLAN_SKU_OPTIONS = Object.entries(AppServicePlanSkuEnum).map(([key, value]) => ({
  label: key,
  value,
}));
