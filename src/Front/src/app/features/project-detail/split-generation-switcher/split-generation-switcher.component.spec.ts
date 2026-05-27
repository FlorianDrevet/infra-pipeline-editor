import { Component, input, output } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';

import {
  DsButtonComponent,
} from '../../../shared/components/ds';
import {
  BicepFilePanelComponent,
} from '../../../shared/components/bicep-file-panel/bicep-file-panel.component';
import {
  GenerateProjectBicepResponse,
} from '../../../shared/interfaces/project.interface';
import { BootstrapSetupGuideComponent } from '../bootstrap-setup-guide/bootstrap-setup-guide.component';
import { SplitGenerationSwitcherComponent } from './split-generation-switcher.component';

@Component({
  selector: 'app-ds-button',
  standalone: true,
  template: '<button type="button" [disabled]="disabled()" (click)="clicked.emit()"><ng-content /></button>',
})
class DsButtonStubComponent {
  readonly variant = input('primary');
  readonly icon = input('');
  readonly disabled = input(false);
  readonly loading = input(false);
  readonly clicked = output<void>();
}



@Component({
  selector: 'app-bicep-file-panel',
  standalone: true,
  template: '<div class="bicep-file-panel-stub" [attr.data-embedded]="embedded()">{{ barTitle() }}</div>',
})
class BicepFilePanelStubComponent {
  readonly nodes = input<readonly unknown[]>([]);
  readonly barTitle = input('');
  readonly terminalDoneText = input('');
  readonly fileLoadingText = input('');
  readonly fileErrorText = input('');
  readonly loadFile = input<(filePath: string) => Promise<string>>(() => Promise.resolve(''));
  readonly embedded = input(false);
}

@Component({
  selector: 'app-bootstrap-setup-guide',
  standalone: true,
  template: '<div class="bootstrap-setup-guide-stub"></div>',
})
class BootstrapSetupGuideStubComponent {
}

describe('SplitGenerationSwitcherComponent', () => {
  let fixture: ComponentFixture<SplitGenerationSwitcherComponent>;

  beforeEach(async () => {
    TestBed.overrideComponent(SplitGenerationSwitcherComponent, {
      remove: {
        imports: [
          DsButtonComponent,
          BicepFilePanelComponent,
          BootstrapSetupGuideComponent,
        ],
      },
      add: {
        imports: [
          DsButtonStubComponent,
          BicepFilePanelStubComponent,
          BootstrapSetupGuideStubComponent,
        ],
      },
    });

    await TestBed.configureTestingModule({
      imports: [
        SplitGenerationSwitcherComponent,
        TranslateModule.forRoot(),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SplitGenerationSwitcherComponent);
  });

  it('keeps split infra results hidden while batch reveal is deferred', async () => {
    fixture.componentRef.setInput('deferBatchReveal', true);
    fixture.componentRef.setInput('bicepResult', createBicepResult());

    await detectChanges();

    expect(queryBySelector('.split-switcher__loading')).not.toBeNull();
    expect(queryBySelector('app-bicep-file-panel')).toBeNull();
    expect(getChipText('.split-switcher__chip--infra')).toBe('0');
  });

  it('reveals split infra results once batch reveal is released', async () => {
    fixture.componentRef.setInput('bicepResult', createBicepResult());

    await detectChanges();

    expect(queryBySelector('.split-switcher__loading')).toBeNull();
    expect(queryBySelector('app-bicep-file-panel')).not.toBeNull();
    expect(getChipText('.split-switcher__chip--infra')).toBe('1');
  });

  it('renders every split file panel inside a full-bleed embedded host', async () => {
    fixture.componentRef.setInput('bicepResult', createBicepResult());
    fixture.componentRef.setInput('pipelineResult', createPipelineResult());
    fixture.componentRef.setInput('bootstrapResult', createBootstrapResult());

    await detectChanges();

    assertEmbeddedPanelRenderedInFullBleedHost();

    setInfraInnerTab('pipeline');
    await detectChanges();
    assertEmbeddedPanelRenderedInFullBleedHost();

    setInfraInnerTab('bootstrap');
    await detectChanges();
    assertEmbeddedPanelRenderedInFullBleedHost();

    setOuterTab('code');
    await detectChanges();
    assertEmbeddedPanelRenderedInFullBleedHost();

    setCodeInnerTab('bootstrap');
    await detectChanges();
    assertEmbeddedPanelRenderedInFullBleedHost();
  });

  it('keeps split generation errors hidden until the batch reveal settles', async () => {
    fixture.componentRef.setInput('deferBatchReveal', true);
    fixture.componentRef.setInput('bicepGenerationError', 'PROJECT_DETAIL.BICEP.ERROR');

    await detectChanges();

    expect(queryBySelector('.split-switcher__error')).toBeNull();
    expect(queryBySelector('.split-switcher__loading')).not.toBeNull();

    fixture.componentRef.setInput('deferBatchReveal', false);
    await detectChanges();

    expect(getText('.split-switcher__error')).toContain('PROJECT_DETAIL.BICEP.ERROR');
  });

  async function detectChanges(): Promise<void> {
    fixture.detectChanges();
    await fixture.whenStable();
    await fixture.whenRenderingDone();
    fixture.detectChanges();
  }

  function queryBySelector(selector: string): HTMLElement | null {
    return (fixture.nativeElement as HTMLElement).querySelector(selector);
  }

  function getChipText(selector: string): string {
    return queryBySelector(selector)?.textContent?.trim() ?? '';
  }

  function getText(selector: string): string {
    return queryBySelector(selector)?.textContent ?? '';
  }

  function assertEmbeddedPanelRenderedInFullBleedHost(): void {
    const panelElement = queryBySelector('app-bicep-file-panel');

    expect(panelElement).withContext('Expected a rendered split file panel').not.toBeNull();
    expect(panelElement?.parentElement?.classList.contains('split-switcher__embedded-panel'))
      .withContext('Expected the split file panel to sit in a full-bleed host wrapper')
      .toBeTrue();

    const panelStubElement = panelElement?.querySelector('.bicep-file-panel-stub') as HTMLElement | null;
    expect(panelStubElement?.dataset['embedded']).toBe('true');
  }

  function setOuterTab(tabId: 'infra' | 'code'): void {
    (fixture.componentInstance as SplitGenerationSwitcherComponentTestApi).onOuterTabChange(tabId);
  }

  function setInfraInnerTab(tabId: 'bicep' | 'pipeline' | 'bootstrap'): void {
    (fixture.componentInstance as SplitGenerationSwitcherComponentTestApi).onInfraInnerTabChange(tabId);
  }

  function setCodeInnerTab(tabId: 'pipeline' | 'bootstrap'): void {
    (fixture.componentInstance as SplitGenerationSwitcherComponentTestApi).onCodeInnerTabChange(tabId);
  }
});

type SplitGenerationSwitcherComponentTestApi = SplitGenerationSwitcherComponent & {
  onOuterTabChange(tabId: 'infra' | 'code'): void;
  onInfraInnerTabChange(tabId: 'bicep' | 'pipeline' | 'bootstrap'): void;
  onCodeInnerTabChange(tabId: 'pipeline' | 'bootstrap'): void;
};

function createBicepResult(): GenerateProjectBicepResponse {
  return {
    commonFileUris: {
      'Common/main.bicep': 'artifact://Common/main.bicep',
    },
    configFileUris: {},
  };
}

function createPipelineResult() {
  return {
    infraCommonFileUris: {
      'infra/pipeline.yml': 'artifact://infra/pipeline.yml',
    },
    infraConfigFileUris: {},
    appCommonFileUris: {
      'app/pipeline.yml': 'artifact://app/pipeline.yml',
    },
    appConfigFileUris: {},
  };
}

function createBootstrapResult() {
  return {
    infraFileUris: {
      '.ado/bootstrap-infra.yml': 'artifact://.ado/bootstrap-infra.yml',
    },
    appFileUris: {
      '.ado/bootstrap-app.yml': 'artifact://.ado/bootstrap-app.yml',
    },
  };
}