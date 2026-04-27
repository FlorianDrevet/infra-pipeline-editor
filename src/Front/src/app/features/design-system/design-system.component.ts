import { ChangeDetectionStrategy, Component } from '@angular/core';

import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import {
  DsAlertComponent,
  DsButtonComponent,
  DsCardComponent,
  DsCheckboxComponent,
  DsChipComponent,
  DsIconButtonComponent,
  DsPageHeaderComponent,
  DsRadioGroupComponent,
  DsRadioOption,
  DsSectionHeaderComponent,
  DsSelectComponent,
  DsSelectOption,
  DsTextFieldComponent,
  DsTextareaComponent,
  DsToggleComponent,
} from '../../shared/components/ds';

interface Swatch {
  readonly name: string;
  readonly value: string;
}

interface Gradient {
  readonly name: string;
  readonly css: string;
}

interface ShadowSample {
  readonly name: string;
  readonly css: string;
}

interface RadiusSample {
  readonly name: string;
  readonly value: string;
}

/**
 * Internal showcase page documenting Design System tokens and components.
 */
@Component({
  selector: 'app-design-system',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    DsAlertComponent,
    DsButtonComponent,
    DsCardComponent,
    DsCheckboxComponent,
    DsChipComponent,
    DsIconButtonComponent,
    DsPageHeaderComponent,
    DsRadioGroupComponent,
    DsSectionHeaderComponent,
    DsSelectComponent,
    DsTextFieldComponent,
    DsTextareaComponent,
    DsToggleComponent
],
  templateUrl: './design-system.component.html',
  styleUrl: './design-system.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DesignSystemComponent {
  protected readonly brandSwatches: readonly Swatch[] = [
    { name: 'ifs-brand-dark-blue', value: '#0d2f66' },
    { name: 'ifs-brand-blue', value: '#1565c0' },
    { name: 'ifs-brand-blue-deep', value: '#1a3a8a' },
    { name: 'ifs-brand-cyan', value: '#0288d1' },
    { name: 'ifs-brand-cyan-light', value: '#00bcd4' },
    { name: 'ifs-brand-cyan-deep', value: '#00acc1' },
    { name: 'ifs-brand-teal', value: '#009fbd' },
  ];

  protected readonly inkSwatches: readonly Swatch[] = [
    { name: 'ink-900', value: '#0d2b4f' },
    { name: 'ink-800', value: '#103454' },
    { name: 'ink-700', value: '#1a1a2e' },
    { name: 'ink-500', value: '#60758e' },
    { name: 'ink-400', value: '#8da4ba' },
    { name: 'ink-300', value: '#b6c3d4' },
    { name: 'ink-200', value: '#d1d5db' },
    { name: 'ink-100', value: '#e5e7eb' },
  ];

  protected readonly surfaceSwatches: readonly Swatch[] = [
    { name: 'surface-0', value: '#ffffff' },
    { name: 'surface-50', value: '#f9fafb' },
    { name: 'surface-100', value: '#f4f9ff' },
    { name: 'surface-200', value: '#eef5ff' },
    { name: 'surface-300', value: '#e8f4fd' },
    { name: 'surface-tinted', value: '#f1f7ff' },
  ];

  protected readonly semanticSwatches: readonly Swatch[] = [
    { name: 'success', value: '#2e7d32' },
    { name: 'error', value: '#c62828' },
    { name: 'error-strong', value: '#b91c1c' },
    { name: 'warning', value: '#b45309' },
    { name: 'warning-strong', value: '#92400e' },
    { name: 'info', value: '#1565c0' },
  ];

  protected readonly gradients: readonly Gradient[] = [
    { name: 'gradient-brand', css: 'linear-gradient(145deg, #0d2f66 0%, #1565c0 40%, #009fbd 100%)' },
    { name: 'gradient-brand-soft', css: 'linear-gradient(135deg, #0288d1, #00bcd4)' },
    { name: 'gradient-login', css: 'linear-gradient(160deg, #1a3a8a 0%, #1565c0 35%, #0288d1 65%, #00acc1 100%)' },
    { name: 'gradient-nav', css: 'linear-gradient(135deg, #0d2f66 0%, #1565c0 55%, #0288d1 100%)' },
    { name: 'gradient-cta', css: 'linear-gradient(145deg, #0d2f66 0%, #1565c0 100%)' },
  ];

  protected readonly shadows: readonly ShadowSample[] = [
    { name: 'shadow-xs', css: '0 2px 8px rgba(13, 47, 102, 0.08)' },
    { name: 'shadow-sm', css: '0 4px 12px rgba(13, 47, 102, 0.1)' },
    { name: 'shadow-md', css: '0 4px 16px rgba(21, 101, 192, 0.08)' },
    { name: 'shadow-lg', css: '0 10px 28px rgba(16, 52, 86, 0.06)' },
    { name: 'shadow-xl', css: '0 16px 40px rgba(16, 52, 86, 0.06)' },
    { name: 'shadow-cta', css: '0 8px 24px rgba(13, 47, 102, 0.22)' },
  ];

  protected readonly radii: readonly RadiusSample[] = [
    { name: 'radius-sm', value: '0.375rem' },
    { name: 'radius-md', value: '0.75rem' },
    { name: 'radius-lg', value: '1rem' },
    { name: 'radius-xl', value: '1.3rem' },
    { name: 'radius-2xl', value: '1.4rem' },
    { name: 'radius-3xl', value: '1.6rem' },
  ];

  protected readonly showcaseForm = new FormGroup({
    text: new FormControl<string | null>('Hello world'),
    email: new FormControl<string | null>(''),
    password: new FormControl<string | null>(''),
    bio: new FormControl<string | null>(''),
    role: new FormControl<string | null>(null),
    notify: new FormControl<boolean | null>(true),
    accept: new FormControl<boolean | null>(false),
    plan: new FormControl<string | number | null>('starter'),
  });

  protected readonly roleOptions: DsSelectOption[] = [
    { value: 'owner', label: 'Owner', icon: 'admin_panel_settings', description: 'Full administrative access' },
    { value: 'contributor', label: 'Contributor', icon: 'edit', description: 'Can edit configurations' },
    { value: 'reader', label: 'Reader', icon: 'visibility', description: 'Read-only access' },
  ];

  protected readonly planOptions: DsRadioOption[] = [
    { value: 'starter', label: 'Starter', description: 'For small teams' },
    { value: 'pro', label: 'Pro', description: 'For growing teams' },
    { value: 'enterprise', label: 'Enterprise', description: 'For large organizations' },
  ];
}
