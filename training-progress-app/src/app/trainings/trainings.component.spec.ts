import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { signal } from '@angular/core';
import { TrainingsComponent } from './trainings.component';
import { TrainingService } from './training.service';
import { ListTrainingsResponse } from './training.model';
import { of } from 'rxjs';

class MockTrainingService {
  knownExerciseNames = signal<string[]>([]);
  list = jasmine.createSpy('list').and.returnValue(
    of<ListTrainingsResponse>({ items: [], totalCount: 0, page: 1, pageSize: 20 }),
  );
  delete = jasmine.createSpy('delete').and.returnValue(of(undefined));
}

describe('TrainingsComponent', () => {
  let mockService: MockTrainingService;

  beforeEach(async () => {
    mockService = new MockTrainingService();

    await TestBed.configureTestingModule({
      imports: [TrainingsComponent],
      providers: [
        provideRouter([]),
        { provide: TrainingService, useValue: mockService },
      ],
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(TrainingsComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should call service.list on init', () => {
    TestBed.createComponent(TrainingsComponent);
    TestBed.flushEffects();
    expect(mockService.list).toHaveBeenCalledWith(1, 20);
  });

  it('should show empty state when there are no trainings', () => {
    const fixture = TestBed.createComponent(TrainingsComponent);
    TestBed.flushEffects();
    fixture.detectChanges();
    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelector('.empty-state')).toBeTruthy();
  });
});
