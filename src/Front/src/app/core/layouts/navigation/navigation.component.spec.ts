import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { AccountInfo } from '@azure/msal-browser';
import { TranslateModule } from '@ngx-translate/core';

import { AuthenticationService } from '../../../shared/services/authentication.service';
import { AppLanguage, LanguageService, LanguageOption } from '../../../shared/services/language.service';
import { MicrosoftGraphProfilePhotoService } from '../../../shared/services/microsoft-graph-profile-photo.service';
import { MsalAuthService } from '../../../shared/services/msal-auth.service';
import { NavigationComponent } from './navigation.component';

describe('NavigationComponent', () => {
  let fixture: ComponentFixture<NavigationComponent>;
  let msalAuthServiceSpy: jasmine.SpyObj<MsalAuthService>;
  let graphProfilePhotoServiceSpy: jasmine.SpyObj<MicrosoftGraphProfilePhotoService>;
  let createObjectUrlSpy: jasmine.Spy<(blob: Blob) => string>;
  let revokeObjectUrlSpy: jasmine.Spy<(url: string) => void>;

  const selectedLanguage = signal<AppLanguage>('fr');
  const languageOptions: readonly LanguageOption[] = [
    { code: 'fr', labelKey: 'LANGUAGE.FRENCH_SHORT' },
    { code: 'en', labelKey: 'LANGUAGE.ENGLISH_SHORT' },
  ];

  const languageServiceStub: Pick<LanguageService, 'currentLanguage' | 'availableLanguages' | 'setLanguage'> = {
    currentLanguage: selectedLanguage.asReadonly(),
    availableLanguages: languageOptions,
    setLanguage: jasmine.createSpy('setLanguage'),
  };

  const authenticationServiceStub: Pick<AuthenticationService, 'getMsalAccount'> = {
    get getMsalAccount(): AccountInfo | null {
      return null;
    },
  };

  beforeEach(async () => {
    msalAuthServiceSpy = jasmine.createSpyObj<MsalAuthService>('MsalAuthService', ['getActiveAccount', 'logout']);
    graphProfilePhotoServiceSpy = jasmine.createSpyObj<MicrosoftGraphProfilePhotoService>(
      'MicrosoftGraphProfilePhotoService',
      ['getCurrentUserPhotoBlob']
    );
    createObjectUrlSpy = spyOn(globalThis.URL, 'createObjectURL').and.returnValue('blob:graph-photo');
    revokeObjectUrlSpy = spyOn(globalThis.URL, 'revokeObjectURL');

    await TestBed.configureTestingModule({
      imports: [NavigationComponent, TranslateModule.forRoot()],
      providers: [
        provideRouter([]),
        {
          provide: AuthenticationService,
          useValue: authenticationServiceStub,
        },
        {
          provide: LanguageService,
          useValue: languageServiceStub,
        },
        {
          provide: MsalAuthService,
          useValue: msalAuthServiceSpy,
        },
        {
          provide: MicrosoftGraphProfilePhotoService,
          useValue: graphProfilePhotoServiceSpy,
        },
      ],
    }).compileComponents();
  });

  it('Given_ProfilePhotoUrl_When_ComponentInitializes_Then_RendersMicrosoftPhoto', async () => {
    msalAuthServiceSpy.getActiveAccount.and.resolveTo(createAccountInfo());
    graphProfilePhotoServiceSpy.getCurrentUserPhotoBlob.and.resolveTo(new Blob(['graph-photo']));

    fixture = TestBed.createComponent(NavigationComponent);
    fixture.detectChanges();
    await waitForAsyncInitialization();
    fixture.detectChanges();

    const avatar = fixture.debugElement.query(By.css('.user-card__avatar')).nativeElement as HTMLElement;
    const avatarImage = avatar.querySelector<HTMLImageElement>('img.user-card__avatar-image');

    expect(createObjectUrlSpy).toHaveBeenCalled();
    expect(avatarImage?.getAttribute('src')).toBe('blob:graph-photo');
    expect(avatar.textContent?.trim()).toBe('');
  });

  it('Given_NoProfilePhoto_When_ComponentInitializes_Then_KeepsInitialsFallback', async () => {
    msalAuthServiceSpy.getActiveAccount.and.resolveTo(createAccountInfo());
    graphProfilePhotoServiceSpy.getCurrentUserPhotoBlob.and.resolveTo(null);

    fixture = TestBed.createComponent(NavigationComponent);
    fixture.detectChanges();
    await waitForAsyncInitialization();
    fixture.detectChanges();

    const avatar = fixture.debugElement.query(By.css('.user-card__avatar')).nativeElement as HTMLElement;
    const userName = fixture.debugElement.query(By.css('.user-card__details strong')).nativeElement as HTMLElement;
    const userEmail = fixture.debugElement.query(By.css('.user-card__details span')).nativeElement as HTMLElement;

    expect(avatar.querySelector('img.user-card__avatar-image')).toBeNull();
    expect(avatar.textContent?.trim()).toBe('JD');
    expect(userName.textContent?.trim()).toBe('John Doe');
    expect(userEmail.textContent?.trim()).toBe('john.doe@contoso.com');
  });

  it('Given_ProfilePhotoBlob_When_ComponentDestroys_Then_RevokesObjectUrl', async () => {
    msalAuthServiceSpy.getActiveAccount.and.resolveTo(createAccountInfo());
    graphProfilePhotoServiceSpy.getCurrentUserPhotoBlob.and.resolveTo(new Blob(['graph-photo']));

    fixture = TestBed.createComponent(NavigationComponent);
    fixture.detectChanges();
    await waitForAsyncInitialization();

    fixture.destroy();

    expect(revokeObjectUrlSpy).toHaveBeenCalledWith('blob:graph-photo');
  });

  it('Given_ComponentRenders_When_TopbarIsVisible_Then_ExposesSettingsShortcut', async () => {
    msalAuthServiceSpy.getActiveAccount.and.resolveTo(createAccountInfo());
    graphProfilePhotoServiceSpy.getCurrentUserPhotoBlob.and.resolveTo(null);

    fixture = TestBed.createComponent(NavigationComponent);
    fixture.detectChanges();
    await waitForAsyncInitialization();
    fixture.detectChanges();

    const settingsLink = fixture.debugElement.query(By.css('.topbar__settings'));

    expect(settingsLink).not.toBeNull();
    expect((settingsLink.nativeElement as HTMLAnchorElement).getAttribute('href')).toContain('/settings');
  });
});

function createAccountInfo(): AccountInfo {
  return {
    homeAccountId: 'home-account-id',
    environment: 'login.microsoftonline.com',
    tenantId: 'tenant-id',
    username: 'john.doe@contoso.com',
    localAccountId: 'local-account-id',
    name: 'John Doe',
  };
}

async function waitForAsyncInitialization(): Promise<void> {
  await Promise.resolve();
  await new Promise<void>((resolve) => {
    globalThis.setTimeout(resolve, 0);
  });
}