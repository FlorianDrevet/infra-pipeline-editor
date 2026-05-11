import { parse as parseYaml } from 'yaml';

export interface ImportAppSettingsContext {
  resourceType: string;
  deploymentMode: string | null;
  runtimeStack: string | null;
}

export type ImportAppSettingsMode =
  | 'dotnetJson'
  | 'functionAppJson'
  | 'env'
  | 'javaProperties'
  | 'javaYaml';

export interface ImportAppSettingsModeDefinition {
  mode: ImportAppSettingsMode;
  labelKey: string;
  descriptionKey: string;
  exampleFileNames: readonly string[];
  acceptExtensions: readonly string[];
}

export interface ParsedImportAppSettingEntry {
  key: string;
  value: string;
}

const IMPORT_APP_SETTINGS_MODE_DEFINITIONS: readonly ImportAppSettingsModeDefinition[] = [
  {
    mode: 'dotnetJson',
    labelKey: 'RESOURCE_EDIT.IMPORT_APP_SETTINGS.MODE_DOTNET_JSON',
    descriptionKey: 'RESOURCE_EDIT.IMPORT_APP_SETTINGS.MODE_DOTNET_JSON_DESC',
    exampleFileNames: ['appsettings.json', 'appsettings.Development.json'],
    acceptExtensions: ['.json'],
  },
  {
    mode: 'functionAppJson',
    labelKey: 'RESOURCE_EDIT.IMPORT_APP_SETTINGS.MODE_FUNCTION_JSON',
    descriptionKey: 'RESOURCE_EDIT.IMPORT_APP_SETTINGS.MODE_FUNCTION_JSON_DESC',
    exampleFileNames: ['local.settings.json'],
    acceptExtensions: ['.json'],
  },
  {
    mode: 'env',
    labelKey: 'RESOURCE_EDIT.IMPORT_APP_SETTINGS.MODE_ENV',
    descriptionKey: 'RESOURCE_EDIT.IMPORT_APP_SETTINGS.MODE_ENV_DESC',
    exampleFileNames: ['.env', '.env.production'],
    acceptExtensions: ['.env', '.txt'],
  },
  {
    mode: 'javaProperties',
    labelKey: 'RESOURCE_EDIT.IMPORT_APP_SETTINGS.MODE_JAVA_PROPERTIES',
    descriptionKey: 'RESOURCE_EDIT.IMPORT_APP_SETTINGS.MODE_JAVA_PROPERTIES_DESC',
    exampleFileNames: ['application.properties'],
    acceptExtensions: ['.properties', '.txt'],
  },
  {
    mode: 'javaYaml',
    labelKey: 'RESOURCE_EDIT.IMPORT_APP_SETTINGS.MODE_JAVA_YAML',
    descriptionKey: 'RESOURCE_EDIT.IMPORT_APP_SETTINGS.MODE_JAVA_YAML_DESC',
    exampleFileNames: ['application.yml', 'application.yaml'],
    acceptExtensions: ['.yml', '.yaml'],
  },
] as const;

const IMPORT_APP_SETTINGS_MODE_DEFINITION_MAP = new Map(
  IMPORT_APP_SETTINGS_MODE_DEFINITIONS.map((definition) => [definition.mode, definition]),
);

export function listImportAppSettingsModes(): readonly ImportAppSettingsModeDefinition[] {
  return IMPORT_APP_SETTINGS_MODE_DEFINITIONS;
}

export function getImportAppSettingsModeDefinition(mode: ImportAppSettingsMode): ImportAppSettingsModeDefinition {
  return IMPORT_APP_SETTINGS_MODE_DEFINITION_MAP.get(mode) ?? IMPORT_APP_SETTINGS_MODE_DEFINITIONS[0];
}

export function resolveImportAppSettingsMode(context: ImportAppSettingsContext): ImportAppSettingsMode {
  const normalizedResourceType = normalizeToken(context.resourceType);
  const normalizedDeploymentMode = normalizeToken(context.deploymentMode);
  const normalizedRuntimeStack = normalizeToken(context.runtimeStack);

  if (normalizedResourceType === 'functionapp') {
    return 'functionAppJson';
  }

  if (normalizedResourceType === 'containerapp' || normalizedDeploymentMode === 'container') {
    return 'env';
  }

  switch (normalizedRuntimeStack) {
    case 'dotnet':
      return 'dotnetJson';
    case 'java':
      return 'javaProperties';
    default:
      return 'env';
  }
}

export function resolveImportAppSettingsModeForFileName(
  fileName: string,
  fallbackMode: ImportAppSettingsMode,
): ImportAppSettingsMode {
  const normalizedFileName = normalizeFileName(fileName);
  if (!normalizedFileName) {
    return fallbackMode;
  }

  if (normalizedFileName.endsWith('.properties')) {
    return 'javaProperties';
  }

  if (normalizedFileName.endsWith('.yml') || normalizedFileName.endsWith('.yaml')) {
    return 'javaYaml';
  }

  if (normalizedFileName === '.env' || normalizedFileName.startsWith('.env.') || normalizedFileName.endsWith('.env')) {
    return 'env';
  }

  if (normalizedFileName.endsWith('.json')) {
    return fallbackMode === 'functionAppJson' ? 'functionAppJson' : 'dotnetJson';
  }

  return fallbackMode;
}

export function buildImportAppSettingsAcceptAttribute(): string {
  const extensions = new Set<string>();

  for (const definition of IMPORT_APP_SETTINGS_MODE_DEFINITIONS) {
    for (const extension of definition.acceptExtensions) {
      extensions.add(extension);
    }
  }

  return [...extensions].join(',');
}

export function parseImportAppSettingsContent(
  content: string,
  mode: ImportAppSettingsMode,
): ParsedImportAppSettingEntry[] {
  switch (mode) {
    case 'dotnetJson':
      return parseJsonContent(content, '__');
    case 'functionAppJson':
      return parseFunctionAppJsonContent(content);
    case 'env':
      return parseEnvContent(content);
    case 'javaProperties':
      return parseJavaPropertiesContent(content);
    case 'javaYaml':
      return parseYamlContent(content, '.');
  }
}

function parseJsonContent(content: string, separator: '__'): ParsedImportAppSettingEntry[] {
  const parsed = JSON.parse(content) as unknown;
  const record = ensureRecord(parsed);

  return flattenRecord(record, separator);
}

function parseFunctionAppJsonContent(content: string): ParsedImportAppSettingEntry[] {
  const parsed = JSON.parse(content) as unknown;
  const record = ensureRecord(parsed);
  const valuesNode = isRecord(record['Values']) ? record['Values'] : record;

  return flattenRecord(valuesNode, '__');
}

function parseEnvContent(content: string): ParsedImportAppSettingEntry[] {
  const entries: ParsedImportAppSettingEntry[] = [];

  for (const rawLine of splitLines(content)) {
    const trimmed = rawLine.trim();
    if (!trimmed || trimmed.startsWith('#')) {
      continue;
    }

    const withoutExport = trimmed.startsWith('export ') ? trimmed.slice('export '.length).trim() : trimmed;
    const separatorIndex = findFirstUnescapedDelimiter(withoutExport, ['=']);
    if (separatorIndex < 1) {
      continue;
    }

    const key = withoutExport.slice(0, separatorIndex).trim();
    const rawValue = withoutExport.slice(separatorIndex + 1).trim();
    if (!key) {
      continue;
    }

    entries.push({ key, value: stripQuotes(rawValue) });
  }

  return entries;
}

function parseJavaPropertiesContent(content: string): ParsedImportAppSettingEntry[] {
  const entries: ParsedImportAppSettingEntry[] = [];
  const logicalLines = joinLineContinuations(splitLines(content));

  for (const logicalLine of logicalLines) {
    const trimmed = logicalLine.trim();
    if (!trimmed || trimmed.startsWith('#') || trimmed.startsWith('!')) {
      continue;
    }

    const separatorIndex = findPropertySeparatorIndex(logicalLine);
    const key = (separatorIndex < 0 ? logicalLine : logicalLine.slice(0, separatorIndex)).trim();
    const rawValue = separatorIndex < 0 ? '' : logicalLine.slice(separatorIndex + 1).trim();

    if (!key) {
      continue;
    }

    entries.push({ key: unescapeJavaProperties(key), value: unescapeJavaProperties(rawValue) });
  }

  return entries;
}

function parseYamlContent(content: string, separator: '.'): ParsedImportAppSettingEntry[] {
  const parsed = parseYaml(content) as unknown;
  const record = ensureRecord(parsed);

  return flattenRecord(record, separator);
}

function flattenRecord(
  record: Record<string, unknown>,
  separator: '__' | '.',
  prefix: string = '',
): ParsedImportAppSettingEntry[] {
  const entries: ParsedImportAppSettingEntry[] = [];

  for (const [key, value] of Object.entries(record)) {
    const fullKey = prefix ? `${prefix}${separator}${key}` : key;

    if (isRecord(value)) {
      entries.push(...flattenRecord(value, separator, fullKey));
      continue;
    }

    entries.push({ key: fullKey, value: stringifyLeaf(value) });
  }

  return entries;
}

function ensureRecord(value: unknown): Record<string, unknown> {
  if (!isRecord(value)) {
    throw new Error('Root content must be an object.');
  }

  return value;
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}

function stringifyLeaf(value: unknown): string {
  if (value === null || value === undefined) {
    return '';
  }

  if (typeof value === 'string') {
    return value;
  }

  if (typeof value === 'number' || typeof value === 'boolean' || typeof value === 'bigint') {
    return String(value);
  }

  return JSON.stringify(value);
}

function splitLines(content: string): string[] {
  return content.replace(/^\uFEFF/, '').split(/\r?\n/);
}

function normalizeToken(value: string | null | undefined): string {
  return (value ?? '').trim().toLowerCase();
}

function normalizeFileName(fileName: string): string {
  const normalized = fileName.replace(/\\/g, '/').trim().toLowerCase();
  const segments = normalized.split('/');
  return segments[segments.length - 1] ?? '';
}

function findFirstUnescapedDelimiter(value: string, delimiters: readonly string[]): number {
  let isEscaped = false;

  for (let index = 0; index < value.length; index += 1) {
    const char = value[index];
    if (isEscaped) {
      isEscaped = false;
      continue;
    }

    if (char === '\\') {
      isEscaped = true;
      continue;
    }

    if (delimiters.includes(char)) {
      return index;
    }
  }

  return -1;
}

function stripQuotes(rawValue: string): string {
  if (rawValue.length < 2) {
    return rawValue;
  }

  const first = rawValue[0];
  const last = rawValue[rawValue.length - 1];

  if ((first === '"' && last === '"') || (first === '\'' && last === '\'')) {
    const innerValue = rawValue.slice(1, -1);
    return first === '"'
      ? innerValue
        .replace(/\\n/g, '\n')
        .replace(/\\r/g, '\r')
        .replace(/\\t/g, '\t')
        .replace(/\\"/g, '"')
        .replace(/\\\\/g, '\\')
      : innerValue;
  }

  return rawValue;
}

function joinLineContinuations(lines: readonly string[]): string[] {
  const logicalLines: string[] = [];
  let current = '';

  for (const line of lines) {
    if (!current) {
      current = line;
    } else {
      current += line;
    }

    if (hasContinuation(line)) {
      current = current.slice(0, -1);
      continue;
    }

    logicalLines.push(current);
    current = '';
  }

  if (current) {
    logicalLines.push(current);
  }

  return logicalLines;
}

function hasContinuation(line: string): boolean {
  let trailingBackslashCount = 0;

  for (let index = line.length - 1; index >= 0 && line[index] === '\\'; index -= 1) {
    trailingBackslashCount += 1;
  }

  return trailingBackslashCount % 2 === 1;
}

function findPropertySeparatorIndex(value: string): number {
  let isEscaped = false;

  for (let index = 0; index < value.length; index += 1) {
    const char = value[index];
    if (isEscaped) {
      isEscaped = false;
      continue;
    }

    if (char === '\\') {
      isEscaped = true;
      continue;
    }

    if (char === '=' || char === ':' || /\s/.test(char)) {
      return index;
    }
  }

  return -1;
}

function unescapeJavaProperties(value: string): string {
  return value
    .replace(/\\n/g, '\n')
    .replace(/\\r/g, '\r')
    .replace(/\\t/g, '\t')
    .replace(/\\f/g, '\f')
    .replace(/\\:/g, ':')
    .replace(/\\=/g, '=')
    .replace(/\\ /g, ' ')
    .replace(/\\\\/g, '\\');
}