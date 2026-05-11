import {
  parseImportAppSettingsContent,
  resolveImportAppSettingsMode,
  type ImportAppSettingsContext,
} from './import-app-settings-parser';

describe('import app settings parser', () => {
  function createContext(overrides: Partial<ImportAppSettingsContext> = {}): ImportAppSettingsContext {
    return {
      resourceType: 'WebApp',
      deploymentMode: 'Code',
      runtimeStack: 'DotNet',
      ...overrides,
    };
  }

  it('defaults to dotnet json for dotnet web apps', () => {
    const mode = resolveImportAppSettingsMode(createContext({ runtimeStack: 'DotNet' }));

    expect(mode).toBe('dotnetJson');
  });

  it('defaults to env files for python web apps', () => {
    const mode = resolveImportAppSettingsMode(createContext({ runtimeStack: 'Python' }));

    expect(mode).toBe('env');
  });

  it('defaults to java properties for java web apps', () => {
    const mode = resolveImportAppSettingsMode(createContext({ runtimeStack: 'Java' }));

    expect(mode).toBe('javaProperties');
  });

  it('defaults to functions json for function apps regardless of runtime', () => {
    const mode = resolveImportAppSettingsMode(createContext({ resourceType: 'FunctionApp', runtimeStack: 'Python' }));

    expect(mode).toBe('functionAppJson');
  });

  it('defaults to env files for container deployments', () => {
    const mode = resolveImportAppSettingsMode(createContext({ deploymentMode: 'Container', runtimeStack: 'DotNet' }));

    expect(mode).toBe('env');
  });

  it('parses nested dotnet json with double underscore keys', () => {
    const entries = parseImportAppSettingsContent('{"ConnectionStrings":{"Default":"Server=db"}}', 'dotnetJson');

    expect(entries).toEqual([
      { key: 'ConnectionStrings__Default', value: 'Server=db' },
    ]);
  });

  it('parses function local settings from the Values node only', () => {
    const entries = parseImportAppSettingsContent(
      '{"IsEncrypted":false,"Values":{"AzureWebJobsStorage":"UseDevelopmentStorage=true","MySetting":"42"}}',
      'functionAppJson'
    );

    expect(entries).toEqual([
      { key: 'AzureWebJobsStorage', value: 'UseDevelopmentStorage=true' },
      { key: 'MySetting', value: '42' },
    ]);
  });

  it('parses env files', () => {
    const entries = parseImportAppSettingsContent('# comment\nexport API_URL=https://api.test\nDEBUG="1"', 'env');

    expect(entries).toEqual([
      { key: 'API_URL', value: 'https://api.test' },
      { key: 'DEBUG', value: '1' },
    ]);
  });

  it('parses java properties files', () => {
    const entries = parseImportAppSettingsContent('server.port=8080\nspring.datasource.url=jdbc:sqlserver://db', 'javaProperties');

    expect(entries).toEqual([
      { key: 'server.port', value: '8080' },
      { key: 'spring.datasource.url', value: 'jdbc:sqlserver://db' },
    ]);
  });

  it('parses java yaml files with dot-separated keys', () => {
    const entries = parseImportAppSettingsContent('server:\n  port: 8080\nspring:\n  datasource:\n    url: jdbc:sqlserver://db', 'javaYaml');

    expect(entries).toEqual([
      { key: 'server.port', value: '8080' },
      { key: 'spring.datasource.url', value: 'jdbc:sqlserver://db' },
    ]);
  });
});