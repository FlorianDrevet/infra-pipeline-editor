import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DsTreeViewComponent } from './ds-tree-view.component';

describe('DsTreeViewComponent', () => {
  let fixture: ComponentFixture<DsTreeViewComponent>;
  let component: DsTreeViewComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsTreeViewComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DsTreeViewComponent);
    component = fixture.componentInstance;
    component.nodes = [];
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
