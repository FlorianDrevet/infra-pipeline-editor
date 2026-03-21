import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ReactiveFormsModule, Validators, FormBuilder } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import {
  CreateInfrastructureConfigRequest,
  InfrastructureConfigResponse,
} from '../../shared/interfaces/infra-config.interface';
import { InfraConfigService } from '../../shared/services/infra-config.service';
import { LanguageService } from '../../shared/services/language.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [ReactiveFormsModule, TranslateModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
})
export class HomeComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly infraConfigService = inject(InfraConfigService);
  private readonly languageService = inject(LanguageService);

  protected readonly configs = signal<InfrastructureConfigResponse[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly isCreating = signal(false);
  protected readonly loadError = signal('');
  protected readonly createError = signal('');
  protected readonly createdConfigName = signal('');
  protected readonly totalEnvironmentCount = computed(() =>
    this.configs().reduce(
      (count, config) => count + config.environmentDefinitions.length,
      0
    )
  );

  protected readonly createForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(80)]],
  });

  protected get nameControl() {
    return this.createForm.controls.name;
  }

  public async ngOnInit(): Promise<void> {
    await this.loadConfigs();
  }

  protected async refreshConfigs(): Promise<void> {
    await this.loadConfigs();
  }

  protected async createConfig(): Promise<void> {
    this.createdConfigName.set('');
    this.createError.set('');

    if (this.createForm.invalid || this.isCreating()) {
      this.createForm.markAllAsTouched();
      return;
    }

    const rawName = this.nameControl.getRawValue().trim();

    if (rawName.length < 3) {
      this.nameControl.setValue(rawName);
      this.nameControl.markAsTouched();
      this.nameControl.setErrors({ minlength: true });
      return;
    }

    this.isCreating.set(true);

    try {
      const request: CreateInfrastructureConfigRequest = { name: rawName };
      const createdConfig = await this.infraConfigService.create(request);

      this.configs.update((configs) => [createdConfig, ...configs]);
      this.createForm.reset({ name: '' });
      this.createdConfigName.set(createdConfig.name);
    } catch (error) {
      this.createError.set(
        this.extractErrorMessage(
          error,
          'HOME.FEEDBACK.CREATE_ERROR_FALLBACK'
        )
      );
    } finally {
      this.isCreating.set(false);
    }
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