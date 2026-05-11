import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { NgClass } from '@angular/common';

import { MatIconModule } from '@angular/material/icon';

import { DsChipComponent } from '../ds-chip/ds-chip.component';
import { DsTreeNode } from './ds-tree-view.types';

/**
 * Internal recursive renderer for a single {@link DsTreeNode} and its
 * descendants. Delegates click / toggle / keyboard interactions back to the
 * top-level tree through outputs.
 *
 * The component is pure — it owns no expansion state; the parent passes in
 * the live `expandedIds` and `selectedId` snapshots.
 */
@Component({
  selector: 'app-ds-tree-node',
  standalone: true,
  imports: [NgClass, MatIconModule, DsChipComponent],
  templateUrl: './ds-tree-node.component.html',
  styleUrl: './ds-tree-node.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DsTreeNodeComponent {
  @Input({ required: true }) public node!: DsTreeNode<unknown>;
  @Input({ required: true }) public depth!: number;
  @Input({ required: true }) public expandedIds!: ReadonlySet<string>;
  @Input() public selectedId: string | null = null;

  @Output() public readonly nodeActivate = new EventEmitter<DsTreeNode<unknown>>();
  @Output() public readonly nodeToggle = new EventEmitter<DsTreeNode<unknown>>();
  @Output() public readonly nodeKey = new EventEmitter<{ node: DsTreeNode<unknown>; event: KeyboardEvent }>();

  protected get hasChildren(): boolean {
    return !!this.node.children && this.node.children.length > 0;
  }

  protected get expanded(): boolean {
    return this.hasChildren && this.expandedIds.has(this.node.id);
  }

  protected get isSelected(): boolean {
    return this.selectedId === this.node.id;
  }

  protected get indentPx(): number {
    return 8 + this.depth * 16;
  }

  protected onRowClick(): void {
    if (this.node.disabled) {
      return;
    }
    this.nodeActivate.emit(this.node);
  }

  protected onChevronClick(event: MouseEvent): void {
    event.stopPropagation();
    if (this.node.disabled || !this.hasChildren) {
      return;
    }
    this.nodeToggle.emit(this.node);
  }

  protected onKeyDown(event: KeyboardEvent): void {
    this.nodeKey.emit({ node: this.node, event });
  }

  protected onChildActivate(node: DsTreeNode<unknown>): void {
    this.nodeActivate.emit(node);
  }

  protected onChildToggle(node: DsTreeNode<unknown>): void {
    this.nodeToggle.emit(node);
  }

  protected onChildKey(payload: { node: DsTreeNode<unknown>; event: KeyboardEvent }): void {
    this.nodeKey.emit(payload);
  }

  protected trackChild = (_: number, child: DsTreeNode<unknown>): string => child.id;
}
