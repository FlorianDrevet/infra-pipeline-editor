import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DsTableComponent } from './ds-table.component';

describe('DsTableComponent', () => {
  let fixture: ComponentFixture<DsTableComponent<unknown>>;
  let component: DsTableComponent<unknown>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsTableComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DsTableComponent);
    component = fixture.componentInstance;
    component.columns = [{ key: 'name', header: 'Name' }];
    component.rows = [];
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
