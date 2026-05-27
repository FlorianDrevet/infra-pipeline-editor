import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';

import { DsAlertComponent } from '../../../shared/components/ds';

@Component({
  selector: 'app-docker-image-instructions-dialog',
  standalone: true,
  imports: [
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    TranslateModule,
    DsAlertComponent,
  ],
  templateUrl: './docker-image-instructions-dialog.component.html',
  styleUrl: './docker-image-instructions-dialog.component.scss',
})
export class DockerImageInstructionsDialogComponent {}
