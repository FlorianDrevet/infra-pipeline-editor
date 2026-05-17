import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DsButtonComponent } from './ds-button.component';

describe('DsButtonComponent', () => {
  let fixture: ComponentFixture<DsButtonComponent>;
  let component: DsButtonComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsButtonComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DsButtonComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
