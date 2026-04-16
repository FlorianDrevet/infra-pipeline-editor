import { inject, Injectable, PLATFORM_ID, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

const STORAGE_KEY = 'ifs_favorite_projects';

@Injectable({
  providedIn: 'root',
})
export class FavoritesService {
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));
  private readonly favoriteIds = signal<string[]>(this.loadFromStorage());

  readonly favorites = this.favoriteIds.asReadonly();

  isFavorite(projectId: string): boolean {
    return this.favoriteIds().includes(projectId);
  }

  toggle(projectId: string): void {
    const current = this.favoriteIds();
    if (current.includes(projectId)) {
      this.favoriteIds.set(current.filter((id) => id !== projectId));
    } else {
      this.favoriteIds.set([...current, projectId]);
    }
    this.saveToStorage();
  }

  private loadFromStorage(): string[] {
    if (!this.isBrowser) return [];
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (!raw) return [];
      const parsed = JSON.parse(raw);
      return Array.isArray(parsed) ? parsed.filter((id): id is string => typeof id === 'string') : [];
    } catch {
      return [];
    }
  }

  private saveToStorage(): void {
    if (!this.isBrowser) return;
    localStorage.setItem(STORAGE_KEY, JSON.stringify(this.favoriteIds()));
  }
}
