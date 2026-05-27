import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';

import { AppConfigurationKeyResponse } from '../../models/app-configuration-key.interface';
import { ResourceEditConfigKeysSectionComponent } from './resource-edit-config-keys-section.component';

describe('ResourceEditConfigKeysSectionComponent', () => {
  let fixture: ComponentFixture<ResourceEditConfigKeysSectionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ResourceEditConfigKeysSectionComponent, TranslateModule.forRoot()],
    }).compileComponents();

    fixture = TestBed.createComponent(ResourceEditConfigKeysSectionComponent);
  });

  it('shows the empty state when there are no configuration keys', () => {
    fixture.componentRef.setInput('canWrite', true);
    fixture.componentRef.setInput('environments', [{ id: 'env-1', name: 'dev', order: 1 }]);
    fixture.componentRef.setInput('section', createSection());
    fixture.detectChanges();

    expect(getElement('.empty-state')).not.toBeNull();
  });

  it('delegates add and remove actions to the section controller', () => {
    const addConfigKey = jasmine.createSpy('addConfigKey');
    const removeConfigKey = jasmine.createSpy('removeConfigKey');
    const configKey = createConfigKey('config-key-1', 'Feature:Toggle');

    fixture.componentRef.setInput('canWrite', true);
    fixture.componentRef.setInput('environments', [{ id: 'env-1', name: 'dev', order: 1 }]);
    fixture.componentRef.setInput('section', createSection({
      configKeys: signal([configKey]),
      groupedKeys: signal({ byVg: [], outputs: [], statics: [configKey] }),
      openAddDialog: addConfigKey,
      openRemoveDialog: removeConfigKey,
    }));
    fixture.detectChanges();

    getButton('.ra-actions app-ds-button button').click();
    getButton('app-ds-icon-button[icon="close"] button').click();

    expect(addConfigKey).toHaveBeenCalledOnceWith();
    expect(removeConfigKey).toHaveBeenCalledOnceWith(configKey);
  });

  function getButton(selector: string): HTMLButtonElement {
    return fixture.nativeElement.querySelector(selector) as HTMLButtonElement;
  }

  function getElement(selector: string): HTMLElement | null {
    return fixture.nativeElement.querySelector(selector) as HTMLElement | null;
  }
});

function createSection(overrides: Partial<ResourceEditConfigKeysSectionStub> = {}): ResourceEditConfigKeysSectionStub {
  return {
    configKeys: signal<AppConfigurationKeyResponse[]>([]),
    isLoading: signal(false),
    errorKey: signal(''),
    kvMissingRoleEntries: signal([]),
    groupedKeys: signal({ byVg: [], outputs: [], statics: [] }),
    openAddDialog: () => undefined,
    openRemoveDialog: () => undefined,
    resolveSourceName: () => '',
    resolveSourceType: () => '',
    assignedUai: () => null,
    uaiOptions: () => [],
    getConfigKeyType: () => 'RESOURCE_EDIT.CONFIG_KEYS.TYPE_STATIC',
    setKvEntryIdentityType: () => undefined,
    setKvEntryUaiId: () => undefined,
    createNewUaiForEntry: () => undefined,
    assignKvRole: async () => undefined,
    ...overrides,
  };
}

function createConfigKey(id: string, key: string): AppConfigurationKeyResponse {
  return {
    id,
    appConfigurationId: 'app-configuration-1',
    key,
    label: null,
    isOutputReference: false,
    isKeyVaultReference: false,
    secretValueAssignment: null,
    isViaVariableGroup: false,
    variableGroupId: null,
    variableGroupName: null,
    pipelineVariableName: null,
    sourceResourceId: null,
    sourceOutputName: null,
    keyVaultResourceId: null,
    secretName: null,
    hasKeyVaultAccess: null,
    environmentValues: { dev: 'true' },
  };
}

interface ResourceEditConfigKeysSectionStub {
  configKeys: ReturnType<typeof signal<AppConfigurationKeyResponse[]>>;
  isLoading: ReturnType<typeof signal<boolean>>;
  errorKey: ReturnType<typeof signal<string>>;
  kvMissingRoleEntries: ReturnType<typeof signal<unknown[]>>;
  groupedKeys: ReturnType<typeof signal<{ byVg: unknown[]; outputs: AppConfigurationKeyResponse[]; statics: AppConfigurationKeyResponse[] }>>;
  openAddDialog(): void;
  openRemoveDialog(configKey: AppConfigurationKeyResponse): void;
  resolveSourceName(sourceResourceId: string): string;
  resolveSourceType(sourceResourceId: string): string;
  assignedUai(): { identityId: string; identityName: string } | null;
  uaiOptions(): unknown[];
  getConfigKeyType(configKey: AppConfigurationKeyResponse): string;
  setKvEntryIdentityType(kvId: string, type: 'UserAssigned' | 'SystemAssigned'): void;
  setKvEntryUaiId(kvId: string, uaiId: string | null): void;
  createNewUaiForEntry(kvId: string): void;
  assignKvRole(entry: unknown): Promise<void>;
}