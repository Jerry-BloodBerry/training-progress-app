import { Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DecimalPipe } from '@angular/common';
import { ClerkService } from 'ngx-clerk';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { DatePickerModule } from 'primeng/datepicker';
import { ChartModule } from 'primeng/chart';
import { DashboardService, BodyMassPeriodMonths } from './dashboard.service';

export interface PeriodOption {
  label: string;
  months: BodyMassPeriodMonths;
}

export interface CalendarDate {
  day: number;
  month: number; // 0-indexed, matching JS Date
  year: number;
  today: boolean;
  selectable: boolean;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, DecimalPipe, CardModule, ButtonModule, DatePickerModule, ChartModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent {
  protected readonly clerk = inject(ClerkService);
  protected readonly service = inject(DashboardService);

  protected readonly userName = computed<string>(() => {
    const user = this.clerk.user();
    if (!user) return '';
    return user.fullName ?? user.firstName ?? user.username ?? '';
  });

  protected readonly today = new Date();

  protected readonly periodOptions: PeriodOption[] = [
    { label: '1M', months: 1 },
    { label: '3M', months: 3 },
    { label: '6M', months: 6 },
    { label: '12M', months: 12 },
  ];

  protected readonly chartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
      tooltip: {
        callbacks: {
          label: (ctx: { raw: unknown }): string =>
            `${typeof ctx.raw === 'number' ? ctx.raw.toFixed(1) : String(ctx.raw)} kg`,
        },
      },
    },
    scales: {
      x: {
        ticks: { color: 'var(--p-text-muted-color)', maxTicksLimit: 8 },
        grid: { color: 'var(--p-surface-700)' },
      },
      y: {
        ticks: { color: 'var(--p-text-muted-color)' },
        grid: { color: 'var(--p-surface-700)' },
      },
    },
  };

  isTrainingDay(date: CalendarDate): boolean {
    const key = `${date.year}-${String(date.month + 1).padStart(2, '0')}-${String(date.day).padStart(2, '0')}`;
    return this.service.trainingDateSet().has(key);
  }

  signOut(): void {
    void this.clerk.signOut();
  }
}
