import { DecimalPipe } from '@angular/common';
import { HttpClient, HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { bootstrapApplication } from '@angular/platform-browser';
import { Subscription } from 'rxjs';

interface WavMetadata {
  durationSeconds: number;
  channels: number;
  sampleRateHz: number;
}

@Component({
  selector: 'sonora-app',
  imports: [DecimalPipe],
  templateUrl: './app.html',
})
class App {
  private readonly http = inject(HttpClient);
  private inspection?: Subscription;
  readonly source = signal('');
  readonly metadata = signal<WavMetadata | null>(null);
  readonly loading = signal(false);
  readonly error = signal('');

  constructor() {
    inject(DestroyRef).onDestroy(() => this.clearSelection());
  }

  selectFile(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.clearSelection();
    input.value = '';
    if (file.size > 20 * 1024 * 1024) {
      this.error.set('O limite por arquivo é de 20 MiB.');
      return;
    }

    const form = new FormData();
    form.set('file', file);
    this.loading.set(true);
    this.inspection = this.http.post<WavMetadata>('/api/audio/inspect', form).subscribe({
      next: (metadata) => {
        this.source.set(URL.createObjectURL(file));
        this.metadata.set(metadata);
        this.loading.set(false);
      },
      error: (failure: HttpErrorResponse) => {
        const title = failure.error?.title;
        this.error.set(typeof title === 'string'
          ? title
          : 'A API não respondeu. Confira se o backend está em execução.');
        this.loading.set(false);
      },
    });
  }

  private clearSelection(): void {
    this.inspection?.unsubscribe();
    if (this.source()) URL.revokeObjectURL(this.source());
    this.source.set('');
    this.metadata.set(null);
    this.error.set('');
    this.loading.set(false);
  }
}

bootstrapApplication(App, { providers: [provideHttpClient()] }).catch(console.error);
