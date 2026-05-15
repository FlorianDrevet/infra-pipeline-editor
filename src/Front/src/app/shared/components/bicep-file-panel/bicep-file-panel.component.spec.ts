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
    expect(scrolledElements).toContain(viewerElement!);
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

  it('reflects the selected Bicep viewer theme in the rendered viewer state', async () => {
    await clickFile('main.bicep');

    expect(getViewerElement()?.dataset['theme']).toBe('graphite-frost');

    userPreferencesServiceStub.setBicepViewerTheme('sand-dusk');
    await flushComponent();

    expect(getViewerElement()?.dataset['theme']).toBe('sand-dusk');
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

  function getViewerElement(): HTMLElement | null {
    return fixture.nativeElement.querySelector('.bicep-viewer') as HTMLElement | null;
  }

  function queryViewerElement(): HTMLElement | null {
    return fixture.nativeElement.querySelector('.bicep-viewer') as HTMLElement | null;
  }

  function getBackButton(): HTMLButtonElement {
    return fixture.nativeElement.querySelector('.bicep-viewer__jump-back') as HTMLButtonElement;
  }

  function getFolderButton(name: string): HTMLButtonElement {
    return Array.from(fixture.nativeElement.querySelectorAll('.bicep-tree__folder'))
      .find((button): button is HTMLButtonElement => button instanceof HTMLButtonElement && !!button.textContent?.includes(name))!;
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