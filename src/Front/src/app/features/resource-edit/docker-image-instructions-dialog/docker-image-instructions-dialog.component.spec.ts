import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';

import { DockerImageInstructionsDialogComponent } from './docker-image-instructions-dialog.component';

describe('DockerImageInstructionsDialogComponent', () => {
  let fixture: ComponentFixture<DockerImageInstructionsDialogComponent>;
  let component: DockerImageInstructionsDialogComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DockerImageInstructionsDialogComponent, TranslateModule.forRoot()],
    }).compileComponents();

    fixture = TestBed.createComponent(DockerImageInstructionsDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
