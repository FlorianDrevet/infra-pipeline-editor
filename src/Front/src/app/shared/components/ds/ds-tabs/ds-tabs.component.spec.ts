import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DsTabsComponent } from './ds-tabs.component';

describe('DsTabsComponent', () => {
  let fixture: ComponentFixture<DsTabsComponent>;
  let component: DsTabsComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsTabsComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DsTabsComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('tabs', [{ id: 'tab1', label: 'Tab 1' }]);
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
