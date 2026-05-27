import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { BicepFilePanelComponent } from '../../../../shared/components/bicep-file-panel/bicep-file-panel.component';
import { DsButtonComponent, DsPanelActionButtonComponent } from '../../../../shared/components/ds';
import { DsTabsComponent } from '../../../../shared/components/ds/ds-tabs/ds-tabs.component';
import { DsTabDefinition } from '../../../../shared/components/ds/ds-tabs/ds-tabs.types';
import { LanguageService } from '../../../../shared/services/language.service';
import { ConfigDetailGenerationSectionViewModel } from './config-detail-generation-section.view-model';

const GENERATION_TAB_BICEP = 'bicep' as const;
const GENERATION_TAB_PIPELINE = 'pipeline' as const;

@Component({
  selector: 'app-config-detail-generation-section',
  standalone: true,
  imports: [
    BicepFilePanelComponent,
    DsButtonComponent,
    DsPanelActionButtonComponent,
    DsTabsComponent,
    MatIconModule,
    TranslateModule,
  ],
  templateUrl: './config-detail-generation-section.component.html',
  styleUrl: './config-detail-generation-section.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfigDetailGenerationSectionComponent {
  private readonly translate = inject(TranslateService);
  private readonly languageService = inject(LanguageService);

  public readonly viewModel = input.required<ConfigDetailGenerationSectionViewModel>();

  protected readonly TAB_BICEP = GENERATION_TAB_BICEP;
  protected readonly TAB_PIPELINE = GENERATION_TAB_PIPELINE;

  protected readonly activeGenerationTabId = signal<string | null>(GENERATION_TAB_BICEP);
  protected readonly generationTabs = computed<readonly DsTabDefinition[]>(() => {
    this.languageService.currentLanguage();
    return [
      { id: GENERATION_TAB_BICEP, label: this.translate.instant('CONFIG_DETAIL.GENERATION.TAB_BICEP'), icon: 'terminal' },
      { id: GENERATION_TAB_PIPELINE, label: this.translate.instant('CONFIG_DETAIL.GENERATION.TAB_PIPELINE'), icon: 'account_tree' },
    ];
  });
}