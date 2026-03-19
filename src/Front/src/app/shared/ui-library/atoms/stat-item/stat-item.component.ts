import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-stat-item',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './stat-item.component.html',
  styleUrl: './stat-item.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatItemComponent {
  @Input() icon!: string;
  @Input() value!: string | number;
  @Input() label!: string;
  @Input() color: 'primary' | 'accent' | 'warn' = 'primary';
}
