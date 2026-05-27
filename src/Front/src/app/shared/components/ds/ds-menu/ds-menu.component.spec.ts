import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DsMenuComponent } from './ds-menu.component';
import { DsMenuItem } from './ds-menu.types';

describe('DsMenuComponent', () => {
  let fixture: ComponentFixture<DsMenuComponent>;
  let component: DsMenuComponent;

  const items: DsMenuItem[] = [
    { id: 'open', label: 'Open', icon: 'folder_open' },
    { id: 'sep', label: '', divider: true },
    { id: 'delete', label: 'Delete', tone: 'danger' },
    { id: 'disabled', label: 'Disabled', disabled: true },
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsMenuComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DsMenuComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('items', items);
    fixture.detectChanges();
  });

  it('should render one menuitem per non-divider entry and a separator', () => {
    const menuItems = fixture.nativeElement.querySelectorAll('[role="menuitem"]');
    const separators = fixture.nativeElement.querySelectorAll('[role="separator"]');
    expect(menuItems.length).toBe(3);
    expect(separators.length).toBe(1);
  });

  it('should disable the disabled item', () => {
    const buttons: HTMLButtonElement[] = Array.from(
      fixture.nativeElement.querySelectorAll('button[role="menuitem"]'),
    );
    const disabled = buttons.find((b) => b.textContent?.trim() === 'Disabled');
    expect(disabled?.disabled).toBe(true);
  });

  it('should apply danger tone class', () => {
    const danger = fixture.nativeElement.querySelector('.ds-menu__item--danger');
    expect(danger).toBeTruthy();
  });

  it('should emit itemSelected when a non-disabled item is clicked', () => {
    let received: DsMenuItem | null = null;
    component.itemSelected.subscribe((item) => (received = item));
    const openButton: HTMLButtonElement = fixture.nativeElement.querySelector(
      'button[role="menuitem"]',
    );
    openButton.click();
    expect(received).toBeTruthy();
    expect(received!.id).toBe('open');
  });

  it('should not emit itemSelected when a disabled item is clicked', () => {
    let emitted = false;
    component.itemSelected.subscribe(() => (emitted = true));
    const buttons: HTMLButtonElement[] = Array.from(
      fixture.nativeElement.querySelectorAll('button[role="menuitem"]'),
    );
    const disabled = buttons.find((b) => b.textContent?.trim() === 'Disabled');
    disabled?.click();
    expect(emitted).toBe(false);
  });
});
