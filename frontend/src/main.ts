import { DecimalPipe } from '@angular/common';
import { HttpClient, HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { Component, computed, DestroyRef, inject, signal } from '@angular/core';
import { bootstrapApplication } from '@angular/platform-browser';
import { Subscription } from 'rxjs';
import { demoTone } from './demo-tone';

interface WavMetadata {
  durationSeconds: number;
  channels: number;
  sampleRateHz: number;
  waveform: { min: number; max: number }[];
}

interface Spectrum {
  dominantFrequencyHz: number | null;
  sampleCount: number;
  fftSize: number;
  binSpacingHz: number;
  nyquistHz: number;
  points: { frequencyHz: number; magnitude: number }[];
}

@Component({
  selector: 'sonora-app',
  imports: [DecimalPipe],
  templateUrl: './app.html',
})
class App {
  private readonly http = inject(HttpClient);
  private inspection?: Subscription;
  private analysis?: Subscription;
  private file?: File;
  private dragStart?: number;

  readonly source = signal('');
  readonly metadata = signal<WavMetadata | null>(null);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly start = signal(0);
  readonly end = signal(0);
  readonly spectrum = signal<Spectrum | null>(null);
  readonly analyzing = signal(false);
  readonly analysisError = signal('');
  readonly validRange = computed(() => {
    const wav = this.metadata();
    if (!wav || this.start() < 0 || this.end() > wav.durationSeconds) return false;
    const samples = Math.round(this.end() * wav.sampleRateHz) - Math.round(this.start() * wav.sampleRateHz);
    return samples >= 4 && samples <= Math.min(524288, 5 * wav.sampleRateHz);
  });

  readonly waveformPath = computed(() => {
    const blocks = this.metadata()?.waveform ?? [];
    return blocks.map(({ min, max }, i) => {
      const x = (i + 0.5) * 800 / blocks.length;
      return `M${x} ${60 - max * 58}V${60 - min * 58}`;
    }).join(' ');
  });

  readonly spectrumPath = computed(() => {
    const result = this.spectrum();
    if (!result) return '';
    const peak = Math.max(...result.points.map(point => point.magnitude));
    return result.points.map(point => {
      const x = point.frequencyHz * 800 / result.nyquistHz;
      const height = peak > 0 ? point.magnitude * 113 / peak : 0;
      return `M${x} 119V${119 - height}`;
    }).join(' ');
  });

  constructor() {
    inject(DestroyRef).onDestroy(() => this.clearSelection());
  }

  selectFile(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (file) this.inspect(file);
  }

  loadDemo(): void {
    this.inspect(demoTone());
  }

  private inspect(file: File): void {
    this.clearSelection();
    if (file.size > 20 * 1024 * 1024) {
      this.error.set('O limite por arquivo é de 20 MiB.');
      return;
    }

    const form = new FormData();
    form.set('file', file);
    this.loading.set(true);
    this.inspection = this.http.post<WavMetadata>('/api/audio/inspect', form).subscribe({
      next: (metadata) => {
        this.file = file;
        this.source.set(URL.createObjectURL(file));
        this.metadata.set(metadata);
        this.end.set(Math.min(1, metadata.durationSeconds, 524288 / metadata.sampleRateHz));
        this.loading.set(false);
      },
      error: (failure: HttpErrorResponse) => {
        this.error.set(this.errorMessage(failure));
        this.loading.set(false);
      },
    });
  }

  selectBoundary(boundary: 'start' | 'end', event: Event): void {
    const value = (event.target as HTMLInputElement).valueAsNumber;
    this.clearAnalysis();
    this[boundary].set(value);
  }

  beginDrag(event: PointerEvent): void {
    if (!event.isPrimary || event.button !== 0) return;
    (event.currentTarget as SVGSVGElement).setPointerCapture(event.pointerId);
    this.clearAnalysis();
    this.dragStart = this.pointerTime(event);
    this.start.set(this.dragStart);
    this.end.set(this.dragStart);
  }

  updateDrag(event: PointerEvent): void {
    if (!event.isPrimary || this.dragStart === undefined) return;
    const time = this.pointerTime(event);
    this.start.set(Math.min(time, this.dragStart));
    this.end.set(Math.max(time, this.dragStart));
  }

  endDrag(): void {
    this.dragStart = undefined;
  }

  analyze(): void {
    if (!this.file || !this.validRange() || this.analyzing()) return;
    this.clearAnalysis();
    const form = new FormData();
    form.set('file', this.file);
    form.set('startSeconds', String(this.start()));
    form.set('endSeconds', String(this.end()));
    this.analyzing.set(true);
    this.analysis = this.http.post<Spectrum>('/api/audio/analyze', form).subscribe({
      next: (result) => {
        this.spectrum.set(result);
        this.analyzing.set(false);
      },
      error: (failure: HttpErrorResponse) => {
        this.analysisError.set(this.errorMessage(failure));
        this.analyzing.set(false);
      },
    });
  }

  private pointerTime(event: PointerEvent): number {
    const svg = event.currentTarget as SVGSVGElement;
    const bounds = svg.getBoundingClientRect();
    const ratio = Math.min(1, Math.max(0, (event.clientX - bounds.left) / bounds.width));
    return ratio * (this.metadata()?.durationSeconds ?? 0);
  }

  private errorMessage(failure: HttpErrorResponse): string {
    const title = failure.error?.title;
    return typeof title === 'string'
      ? title
      : 'A API não respondeu como esperado. Confira se o backend está em execução.';
  }

  private clearAnalysis(): void {
    this.analysis?.unsubscribe();
    this.spectrum.set(null);
    this.analysisError.set('');
    this.analyzing.set(false);
  }

  private clearSelection(): void {
    this.inspection?.unsubscribe();
    this.clearAnalysis();
    if (this.source()) URL.revokeObjectURL(this.source());
    this.file = undefined;
    this.dragStart = undefined;
    this.start.set(0);
    this.end.set(0);
    this.source.set('');
    this.metadata.set(null);
    this.error.set('');
    this.loading.set(false);
  }
}

bootstrapApplication(App, { providers: [provideHttpClient()] }).catch(console.error);
