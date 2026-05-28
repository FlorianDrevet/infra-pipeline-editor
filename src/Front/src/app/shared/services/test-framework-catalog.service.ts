import { inject, Injectable, signal } from '@angular/core';
import { AxiosService } from './axios.service';
import { MethodEnum } from '../enums/method.enum';
import { TestFrameworkDefinitionResponse } from '../interfaces/test-framework-catalog.interface';
import { DsSelectOption } from '../components/ds';

@Injectable({
  providedIn: 'root',
})
export class TestFrameworkCatalogService {
  private readonly axios = inject(AxiosService);

  private readonly _frameworks = signal<TestFrameworkDefinitionResponse[]>([]);
  readonly frameworks = this._frameworks.asReadonly();

  private loaded = false;

  async loadCatalog(): Promise<void> {
    if (this.loaded) return;

    const data = await this.axios.request$<TestFrameworkDefinitionResponse[]>(
      MethodEnum.GET,
      '/catalogs/test-frameworks'
    );
    this._frameworks.set(data);
    this.loaded = true;
  }

  getOptionsForStack(stack: string | null | undefined): DsSelectOption[] {
    if (!stack) return [];
    return this._frameworks()
      .filter((f) => f.stack === stack)
      .map((f) => ({ value: f.frameworkKey, label: f.displayName }));
  }

  getDefinition(stack: string, frameworkKey: string): TestFrameworkDefinitionResponse | undefined {
    return this._frameworks().find((f) => f.stack === stack && f.frameworkKey === frameworkKey);
  }
}
