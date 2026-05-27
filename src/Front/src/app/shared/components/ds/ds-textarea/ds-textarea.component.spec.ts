import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DsTextareaComponent } from './ds-textarea.component';

describe('DsTextareaComponent', () => {
  let fixture: ComponentFixture<DsTextareaComponent>;
  let component: DsTextareaComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsTextareaComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DsTextareaComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
