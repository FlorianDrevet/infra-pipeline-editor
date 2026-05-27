import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';

import { ConfigDetailGitSectionViewModel } from './config-detail-git-section.view-model';
import { InfraConfigRepositoryResponse } from '../../../../shared/interfaces/infra-config-repository.interface';

@Component({
  selector: 'app-config-detail-git-section',
  standalone: true,
  imports: [MatButtonModule, MatCardModule, MatChipsModule, MatIconModule, TranslateModule],
  templateUrl: './config-detail-git-section.component.html',
  styleUrl: './config-detail-git-section.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfigDetailGitSectionComponent {
  readonly viewModel = input.required<ConfigDetailGitSectionViewModel>();

  protected repositoryDisplayName(repo: InfraConfigRepositoryResponse): string {
    if (repo.owner && repo.repositoryName) {
      return `${repo.owner}/${repo.repositoryName}`;
    }

    return repo.repositoryName ?? repo.repositoryUrl ?? repo.id;
  }

  protected repositoryProviderLabel(repo: InfraConfigRepositoryResponse): string {
    return repo.providerType ?? 'CONFIG_DETAIL.REPOSITORIES.UNCONFIGURED';
  }
}