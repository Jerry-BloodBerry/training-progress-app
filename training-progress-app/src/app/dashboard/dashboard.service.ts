import { Injectable, computed, signal } from '@angular/core';

export interface TrainingSession {
  id: string;
  date: Date;
  kgLifted: number;
  caloriesBurned: number;
  kmTravelled: number;
}

export interface BodyMassEntry {
  date: Date;
  massKg: number;
}

export type BodyMassPeriodMonths = 1 | 3 | 6 | 12;

export interface BodyMassChartData {
  labels: string[];
  datasets: Array<{
    label: string;
    data: number[];
    borderColor: string;
    backgroundColor: string;
    tension: number;
    fill: boolean;
    pointRadius: number;
    pointBackgroundColor: string;
  }>;
}

// Deterministic seeded random (LCG) for consistent test data across reloads
function seededRandom(seed: number): () => number {
  let s = seed;
  return (): number => {
    s = Math.imul(1664525, s) + 1013904223;
    return (s >>> 0) / 0x100000000;
  };
}

function daysAgo(n: number): Date {
  const d = new Date();
  d.setDate(d.getDate() - n);
  d.setHours(0, 0, 0, 0);
  return d;
}

export function toDateKey(date: Date): string {
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, '0');
  const d = String(date.getDate()).padStart(2, '0');
  return `${y}-${m}-${d}`;
}

function formatChartLabel(date: Date): string {
  return date.toLocaleDateString('en-GB', { day: 'numeric', month: 'short' });
}

const rand = seededRandom(42);

const TRAINING_SESSIONS: TrainingSession[] = (() => {
  const sessions: TrainingSession[] = [];
  let idx = 0;
  for (let daysBack = 0; daysBack <= 90; daysBack++) {
    const d = daysAgo(daysBack);
    const dow = d.getDay(); // 0=Sun, 1=Mon, ... 6=Sat
    if ([1, 2, 4, 6].includes(dow)) {
      sessions.push({
        id: `session-${idx++}`,
        date: d,
        kgLifted: Math.round(1000 + rand() * 1000),
        caloriesBurned: Math.round(300 + rand() * 400),
        kmTravelled: Math.round((1 + rand() * 9) * 10) / 10,
      });
    }
  }
  return sessions;
})();

const BODY_MASS_ENTRIES: BodyMassEntry[] = (() => {
  const entries: BodyMassEntry[] = [];
  let massKg = 84.0;
  for (let daysBack = 365; daysBack >= 0; daysBack -= 3) {
    massKg += (rand() - 0.52) * 0.4; // slight downward trend
    massKg = Math.round(Math.max(76, Math.min(92, massKg)) * 10) / 10;
    entries.push({ date: daysAgo(daysBack), massKg });
  }
  return entries;
})();

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly _sessions = TRAINING_SESSIONS;
  private readonly _bodyMassEntries = BODY_MASS_ENTRIES;

  readonly bodyMassPeriod = signal<BodyMassPeriodMonths>(3);

  private readonly _sortedSessions: TrainingSession[] = [...this._sessions].sort(
    (a, b) => b.date.getTime() - a.date.getTime(),
  );

  readonly lastSession = computed<TrainingSession | null>(() => this._sortedSessions[0] ?? null);

  readonly todayKmTravelled = computed<number>(() => {
    const todayKey = toDateKey(new Date());
    return this._sessions
      .filter((s) => toDateKey(s.date) === todayKey)
      .reduce((sum, s) => sum + s.kmTravelled, 0);
  });

  readonly trainingDateSet = computed<Set<string>>(
    () => new Set(this._sessions.map((s) => toDateKey(s.date))),
  );

  readonly bodyMassChartData = computed<BodyMassChartData>(() => {
    const months = this.bodyMassPeriod();
    const cutoff = new Date();
    cutoff.setMonth(cutoff.getMonth() - months);
    cutoff.setHours(0, 0, 0, 0);

    const filtered = this._bodyMassEntries
      .filter((e) => e.date >= cutoff)
      .sort((a, b) => a.date.getTime() - b.date.getTime());

    return {
      labels: filtered.map((e) => formatChartLabel(e.date)),
      datasets: [
        {
          label: 'Body Mass (kg)',
          data: filtered.map((e) => e.massKg),
          borderColor: '#6366f1',
          backgroundColor: 'rgba(99, 102, 241, 0.08)',
          tension: 0.4,
          fill: true,
          pointRadius: 2,
          pointBackgroundColor: '#6366f1',
        },
      ],
    };
  });

  selectBodyMassPeriod(months: BodyMassPeriodMonths): void {
    this.bodyMassPeriod.set(months);
  }
}
