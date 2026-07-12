import { Component, effect, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { DatePipe, DecimalPipe } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { SkeletonModule } from 'primeng/skeleton';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { CardModule } from 'primeng/card';
import { ConfirmationService, MessageService } from 'primeng/api';
import { TrainingService } from './training.service';
import { TrainingResponse } from './training.model';

@Component({
  selector: 'app-trainings',
  standalone: true,
  imports: [
    RouterLink,
    DatePipe,
    DecimalPipe,
    ButtonModule,
    TableModule,
    SkeletonModule,
    ToastModule,
    ConfirmDialogModule,
    CardModule,
  ],
  templateUrl: './trainings.component.html',
  styleUrl: './trainings.component.scss',
  providers: [ConfirmationService, MessageService],
})
export class TrainingsComponent {
  private readonly service = inject(TrainingService);
  private readonly router = inject(Router);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly messageService = inject(MessageService);

  protected readonly trainings = signal<TrainingResponse[]>([]);
  protected readonly totalCount = signal<number>(0);
  protected readonly loading = signal<boolean>(true);
  protected readonly page = signal<number>(1);
  protected readonly pageSize = 20;

  constructor() {
    effect(() => {
      this.loadPage(this.page());
    });
  }

  private loadPage(page: number): void {
    this.loading.set(true);
    this.service.list(page, this.pageSize).subscribe({
      next: (res) => {
        this.trainings.set(res.items);
        this.totalCount.set(res.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load trainings.' });
        this.loading.set(false);
      },
    });
  }

  protected onPageChange(event: { first: number; rows: number }): void {
    this.page.set(Math.floor(event.first / event.rows) + 1);
  }

  protected onDelete(training: TrainingResponse): void {
    this.confirmationService.confirm({
      message: `Delete the training on ${training.date}? This cannot be undone.`,
      header: 'Delete Training',
      icon: 'pi pi-trash',
      acceptLabel: 'Delete',
      rejectLabel: 'Cancel',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.service.delete(training.id).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'Deleted', detail: 'Training deleted.' });
            this.loadPage(this.page());
          },
          error: () => {
            this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete training.' });
          },
        });
      },
    });
  }

  protected navigateToNew(): void {
    this.router.navigate(['/trainings', 'new']);
  }
}
