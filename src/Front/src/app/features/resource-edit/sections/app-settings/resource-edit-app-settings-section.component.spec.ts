import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';

import { AppSettingResponse } from '../../../../shared/interfaces/app-setting.interface';
import { ResourceEditAppSettingsSectionComponent } from './resource-edit-app-settings-section.component';

describe('ResourceEditAppSettingsSectionComponent', () => {
  let fixture: ComponentFixture<ResourceEditAppSettingsSectionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ResourceEditAppSettingsSectionComponent, TranslateModule.forRoot()],
    }).compileComponents();

    fixture = TestBed.createComponent(ResourceEditAppSettingsSectionComponent);
  });

  it('shows the empty state when there are no app settings', () => {
    fixture.componentRef.setInput('canWrite', true);
    fixture.componentRef.setInput('environments', [{ id: 'env-1', name: 'dev', order: 1 }]);
    fixture.componentRef.setInput('section', createSection());
    fixture.detectChanges();

    expect(getElement('.empty-state')).not.toBeNull();
  });

  it('delegates add, import, edit, and remove actions to the section controller', () => {
    const addSetting = jasmine.createSpy('addSetting');
    const importSettings = jasmine.createSpy('importSettings');
    const editSetting = jasmine.createSpy('editSetting');
    const removeSetting = jasmine.createSpy('removeSetting');
    const setting = createSetting('setting-1', 'API_URL');

    fixture.componentRef.setInput('canWrite', true);
    fixture.componentRef.setInput('environments', [{ id: 'env-1', name: 'dev', order: 1 }]);
    fixture.componentRef.setInput('section', createSection({
      appSettings: signal([setting]),
      groupedSettings: signal({ byVg: [], outputs: [], statics: [setting] }),
      openAddDialog: addSetting,
      openImportDialog: importSettings,
      openEditStaticDialog: editSetting,
      openRemoveDialog: removeSetting,
    }));
    fixture.detectChanges();

    getButton('.ra-add-btn').click();
    getButton('.ra-add-btn--secondary').click();
    getButton('.as-row__edit').click();
    getButton('.as-row__delete').click();

    expect(addSetting).toHaveBeenCalledOnceWith();
    expect(importSettings).toHaveBeenCalledOnceWith();
    expect(editSetting).toHaveBeenCalledOnceWith(setting);
    expect(removeSetting).toHaveBeenCalledOnceWith(setting);
  });

  function getButton(selector: string): HTMLButtonElement {
    return fixture.nativeElement.querySelector(selector) as HTMLButtonElement;
  }

  function getElement(selector: string): HTMLElement | null {
    return fixture.nativeElement.querySelector(selector) as HTMLElement | null;
  }
});

function createSection(overrides: Partial<ResourceEditAppSettingsSectionStub> = {}): ResourceEditAppSettingsSectionStub {
  return {
    appSettings: signal<AppSettingResponse[]>([]),
    isLoading: signal(false),
    errorKey: signal(''),
    kvMissingRoleEntries: signal([]),
    groupedSettings: signal({ byVg: [], outputs: [], statics: [] }),
    openAddDialog: () => undefined,
    openImportDialog: () => undefined,
    openEditStaticDialog: () => undefined,
    openRemoveDialog: () => undefined,
    resolveSourceName: () => '',
    resolveSourceType: () => '',
    assignedUai: () => null,
    uaiOptions: () => [],
    setKvEntryIdentityType: () => undefined,
    setKvEntryUaiId: () => undefined,
    createNewUaiForEntry: () => undefined,
    assignKvRole: async () => undefined,
    ...overrides,
  };
}

function createSetting(id: string, name: string): AppSettingResponse {
  return {
    id,
    resourceId: 'resource-1',
    name,
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
    environmentValues: { dev: 'https://api.contoso.com' },
  };
}

interface ResourceEditAppSettingsSectionStub {
  appSettings: ReturnType<typeof signal<AppSettingResponse[]>>;
  isLoading: ReturnType<typeof signal<boolean>>;
  errorKey: ReturnType<typeof signal<string>>;
  kvMissingRoleEntries: ReturnType<typeof signal<unknown[]>>;
  groupedSettings: ReturnType<typeof signal<{ byVg: unknown[]; outputs: AppSettingResponse[]; statics: AppSettingResponse[] }>>;
  openAddDialog(): void;
  openImportDialog(): void;
  openEditStaticDialog(setting: AppSettingResponse): void;
  openRemoveDialog(setting: AppSettingResponse): void;
  resolveSourceName(sourceResourceId: string): string;
  resolveSourceType(sourceResourceId: string): string;
  assignedUai(): { identityId: string; identityName: string } | null;
  uaiOptions(): unknown[];
  setKvEntryIdentityType(kvId: string, type: 'UserAssigned' | 'SystemAssigned'): void;
  setKvEntryUaiId(kvId: string, uaiId: string | null): void;
  createNewUaiForEntry(kvId: string): void;
  assignKvRole(entry: unknown): Promise<void>;
}