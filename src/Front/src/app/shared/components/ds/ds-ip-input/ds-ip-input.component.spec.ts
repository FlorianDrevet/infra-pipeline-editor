import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ComponentRef } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';

import { DsIpInputComponent } from './ds-ip-input.component';

describe('DsIpInputComponent', () => {
  let fixture: ComponentFixture<DsIpInputComponent>;
  let component: DsIpInputComponent;
  let componentRef: ComponentRef<DsIpInputComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsIpInputComponent, TranslateModule.forRoot()],
    }).compileComponents();

    fixture = TestBed.createComponent(DsIpInputComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
  });

  function segmentInputs(): HTMLInputElement[] {
    return Array.from(fixture.nativeElement.querySelectorAll('input.ds-ip-input__segment'));
  }

  function type(input: HTMLInputElement, value: string): void {
    input.value = value;
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
  }

  it('renders four octet segments in ipv4 mode', () => {
    fixture.detectChanges();
    expect(segmentInputs().length).toBe(4);
  });

  it('renders five segments and a slash separator in cidr mode', () => {
    componentRef.setInput('mode', 'cidr');
    fixture.detectChanges();
    expect(segmentInputs().length).toBe(5);
    const separators = Array.from(
      fixture.nativeElement.querySelectorAll('.ds-ip-input__separator'),
    ).map((node) => (node as HTMLElement).textContent?.trim());
    expect(separators).toEqual(['.', '.', '.', '/']);
  });

  it('distributes a written ipv4 value across segments', () => {
    fixture.detectChanges();
    component.writeValue('10.0.0.4');
    fixture.detectChanges();
    expect(segmentInputs().map((input) => input.value)).toEqual(['10', '0', '0', '4']);
  });

  it('distributes a written cidr value across segments', () => {
    componentRef.setInput('mode', 'cidr');
    fixture.detectChanges();
    component.writeValue('10.0.0.0/16');
    fixture.detectChanges();
    expect(segmentInputs().map((input) => input.value)).toEqual(['10', '0', '0', '0', '16']);
  });

  it('emits the joined ipv4 string on input', () => {
    fixture.detectChanges();
    const emitted: string[] = [];
    component.registerOnChange((value) => emitted.push(value));

    const inputs = segmentInputs();
    type(inputs[0], '10');
    type(inputs[1], '0');
    type(inputs[2], '0');
    type(inputs[3], '4');

    expect(emitted[emitted.length - 1]).toBe('10.0.0.4');
  });

  it('emits the joined cidr string on input', () => {
    componentRef.setInput('mode', 'cidr');
    fixture.detectChanges();
    let lastValue = '';
    component.registerOnChange((value) => (lastValue = value));

    const inputs = segmentInputs();
    type(inputs[0], '10');
    type(inputs[1], '0');
    type(inputs[2], '0');
    type(inputs[3], '0');
    type(inputs[4], '16');

    expect(lastValue).toBe('10.0.0.0/16');
  });

  it('clamps an out-of-range octet', () => {
    fixture.detectChanges();
    const inputs = segmentInputs();
    type(inputs[0], '300');
    expect(inputs[0].value).toBe('255');
  });

  it('auto-advances focus to the next segment when an octet is full', () => {
    fixture.detectChanges();
    const inputs = segmentInputs();
    inputs[0].focus();
    type(inputs[0], '192');
    expect(document.activeElement).toBe(inputs[1]);
  });

  it('steps back to the previous segment on Backspace at the start of an empty segment', () => {
    fixture.detectChanges();
    component.writeValue('10.0.0.0');
    fixture.detectChanges();
    const inputs = segmentInputs();
    inputs[1].value = '';
    inputs[1].focus();
    inputs[1].setSelectionRange(0, 0);

    inputs[1].dispatchEvent(new KeyboardEvent('keydown', { key: 'Backspace', bubbles: true }));
    fixture.detectChanges();

    expect(document.activeElement).toBe(inputs[0]);
  });

  it('distributes a pasted value across all segments', () => {
    componentRef.setInput('mode', 'cidr');
    fixture.detectChanges();
    let lastValue = '';
    component.registerOnChange((value) => (lastValue = value));

    const event = new ClipboardEvent('paste', { clipboardData: new DataTransfer() });
    event.clipboardData?.setData('text', '172.16.0.0/12');
    segmentInputs()[0].dispatchEvent(event);
    fixture.detectChanges();

    expect(segmentInputs().map((input) => input.value)).toEqual(['172', '16', '0', '0', '12']);
    expect(lastValue).toBe('172.16.0.0/12');
  });

  it('clears all segments when the form value is reset', () => {
    fixture.detectChanges();
    component.writeValue('10.0.0.4');
    fixture.detectChanges();
    component.writeValue('');
    fixture.detectChanges();
    expect(segmentInputs().map((input) => input.value)).toEqual(['', '', '', '']);
  });
});
