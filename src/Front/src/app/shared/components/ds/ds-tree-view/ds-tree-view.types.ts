/**
 * Recursive node descriptor consumed by {@link DsTreeViewComponent}.
 *
 * Nodes are immutable from the tree's perspective — toggling expansion or
 * selection is delegated to the parent through outputs.
 *
 * @typeParam T Optional payload attached to each node, surfaced to consumers
 *              via the `data` field.
 */
export interface DsTreeNode<T = unknown> {
  /** Stable identifier (used for tracking, expansion state and selection). */
  readonly id: string;
  /** Visible label rendered next to the optional icon. */
  readonly label: string;
  /** Optional Material icon ligature rendered before the label. */
  readonly icon?: string;
  /** Optional badge text rendered as a small chip on the right. */
  readonly badge?: string;
  /** When true, the node is rendered greyed out and is not selectable. */
  readonly disabled?: boolean;
  /** Recursive children. When undefined or empty the chevron slot is hidden. */
  readonly children?: readonly DsTreeNode<T>[];
  /** Optional payload returned in `nodeClick` / `nodeToggle` events. */
  readonly data?: T;
}

/** Payload emitted by {@link DsTreeViewComponent.nodeToggle}. */
export interface DsTreeToggleEvent<T = unknown> {
  readonly node: DsTreeNode<T>;
  readonly expanded: boolean;
}
