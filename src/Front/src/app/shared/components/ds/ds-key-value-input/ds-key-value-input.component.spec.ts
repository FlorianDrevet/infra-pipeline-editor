import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DsKeyValueInputComponent } from './ds-key-value-input.component';
import { DsKeyValueItem } from './ds-key-value-input.types';

describe('DsKeyValueInputComponent', () => {
  let fixture: ComponentFixture<DsKeyValueInputComponent>;
  let component: DsKeyValueInputComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsKeyValueInputComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DsKeyValueInputComponent);
    component = fixture.componentInstance;
  });

  it('should create with empty items list', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
    expect(fixture.nativeElement.querySelectorAll('app-ds-chip').length).toBe(0);
  });

  it('should add an item when tryAdd is called with valid key and value', () => {
    fixture.detectChanges();
    component.writeValue([]);
    component['keyCtrl'].setValue('myKey');
    component['valueCtrl'].setValue('myValue');
    component['tryAdd']();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('app-ds-chip').length).toBe(1);
  });

  it('should not add item when key is empty', () => {
    fixture.detectChanges();
    component['keyCtrl'].setValue('');
    component['valueCtrl'].setValue('someValue');
    component['tryAdd']();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('app-ds-chip').length).toBe(0);
  });

  it('should not add item when value is empty', () => {
    fixture.detectChanges();
    component['keyCtrl'].setValue('someKey');
    component['valueCtrl'].setValue('');
    component['tryAdd']();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('app-ds-chip').length).toBe(0);
  });

  it('should reject duplicate keys', () => {
    fixture.componentRef.setInput('duplicateKeyError', 'Key exists');
    fixture.detectChanges();
    component.writeValue([{ key: 'env', value: 'prod' }]);
    component['keyCtrl'].setValue('env');
    component['valueCtrl'].setValue('staging');
    component['tryAdd']();
    fixture.detectChanges();
    expect(component['items']().length).toBe(1);
    expect(component['internalError']()).toBe('Key exists');
  });

  it('should respect maxItems', () => {
    fixture.componentRef.setInput('maxItems', 2);
    fixture.detectChanges();
    component.writeValue([{ key: 'a', value: '1' }, { key: 'b', value: '2' }]);
    component['keyCtrl'].setValue('c');
    component['valueCtrl'].setValue('3');
    component['tryAdd']();
    fixture.detectChanges();
    expect(component['items']().length).toBe(2);
  });

  it('should remove item by index', () => {
    fixture.detectChanges();
    component.writeValue([
      { key: 'a', value: '1' },
      { key: 'b', value: '2' },
    ]);
    fixture.detectChanges();
    component['removeItem'](0);
    fixture.detectChanges();
    expect(component['items']().length).toBe(1);
    expect(component['items']()[0].key).toBe('b');
  });

  it('should emit itemAdded on successful add', () => {
    fixture.detectChanges();
    let emittedItem: DsKeyValueItem | undefined;
    component.itemAdded.subscribe(item => (emittedItem = item));
    component['keyCtrl'].setValue('foo');
    component['valueCtrl'].setValue('bar');
    component['tryAdd']();
    expect(emittedItem).toEqual({ key: 'foo', value: 'bar' });
  });

  it('should emit itemRemoved on remove', () => {
    fixture.detectChanges();
    let emittedItem: DsKeyValueItem | undefined;
    component.itemRemoved.subscribe(item => (emittedItem = item));
    component.writeValue([{ key: 'x', value: 'y' }]);
    component['removeItem'](0);
    expect(emittedItem).toEqual({ key: 'x', value: 'y' });
  });

  it('should emit itemsChange on add and remove', () => {
    fixture.detectChanges();
    const emitted: DsKeyValueItem[][] = [];
    component.itemsChange.subscribe(items => emitted.push(items));
    component['keyCtrl'].setValue('k1');
    component['valueCtrl'].setValue('v1');
    component['tryAdd']();
    component['removeItem'](0);
    expect(emitted.length).toBe(2);
    expect(emitted[0]).toEqual([{ key: 'k1', value: 'v1' }]);
    expect(emitted[1]).toEqual([]);
  });

  it('should call custom validator and reject with error string', () => {
    fixture.componentRef.setInput('validator', (_k: string, _v: string) => 'Custom error');
    fixture.detectChanges();
    component['keyCtrl'].setValue('k');
    component['valueCtrl'].setValue('v');
    component['tryAdd']();
    expect(component['items']().length).toBe(0);
    expect(component['internalError']()).toBe('Custom error');
  });

  it('should implement ControlValueAccessor writeValue', () => {
    fixture.detectChanges();
    const items: DsKeyValueItem[] = [{ key: 'env', value: 'prod' }];
    component.writeValue(items);
    expect(component['items']()).toEqual(items);
  });

  it('should handle writeValue with null', () => {
    fixture.detectChanges();
    component.writeValue(null);
    expect(component['items']()).toEqual([]);
  });

  it('should set disabled state via setDisabledState', () => {
    fixture.detectChanges();
    component.setDisabledState(true);
    expect(component['disabledState']()).toBe(true);
  });

  it('should not add when disabled', () => {
    fixture.componentRef.setInput('disabled', true);
    fixture.detectChanges();
    component['keyCtrl'].setValue('k');
    component['valueCtrl'].setValue('v');
    component['tryAdd']();
    expect(component['items']().length).toBe(0);
  });

  it('should not remove when disabled', () => {
    fixture.componentRef.setInput('disabled', true);
    fixture.detectChanges();
    component.writeValue([{ key: 'a', value: 'b' }]);
    component['removeItem'](0);
    expect(component['items']().length).toBe(1);
  });
});
