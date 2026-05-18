import { Component } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';

import { DsAutocompleteComponent, DsAutocompleteOption } from './ds-autocomplete.component';

@Component({
  standalone: true,
  imports: [ReactiveFormsModule, DsAutocompleteComponent],
  template: `
    <form [formGroup]="form">
      <div class="host-shell">
        <app-ds-autocomplete
          formControlName="search"
          label="Utilisateur"
          placeholder="Rechercher un utilisateur"
          prefixIcon="search"
          emptyStateLabel="Aucun utilisateur trouve"
          [clearable]="true"
          [options]="options"
          (optionSelected)="onOptionSelected($event)"
          (searchChanged)="onSearchChanged($event)" />
      </div>
    </form>
  `,
  styles: [
    `
      .host-shell {
        width: 280px;
      }
    `,
  ],
})
class DsAutocompleteHostComponent {
  private readonly allOptions: DsAutocompleteOption<string>[] = [
    { value: 'alice', label: 'Alice Martin', description: 'alice@example.com' },
    { value: 'bob', label: 'Bob Dupont', description: 'bob@example.com' },
  ];

  public readonly form = new FormGroup({
    search: new FormControl('', { nonNullable: true }),
  });

  public selectedOption: DsAutocompleteOption<string> | null = null;
  public latestSearch = '';

  public get options(): DsAutocompleteOption<string>[] {
    const searchTerm = this.latestSearch.trim().toLowerCase();
    if (searchTerm === '') {
      return this.allOptions;
    }

    return this.allOptions.filter(option =>
      option.label.toLowerCase().includes(searchTerm)
      || option.description?.toLowerCase().includes(searchTerm),
    );
  }

  public onOptionSelected(option: DsAutocompleteOption<string>): void {
    this.selectedOption = option;
  }

  public onSearchChanged(value: string): void {
    this.latestSearch = value;
  }
}

describe('DsAutocompleteComponent', () => {
  let fixture: ComponentFixture<DsAutocompleteHostComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsAutocompleteHostComponent],
      providers: [provideNoopAnimations()],
    }).compileComponents();

    fixture = TestBed.createComponent(DsAutocompleteHostComponent);
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('keeps the input value in sync with the bound form control', async () => {
    const host = fixture.componentInstance;
    const input = fixture.nativeElement.querySelector('.ds-autocomplete__input') as HTMLInputElement | null;

    expect(input).withContext('autocomplete input should exist').not.toBeNull();

    host.form.controls.search.setValue('Alice Martin');
    fixture.detectChanges();
    await fixture.whenStable();

    expect(input?.value).toBe('Alice Martin');
  });

  it('updates the form control and emits search changes when the user types', async () => {
    const host = fixture.componentInstance;
    const input = fixture.nativeElement.querySelector('.ds-autocomplete__input') as HTMLInputElement | null;

    if (input) {
      input.value = 'Alice';
      input.dispatchEvent(new Event('input'));
    }
    fixture.detectChanges();
    await fixture.whenStable();

    expect(host.form.controls.search.value).toBe('Alice');
    expect(host.latestSearch).toBe('Alice');
  });

  it('shows a clear action and resets the control when it is clicked', async () => {
    const host = fixture.componentInstance;
    const input = fixture.nativeElement.querySelector('.ds-autocomplete__input') as HTMLInputElement | null;

    if (input) {
      input.value = 'Alice Martin';
      input.dispatchEvent(new Event('input'));
    }
    fixture.detectChanges();
    await fixture.whenStable();

    const clearButton = fixture.nativeElement.querySelector('.ds-field__clear') as HTMLButtonElement | null;

    expect(clearButton).withContext('clear button should be visible').not.toBeNull();

    clearButton?.click();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(host.form.controls.search.value).toBe('');
    expect(host.latestSearch).toBe('');
    expect(input?.value).toBe('');
  });
});