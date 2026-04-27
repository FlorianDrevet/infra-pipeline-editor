import { Injectable } from '@angular/core';
import { CreateProjectWizardDraft } from './create-project-wizard.types';

const STORAGE_KEY = 'ifs:create-project-draft';

/**
 * Persists the in-flight create-project wizard draft to sessionStorage so users can
 * leave and resume the dialog without losing data.
 */
@Injectable({ providedIn: 'root' })
export class CreateProjectWizardDraftService {
  public load(): CreateProjectWizardDraft | null {
    try {
      const raw = sessionStorage.getItem(STORAGE_KEY);
      if (!raw) {
        return null;
      }
      const parsed = JSON.parse(raw) as CreateProjectWizardDraft;
      return parsed;
    } catch {
      return null;
    }
  }

  public save(draft: CreateProjectWizardDraft): void {
    try {
      sessionStorage.setItem(STORAGE_KEY, JSON.stringify({ ...draft, updatedAt: Date.now() }));
    } catch {
      // sessionStorage may be unavailable (private mode, quota); silently ignore.
    }
  }

  public clear(): void {
    try {
      sessionStorage.removeItem(STORAGE_KEY);
    } catch {
      // ignore
    }
  }
}
