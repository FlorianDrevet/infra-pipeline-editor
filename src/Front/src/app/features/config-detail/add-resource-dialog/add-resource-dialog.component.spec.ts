import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { AddResourceDialogComponent, AddResourceDialogData } from './add-resource-dialog.component';
import { NameAvailabilityService } from '../../../shared/services/name-availability.service';
import { AddResourceDialogPlanWorkflowService } from './add-resource-dialog-plan-workflow.service';
import { AddResourceDialogResourceSubmitterService } from './add-resource-dialog-resource-submitter.service';
import { ResourceTypeEnum } from '../enums/resource-type.enum';

describe('AddResourceDialogComponent', () => {
  let fixture: ComponentFixture<AddResourceDialogComponent>;
  let component: AddResourceDialogComponent;
  let translateService: TranslateService;

  const mockData: AddResourceDialogData = {
    resourceGroupId: 'rg-1',
    configId: 'cfg-1',
    projectId: 'proj-1',
    location: 'westeurope',
    environments: [{ name: 'Development' }],
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddResourceDialogComponent, TranslateModule.forRoot()],
      providers: [
        provideNoopAnimations(),
        { provide: MatDialogRef, useValue: jasmine.createSpyObj('MatDialogRef', ['close']) },
        { provide: MAT_DIALOG_DATA, useValue: mockData },
        {
          provide: NameAvailabilityService,
          useValue: jasmine.createSpyObj('NameAvailabilityService', ['checkAvailability']),
        },
        {
          provide: AddResourceDialogPlanWorkflowService,
          useValue: jasmine.createSpyObj('AddResourceDialogPlanWorkflowService', ['selectExistingPlan', 'createNewPlan']),
        },
        {
          provide: AddResourceDialogResourceSubmitterService,
          useValue: jasmine.createSpyObj('AddResourceDialogResourceSubmitterService', ['submit']),
        },
      ],
    }).compileComponents();

    translateService = TestBed.inject(TranslateService);
    translateService.setTranslation('en', {
      ADD_RESOURCE_DIALOG: {
        IS_EXISTING: 'Existing resource (already deployed in Azure)',
      },
      CONFIG_DETAIL: {
        RESOURCES: {
          FORM: {
            COMMON_FIELDS: 'Common fields',
            NAME: 'Name',
            NAME_PLACEHOLDER: 'resource-name',
            LOCATION: 'Azure region',
            VNET_ADDRESS_SPACES: 'Address spaces',
            VNET_ADDRESS_SPACES_HINT: 'Use CIDR ranges such as 10.0.0.0/16. Separate multiple ranges with commas or new lines.',
            VNET_DNS_SERVERS: 'DNS servers',
            VNET_DNS_SERVERS_HINT: 'Optional. Enter IPv4 addresses separated by commas or new lines.',
          },
        },
      },
      RESOURCE_EDIT: {
        FIELDS: {
          ENABLE_DDOS_PROTECTION: 'Enable DDoS protection',
        },
      },
    });
    translateService.use('en');

    fixture = TestBed.createComponent(AddResourceDialogComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('renders VNet-specific common inputs and guidance in the add-resource flow', () => {
    const componentInstance = component as unknown as {
      applySelectedType(type: ResourceTypeEnum): void;
      step: { set(value: 'type' | 'plan-selection' | 'create-plan' | 'common' | 'environments'): void };
    };

    componentInstance.applySelectedType(ResourceTypeEnum.VirtualNetwork);
    componentInstance.step.set('common');
    fixture.detectChanges();

    const textContent = fixture.nativeElement.textContent as string;

    expect(textContent).toContain('Enable DDoS protection');
    expect(textContent).toContain('Address spaces');
    expect(textContent).toContain('Use CIDR ranges such as 10.0.0.0/16. Separate multiple ranges with commas or new lines.');
    expect(textContent).toContain('DNS servers');
    expect(textContent).toContain('Optional. Enter IPv4 addresses separated by commas or new lines.');
  });
});
