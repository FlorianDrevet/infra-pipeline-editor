import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule } from '@ngx-translate/core';
import { BootstrapSetupGuideComponent } from './bootstrap-setup-guide.component';

describe('BootstrapSetupGuideComponent', () => {
  let fixture: ComponentFixture<BootstrapSetupGuideComponent>;
  let component: BootstrapSetupGuideComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BootstrapSetupGuideComponent, TranslateModule.forRoot()],
    }).compileComponents();

    fixture = TestBed.createComponent(BootstrapSetupGuideComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });
});
