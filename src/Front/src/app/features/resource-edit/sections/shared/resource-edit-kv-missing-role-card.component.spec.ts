import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';

import { ResourceEditKvMissingRoleCardComponent } from './resource-edit-kv-missing-role-card.component';
import { ResourceEditKvMissingRoleEntry } from './resource-edit-kv-missing-role-entry.interface';

describe('ResourceEditKvMissingRoleCardComponent', () => {
  let fixture: ComponentFixture<ResourceEditKvMissingRoleCardComponent>;
  let component: ResourceEditKvMissingRoleCardComponent;

  const mockEntry: ResourceEditKvMissingRoleEntry = {
    keyVaultResourceId: 'kv-1',
    keyVaultName: 'my-kv',
    missingRoleName: 'Key Vault Secrets User',
    missingRoleDefinitionId: 'role-def-1',
    affectedSettingsCount: 2,
    checking: false,
    assigning: false,
    selectedIdentityType: 'SystemAssigned',
    selectedUaiId: null,
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ResourceEditKvMissingRoleCardComponent, TranslateModule.forRoot()],
    }).compileComponents();

    fixture = TestBed.createComponent(ResourceEditKvMissingRoleCardComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('canWrite', true);
    fixture.componentRef.setInput('entry', mockEntry);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
