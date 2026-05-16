import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { CustomDomainResponse, DnsInstructionsResponse } from '../../../shared/interfaces/custom-domain.interface';
import { CustomDomainService } from '../../../shared/services/custom-domain.service';
import { DnsInstructionsDialogComponent, DnsInstructionsDialogData } from './dns-instructions-dialog.component';

describe('DnsInstructionsDialogComponent', () => {
  let fixture: ComponentFixture<DnsInstructionsDialogComponent>;
  let customDomainServiceSpy: jasmine.SpyObj<CustomDomainService>;
  let translateService: TranslateService;

  beforeEach(async () => {
    customDomainServiceSpy = jasmine.createSpyObj<CustomDomainService>('CustomDomainService', ['getDnsInstructions']);
    customDomainServiceSpy.getDnsInstructions.and.resolveTo(createInstructionsResponse());

    await TestBed.configureTestingModule({
      imports: [
        DnsInstructionsDialogComponent,
        TranslateModule.forRoot(),
      ],
      providers: [
        {
          provide: MAT_DIALOG_DATA,
          useValue: {
            resourceId: 'resource-1',
            resourceType: 'ContainerApp',
            domain: createCustomDomain(),
          } satisfies DnsInstructionsDialogData,
        },
        { provide: CustomDomainService, useValue: customDomainServiceSpy },
        { provide: MatSnackBar, useValue: jasmine.createSpyObj<MatSnackBar>('MatSnackBar', ['open']) },
      ],
    }).compileComponents();

    translateService = TestBed.inject(TranslateService);
    translateService.setTranslation('fr', {
      RESOURCE_EDIT: {
        CUSTOM_DOMAINS: {
          DNS_DIALOG_TITLE: 'Configuration DNS',
          DNS_DIALOG_SUBTITLE: 'Suivez ces étapes pour configurer votre domaine et récupérer les valeurs nécessaires dans le portail Azure.',
          DNS_DIALOG_PREREQUISITE_TITLE: 'Avant de commencer',
          DNS_DIALOG_PREREQUISITE: 'Commencez par déployer l’infrastructure. Tant que l’infra n’est pas déployée, Azure ne fournit pas le domaine par défaut ni le jeton de vérification nécessaires pour configurer le DNS.',
          DNS_DIALOG_RECORD_CARD: 'Enregistrement DNS à créer',
          DNS_DIALOG_RECORD_TYPE: 'Type',
          DNS_DIALOG_RECORD_NAME: 'Nom',
          DNS_DIALOG_RECORD_VALUE: 'Valeur',
          DNS_DIALOG_COPIED: 'Copié !',
          DNS_DIALOG_CLOSE: 'Fermer',
          DNS_DIALOG_STEPS: {
            CNAME_TITLE: 'Déployer l’infrastructure puis créer un enregistrement CNAME',
            CONTAINER_APP_CNAME_DESC: "Déployez d’abord l’infrastructure. Ensuite, dans le portail Azure, ouvrez votre Container App, puis allez dans Networking > Custom domains > Add custom domain. Si l’entrée réseau n’est pas encore activée, activez d’abord l’ingress sur le Container App. Dans la section Domain validation, copiez la valeur Generated domain et utilisez-la comme cible CNAME pour '{{domainName}}'. N’ouvrez pas l’environnement Container App pour cette étape.",
            TXT_TITLE: 'Créer un enregistrement TXT de validation',
            CONTAINER_APP_TXT_DESC: "Toujours dans le Container App, dans Networking > Custom domains > Add custom domain, récupérez la valeur Domain verification code affichée dans la section Domain validation. Créez ensuite un enregistrement TXT sur '{{recordName}}' avec cette valeur.",
            VALIDATE_TITLE: 'Valider le DNS',
            VALIDATE_DESC: 'Après propagation DNS, retournez dans le Container App sur Networking > Custom domains, relancez Add custom domain si nécessaire, puis cliquez sur Validate dans Azure. Une fois les valeurs reconnues, revenez ici et cliquez sur « Valider le DNS ».',
          },
        },
      },
    }, true);
    translateService.use('fr');

    fixture = TestBed.createComponent(DnsInstructionsDialogComponent);
  });

  it('renders localized French DNS instructions instead of backend English text', async () => {
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const renderedText = normalizeWhitespace(fixture.nativeElement.textContent ?? '');

    expect(customDomainServiceSpy.getDnsInstructions).toHaveBeenCalledOnceWith('resource-1', 'domain-1');
    expect(fixture.nativeElement.querySelector('app-ds-alert')).not.toBeNull();
    expect(renderedText).toContain('Avant de commencer');
    expect(renderedText).toContain('Commencez par déployer l’infrastructure. Tant que l’infra n’est pas déployée');
    expect(renderedText).toContain('Déployer l’infrastructure puis créer un enregistrement CNAME');
    expect(renderedText).toContain('portail Azure');
    expect(renderedText).toContain('Networking > Custom domains > Add custom domain');
    expect(renderedText).toContain('Domain validation');
    expect(renderedText).toContain('Generated domain');
    expect(renderedText).toContain('Domain verification code');
    expect(renderedText).toContain('activez d’abord l’ingress');
    expect(renderedText).toContain('N’ouvrez pas l’environnement Container App pour cette étape');
    expect(renderedText).toContain('Créer un enregistrement TXT de validation');
    expect(renderedText).not.toContain('Create a CNAME record');
    expect(renderedText).not.toContain('Create a TXT verification record');
  });
});

function createCustomDomain(): CustomDomainResponse {
  return {
    id: 'domain-1',
    resourceId: 'resource-1',
    environmentName: 'dev',
    domainName: 'infraflowsculptor.fr',
    bindingType: 'SniEnabled',
    dnsValidationStatus: 'Pending',
  };
}

function createInstructionsResponse(): DnsInstructionsResponse {
  return {
    domainName: 'infraflowsculptor.fr',
    dnsValidationStatus: 'Pending',
    steps: [
      {
        order: 1,
        title: 'Create a CNAME record',
        description: 'Point the domain to the container app environment default domain.',
        recordType: 'CNAME',
        recordName: 'infraflowsculptor.fr',
        recordValue: '<generated-domain-from-container-app-custom-domains>',
      },
      {
        order: 2,
        title: 'Create a TXT verification record',
        description: 'Create a TXT record for verification.',
        recordType: 'TXT',
        recordName: 'asuid.infraflowsculptor.fr',
        recordValue: '<domain-verification-code-from-container-app-custom-domains>',
      },
      {
        order: 3,
        title: 'Validate DNS',
        description: 'Validate the DNS once propagation is complete.',
        recordType: null,
        recordName: null,
        recordValue: null,
      },
    ],
  };
}

function normalizeWhitespace(value: string): string {
  return value.replace(/\s+/g, ' ').trim();
}