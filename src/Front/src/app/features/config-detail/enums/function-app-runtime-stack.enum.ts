export enum FunctionAppRuntimeStackEnum {
  DotNet = 'DotNet',
  Node = 'Node',
  Python = 'Python',
  Java = 'Java',
  PowerShell = 'PowerShell',
}

export const FUNCTION_APP_RUNTIME_STACK_OPTIONS = Object.entries(FunctionAppRuntimeStackEnum).map(([key, value]) => ({
  label: key,
  value,
}));
