import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';

import { ProjectResponse } from '../../../shared/interfaces/project.interface';
import { TagRequest } from '../../../shared/interfaces/infra-config.interface';
import { ProjectService } from '../../../shared/services/project.service';
import { DsButtonComponent, DsChipComponent, DsIconButtonComponent, DsKeyValueInputComponent, DsKeyValueItem } from '../../../shared/components/ds';

@Component({
  selector: 'app-project-detail-tags-section',
  standalone: true,
  imports: [
    FormsModule,
    MatIconModule,
    TranslateModule,
    DsButtonComponent,
    DsChipComponent,
    DsIconButtonComponent,
    DsKeyValueInputComponent,
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

  protected readonly projectTags = computed(() => this.project()?.tags ?? []);

  protected readonly editingKvItems = computed<DsKeyValueItem[]>(() =>
    this.editingTags().map(t => ({ key: t.name, value: t.value })),
  );

  protected startEditProjectTags(): void {
    this.editingTags.set(this.projectTags().map(t => ({ name: t.name, value: t.value })));
    this.tagsErrorKey.set('');
    this.isEditingProjectTags.set(true);
  }

  protected onTagsChange(items: DsKeyValueItem[]): void {
    this.editingTags.set(items.map(item => ({ name: item.key, value: item.value })));
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
