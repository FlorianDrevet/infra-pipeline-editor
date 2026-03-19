import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

export type CardVariant = 'default' | 'outlined' | 'elevated';

@Component({
  selector: 'app-card',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule, MatButtonModule],
  templateUrl: './card.component.html',
  styleUrl: './card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CardComponent {
  @Input() variant: CardVariant = 'default';
  @Input() clickable: boolean = false;
  @Output() cardClick = new EventEmitter<void>();

  get cardClass(): string {
    return `card--${this.variant}`;
  }

  onClick(): void {
    if (this.clickable) {
      this.cardClick.emit();
    }
  }
}
