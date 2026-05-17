import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CompactSelectComponent } from './compact-select.component';

describe('CompactSelectComponent', () => {
  let fixture: ComponentFixture<CompactSelectComponent>;
  let component: CompactSelectComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CompactSelectComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(CompactSelectComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
