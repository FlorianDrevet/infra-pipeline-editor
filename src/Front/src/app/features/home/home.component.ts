import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';
import { InfrastructureConfigResponse } from '../../shared/interfaces/infra-config.interface';
import { InfraConfigService } from '../../shared/services/infra-config.service';
import { LanguageService } from '../../shared/services/language.service';
import { CreateConfigDialogComponent } from './create-config-dialog/create-config-dialog.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [TranslateModule, RouterLink, MatDialogModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
})
export class HomeComponent implements OnInit {
  private readonly infraConfigService = inject(InfraConfigService);
  private readonly languageService = inject(LanguageService);
  private readonly dialog = inject(MatDialog);

  protected readonly configs = signal<InfrastructureConfigResponse[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly loadError = signal('');
  protected readonly createdConfigName = signal('');
  protected readonly searchQuery = signal('');
  protected readonly sortBy = signal<'name' | 'environments' | 'members'>('name');

  protected readonly totalEnvironmentCount = computed(() =>
    this.configs().reduce(
      (count, config) => count + config.environmentDefinitions.length,
      0
    )
  );

  protected readonly filteredConfigs = computed(() => {
    const query = this.searchQuery().toLowerCase().trim();
    const sort = this.sortBy();
    let filtered = this.configs();

    if (query) {
      filtered = filtered.filter(c => c.name.toLowerCase().includes(query));
    }

    return [...filtered].sort((a, b) => {
      switch (sort) {
        case 'environments':
          return b.environmentDefinitions.length - a.environmentDefinitions.length;
        case 'members':
          return b.members.length - a.members.length;
        default:
          return a.name.localeCompare(b.name);
      }
    });
  });

  public async ngOnInit(): Promise<void> {
    await this.loadConfigs();
  }

  protected async refreshConfigs(): Promise<void> {
    await this.loadConfigs();
  }

  protected openCreateDialog(): void {
    const dialogRef = this.dialog.open(CreateConfigDialogComponent, {
      width: '480px',
    });

    dialogRef.afterClosed().subscribe((result?: InfrastructureConfigResponse) => {
      if (result) {
        this.configs.update(configs => [result, ...configs]);
        this.createdConfigName.set(result.name);
      }
    });
  }

  protected onSearchInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.searchQuery.set(input.value);
  }

  protected onSortChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.sortBy.set(select.value as 'name' | 'environments' | 'members');
  }

  private async loadConfigs(): Promise<void> {
    this.isLoading.set(true);
    this.loadError.set('');

    try {
      const configs = await this.infraConfigService.getAll();
      this.configs.set(configs);
    } catch (error) {
      this.loadError.set(
        this.extractErrorMessage(
          error,
          'HOME.FEEDBACK.LOAD_ERROR_FALLBACK'
        )
      );
    } finally {
      this.isLoading.set(false);
    }
  }

  private extractErrorMessage(error: unknown, fallbackKey: string): string {
    if (typeof error !== 'object' || error === null) {
      return this.languageService.translate(fallbackKey);
    }

    const maybeResponse = error as {
      response?: {
        data?: {
          title?: string;
          detail?: string;
          errors?: Record<string, string[]>;
        };
      };
      message?: string;
    };

    const details = maybeResponse.response?.data;

    if (details?.errors) {
      const validationMessages = Object.values(details.errors).flat();
      if (validationMessages.length > 0) {
        return validationMessages[0] ?? this.languageService.translate(fallbackKey);
      }
    }

    return (
      details?.detail ||
      details?.title ||
      maybeResponse.message ||
      this.languageService.translate(fallbackKey)
    );
  }
}