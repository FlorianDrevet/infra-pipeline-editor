import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';

import { ProjectResponse } from '../../../shared/interfaces/project.interface';
import { TagRequest } from '../../../shared/interfaces/infra-config.interface';
import { ProjectService } from '../../../shared/services/project.service';
import { DsButtonComponent, DsIconButtonComponent, DsTextFieldComponent } from '../../../shared/components/ds';

@Component({
  selector: 'app-project-detail-tags-section',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatChipsModule,
    MatIconModule,
    TranslateModule,
    DsButtonComponent,
    DsIconButtonComponent,
    DsTextFieldComponent,
  ],
  templateUrl: './project-detail-tags-section.component.html',
  styleUrl: './project-detail-tags-section.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProjectDetailTagsSectionComponent {
  private readonly projectService = inject(ProjectService);

  readonly project = input.required<ProjectResponse | null>();
  readonly canWrite = input.required<boolean>();

  protected readonly isEditingProjectTags = signal(false);
  protected readonly editingTags = signal<TagRequest[]>([]);
  protected readonly tagsErrorKey = signal('');
  protected readonly tagsSaving = signal(false);
  protected readonly tagNameCtrl = new FormControl('', { nonNullable: true });
  protected readonly tagValueCtrl = new FormControl('', { nonNullable: true });

  protected readonly projectTags = computed(() => this.project()?.tags ?? []);

  protected startEditProjectTags(): void {
    this.editingTags.set(this.projectTags().map(t => ({ name: t.name, value: t.value })));
    this.tagNameCtrl.reset();
    this.tagValueCtrl.reset();
    this.tagsErrorKey.set('');
    this.isEditingProjectTags.set(true);
  }

  protected addProjectTag(): void {
    const name = this.tagNameCtrl.value.trim();
    const value = this.tagValueCtrl.value.trim();
    if (!name || !value) return;
    if (this.editingTags().some(t => t.name === name)) return;
    this.editingTags.update(tags => [...tags, { name, value }]);
    this.tagNameCtrl.reset();
    this.tagValueCtrl.reset();
  }

  protected removeProjectTag(name: string): void {
    this.editingTags.update(tags => tags.filter(t => t.name !== name));
  }

  protected cancelProjectTagsEdit(): void {
    this.isEditingProjectTags.set(false);
    this.tagsErrorKey.set('');
  }

  protected async saveProjectTags(): Promise<void> {
    const projectId = this.project()?.id;
    if (!projectId) return;

    this.tagsSaving.set(true);
    this.tagsErrorKey.set('');

    try {
      await this.projectService.setTags(projectId, { tags: this.editingTags() });
      this.isEditingProjectTags.set(false);
    } catch {
      this.tagsErrorKey.set('PROJECT_DETAIL.TAGS.SAVE_ERROR');
    } finally {
      this.tagsSaving.set(false);
    }
  }
}
