import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatDialog } from '@angular/material/dialog';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { By } from '@angular/platform-browser';
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
  let dialogSpy: jasmine.SpyObj<MatDialog>;

  const mockData: AddResourceDialogData = {
    resourceGroupId: 'rg-1',
    configId: 'cfg-1',
    projectId: 'proj-1',
    location: 'westeurope',
    environments: [{ name: 'Development' }],
  };

  beforeEach(async () => {
    dialogSpy = jasmine.createSpyObj<MatDialog>('MatDialog', ['open']);

    TestBed.overrideProvider(MatDialog, { useValue: dialogSpy });

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
            VNET_DNS_SERVERS: 'DNS servers',
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

  it('renders VNet-specific chip inputs without inline delimiter guidance in the add-resource flow', () => {
    const componentInstance = component as unknown as {
      applySelectedType(type: ResourceTypeEnum): void;
      step: { set(value: 'type' | 'plan-selection' | 'create-plan' | 'common' | 'environments'): void };
    };

    componentInstance.applySelectedType(ResourceTypeEnum.VirtualNetwork);
    componentInstance.step.set('common');
    fixture.detectChanges();

    const tagInputs = fixture.debugElement.queryAll(By.css('app-ds-tag-input'));
    const textContent = fixture.nativeElement.textContent as string;

    expect(tagInputs.length).toBe(2);
    expect(textContent).toContain('Enable DDoS protection');
    expect(textContent).toContain('Address spaces');
    expect(textContent).toContain('DNS servers');
    expect(textContent).not.toContain('Separate multiple ranges with commas or new lines.');
    expect(textContent).not.toContain('Enter IPv4 addresses separated by commas or new lines.');
  });

  it('opens the shared VNet help dialog from the add-resource VNet section', () => {
    const componentInstance = component as unknown as {
      applySelectedType(type: ResourceTypeEnum): void;
      step: { set(value: 'type' | 'plan-selection' | 'create-plan' | 'common' | 'environments'): void };
    };

    componentInstance.applySelectedType(ResourceTypeEnum.VirtualNetwork);
    componentInstance.step.set('common');
    fixture.detectChanges();

    const helpButton = fixture.debugElement.query(By.css('app-ds-panel-action-button button'));

    expect(helpButton).not.toBeNull();

    helpButton.nativeElement.click();

    expect(dialogSpy.open).toHaveBeenCalledOnceWith(jasmine.any(Function), jasmine.objectContaining({
      data: jasmine.objectContaining({ context: 'resourceCreate' }),
    }));
  });
});
