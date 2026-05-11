import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

import { DsTreeNodeComponent } from './ds-tree-node.component';
import { DsTreeNode, DsTreeToggleEvent } from './ds-tree-view.types';

/**
 * Design system tree view (V3). Renders a recursive `role="tree"` driven by
 * an immutable `nodes` input. Expansion and selection are externally owned —
 * the parent passes in the live `expandedIds` set and `selectedId`, and
 * receives `nodeClick`, `nodeToggle` events to mutate them.
 *
 * Keyboard navigation:
 * - ArrowUp / ArrowDown move focus across visible nodes.
 * - ArrowRight expands a collapsed node, or jumps to first child if expanded.
 * - ArrowLeft collapses an expanded node, or jumps to parent if collapsed.
 * - Enter / Space activate the focused node.
 *
 * @typeParam T Optional node payload, surfaced through the events.
 */
@Component({
  selector: 'app-ds-tree-view',
  standalone: true,
  imports: [DsTreeNodeComponent],
  templateUrl: './ds-tree-view.component.html',
  styleUrl: './ds-tree-view.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DsTreeViewComponent<T = unknown> {
  @Input({ required: true }) public nodes!: readonly DsTreeNode<T>[];
  @Input() public expandedIds: ReadonlySet<string> = new Set<string>();
  @Input() public selectedId: string | null = null;
  @Input() public ariaLabel?: string;

  @Output() public readonly nodeClick = new EventEmitter<DsTreeNode<T>>();
  @Output() public readonly nodeToggle = new EventEmitter<DsTreeToggleEvent<T>>();

  protected onActivate(node: DsTreeNode<unknown>): void {
    this.nodeClick.emit(node as DsTreeNode<T>);
  }

  protected onToggle(node: DsTreeNode<unknown>): void {
    const expanded = !this.expandedIds.has(node.id);
    this.nodeToggle.emit({ node: node as DsTreeNode<T>, expanded });
  }

  protected onKey(payload: { node: DsTreeNode<unknown>; event: KeyboardEvent }): void {
    const { node, event } = payload;
    const key = event.key;

    if (key === 'Enter' || key === ' ') {
      event.preventDefault();
      this.nodeClick.emit(node as DsTreeNode<T>);
      return;
    }

    if (key === 'ArrowRight') {
      const hasChildren = !!node.children && node.children.length > 0;
      if (hasChildren && !this.expandedIds.has(node.id)) {
        event.preventDefault();
        this.nodeToggle.emit({ node: node as DsTreeNode<T>, expanded: true });
      }
      return;
    }

    if (key === 'ArrowLeft') {
      if (this.expandedIds.has(node.id)) {
        event.preventDefault();
        this.nodeToggle.emit({ node: node as DsTreeNode<T>, expanded: false });
      }
      return;
    }
  }

  protected trackNode = (_: number, node: DsTreeNode<T>): string => node.id;
}
