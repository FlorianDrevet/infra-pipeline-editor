import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  effect,
  model,
  output,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import {
  DsButtonComponent,
  DsTextFieldComponent,
  DsTextareaComponent,
} from '../../../../shared/components/ds';
import { CreateProjectWizardDraft } from '../create-project-wizard.types';

/**
 * Step 1 — Project identity (name + description) plus a Quick Start shortcut that
 * pre-fills the rest of the wizard with sensible defaults.
 */
@Component({
  selector: 'app-identity-step',
  standalone: true,
  imports: [
    FormsModule,
    TranslateModule,
    DsButtonComponent,
    DsTextFieldComponent,
    DsTextareaComponent,
  ],
  templateUrl: './identity-step.component.html',
  styleUrl: './identity-step.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IdentityStepComponent implements OnInit {
  public readonly draft = model.required<CreateProjectWizardDraft>();
  public readonly validityChange = output<boolean>();
  public readonly quickStart = output<void>();

  protected readonly trimmedName = computed(() => this.draft().name.trim());
  protected readonly nameValid = computed(() => {
    const length = this.trimmedName().length;
    return length >= 3 && length <= 80;
  });

  protected readonly descriptionTooLong = computed(() => this.draft().description.length > 1000);

  public constructor() {
    effect(() => {
      const valid = this.nameValid() && !this.descriptionTooLong();
      this.validityChange.emit(valid);
    });
  }

  public ngOnInit(): void {
    this.validityChange.emit(this.nameValid() && !this.descriptionTooLong());
  }

  protected onNameChange(value: string): void {
    this.draft.update((d) => ({ ...d, name: value }));
  }

  protected onDescriptionChange(value: string): void {
    this.draft.update((d) => ({ ...d, description: value }));
  }

  protected onQuickStart(): void {
    this.quickStart.emit();
  }
}
