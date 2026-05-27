import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DsTagInputComponent } from './ds-tag-input.component';

describe('DsTagInputComponent', () => {
  let fixture: ComponentFixture<DsTagInputComponent>;
  let component: DsTagInputComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsTagInputComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DsTagInputComponent);
    component = fixture.componentInstance;
  });

  function getInput(): HTMLInputElement {
    return fixture.nativeElement.querySelector('.ds-tag-input__input') as HTMLInputElement;
  }

  it('should create with empty tag list', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
    expect(fixture.nativeElement.querySelectorAll('app-ds-chip').length).toBe(0);
  });

  it('should add a tag on Enter', () => {
    fixture.detectChanges();
    const input = getInput();
    input.value = 'alpha';
    input.dispatchEvent(new Event('input'));
    input.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter' }));
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('app-ds-chip').length).toBe(1);
  });

  it('should add tags split by comma when addOnComma is enabled', () => {
    fixture.detectChanges();
    const input = getInput();
    input.value = 'red,green,blue';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('app-ds-chip').length).toBe(2);
    // The trailing "blue" remains in the input draft, not yet committed.
  });

  it('should deduplicate values', () => {
    fixture.detectChanges();
    const input = getInput();
    input.value = 'alpha';
    input.dispatchEvent(new Event('input'));
    input.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter' }));
    fixture.detectChanges();
    input.value = 'alpha';
    input.dispatchEvent(new Event('input'));
    input.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter' }));
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('app-ds-chip').length).toBe(1);
  });

  it('should respect maxTags', () => {
    fixture.componentRef.setInput('maxTags', 1);
    fixture.detectChanges();
    const input = getInput();
    for (const value of ['a', 'b']) {
      input.value = value;
      input.dispatchEvent(new Event('input'));
      input.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter' }));
    }
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('app-ds-chip').length).toBe(1);
  });

  it('should remove last tag on Backspace when input is empty', () => {
    fixture.detectChanges();
    const input = getInput();
    input.value = 'alpha';
    input.dispatchEvent(new Event('input'));
    input.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter' }));
    fixture.detectChanges();
    input.dispatchEvent(new KeyboardEvent('keydown', { key: 'Backspace' }));
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('app-ds-chip').length).toBe(0);
  });

  it('should expose aria-invalid when error is set', () => {
    fixture.componentRef.setInput('errorText', 'Invalid');
    fixture.detectChanges();
    const input = getInput();
    expect(input.getAttribute('aria-invalid')).toBe('true');
  });
});
