import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';
import { DiagnosticPopoverComponent } from './diagnostic-popover.component';

describe('DiagnosticPopoverComponent', () => {
  let fixture: ComponentFixture<DiagnosticPopoverComponent>;
  let component: DiagnosticPopoverComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DiagnosticPopoverComponent, TranslateModule.forRoot()],
    }).compileComponents();

    fixture = TestBed.createComponent(DiagnosticPopoverComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('diagnostics', []);
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
