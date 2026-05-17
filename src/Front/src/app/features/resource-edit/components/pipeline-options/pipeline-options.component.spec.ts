import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';

import { PipelineOptionsComponent } from './pipeline-options.component';

describe('PipelineOptionsComponent', () => {
  let fixture: ComponentFixture<PipelineOptionsComponent>;
  let component: PipelineOptionsComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PipelineOptionsComponent, TranslateModule.forRoot()],
    }).compileComponents();

    fixture = TestBed.createComponent(PipelineOptionsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
