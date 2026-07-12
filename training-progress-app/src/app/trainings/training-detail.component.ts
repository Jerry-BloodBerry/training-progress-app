import { Component, effect, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { DatePipe, DecimalPipe } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { SkeletonModule } from 'primeng/skeleton';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { TrainingService } from './training.service';
import { TrainingResponse } from './training.model';

@Component({
  selector: 'app-training-detail',
  standalone: true,
  imports: [
    RouterLink,
    DatePipe,
    DecimalPipe,
    ButtonModule,
    CardModule,
    SkeletonModule,
    ToastModule,
    ConfirmDialogModule,
  ],
  templateUrl: './training-detail.component.html',
  styleUrl: './training-detail.component.scss',
  providers: [ConfirmationService, MessageService],
})
export class TrainingDetailComponent {
  private readonly service = inject(TrainingService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly messageService = inject(MessageService);

  protected readonly training = signal<TrainingResponse | null>(null);
  protected readonly loading = signal<boolean>(true);
  protected readonly notFound = signal<boolean>(false);

  constructor() {
    effect(() => {
      const id = this.route.snapshot.paramMap.get('id');
      if (!id) {
        this.notFound.set(true);
        this.loading.set(false);
        return;
      }
      this.service.getById(id).subscribe({
        next: (t) => {
          this.training.set(t);
          this.loading.set(false);
        },
        error: (err) => {
          if (err.status === 404) {
            this.notFound.set(true);
          } else {
            this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load training.' });
          }
          this.loading.set(false);
        },
      });
    });
  }

  protected onDelete(): void {
    const t = this.training();
    if (!t) return;
    this.confirmationService.confirm({
      message: `Delete the training on ${t.date}? This cannot be undone.`,
      header: 'Delete Training',
      icon: 'pi pi-trash',
      acceptLabel: 'Delete',
      rejectLabel: 'Cancel',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.service.delete(t.id).subscribe({
          next: () => this.router.navigate(['/trainings']),
          error: () =>
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'Failed to delete training.',
            }),
        });
      },
    });
  }

  protected formatDuration(minutes: number): string {
    if (minutes < 60) return `${minutes} min`;
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;
    return m > 0 ? `${h}h ${m}min` : `${h}h`;
  }

  protected formatSet(set: TrainingResponse['exercises'][number]['sets'][number]): string {
    const parts: string[] = [];
    if (set.reps !== null && set.weightKg !== null) {
      parts.push(`${set.reps} × ${set.weightKg} kg`);
    } else if (set.reps !== null) {
      parts.push(`${set.reps} reps`);
    }
    if (set.durationSeconds !== null) {
      parts.push(`${set.durationSeconds}s`);
    }
    return parts.length > 0 ? parts.join(', ') : '—';
  }
}
