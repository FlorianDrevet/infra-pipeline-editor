import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ToggleSectionCardComponent } from './toggle-section-card.component';

describe('ToggleSectionCardComponent', () => {
  let fixture: ComponentFixture<ToggleSectionCardComponent>;
  let component: ToggleSectionCardComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ToggleSectionCardComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ToggleSectionCardComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('icon', 'settings');
    fixture.componentRef.setInput('title', 'Test Section');
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
