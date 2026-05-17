import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DsBannerComponent } from './ds-banner.component';

describe('DsBannerComponent', () => {
  let fixture: ComponentFixture<DsBannerComponent>;
  let component: DsBannerComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsBannerComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DsBannerComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
