import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';

import { ConfigDetailGitSectionComponent } from './config-detail-git-section.component';

describe('ConfigDetailGitSectionComponent', () => {
  let fixture: ComponentFixture<ConfigDetailGitSectionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConfigDetailGitSectionComponent, TranslateModule.forRoot()],
    }).compileComponents();

    fixture = TestBed.createComponent(ConfigDetailGitSectionComponent);
  });

  it('opens the matching split slot when the slot is empty', () => {
    const onOpenSlot = jasmine.createSpy('onOpenSlot');

    fixture.componentRef.setInput('viewModel', createViewModel({
      layoutMode: 'SplitInfraCode',
      splitSlots: [
        { kind: 'Infrastructure', labelKey: 'CONFIG_DETAIL.REPOSITORIES.SLOT_INFRASTRUCTURE', repo: null },
        { kind: 'ApplicationCode', labelKey: 'CONFIG_DETAIL.REPOSITORIES.SLOT_APPLICATION_CODE', repo: createRepository('app-repo', ['ApplicationCode']) },
      ],
      onOpenSlot,
    }));
    fixture.detectChanges();

    getButton('.config-detail-git__slot-empty').click();

    expect(onOpenSlot).toHaveBeenCalledOnceWith('Infrastructure', null);
  });

  it('delegates layout and repository removal actions to the parent callbacks', () => {
    const onSetLayoutMode = jasmine.createSpy('onSetLayoutMode');
    const onRemoveRepository = jasmine.createSpy('onRemoveRepository');
    const repo = createRepository('infra-repo', ['Infrastructure']);

    fixture.componentRef.setInput('viewModel', createViewModel({
      layoutMode: 'AllInOne',
      allInOneRepo: repo,
      onSetLayoutMode,
      onRemoveRepository,
    }));
    fixture.detectChanges();

    getButton('.config-detail-git__mode-card--split').click();
    getButton('.config-detail-git__remove-repo').click();

    expect(onSetLayoutMode).toHaveBeenCalledOnceWith('SplitInfraCode');
    expect(onRemoveRepository).toHaveBeenCalledOnceWith(repo);
  });

  function getButton(selector: string): HTMLButtonElement {
    return fixture.nativeElement.querySelector(selector) as HTMLButtonElement;
  }
});

function createViewModel(overrides: Partial<Record<string, unknown>> = {}): Record<string, unknown> {
  return {
    gitActionError: '',
    layoutMode: 'SplitInfraCode',
    layoutModeSaving: false,
    canWrite: true,
    actionRepoId: null,
    allInOneRepo: null,
    splitSlots: [
      { kind: 'Infrastructure', labelKey: 'CONFIG_DETAIL.REPOSITORIES.SLOT_INFRASTRUCTURE', repo: null },
      { kind: 'ApplicationCode', labelKey: 'CONFIG_DETAIL.REPOSITORIES.SLOT_APPLICATION_CODE', repo: null },
    ],
    onSetLayoutMode: () => undefined,
    onOpenAllInOne: () => undefined,
    onOpenSlot: () => undefined,
    onRemoveRepository: () => undefined,
    ...overrides,
  };
}

function createRepository(alias: string, contentKinds: string[]): Record<string, unknown> {
  return {
    id: `${alias}-id`,
    alias,
    providerType: 'GitHub',
    repositoryUrl: `https://github.com/example/${alias}`,
    owner: 'example',
    repositoryName: alias,
    defaultBranch: 'main',
    contentKinds,
  };
}