import { Component, effect, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
  AbstractControl,
  FormArray,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { trigger, transition, style, animate } from '@angular/animations';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { TextareaModule } from 'primeng/textarea';
import { DatePickerModule } from 'primeng/datepicker';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { StepperModule } from 'primeng/stepper';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TrainingService } from './training.service';
import { CreateExerciseEntryRequest, CreateExerciseSetRequest, CreateTrainingRequest } from './training.model';

export interface SetFormValue {
  reps: number | null;
  weightKg: number | null;
  durationSeconds: number | null;
  setNotes: string | null;
}

export interface ExerciseFormValue {
  exerciseName: string;
  sets: SetFormValue[];
}

export interface TrainingFormValue {
  date: Date;
  durationMinutes: number;
  notes: string | null;
  exercises: ExerciseFormValue[];
}

@Component({
  selector: 'app-training-form',
  standalone: true,
  imports: [
    RouterLink,
    ReactiveFormsModule,
    ButtonModule,
    InputTextModule,
    InputNumberModule,
    TextareaModule,
    DatePickerModule,
    AutoCompleteModule,
    StepperModule,
    ToastModule,
  ],
  templateUrl: './training-form.component.html',
  styleUrl: './training-form.component.scss',
  providers: [MessageService],
  animations: [
    trigger('fadeUp', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(-8px)' }),
        animate('200ms ease-out', style({ opacity: 1, transform: 'translateY(0)' })),
      ]),
    ]),
  ],
})
export class TrainingFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(TrainingService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly messageService = inject(MessageService);

  protected readonly isEditMode = signal<boolean>(false);
  protected readonly editId = signal<string | null>(null);
  protected readonly loading = signal<boolean>(false);
  protected readonly submitting = signal<boolean>(false);
  protected readonly activeStep = signal<number>(1);
  protected readonly filteredExercises = signal<string[]>([]);
  protected readonly today = new Date();

  protected readonly form = this.fb.group({
    date: [new Date(), Validators.required],
    durationMinutes: [60, [Validators.required, Validators.min(1)]],
    notes: [''],
    exercises: this.fb.array<FormGroup>([]),
  });

  get exercises(): FormArray<FormGroup> {
    return this.form.get('exercises') as FormArray<FormGroup>;
  }

  setsFor(exerciseIndex: number): FormArray<FormGroup> {
    return this.exercises.at(exerciseIndex).get('sets') as FormArray<FormGroup>;
  }

  constructor() {
    effect(() => {
      const id = this.route.snapshot.paramMap.get('id');
      if (id) {
        this.isEditMode.set(true);
        this.editId.set(id);
        this.loadTraining(id);
      } else {
        this.addExercise();
      }
    });
  }

  private loadTraining(id: string): void {
    this.loading.set(true);
    this.service.getById(id).subscribe({
      next: (t) => {
        this.form.patchValue({
          date: new Date(t.date),
          durationMinutes: t.durationMinutes,
          notes: t.notes ?? '',
        });
        this.exercises.clear();
        for (const ex of t.exercises) {
          const exGroup = this.createExerciseGroup(ex.exerciseName);
          const setsArr = exGroup.get('sets') as FormArray<FormGroup>;
          setsArr.clear();
          for (const s of ex.sets) {
            setsArr.push(
              this.createSetGroup({
                reps: s.reps,
                weightKg: s.weightKg,
                durationSeconds: s.durationSeconds,
                setNotes: s.notes,
              }),
            );
          }
          this.exercises.push(exGroup);
        }
        if (this.exercises.length === 0) {
          this.addExercise();
        }
        this.loading.set(false);
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load training.' });
        this.loading.set(false);
      },
    });
  }

  private createExerciseGroup(name = ''): FormGroup {
    return this.fb.group({
      exerciseName: [name, Validators.required],
      sets: this.fb.array<FormGroup>([this.createSetGroup()]),
    });
  }

  private createSetGroup(value?: Partial<SetFormValue>): FormGroup {
    return this.fb.group({
      reps: [value?.reps ?? null],
      weightKg: [value?.weightKg ?? null],
      durationSeconds: [value?.durationSeconds ?? null],
      setNotes: [value?.setNotes ?? null],
    });
  }

  protected addExercise(): void {
    this.exercises.push(this.createExerciseGroup());
  }

  protected removeExercise(index: number): void {
    this.exercises.removeAt(index);
  }

  protected addSet(exerciseIndex: number): void {
    const setsArr = this.setsFor(exerciseIndex);
    const last = setsArr.length > 0 ? setsArr.at(setsArr.length - 1) : null;
    // Pre-fill numeric fields from the previous set; skip notes (they are set-specific)
    const prefill: Partial<SetFormValue> | undefined = last
      ? {
          reps: last.get('reps')?.value as number | null,
          weightKg: last.get('weightKg')?.value as number | null,
          durationSeconds: last.get('durationSeconds')?.value as number | null,
        }
      : undefined;
    setsArr.push(this.createSetGroup(prefill));
  }

  protected removeSet(exerciseIndex: number, setIndex: number): void {
    this.setsFor(exerciseIndex).removeAt(setIndex);
  }

  protected filterExercises(event: { query: string }): void {
    const query = event.query.toLowerCase();
    const known = this.service.knownExerciseNames();
    this.filteredExercises.set(
      query ? known.filter((name) => name.toLowerCase().includes(query)) : known,
    );
  }

  protected exerciseSummary(exerciseIndex: number): string {
    const setsArr = this.setsFor(exerciseIndex);
    const count = setsArr.length;
    let totalReps = 0;
    const weights: number[] = [];

    for (let i = 0; i < count; i++) {
      const set = setsArr.at(i);
      const reps = set.get('reps')?.value as number | null;
      const weightKg = set.get('weightKg')?.value as number | null;
      if (reps != null && reps > 0) totalReps += reps;
      if (weightKg != null && weightKg > 0) weights.push(weightKg);
    }

    const summaryParts: string[] = [`${count} set${count !== 1 ? 's' : ''}`];
    if (totalReps > 0) summaryParts.push(`${totalReps} reps`);
    if (weights.length > 0) {
      const min = Math.min(...weights);
      const max = Math.max(...weights);
      summaryParts.push(min === max ? `${min} kg` : `${min}–${max} kg`);
    }
    return summaryParts.join(' · ');
  }

  protected getControl(group: AbstractControl, name: string): AbstractControl {
    return group.get(name)!;
  }

  protected step1Valid(): boolean {
    const dateCtrl = this.form.get('date');
    const durationCtrl = this.form.get('durationMinutes');
    return (dateCtrl?.valid ?? false) && (durationCtrl?.valid ?? false);
  }

  protected goToStep2(activateCallback: (step: number) => void): void {
    this.form.get('date')?.markAsTouched();
    this.form.get('durationMinutes')?.markAsTouched();
    if (this.step1Valid()) {
      activateCallback(2);
    }
  }

  protected submit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    const value = this.form.getRawValue() as TrainingFormValue;
    const request = this.buildRequest(value);
    this.submitting.set(true);

    const call = this.isEditMode()
      ? this.service.update(this.editId()!, request)
      : this.service.create(request);

    call.subscribe({
      next: (result) => {
        this.messageService.add({
          severity: 'success',
          summary: this.isEditMode() ? 'Updated' : 'Created',
          detail: `Training ${this.isEditMode() ? 'updated' : 'saved'} successfully.`,
        });
        this.router.navigate(['/trainings', result.id]);
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to save training.' });
        this.submitting.set(false);
      },
    });
  }

  private buildRequest(value: TrainingFormValue): CreateTrainingRequest {
    const date = value.date;
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');

    const exercises: CreateExerciseEntryRequest[] = value.exercises.map((ex, i) => ({
      exerciseName: ex.exerciseName,
      orderIndex: i,
      sets: ex.sets.map((s, si): CreateExerciseSetRequest => ({
        setNumber: si + 1,
        reps: s.reps,
        weightKg: s.weightKg,
        durationSeconds: s.durationSeconds,
        notes: s.setNotes ?? null,
      })),
    }));

    return {
      date: `${y}-${m}-${d}`,
      durationMinutes: value.durationMinutes,
      notes: value.notes?.trim() || null,
      exercises,
    };
  }
}
