import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Input,
  Output,
  TemplateRef,
} from '@angular/core';
import { NgClass, NgTemplateOutlet } from '@angular/common';

import { MatIconModule } from '@angular/material/icon';

import {
  DsTableColumn,
  DsTableDensity,
  DsTableSortDirection,
  DsTableSortState,
} from './ds-table.types';

/**
 * Design system data table (V3). Renders a CSS-grid table driven by an
 * immutable column descriptor list and a row array. Custom cell rendering
 * is opt-in through the `cellTemplates` map keyed by column id.
 *
 * Sort state is externally owned: the table emits `sortChange` cycling
 * through `null` → `asc` → `desc` → `null` and the parent updates the
 * `sortState` input. Default cell content falls back to `row[column.key]`
 * coerced to string.
 *
 * @typeParam T Row type. The default cell template uses an indexed access on
 *              `row[column.key]`, which works for any plain object shape.
 */
@Component({
  selector: 'app-ds-table',
  standalone: true,
  imports: [NgClass, NgTemplateOutlet, MatIconModule],
  templateUrl: './ds-table.component.html',
  styleUrl: './ds-table.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DsTableComponent<T> {
  @Input({ required: true }) public columns!: readonly DsTableColumn<T>[];
  @Input({ required: true }) public rows!: readonly T[];
  @Input() public density: DsTableDensity = 'cozy';
  @Input() public sortState: DsTableSortState | null = null;
  @Input() public emptyMessage = 'No data';
  @Input() public ariaLabel?: string;
  @Input() public cellTemplates: Readonly<Record<string, TemplateRef<unknown>>> = {};
  @Input() public trackBy: (index: number, row: T) => unknown = (i) => i;

  @Output() public readonly sortChange = new EventEmitter<DsTableSortState>();
  @Output() public readonly rowClick = new EventEmitter<T>();

  protected get gridTemplateColumns(): string {
    return this.columns
      .map((column) => column.width ?? 'minmax(120px, 1fr)')
      .join(' ');
  }

  protected get rowHeightVar(): string {
    switch (this.density) {
      case 'compact':
        return 'var(--ifs-density-row-sm)';
      case 'comfortable':
        return 'var(--ifs-density-row-lg)';
      default:
        return 'var(--ifs-density-row-md)';
    }
  }

  protected get isInteractive(): boolean {
    return this.rowClick.observed;
  }

  protected getCellValue(row: T, column: DsTableColumn<T>): string {
    const raw = (row as unknown as Record<string, unknown>)[column.key];
    return this.stringifyCellValue(raw);
  }

  protected getColumnSortDirection(column: DsTableColumn<T>): DsTableSortDirection {
    if (this.sortState?.key !== column.key) {
      return null;
    }
    return this.sortState.direction;
  }

  protected onHeaderClick(column: DsTableColumn<T>): void {
    if (!column.sortable) {
      return;
    }
    const current = this.getColumnSortDirection(column);
    let next: DsTableSortDirection;
    if (current === null) {
      next = 'asc';
    } else if (current === 'asc') {
      next = 'desc';
    } else {
      next = null;
    }
    this.sortChange.emit({ key: column.key, direction: next });
  }

  protected onRowClick(row: T): void {
    if (!this.isInteractive) {
      return;
    }
    this.rowClick.emit(row);
  }

  protected onRowKeyDown(event: KeyboardEvent, row: T): void {
    if (!this.isInteractive || (event.key !== 'Enter' && event.key !== ' ')) {
      return;
    }

    event.preventDefault();
    this.rowClick.emit(row);
  }

  private stringifyCellValue(raw: unknown): string {
    if (raw === null || raw === undefined) {
      return '';
    }

    if (typeof raw === 'string') {
      return raw;
    }

    if (typeof raw === 'number' || typeof raw === 'boolean' || typeof raw === 'bigint') {
      return String(raw);
    }

    if (raw instanceof Date) {
      return raw.toISOString();
    }

    if (Array.isArray(raw)) {
      return raw.map((item) => this.stringifyCellValue(item)).join(', ');
    }

    try {
      return JSON.stringify(raw);
    } catch {
      return Object.prototype.toString.call(raw);
    }
  }

  protected trackColumn = (_: number, column: DsTableColumn<T>): string => column.key;
  protected trackRow = (index: number, row: T): unknown => this.trackBy(index, row);
}
