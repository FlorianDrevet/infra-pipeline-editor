import { CorsRuleEntry } from '../../../shared/interfaces/storage-account.interface';

const CORS_HEADER_SPECIAL_CHARACTERS = "!#$%&'*+.^_`|~-";

export type CorsServiceKey = 'blob' | 'table';
export type CorsListField = 'allowedOrigins' | 'allowedHeaders' | 'exposedHeaders';
export type CorsMethodField = 'allowedMethods';
export type CorsFieldKey = CorsListField | CorsMethodField | 'maxAgeInSeconds';

export interface StorageCorsValidationResult {
  isValid: boolean;
  errors: Record<string, string>;
}

export function buildCorsErrorKey(service: CorsServiceKey, index: number, field: CorsFieldKey): string {
  return `${service}:${index}:${field}`;
}

export function validateStorageCorsRules(
  blobRules: ReadonlyArray<CorsRuleEntry>,
  tableRules: ReadonlyArray<CorsRuleEntry>,
): StorageCorsValidationResult {
  const errors: Record<string, string> = {};

  for (const [service, rules] of [
    ['blob', blobRules],
    ['table', tableRules],
  ] as const) {
    rules.forEach((rule, index) => {
      if (rule.allowedOrigins.length === 0) {
        setCorsFieldError(errors, service, index, 'allowedOrigins', 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_ORIGIN_REQUIRED');
      }

      if (rule.allowedMethods.length === 0) {
        setCorsFieldError(errors, service, index, 'allowedMethods', 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_METHOD_REQUIRED');
      }

      if (!Number.isInteger(rule.maxAgeInSeconds) || rule.maxAgeInSeconds < 0) {
        setCorsFieldError(errors, service, index, 'maxAgeInSeconds', 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_MAX_AGE');
      }

      for (const origin of rule.allowedOrigins) {
        const validationError = validateCorsOrigin(origin);
        if (validationError) {
          setCorsFieldError(errors, service, index, 'allowedOrigins', validationError);
          break;
        }
      }

      for (const field of ['allowedHeaders', 'exposedHeaders'] as const) {
        for (const header of rule[field]) {
          const validationError = validateCorsHeader(header);
          if (validationError) {
            setCorsFieldError(errors, service, index, field, validationError);
            break;
          }
        }
      }
    });
  }

  return {
    isValid: Object.keys(errors).length === 0,
    errors,
  };
}

export function validateCorsOrigin(value: string): string {
  const normalized = value.trim();
  if (!normalized) {
    return 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_EMPTY_VALUE';
  }

  if (normalized.length > 256) {
    return 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_TOO_LONG';
  }

  if (normalized === '*') {
    return '';
  }

  const withoutTrailingSlash = trimTrailingSlashes(normalized);
  if (withoutTrailingSlash.includes('*.')) {
    return isValidWildcardCorsOrigin(withoutTrailingSlash)
      ? ''
      : 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_INVALID_ORIGIN';
  }

  try {
    const url = new URL(withoutTrailingSlash);
    const isHttp = url.protocol === 'http:' || url.protocol === 'https:';
    const hasNoPath = url.pathname === '' || url.pathname === '/';
    const hasNoSearch = !url.search;
    const hasNoHash = !url.hash;

    return isHttp && hasNoPath && hasNoSearch && hasNoHash
      ? ''
      : 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_INVALID_ORIGIN';
  } catch {
    return 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_INVALID_ORIGIN';
  }
}

export function normalizeCorsOrigin(value: string): string {
  const normalized = trimTrailingSlashes(value.trim());
  if (normalized === '*' || normalized.includes('*.')) {
    return normalized.toLowerCase();
  }

  try {
    const url = new URL(normalized);
    return `${url.protocol}//${url.host}`.toLowerCase();
  } catch {
    return normalized;
  }
}

export function validateCorsHeader(value: string): string {
  const normalized = value.trim();
  if (!normalized) {
    return 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_EMPTY_VALUE';
  }

  if (normalized.length > 256) {
    return 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_TOO_LONG';
  }

  if (!isValidCorsHeaderValue(normalized)) {
    return 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_INVALID_HEADER';
  }

  return '';
}

export function normalizeCorsHeader(value: string): string {
  return value.trim().toLowerCase();
}

function setCorsFieldError(
  errors: Record<string, string>,
  service: CorsServiceKey,
  index: number,
  field: CorsFieldKey,
  errorKey: string,
): void {
  errors[buildCorsErrorKey(service, index, field)] = errorKey;
}

function trimTrailingSlashes(value: string): string {
  let endIndex = value.length;
  while (endIndex > 0 && value[endIndex - 1] === '/') {
    endIndex--;
  }

  return endIndex === value.length
    ? value
    : value.slice(0, endIndex);
}

function isValidWildcardCorsOrigin(value: string): boolean {
  const schemeSeparatorIndex = value.indexOf('://');
  if (schemeSeparatorIndex <= 0) {
    return false;
  }

  const protocol = value.slice(0, schemeSeparatorIndex).toLowerCase();
  if (protocol !== 'http' && protocol !== 'https') {
    return false;
  }

  const hostAndPort = value.slice(schemeSeparatorIndex + 3);
  if (!hostAndPort.startsWith('*.') || hostAndPort.includes('/') || hostAndPort.includes('?') || hostAndPort.includes('#')) {
    return false;
  }

  const wildcardHostAndPort = hostAndPort.slice(2);
  if (!wildcardHostAndPort || wildcardHostAndPort.includes('*')) {
    return false;
  }

  const hostAndPortParts = splitCorsHostAndPort(wildcardHostAndPort);
  if (!hostAndPortParts) {
    return false;
  }

  return isValidCorsDomain(hostAndPortParts.host)
    && (hostAndPortParts.port === null || isValidCorsPort(hostAndPortParts.port));
}

function splitCorsHostAndPort(value: string): { host: string; port: string | null } | null {
  const firstSeparatorIndex = value.indexOf(':');
  if (firstSeparatorIndex < 0) {
    return { host: value, port: null };
  }

  if (firstSeparatorIndex !== value.lastIndexOf(':')) {
    return null;
  }

  const host = value.slice(0, firstSeparatorIndex);
  const port = value.slice(firstSeparatorIndex + 1);
  if (!host || !port) {
    return null;
  }

  return { host, port };
}

function isValidCorsPort(value: string): boolean {
  if (value.length === 0 || value.length > 5) {
    return false;
  }

  for (const character of value) {
    if (character < '0' || character > '9') {
      return false;
    }
  }

  const portNumber = Number(value);
  return Number.isInteger(portNumber) && portNumber >= 1 && portNumber <= 65535;
}

function isValidCorsDomain(value: string): boolean {
  const labels = value.split('.');
  return labels.length > 0
    && labels.every((label) => label.length > 0 && isValidCorsDomainLabel(label));
}

function isValidCorsDomainLabel(value: string): boolean {
  for (const character of value) {
    if (!isAsciiAlphaNumericCharacter(character) && character !== '-') {
      return false;
    }
  }

  return true;
}

function isValidCorsHeaderValue(value: string): boolean {
  for (const character of value) {
    if (!isValidCorsHeaderCharacter(character)) {
      return false;
    }
  }

  return true;
}

function isValidCorsHeaderCharacter(character: string): boolean {
  return isAsciiAlphaNumericCharacter(character)
    || CORS_HEADER_SPECIAL_CHARACTERS.includes(character);
}

function isAsciiAlphaNumericCharacter(character: string): boolean {
  const codePoint = character.codePointAt(0) ?? 0;
  return (codePoint >= 48 && codePoint <= 57)
    || (codePoint >= 65 && codePoint <= 90)
    || (codePoint >= 97 && codePoint <= 122);
}