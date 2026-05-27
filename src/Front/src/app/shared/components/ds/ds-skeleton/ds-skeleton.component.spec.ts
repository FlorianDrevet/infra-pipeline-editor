import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DsSkeletonComponent } from './ds-skeleton.component';

describe('DsSkeletonComponent', () => {
  let fixture: ComponentFixture<DsSkeletonComponent>;
  let component: DsSkeletonComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsSkeletonComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DsSkeletonComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
