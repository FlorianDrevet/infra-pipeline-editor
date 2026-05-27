import {
  ConnectedPosition,
  Overlay,
  OverlayPositionBuilder,
  OverlayRef,
} from '@angular/cdk/overlay';
import { ComponentPortal } from '@angular/cdk/portal';
import {
  Directive,
  ElementRef,
  HostListener,
  OnDestroy,
  inject,
  input,
  output,
} from '@angular/core';
import { Subscription } from 'rxjs';

import { DsMenuComponent } from './ds-menu.component';
import { DsMenuItem, DsMenuPlacement } from './ds-menu.types';

const POSITION_MAP: Record<DsMenuPlacement, ConnectedPosition> = {
  'bottom-start': {
    originX: 'start',
    originY: 'bottom',
    overlayX: 'start',
    overlayY: 'top',
    offsetY: 4,
  },
  'bottom-end': {
    originX: 'end',
    originY: 'bottom',
    overlayX: 'end',
    overlayY: 'top',
    offsetY: 4,
  },
  'top-start': {
    originX: 'start',
    originY: 'top',
    overlayX: 'start',
    overlayY: 'bottom',
    offsetY: -4,
  },
  'top-end': {
    originX: 'end',
    originY: 'top',
    overlayX: 'end',
    overlayY: 'bottom',
    offsetY: -4,
  },
};

const MENU_EXPANDED_ATTR = 'aria-expanded';
const MENU_HASPOPUP_ATTR = 'aria-haspopup';

/**
 * Design system menu trigger. Attach to a focusable element (typically a
 * button) and pass an array of {@link DsMenuItem} via `[appDsMenu]`. The
 * popover opens on click and closes on item selection, Escape, outside
 * click, or scroll.
 */
@Directive({
  selector: '[appDsMenu]',
  standalone: true,
  exportAs: 'dsMenu',
})
export class DsMenuDirective implements OnDestroy {
  public readonly items = input.required<readonly DsMenuItem[]>({ alias: 'appDsMenu' });
  public readonly placement = input<DsMenuPlacement>('bottom-start');
  public readonly menuAriaLabel = input<string | undefined>(undefined);

  public readonly itemSelected = output<DsMenuItem>();
  public readonly opened = output<void>();
  public readonly closed = output<void>();

  private readonly overlay = inject(Overlay);
  private readonly positionBuilder = inject(OverlayPositionBuilder);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  private overlayRef: OverlayRef | null = null;
  private subscriptions: Subscription = new Subscription();

  public constructor() {
    this.host.nativeElement.setAttribute(MENU_HASPOPUP_ATTR, 'menu');
    this.host.nativeElement.setAttribute(MENU_EXPANDED_ATTR, 'false');
  }

  @HostListener('click', ['$event'])
  protected onClick(event: MouseEvent): void {
    event.stopPropagation();
    this.toggle();
  }

  @HostListener('keydown', ['$event'])
  protected onKeyDown(event: KeyboardEvent): void {
    if (event.key === 'ArrowDown' || event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      this.open();
    }
  }

  public ngOnDestroy(): void {
    this.close();
  }

  /** Toggle visibility of the menu popover. */
  public toggle(): void {
    if (this.overlayRef) {
      this.close();
    } else {
      this.open();
    }
  }

  /** Open the menu popover and focus its first interactive item. */
  public open(): void {
    if (this.overlayRef) {
      return;
    }
    const positionStrategy = this.positionBuilder
      .flexibleConnectedTo(this.host)
      .withPositions([POSITION_MAP[this.placement()]]);

    this.overlayRef = this.overlay.create({
      positionStrategy,
      scrollStrategy: this.overlay.scrollStrategies.reposition(),
      hasBackdrop: true,
      backdropClass: 'cdk-overlay-transparent-backdrop',
    });

    const portal = new ComponentPortal(DsMenuComponent);
    const ref = this.overlayRef.attach(portal);
    ref.setInput('items', this.items());
    ref.setInput('ariaLabel', this.menuAriaLabel());

    this.subscriptions = new Subscription();
    this.subscriptions.add(
      ref.instance.itemSelected.subscribe((item) => {
        this.itemSelected.emit(item);
        this.close();
      }),
    );
    this.subscriptions.add(ref.instance.closeRequested.subscribe(() => this.close()));
    this.subscriptions.add(this.overlayRef.backdropClick().subscribe(() => this.close()));

    this.host.nativeElement.setAttribute(MENU_EXPANDED_ATTR, 'true');
    this.opened.emit();
  }

  /** Close the menu popover and return focus to the trigger. */
  public close(): void {
    if (!this.overlayRef) {
      return;
    }
    this.subscriptions.unsubscribe();
    this.subscriptions = new Subscription();
    this.overlayRef.detach();
    this.overlayRef.dispose();
    this.overlayRef = null;
    this.host.nativeElement.setAttribute(MENU_EXPANDED_ATTR, 'false');
    this.host.nativeElement.focus();
    this.closed.emit();
  }
}
