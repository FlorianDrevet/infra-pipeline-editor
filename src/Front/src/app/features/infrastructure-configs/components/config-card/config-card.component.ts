import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { InfrastructureConfigResponse, EnvironmentDefinitionResponse } from '../../../../shared/interfaces/infra-config.interface';
import { CardComponent, AvatarComponent, StatItemComponent } from '../../../../shared/ui-library';

@Component({
  selector: 'app-config-card',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatChipsModule,
    CardComponent,
    AvatarComponent,
    StatItemComponent,
  ],
  templateUrl: './config-card.component.html',
  styleUrl: './config-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfigCardComponent {
  @Input() config!: InfrastructureConfigResponse;

  getResourcesCount(): number {
    return (
      this.config.environmentDefinitions?.reduce((sum: number, env: EnvironmentDefinitionResponse) => {
        return sum + (env.tags?.length || 0);
      }, 0) || 0
    );
  }

  getFirstMemberInitial(id: string): string {
    return id.charAt(0).toUpperCase();
  }
}
