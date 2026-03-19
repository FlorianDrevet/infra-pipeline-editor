import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTabsModule } from '@angular/material/tabs';
import { MatIconModule } from '@angular/material/icon';

export interface TabItem {
  icon?: string;
  label: string;
  content: string; // Template reference name or component
}

@Component({
  selector: 'app-tabbed-view',
  standalone: true,
  imports: [CommonModule, MatTabsModule, MatIconModule],
  templateUrl: './tabbed-view.component.html',
  styleUrl: './tabbed-view.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TabbedViewComponent {
  @Input() tabs: TabItem[] = [];
  @Input() backgroundColor: string = 'transparent';
}
