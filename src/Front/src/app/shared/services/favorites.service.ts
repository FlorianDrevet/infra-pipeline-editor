import { Injectable, signal, computed } from '@angular/core';

const STORAGE_KEY = 'ifs_favorite_projects';

@Injectable({
  providedIn: 'root',
})
export class FavoritesService {
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
    localStorage.setItem(STORAGE_KEY, JSON.stringify(this.favoriteIds()));
  }
}
