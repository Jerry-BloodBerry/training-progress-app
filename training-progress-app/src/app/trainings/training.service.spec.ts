import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TrainingService } from './training.service';
import { ListTrainingsResponse, TrainingResponse } from './training.model';

const MOCK_TRAINING: TrainingResponse = {
  id: 'a1b2c3d4-e5f6-7890-abcd-ef1234567890',
  date: '2026-07-10',
  durationMinutes: 60,
  notes: 'Good session',
  createdAt: '2026-07-10T10:00:00Z',
  updatedAt: '2026-07-10T10:00:00Z',
  exercises: [
    {
      id: 'ex-1',
      exerciseName: 'Bench Press',
      orderIndex: 0,
      sets: [{ id: 'set-1', setNumber: 1, reps: 10, weightKg: 80, durationSeconds: null, notes: null }],
    },
  ],
  statistics: { totalVolumeKg: 800, totalSets: 1, totalReps: 10, estimatedCalories: null },
};

describe('TrainingService', () => {
  let service: TrainingService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(TrainingService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('list', () => {
    it('should GET /trainings with page and pageSize params', () => {
      const mockResponse: ListTrainingsResponse = {
        items: [MOCK_TRAINING],
        totalCount: 1,
        page: 1,
        pageSize: 20,
      };

      service.list(1, 20).subscribe((res) => {
        expect(res.items.length).toBe(1);
        expect(res.totalCount).toBe(1);
      });

      const req = http.expectOne((r) => r.url.includes('/trainings') && r.method === 'GET');
      expect(req.request.params.get('page')).toBe('1');
      expect(req.request.params.get('pageSize')).toBe('20');
      req.flush(mockResponse);
    });

    it('should update knownExerciseNames after listing', () => {
      const mockResponse: ListTrainingsResponse = {
        items: [MOCK_TRAINING],
        totalCount: 1,
        page: 1,
        pageSize: 20,
      };

      service.list().subscribe();
      http.expectOne((r) => r.url.includes('/trainings')).flush(mockResponse);

      expect(service.knownExerciseNames()).toContain('Bench Press');
    });
  });

  describe('getById', () => {
    it('should GET /trainings/:id', () => {
      const id = MOCK_TRAINING.id;
      service.getById(id).subscribe((res) => expect(res.id).toBe(id));
      http.expectOne((r) => r.url.includes(`/trainings/${id}`) && r.method === 'GET').flush(MOCK_TRAINING);
    });
  });

  describe('create', () => {
    it('should POST /trainings', () => {
      const request = {
        date: '2026-07-10',
        durationMinutes: 60,
        notes: null,
        exercises: [],
      };
      service.create(request).subscribe((res) => expect(res.id).toBe(MOCK_TRAINING.id));
      const req = http.expectOne((r) => r.url.includes('/trainings') && r.method === 'POST');
      expect(req.request.body).toEqual(request);
      req.flush(MOCK_TRAINING);
    });
  });

  describe('update', () => {
    it('should PUT /trainings/:id', () => {
      const id = MOCK_TRAINING.id;
      const request = { date: '2026-07-10', durationMinutes: 45, notes: null, exercises: [] };
      service.update(id, request).subscribe((res) => expect(res.id).toBe(id));
      const req = http.expectOne((r) => r.url.includes(`/trainings/${id}`) && r.method === 'PUT');
      expect(req.request.body).toEqual(request);
      req.flush(MOCK_TRAINING);
    });
  });

  describe('delete', () => {
    it('should DELETE /trainings/:id', () => {
      const id = MOCK_TRAINING.id;
      service.delete(id).subscribe();
      http.expectOne((r) => r.url.includes(`/trainings/${id}`) && r.method === 'DELETE').flush(null);
    });
  });
});
