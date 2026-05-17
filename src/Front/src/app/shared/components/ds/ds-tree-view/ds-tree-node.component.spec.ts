import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DsTreeNodeComponent } from './ds-tree-node.component';

describe('DsTreeNodeComponent', () => {
  let fixture: ComponentFixture<DsTreeNodeComponent>;
  let component: DsTreeNodeComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DsTreeNodeComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DsTreeNodeComponent);
    component = fixture.componentInstance;
    component.node = { id: 'node-1', label: 'Node 1' };
    component.depth = 0;
    component.expandedIds = new Set<string>();
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
