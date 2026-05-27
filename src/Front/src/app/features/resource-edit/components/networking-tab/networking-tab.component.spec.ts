import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateModule } from '@ngx-translate/core';

import { PrivateEndpointService } from '../../../../shared/services/private-endpoint.service';
import { NetworkingTabComponent } from './networking-tab.component';

describe('NetworkingTabComponent', () => {
  let fixture: ComponentFixture<NetworkingTabComponent>;
  let component: NetworkingTabComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NetworkingTabComponent, TranslateModule.forRoot()],
      providers: [
        {
          provide: PrivateEndpointService,
          useValue: jasmine.createSpyObj<PrivateEndpointService>('PrivateEndpointService', ['getByResourceId', 'add', 'remove']),
        },
        {
          provide: MatSnackBar,
          useValue: jasmine.createSpyObj<MatSnackBar>('MatSnackBar', ['open']),
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(NetworkingTabComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('resourceId', 'res-1');
    fixture.componentRef.setInput('resourceType', 'KeyVault');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
