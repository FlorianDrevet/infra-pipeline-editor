import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DsOptionCardComponent } from './ds-option-card.component';

describe('DsOptionCardComponent', () => {
  let fixture: ComponentFixture<DsOptionCardComponent>;
  let component: DsOptionCardComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsOptionCardComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DsOptionCardComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('icon', 'folder');
    fixture.componentRef.setInput('title', 'Option');
    fixture.componentRef.setInput('description', 'A test option');
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
