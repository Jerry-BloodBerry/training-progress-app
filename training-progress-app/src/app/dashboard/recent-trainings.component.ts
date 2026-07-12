import { Component, effect, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe, DecimalPipe } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { CardModule } from 'primeng/card';
import { TrainingService } from '../trainings/training.service';
import { TrainingResponse } from '../trainings/training.model';

@Component({
  selector: 'app-recent-trainings',
  standalone: true,
  imports: [RouterLink, DatePipe, DecimalPipe, ButtonModule, SkeletonModule, CardModule],
  templateUrl: './recent-trainings.component.html',
  styleUrl: './recent-trainings.component.scss',
})
export class RecentTrainingsComponent {
  private readonly service = inject(TrainingService);

  protected readonly trainings = signal<TrainingResponse[]>([]);
  protected readonly loading = signal<boolean>(true);

  constructor() {
    effect(() => {
      this.service.list(1, 3).subscribe({
        next: (res) => {
          this.trainings.set(res.items);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
    });
  }
}
