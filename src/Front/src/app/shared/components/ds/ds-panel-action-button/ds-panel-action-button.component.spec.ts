import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DsPanelActionButtonComponent } from './ds-panel-action-button.component';

describe('DsPanelActionButtonComponent', () => {
  let fixture: ComponentFixture<DsPanelActionButtonComponent>;
  let component: DsPanelActionButtonComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsPanelActionButtonComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DsPanelActionButtonComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('icon', 'edit');
    fixture.componentRef.setInput('ariaLabel', 'Edit');
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
