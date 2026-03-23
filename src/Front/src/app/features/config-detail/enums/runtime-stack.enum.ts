export enum RuntimeStackEnum {
  DotNet = 'DotNet',
  Node = 'Node',
  Python = 'Python',
  Java = 'Java',
  Php = 'Php',
}

export const RUNTIME_STACK_OPTIONS = Object.entries(RuntimeStackEnum).map(([key, value]) => ({
  label: key,
  value,
}));
