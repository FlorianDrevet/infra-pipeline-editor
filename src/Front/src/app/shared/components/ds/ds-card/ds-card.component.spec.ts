import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DsCardComponent } from './ds-card.component';

describe('DsCardComponent', () => {
  let fixture: ComponentFixture<DsCardComponent>;
  let component: DsCardComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsCardComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DsCardComponent);
    component = fixture.componentInstance;
  });

  it('adds button semantics only when the card is interactive', () => {
    fixture.detectChanges();

    const root = getCardElement();

    expect(root.getAttribute('role')).toBeNull();
    expect(root.getAttribute('tabindex')).toBeNull();

    fixture.componentRef.setInput('interactive', true);
    fixture.detectChanges();

    expect(root.getAttribute('role')).toBe('button');
    expect(root.getAttribute('tabindex')).toBe('0');
  });

  it('emits the existing click output when activated from the keyboard', () => {
    fixture.componentRef.setInput('interactive', true);
    fixture.detectChanges();

    const emitSpy = spyOn(component.cardClick, 'emit');

    getCardElement().dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter', bubbles: true, cancelable: true }));
    fixture.detectChanges();

    expect(emitSpy).toHaveBeenCalledTimes(1);
    expect(emitSpy.calls.mostRecent().args[0] instanceof MouseEvent).toBeTrue();
  });

  function getCardElement(): HTMLDivElement {
    return fixture.nativeElement.querySelector('.ds-card') as HTMLDivElement;
  }
});