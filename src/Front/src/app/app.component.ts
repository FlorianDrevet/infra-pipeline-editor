import {
  afterNextRender,
  Component,
  CUSTOM_ELEMENTS_SCHEMA,
  DestroyRef,
  ElementRef,
  inject,
  viewChild,
} from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { FooterComponent } from './core/layouts/footer/footer.component';
import { NavigationComponent } from './core/layouts/navigation/navigation.component';
import { toSignal } from '@angular/core/rxjs-interop';
import { filter, map, startWith } from 'rxjs';
import { TranslateModule } from '@ngx-translate/core';
import 'agent-ui-annotation';
import type { AnnotationElement } from 'agent-ui-annotation';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, NavigationComponent, FooterComponent, TranslateModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class AppComponent {
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly annotationRef = viewChild<ElementRef<AnnotationElement>>('annotationRef');

  protected isLoginPage = toSignal(
    this.router.events.pipe(
      filter((e) => e instanceof NavigationEnd),
      map((e) => (e as NavigationEnd).urlAfterRedirects.startsWith('/login')),
      startWith(this.router.url.startsWith('/login'))
    )
  );

  constructor() {
    afterNextRender(() => {
      const annotationElement = this.annotationRef()?.nativeElement;

      if (!annotationElement) {
        return;
      }

      annotationElement.setBeforeCreateHook(() => ({
        context: {
          route: this.router.url,
          createdAt: new Date().toISOString(),
        },
      }));

      annotationElement.addEventListener('annotation:create', this.onAnnotationCreate as EventListener);

      this.destroyRef.onDestroy(() => {
        annotationElement.removeEventListener('annotation:create', this.onAnnotationCreate as EventListener);
      });
    });
  }

  protected activateAnnotation(): void {
    this.annotationRef()?.nativeElement.activate();
  }

  private readonly onAnnotationCreate = (event: Event): void => {
    void event;
  };
}
