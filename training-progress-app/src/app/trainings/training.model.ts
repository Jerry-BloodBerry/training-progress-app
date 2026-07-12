export interface ExerciseSetResponse {
  id: string;
  setNumber: number;
  reps: number | null;
  weightKg: number | null;
  durationSeconds: number | null;
  notes: string | null;
}

export interface ExerciseEntryResponse {
  id: string;
  exerciseName: string;
  orderIndex: number;
  sets: ExerciseSetResponse[];
}

export interface TrainingStatisticsResponse {
  totalVolumeKg: number;
  totalSets: number;
  totalReps: number;
  estimatedCalories: number | null;
}

export interface TrainingResponse {
  id: string;
  date: string; // 'YYYY-MM-DD' (DateOnly from backend)
  durationMinutes: number;
  notes: string | null;
  createdAt: string;
  updatedAt: string;
  exercises: ExerciseEntryResponse[];
  statistics: TrainingStatisticsResponse;
}

export interface ListTrainingsResponse {
  items: TrainingResponse[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface CreateExerciseSetRequest {
  setNumber: number;
  reps: number | null;
  weightKg: number | null;
  durationSeconds: number | null;
  notes: string | null;
}

export interface CreateExerciseEntryRequest {
  exerciseName: string;
  orderIndex: number;
  sets: CreateExerciseSetRequest[];
}

export interface CreateTrainingRequest {
  date: string; // 'YYYY-MM-DD'
  durationMinutes: number;
  notes: string | null;
  exercises: CreateExerciseEntryRequest[];
}

export type UpdateTrainingRequest = CreateTrainingRequest;
