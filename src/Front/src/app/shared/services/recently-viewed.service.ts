import { inject, Injectable, PLATFORM_ID, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { ProjectService } from './project.service';

const STORAGE_KEY = 'ifs_recently_viewed';
const MAX_ITEMS = 8;

export interface RecentlyViewedItem {
  id: string;
  name: string;
  type: 'project' | 'config';
  description?: string;
  timestamp: number;
}

@Injectable({
  providedIn: 'root',
})
export class RecentlyViewedService {
  private readonly projectService = inject(ProjectService);
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));
  private readonly items = signal<RecentlyViewedItem[]>(this.loadFromStorage());

  readonly recentItems = this.items.asReadonly();

  trackView(item: Omit<RecentlyViewedItem, 'timestamp'>): void {
    const current = this.items().filter((i) => !(i.id === item.id && i.type === item.type));
    const updated: RecentlyViewedItem[] = [
      { ...item, timestamp: Date.now() },
      ...current,
    ].slice(0, MAX_ITEMS);
    this.items.set(updated);
    this.saveToStorage();
  }

  /**
   * Validates recently viewed items against the backend.
   * Removes items the user no longer has access to and refreshes names.
   */
  async validateAndRefresh(): Promise<void> {
    const current = this.items();
    if (current.length === 0) return;

    try {
      const validatedItems = await this.projectService.validateRecentItems({
        items: current.map((i) => ({ id: i.id, type: i.type })),
      });

      const validatedMap = new Map(validatedItems.map((v) => [`${v.type}:${v.id}`, v]));

      const refreshed: RecentlyViewedItem[] = current
        .filter((item) => validatedMap.has(`${item.type}:${item.id}`))
        .map((item) => {
          const fresh = validatedMap.get(`${item.type}:${item.id}`)!;
          return {
            ...item,
            name: fresh.name,
            description: fresh.description,
          };
        });

      this.items.set(refreshed);
      this.saveToStorage();
    } catch {
      // If backend is unreachable, keep existing items as-is
    }
  }

  private loadFromStorage(): RecentlyViewedItem[] {
    if (!this.isBrowser) return [];
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (!raw) return [];
      const parsed = JSON.parse(raw);
      return Array.isArray(parsed)
        ? parsed.filter(
            (item): item is RecentlyViewedItem =>
              typeof item === 'object' &&
              item !== null &&
              typeof item.id === 'string' &&
              typeof item.name === 'string' &&
              typeof item.timestamp === 'number'
          )
        : [];
    } catch {
      return [];
    }
  }

  private saveToStorage(): void {
    if (!this.isBrowser) return;
    localStorage.setItem(STORAGE_KEY, JSON.stringify(this.items()));
  }
}
