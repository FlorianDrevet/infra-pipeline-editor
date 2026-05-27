import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';

import { BicepFilePanelComponent, type BicepTreeNode } from './bicep-file-panel.component';
import { BicepViewerTheme, UserPreferencesService } from '../../services/user-preferences.service';

describe('BicepFilePanelComponent', () => {
  let fixture: ComponentFixture<BicepFilePanelComponent>;
  let loadFileSpy: jasmine.Spy<(uri: string) => Promise<string>>;
  let scrolledElements: Element[];
  let userPreferencesServiceStub: UserPreferencesServiceStub;

  const nodes: BicepTreeNode[] = [
    {
      kind: 'folder',
      key: 'Common',
      name: 'Common/',
      folderIcon: 'folder_shared',
      depth: 0,
    },
    {
      kind: 'file',
      path: 'main.bicep',
      displayName: 'main.bicep',
      type: 'entry-point',
      uri: 'https://example.test/main.bicep',
      depth: 0,
      parentFolderKey: '',
    },
    {
      kind: 'file',
      path: 'Common/types.bicep',
      displayName: 'types.bicep',
      type: 'types',
      uri: 'https://example.test/Common/types.bicep',
      depth: 1,
      parentFolderKey: 'Common',
    },
  ];

  beforeEach(async () => {
    scrolledElements = [];
    spyOn(Element.prototype, 'scrollIntoView').and.callFake(function(this: Element): void {
      scrolledElements.push(this);
    });

    loadFileSpy = jasmine.createSpy<(uri: string) => Promise<string>>('loadFile').and.callFake(
      async (uri: string): Promise<string> => `content for ${uri}`,
    );

    userPreferencesServiceStub = new UserPreferencesServiceStub();

    await TestBed.configureTestingModule({
      imports: [BicepFilePanelComponent, TranslateModule.forRoot()],
      providers: [
        {
          provide: UserPreferencesService,
          useValue: userPreferencesServiceStub,
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(BicepFilePanelComponent);
    fixture.componentRef.setInput('nodes', nodes);
    fixture.componentRef.setInput('barTitle', 'generator');
    fixture.componentRef.setInput('terminalDoneText', 'done');
    fixture.componentRef.setInput('fileLoadingText', 'loading');
    fixture.componentRef.setInput('fileErrorText', 'error');
    fixture.componentRef.setInput('loadFile', loadFileSpy);

    await flushComponent();
  });

  it('opens a file, loads its content, and scrolls the viewer into view', async () => {
    await clickFile('Common/types.bicep');

    const viewerElement = getViewerElement();

    expect(loadFileSpy).toHaveBeenCalledOnceWith('https://example.test/Common/types.bicep');
    expect(viewerElement).not.toBeNull();
    if (!viewerElement) {
      throw new Error('Expected the Bicep viewer to be rendered.');
    }

    expect(scrolledElements).toContain(viewerElement);
  });

  it('scrolls the active file entry into view from the viewer back button', async () => {
    await clickFile('Common/types.bicep');

    const activeFileButton = getFileButton('Common/types.bicep');
    scrolledElements = [];

    getBackButton().click();
    await flushComponent();

    expect(scrolledElements).toContain(activeFileButton);
  });

  it('re-expands the active file folder before scrolling back to the current file entry', async () => {
    await clickFile('Common/types.bicep');

    getFolderButton('Common/').click();
    await flushComponent();
    expect(queryFileButton('Common/types.bicep')).toBeNull();

    scrolledElements = [];

    getBackButton().click();
    await flushComponent();

    const activeFileButton = getFileButton('Common/types.bicep');

    expect(activeFileButton).not.toBeNull();
    expect(scrolledElements).toContain(activeFileButton);
  });

  it('closes the viewer when the active file is clicked again without reloading it', async () => {
    await clickFile('main.bicep');

    loadFileSpy.calls.reset();
    scrolledElements = [];

    await clickFile('main.bicep');

    expect(queryViewerElement()).toBeNull();
    expect(loadFileSpy).not.toHaveBeenCalled();
  });

  it('keeps the last selected file when another file is clicked before the previous load finishes', async () => {
    const deferredLoads = new Map<string, (content: string) => void>();
    loadFileSpy.and.callFake((uri: string) => new Promise<string>((resolve) => {
      deferredLoads.set(uri, resolve);
    }));

    getFileButton('main.bicep').click();
    await flushPendingComponent();

    getFileButton('Common/types.bicep').click();
    await flushPendingComponent();

    expect(loadFileSpy.calls.allArgs()).toEqual([
      ['https://example.test/main.bicep'],
      ['https://example.test/Common/types.bicep'],
    ]);

    deferredLoads.get('https://example.test/Common/types.bicep')?.('types content');
    await flushPendingComponent();

    expect(getViewerElement()?.textContent).toContain('types content');

    deferredLoads.get('https://example.test/main.bicep')?.('main content');
    await flushPendingComponent();

    expect(getViewerElement()?.textContent).toContain('types content');
    expect(getViewerElement()?.textContent).not.toContain('main content');
  });

  it('reflects the selected Bicep viewer theme in the rendered viewer state', async () => {
    await clickFile('main.bicep');

    expect(getViewerElement()?.dataset['theme']).toBe('graphite-frost');

    userPreferencesServiceStub.setBicepViewerTheme('sand-dusk');
    await flushComponent();

    expect(getViewerElement()?.dataset['theme']).toBe('sand-dusk');
  });

  it('does not render the workspace bar in embedded mode', async () => {
    fixture.componentRef.setInput('embedded', true);
    await flushComponent();

    expect(queryWorkspaceBarElement()).toBeNull();
  });

  it('marks the embedded workspace body for inset layout', async () => {
    fixture.componentRef.setInput('embedded', true);
    await flushComponent();

    expect(getWorkspaceBodyElement()?.classList.contains('bicep-workspace__body--embedded')).toBeTrue();
  });

  it('renders workspace and viewer in embedded mode when the input is enabled', async () => {
    fixture.componentRef.setInput('embedded', true);
    await flushComponent();

    expect(getWorkspaceElement()?.classList.contains('bicep-workspace--embedded')).toBeTrue();

    await clickFile('main.bicep');

    expect(getViewerElement()?.classList.contains('bicep-viewer--embedded')).toBeTrue();
  });

  async function clickFile(path: string): Promise<void> {
    getFileButton(path).click();
    await flushComponent();
  }

  async function flushComponent(): Promise<void> {
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
  }

  async function flushPendingComponent(): Promise<void> {
    fixture.detectChanges();
    await Promise.resolve();
    fixture.detectChanges();
  }

  function getViewerElement(): HTMLElement | null {
    return fixture.nativeElement.querySelector('.bicep-viewer') as HTMLElement | null;
  }

  function getWorkspaceElement(): HTMLElement | null {
    return fixture.nativeElement.querySelector('.bicep-workspace') as HTMLElement | null;
  }

  function getWorkspaceBodyElement(): HTMLElement | null {
    return fixture.nativeElement.querySelector('.bicep-workspace__body') as HTMLElement | null;
  }

  function queryViewerElement(): HTMLElement | null {
    return fixture.nativeElement.querySelector('.bicep-viewer') as HTMLElement | null;
  }

  function queryWorkspaceBarElement(): HTMLElement | null {
    return fixture.nativeElement.querySelector('.bicep-workspace__bar') as HTMLElement | null;
  }

  function getBackButton(): HTMLButtonElement {
    return fixture.nativeElement.querySelector('.bicep-viewer__jump-back') as HTMLButtonElement;
  }

  function getFolderButton(name: string): HTMLButtonElement {
    const folderButton = Array.from(fixture.nativeElement.querySelectorAll('.bicep-tree__folder'))
      .find((button): button is HTMLButtonElement => button instanceof HTMLButtonElement && !!button.textContent?.includes(name));

    if (!folderButton) {
      throw new Error(`Expected folder button for ${name}.`);
    }

    return folderButton;
  }

  function getFileButton(path: string): HTMLButtonElement {
    return fixture.nativeElement.querySelector(`[data-file-path="${path}"]`) as HTMLButtonElement;
  }

  function queryFileButton(path: string): HTMLButtonElement | null {
    return fixture.nativeElement.querySelector(`[data-file-path="${path}"]`) as HTMLButtonElement | null;
  }
});

class UserPreferencesServiceStub {
  private readonly selectedTheme = signal<BicepViewerTheme>('graphite-frost');

  readonly bicepViewerTheme = this.selectedTheme.asReadonly();

  setBicepViewerTheme(theme: BicepViewerTheme): void {
    this.selectedTheme.set(theme);
  }
}