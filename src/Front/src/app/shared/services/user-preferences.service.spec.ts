import { TestBed } from '@angular/core/testing';

import { UserPreferencesService } from './user-preferences.service';

const USER_PREFERENCES_STORAGE_KEY = 'infra-flow-sculptor.user-preferences';

describe('UserPreferencesService', () => {
  let getItemSpy: jasmine.Spy<(key: string) => string | null>;
  let setItemSpy: jasmine.Spy<(key: string, value: string) => void>;

  beforeEach(() => {
    getItemSpy = spyOn(Storage.prototype, 'getItem').and.returnValue(null);
    setItemSpy = spyOn(Storage.prototype, 'setItem');

    TestBed.configureTestingModule({});
  });

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('loads the persisted Bicep viewer theme from local storage', () => {
    getItemSpy.and.returnValue(JSON.stringify({ bicepViewerTheme: 'graphite-frost' }));

    const service = TestBed.inject(UserPreferencesService);

    expect(service.bicepViewerTheme()).toBe('graphite-frost');
  });

  it('falls back to the default theme when persisted data is invalid', () => {
    getItemSpy.and.returnValue(JSON.stringify({ bicepViewerTheme: 'invalid-theme' }));

    const service = TestBed.inject(UserPreferencesService);

    expect(service.bicepViewerTheme()).toBe('azure-night');
  });

  it('persists Bicep viewer theme changes', () => {
    const service = TestBed.inject(UserPreferencesService);

    service.setBicepViewerTheme('sand-dusk');

    expect(service.bicepViewerTheme()).toBe('sand-dusk');
    expect(setItemSpy).toHaveBeenCalledOnceWith(
      USER_PREFERENCES_STORAGE_KEY,
      JSON.stringify({ bicepViewerTheme: 'sand-dusk', appTheme: 'dark' }),
    );
  });
});