import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { EmptyStateComponent } from '../../atoms/empty-state/empty-state.component';

export interface TableColumn {
  key: string;
  label: string;
  width?: string;
}

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatIconModule, MatButtonModule, EmptyStateComponent],
  templateUrl: './data-table.component.html',
  styleUrl: './data-table.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DataTableComponent {
  @Input() columns: TableColumn[] = [];
  @Input() dataSource: any[] = [];
  @Input() emptyMessage: string = 'No data available';
  @Input() emptyIcon: string = 'folder_open';

  get displayedColumns(): string[] {
    return this.columns.map((col) => col.key);
  }

  get hasData(): boolean {
    return this.dataSource && this.dataSource.length > 0;
  }

  getColumnLabel(columnKey: string): string {
    return this.columns.find((col) => col.key === columnKey)?.label || columnKey;
  }

  getColumnValue(item: any, columnKey: string): any {
    // Support nested properties like 'user.name'
    return columnKey.split('.').reduce((obj, key) => obj?.[key], item);
  }
}
