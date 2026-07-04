import { TestBed } from '@angular/core/testing';
import { DashboardService, toDateKey } from './dashboard.service';

describe('DashboardService', () => {
  let service: DashboardService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DashboardService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('lastSession()', () => {
    it('returns a non-null training session', () => {
      const session = service.lastSession();
      expect(session).not.toBeNull();
    });

    it('returns positive kg and calorie values', () => {
      const session = service.lastSession();
      expect(session!.kgLifted).toBeGreaterThan(0);
      expect(session!.caloriesBurned).toBeGreaterThan(0);
      expect(session!.kmTravelled).toBeGreaterThan(0);
    });
  });

  describe('trainingDateSet()', () => {
    it('returns a non-empty set of date keys', () => {
      const dateSet = service.trainingDateSet();
      expect(dateSet.size).toBeGreaterThan(0);
    });

    it('date keys match YYYY-MM-DD format', () => {
      const [first] = service.trainingDateSet();
      expect(first).toMatch(/^\d{4}-\d{2}-\d{2}$/);
    });
  });

  describe('bodyMassPeriod', () => {
    it('defaults to 3 months', () => {
      expect(service.bodyMassPeriod()).toBe(3);
    });
  });

  describe('selectBodyMassPeriod()', () => {
    it('updates the bodyMassPeriod signal', () => {
      service.selectBodyMassPeriod(6);
      expect(service.bodyMassPeriod()).toBe(6);
    });

    it('accepts all valid period values', () => {
      const periods: Array<1 | 3 | 6 | 12> = [1, 3, 6, 12];
      periods.forEach((p) => {
        service.selectBodyMassPeriod(p);
        expect(service.bodyMassPeriod()).toBe(p);
      });
    });
  });

  describe('bodyMassChartData()', () => {
    it('returns labels and data arrays of equal length', () => {
      const chartData = service.bodyMassChartData();
      expect(chartData.labels.length).toBe(chartData.datasets[0].data.length);
    });

    it('returns non-empty data for the default 3M period', () => {
      const chartData = service.bodyMassChartData();
      expect(chartData.labels.length).toBeGreaterThan(0);
    });

    it('returns more entries for 12M than for 1M', () => {
      service.selectBodyMassPeriod(1);
      const oneMonthCount = service.bodyMassChartData().labels.length;

      service.selectBodyMassPeriod(12);
      const twelveMonthCount = service.bodyMassChartData().labels.length;

      expect(twelveMonthCount).toBeGreaterThan(oneMonthCount);
    });
  });

  describe('toDateKey()', () => {
    it('formats a date as YYYY-MM-DD', () => {
      const d = new Date(2025, 0, 5); // 5 Jan 2025
      expect(toDateKey(d)).toBe('2025-01-05');
    });

    it('zero-pads single-digit month and day', () => {
      const d = new Date(2025, 2, 3); // 3 Mar 2025
      expect(toDateKey(d)).toBe('2025-03-03');
    });
  });
});
