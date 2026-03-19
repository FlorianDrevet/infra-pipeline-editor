import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

export type AvatarSize = 'xs' | 'sm' | 'md' | 'lg' | 'xl';
export type AvatarVariant = 'primary' | 'secondary' | 'accent' | 'success' | 'warning' | 'error';

@Component({
  selector: 'app-avatar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './avatar.component.html',
  styleUrl: './avatar.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AvatarComponent {
  @Input() initials: string = '';
  @Input() size: AvatarSize = 'md';
  @Input() variant: AvatarVariant = 'primary';
  @Input() tooltip?: string;

  get sizeClass(): string {
    return `avatar--${this.size}`;
  }

  get variantClass(): string {
    return `avatar--${this.variant}`;
  }
}
