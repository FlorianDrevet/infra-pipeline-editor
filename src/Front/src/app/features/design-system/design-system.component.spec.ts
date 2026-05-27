import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DesignSystemComponent } from './design-system.component';

describe('DesignSystemComponent', () => {
  let fixture: ComponentFixture<DesignSystemComponent>;
  let component: DesignSystemComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DesignSystemComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DesignSystemComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
