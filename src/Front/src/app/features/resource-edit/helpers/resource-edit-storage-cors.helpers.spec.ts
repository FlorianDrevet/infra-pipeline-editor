import { CorsRuleEntry } from '../../../shared/interfaces/storage-account.interface';

import {
  buildCorsErrorKey,
  normalizeCorsHeader,
  normalizeCorsOrigin,
  validateCorsOrigin,
  validateStorageCorsRules,
} from './resource-edit-storage-cors.helpers';

describe('resource edit storage cors helpers', () => {
  it('normalizes origins and headers to the persisted Azure shape', () => {
    expect(normalizeCorsOrigin('https://Api.Example.com///')).toBe('https://api.example.com');
    expect(normalizeCorsOrigin('https://*.Example.com:443/')).toBe('https://*.example.com:443');
    expect(normalizeCorsHeader(' X-MS-Meta-Test ')).toBe('x-ms-meta-test');
  });

  it('accepts wildcard subdomains on valid http and https origins', () => {
    expect(validateCorsOrigin('https://*.example.com')).toBe('');
    expect(validateCorsOrigin('http://*.example.com:8080')).toBe('');
  });

  it('collects field-specific errors when validating invalid rule sets', () => {
    const result = validateStorageCorsRules([
      createCorsRule({
        allowedOrigins: ['https://api.example.com/path'],
        allowedMethods: [],
        allowedHeaders: ['header with spaces'],
        maxAgeInSeconds: -1,
      }),
    ], []);

    expect(result.isValid).toBeFalse();
    expect(result.errors).toEqual({
      [buildCorsErrorKey('blob', 0, 'allowedOrigins')]: 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_INVALID_ORIGIN',
      [buildCorsErrorKey('blob', 0, 'allowedMethods')]: 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_METHOD_REQUIRED',
      [buildCorsErrorKey('blob', 0, 'allowedHeaders')]: 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_INVALID_HEADER',
      [buildCorsErrorKey('blob', 0, 'maxAgeInSeconds')]: 'RESOURCE_EDIT.STORAGE_SERVICES.CORS_COMMON.ERROR_MAX_AGE',
    });
  });

  function createCorsRule(overrides: Partial<CorsRuleEntry> = {}): CorsRuleEntry {
    return {
      allowedOrigins: ['https://api.example.com'],
      allowedMethods: ['GET'],
      allowedHeaders: ['x-ms-meta-*'],
      exposedHeaders: ['etag'],
      maxAgeInSeconds: 3600,
      ...overrides,
    };
  }
});