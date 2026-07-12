import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { signal, computed } from '@angular/core';
import { of } from 'rxjs';
import { ClerkService } from 'ngx-clerk';
import { DashboardComponent, CalendarDate } from './dashboard.component';
import { DashboardService, BodyMassChartData } from './dashboard.service';
import { TrainingService } from '../trainings/training.service';
import { ListTrainingsResponse } from '../trainings/training.model';

class MockClerkService {
  isLoaded = signal(true);
  isSignedIn = signal(true);
  user = signal<{ fullName: string | null; firstName: string | null; username: string | null }>({
    fullName: 'Jane Smith',
    firstName: 'Jane',
    username: null,
  });
  signOut = jasmine.createSpy('signOut').and.returnValue(Promise.resolve());
}

class MockTrainingService {
  knownExerciseNames = signal<string[]>([]);
  list = jasmine.createSpy('list').and.returnValue(
    of<ListTrainingsResponse>({ items: [], totalCount: 0, page: 1, pageSize: 3 }),
  );
}

class MockDashboardService {
  bodyMassPeriod = signal<1 | 3 | 6 | 12>(3);

  lastSession = computed(() => ({
    id: 'session-1',
    date: new Date(2025, 5, 10),
    kgLifted: 1500,
    caloriesBurned: 450,
    kmTravelled: 5.2,
  }));

  todayKmTravelled = computed(() => 3.5);

  trainingDateSet = computed(
    () => new Set<string>(['2025-06-10', '2025-06-12', '2025-06-14']),
  );

  bodyMassChartData = computed<BodyMassChartData>(() => ({
    labels: ['1 Jan', '2 Jan'],
    datasets: [
      {
        label: 'Body Mass (kg)',
        data: [82.0, 81.5],
        borderColor: '#6366f1',
        backgroundColor: 'rgba(99, 102, 241, 0.08)',
        tension: 0.4,
        fill: true,
        pointRadius: 2,
        pointBackgroundColor: '#6366f1',
      },
    ],
  }));

  selectBodyMassPeriod = jasmine.createSpy('selectBodyMassPeriod');
}

describe('DashboardComponent', () => {
  let mockDashboardService: MockDashboardService;

  beforeEach(async () => {
    mockDashboardService = new MockDashboardService();

    await TestBed.configureTestingModule({
      imports: [DashboardComponent],
      providers: [
        provideRouter([]),
        { provide: ClerkService, useClass: MockClerkService },
        { provide: DashboardService, useValue: mockDashboardService },
        { provide: TrainingService, useClass: MockTrainingService },
      ],
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(DashboardComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });

  describe('isTrainingDay()', () => {
    it('returns true for a date in the training set', () => {
      const fixture = TestBed.createComponent(DashboardComponent);
      const component = fixture.componentInstance;
      // trainingDateSet contains '2025-06-10' => month index 5 (June)
      const date: CalendarDate = { day: 10, month: 5, year: 2025, today: false, selectable: true };
      expect(component.isTrainingDay(date)).toBeTrue();
    });

    it('returns false for a date not in the training set', () => {
      const fixture = TestBed.createComponent(DashboardComponent);
      const component = fixture.componentInstance;
      const date: CalendarDate = { day: 1, month: 0, year: 2025, today: false, selectable: true };
      expect(component.isTrainingDay(date)).toBeFalse();
    });
  });

  describe('signOut()', () => {
    it('calls clerk.signOut()', () => {
      const fixture = TestBed.createComponent(DashboardComponent);
      const component = fixture.componentInstance;
      const clerkService = TestBed.inject(ClerkService) as unknown as MockClerkService;
      component.signOut();
      expect(clerkService.signOut).toHaveBeenCalled();
    });
  });

  describe('period options', () => {
    it('exposes four period options', () => {
      const fixture = TestBed.createComponent(DashboardComponent);
      const component = fixture.componentInstance;
      expect(component['periodOptions'].length).toBe(4);
    });

    it('period options cover 1, 3, 6, and 12 months', () => {
      const fixture = TestBed.createComponent(DashboardComponent);
      const component = fixture.componentInstance;
      const months = component['periodOptions'].map((o) => o.months);
      expect(months).toEqual([1, 3, 6, 12]);
    });
  });
});
