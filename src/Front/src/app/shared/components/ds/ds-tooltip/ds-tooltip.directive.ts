import { ConnectedPosition, Overlay, OverlayPositionBuilder, OverlayRef } from '@angular/cdk/overlay';
import { ComponentPortal } from '@angular/cdk/portal';
import {
  Directive,
  ElementRef,
  HostListener,
  OnDestroy,
  inject,
  input,
} from '@angular/core';

import { DsTooltipComponent, DsTooltipPosition } from './ds-tooltip.component';

const POSITION_MAP: Record<DsTooltipPosition, ConnectedPosition> = {
  top: {
    originX: 'center',
    originY: 'top',
    overlayX: 'center',
    overlayY: 'bottom',
    offsetY: -6,
  },
  bottom: {
    originX: 'center',
    originY: 'bottom',
    overlayX: 'center',
    overlayY: 'top',
    offsetY: 6,
  },
  left: {
    originX: 'start',
    originY: 'center',
    overlayX: 'end',
    overlayY: 'center',
    offsetX: -6,
  },
  right: {
    originX: 'end',
    originY: 'center',
    overlayX: 'start',
    overlayY: 'center',
    offsetX: 6,
  },
};

const TOOLTIP_DESCRIBED_BY_ATTR = 'aria-describedby';

let tooltipIdCounter = 0;

/**
 * Design system tooltip. Attach to any focusable element via `[appDsTooltip]`.
 * Shows after `delay` ms on hover or focus, hides on mouseleave, blur, or Esc.
 */
@Directive({
  selector: '[appDsTooltip]',
  standalone: true,
  exportAs: 'dsTooltip',
})
export class DsTooltipDirective implements OnDestroy {
  public readonly text = input.required<string>({ alias: 'appDsTooltip' });
  public readonly position = input<DsTooltipPosition>('top');
  public readonly delay = input<number>(350);

  private readonly overlay = inject(Overlay);
  private readonly positionBuilder = inject(OverlayPositionBuilder);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  private overlayRef: OverlayRef | null = null;
  private showTimer: ReturnType<typeof setTimeout> | null = null;
  private readonly tooltipId = `ds-tooltip-${++tooltipIdCounter}`;

  @HostListener('mouseenter')
  protected onEnter(): void {
    this.scheduleShow();
  }

  @HostListener('mouseleave')
  protected onLeave(): void {
    this.hide();
  }

  @HostListener('focus')
  protected onFocus(): void {
    this.scheduleShow();
  }

  @HostListener('blur')
  protected onBlur(): void {
    this.hide();
  }

  @HostListener('document:keydown.escape')
  protected onEscape(): void {
    this.hide();
  }

  public ngOnDestroy(): void {
    this.hide();
  }

  private scheduleShow(): void {
    this.clearTimer();
    if (!this.text()) {
      return;
    }
    this.showTimer = setTimeout(() => this.show(), this.delay());
  }

  private show(): void {
    if (this.overlayRef) {
      return;
    }
    const positionStrategy = this.positionBuilder
      .flexibleConnectedTo(this.host)
      .withPositions([POSITION_MAP[this.position()]]);

    this.overlayRef = this.overlay.create({
      positionStrategy,
      scrollStrategy: this.overlay.scrollStrategies.reposition(),
    });

    const portal = new ComponentPortal(DsTooltipComponent);
    const ref = this.overlayRef.attach(portal);
    ref.setInput('text', this.text());
    ref.setInput('position', this.position());

    this.host.nativeElement.setAttribute(TOOLTIP_DESCRIBED_BY_ATTR, this.tooltipId);
    const overlayElement = this.overlayRef.overlayElement;
    if (overlayElement) {
      overlayElement.setAttribute('id', this.tooltipId);
    }
  }

  private hide(): void {
    this.clearTimer();
    if (this.overlayRef) {
      this.overlayRef.detach();
      this.overlayRef.dispose();
      this.overlayRef = null;
      this.host.nativeElement.removeAttribute(TOOLTIP_DESCRIBED_BY_ATTR);
    }
  }

  private clearTimer(): void {
    if (this.showTimer !== null) {
      clearTimeout(this.showTimer);
      this.showTimer = null;
    }
  }
}
