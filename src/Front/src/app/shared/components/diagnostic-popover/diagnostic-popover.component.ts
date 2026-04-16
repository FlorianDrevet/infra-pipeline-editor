import { Component, ElementRef, input, signal, inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ResourceDiagnosticResponse } from '../../interfaces/bicep-generator.interface';

@Component({
  selector: 'app-diagnostic-popover',
  standalone: true,
  imports: [CommonModule, MatIconModule, TranslateModule],
  templateUrl: './diagnostic-popover.component.html',
  styleUrl: './diagnostic-popover.component.scss',
})
export class DiagnosticPopoverComponent {
  private readonly translate = inject(TranslateService);
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));
  private readonly elRef = inject(ElementRef);

  diagnostics = input.required<ResourceDiagnosticResponse[]>();

  protected isOpen = signal(false);
  protected panelStyle = signal<Record<string, string>>({});

  protected onMouseEnter(): void {
    this.computePanelStyle();
    this.isOpen.set(true);
  }

  protected onMouseLeave(): void {
    this.isOpen.set(false);
  }

  protected getSeverityIcon(severity: string): string {
    return severity?.toLowerCase() === 'warning' ? 'warning' : 'gpp_bad';
  }

  protected getSeverityClass(severity: string): string {
    return severity?.toLowerCase() === 'warning' ? 'warning' : 'error';
  }

  protected getTranslatedMessage(diagnostic: ResourceDiagnosticResponse): string {
    const key = `CONFIG_DETAIL.DIAGNOSTICS.${diagnostic.ruleCode}`;
    const params = { target: diagnostic.targetResourceName };
    const translated = this.translate.instant(key, params);

    if (translated === key) {
      return this.translate.instant('CONFIG_DETAIL.DIAGNOSTICS.UNKNOWN_RULE', {
        ruleCode: diagnostic.ruleCode,
        target: diagnostic.targetResourceName,
      });
    }

    return translated;
  }

  private computePanelStyle(): void {
    if (!this.isBrowser) return;

    const el = this.elRef.nativeElement as HTMLElement;
    const rect = el.getBoundingClientRect();
    const panelWidth = 360;
    const spaceBelow = window.innerHeight - rect.bottom;
    const spaceRight = window.innerWidth - rect.left;

    let top: number;
    let left: number;

    if (spaceBelow < 260) {
      // not enough space below — show above
      top = rect.top - 8; // will be adjusted by translateY(-100%)
    } else {
      top = rect.bottom + 8;
    }

    // Try to center on the badge; clamp to viewport
    left = rect.left + rect.width / 2 - panelWidth / 2;
    left = Math.max(8, Math.min(left, window.innerWidth - panelWidth - 8));

    const isAbove = spaceBelow < 260;
    this.panelStyle.set({
      position: 'fixed',
      top: `${top}px`,
      left: `${left}px`,
      transform: isAbove ? 'translateY(-100%)' : 'none',
      width: `${panelWidth}px`,
    });
  }
}
