import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ProjectService } from '../../../shared/services/project.service';

export interface SearchResult {
  readonly id: string;
  readonly name: string;
  readonly type: 'project' | 'config';
  readonly description?: string;
}

@Component({
  selector: 'app-search-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatIconModule, TranslateModule],
  templateUrl: './search-dialog.component.html',
  styleUrl: './search-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SearchDialogComponent implements OnInit {
  private readonly dialogRef = inject(MatDialogRef<SearchDialogComponent>);
  private readonly router = inject(Router);
  private readonly projectService = inject(ProjectService);
  private readonly translate = inject(TranslateService);

  protected readonly searchCtrl = new FormControl('', { nonNullable: true });
  protected readonly results = signal<SearchResult[]>([]);
  protected readonly isSearching = signal(false);
  protected readonly selectedIndex = signal(0);

  private allItems: SearchResult[] = [];
  private debounceTimer: ReturnType<typeof setTimeout> | null = null;

  ngOnInit(): void {
    this.runTask(this.loadAllItems());
    this.searchCtrl.valueChanges.subscribe((value) => {
      this.filterResults(value);
    });
  }

  private async loadAllItems(): Promise<void> {
    try {
      const projects = await this.projectService.getMyProjects();
      this.allItems = projects.map((p) => ({
        id: p.id,
        name: p.name,
        type: 'project' as const,
        description: p.description,
      }));
      this.results.set(this.allItems.slice(0, 8));
    } catch {
      // Silent fail — search will still work on partial data
    }
  }

  private filterResults(query: string): void {
    if (this.debounceTimer) clearTimeout(this.debounceTimer);
    this.debounceTimer = setTimeout(() => {
      const q = query.toLowerCase().trim();
      if (!q) {
        this.results.set(this.allItems.slice(0, 8));
        this.selectedIndex.set(0);
        return;
      }
      const filtered = this.allItems.filter(
        (item) =>
          item.name.toLowerCase().includes(q) ||
          (item.description?.toLowerCase().includes(q) ?? false)
      );
      this.results.set(filtered.slice(0, 10));
      this.selectedIndex.set(0);
    }, 150);
  }

  protected onKeydown(event: KeyboardEvent): void {
    if (this.isResultEvent(event.target) && (event.key === 'Enter' || event.key === ' ')) {
      return;
    }

    const results = this.results();
    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        this.selectedIndex.set(Math.min(this.selectedIndex() + 1, results.length - 1));
        break;
      case 'ArrowUp':
        event.preventDefault();
        this.selectedIndex.set(Math.max(this.selectedIndex() - 1, 0));
        break;
      case 'Enter':
        event.preventDefault();
        if (results[this.selectedIndex()]) {
          this.navigateTo(results[this.selectedIndex()]);
        }
        break;
      case 'Escape':
        this.dialogRef.close();
        break;
    }
  }

  protected selectResult(index: number): void {
    this.selectedIndex.set(index);
  }

  protected onResultKeydown(event: KeyboardEvent, item: SearchResult): void {
    if (event.key !== 'Enter' && event.key !== ' ') {
      return;
    }

    event.preventDefault();
    event.stopPropagation();
    this.navigateTo(item);
  }

  protected navigateTo(item: SearchResult): void {
    this.dialogRef.close();
    if (item.type === 'project') {
      this.runNavigation(this.router.navigate(['/projects', item.id]));
    } else {
      this.runNavigation(this.router.navigate(['/config', item.id]));
    }
  }

  private runNavigation(navigationPromise: Promise<boolean>): void {
    navigationPromise.catch(() => undefined);
  }

  private runTask(taskPromise: Promise<void>): void {
    taskPromise.catch(() => undefined);
  }

  private isResultEvent(target: EventTarget | null): boolean {
    return target instanceof HTMLElement
      && target.closest('.search-dialog__result') !== null;
  }
}
