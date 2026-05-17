import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DsToggleComponent } from './ds-toggle.component';

describe('DsToggleComponent', () => {
  let fixture: ComponentFixture<DsToggleComponent>;
  let component: DsToggleComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsToggleComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DsToggleComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
