import { Injectable, signal } from '@angular/core';

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

  private loadFromStorage(): RecentlyViewedItem[] {
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
    localStorage.setItem(STORAGE_KEY, JSON.stringify(this.items()));
  }
}
