import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DsIconButtonComponent } from './ds-icon-button.component';

describe('DsIconButtonComponent', () => {
  let fixture: ComponentFixture<DsIconButtonComponent>;
  let component: DsIconButtonComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsIconButtonComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DsIconButtonComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('icon', 'close');
    fixture.componentRef.setInput('ariaLabel', 'Close');
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
