import { inject, Injectable, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  CreateTrainingRequest,
  ListTrainingsResponse,
  TrainingResponse,
  UpdateTrainingRequest,
} from './training.model';

@Injectable({ providedIn: 'root' })
export class TrainingService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/trainings`;

  readonly knownExerciseNames = signal<string[]>([]);

  list(page: number = 1, pageSize: number = 20): Observable<ListTrainingsResponse> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<ListTrainingsResponse>(this.baseUrl, { params }).pipe(
      tap((response) => this.updateKnownExercises(response.items)),
    );
  }

  getById(id: string): Observable<TrainingResponse> {
    return this.http.get<TrainingResponse>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateTrainingRequest): Observable<TrainingResponse> {
    return this.http.post<TrainingResponse>(this.baseUrl, request);
  }

  update(id: string, request: UpdateTrainingRequest): Observable<TrainingResponse> {
    return this.http.put<TrainingResponse>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  private updateKnownExercises(trainings: TrainingResponse[]): void {
    const existing = new Set(this.knownExerciseNames());
    for (const training of trainings) {
      for (const ex of training.exercises) {
        existing.add(ex.exerciseName);
      }
    }
    this.knownExerciseNames.set([...existing].sort());
  }
}
