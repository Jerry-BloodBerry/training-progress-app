# Frontend Architecture Rules — Angular 20

## Summary

This document governs the architectural decisions and code organisation practices for all
frontend work in the training-progress-app. It defines how components, services, and facades
must be structured so that the codebase remains maintainable, testable, and aligned with
modern Angular standards. All agents performing tasks that touch components, services, or
application structure must read and follow these rules before making any changes.

---

## Description

The application is built with **Angular 20** using the **standalone component** model and
**signal-based reactivity**. Agents must always follow Angular 20's official documentation and
modern best practices — no legacy patterns (e.g. NgModules, `ngModel` with `FormsModule` in
non-form contexts, `ActivatedRoute` subscribe-based patterns) should be introduced.

Components must be kept lean: they are responsible for rendering the view and delegating user
interactions, not for containing business logic, HTTP calls, or complex state transformations.
HTTP communication is always delegated to dedicated services. When a component needs to
coordinate behaviour across two or more services, a **facade service** acts as the single
entry point, keeping the component free of cross-service orchestration.

Co-location is preferred for components that are tightly coupled to a single parent: if a
child component is only ever used by one parent, it lives in the same folder as that parent
rather than in a shared location.

---

## Technologies

| Technology | Version | Role |
|---|---|---|
| Angular | 20 (standalone) | Framework |
| TypeScript | 5.9 (strict) | Language |
| Angular CLI | latest compatible | Scaffolding (`ng generate`) |
| Angular Signals | built-in (Angular 20) | Reactive state management |
| Angular HttpClient | built-in | HTTP communication |

---

## Rules

### 1. Always Follow Modern Angular Practices

- Use **standalone components** — do not create or reference NgModules.
- Use **signals** (`signal()`, `computed()`, `effect()`) for reactive state instead of
  RxJS Subjects or manual change detection unless RxJS is genuinely warranted (e.g. complex
  stream composition).
- Use the `input()` / `output()` signal-based APIs for component I/O instead of `@Input()` /
  `@Output()` decorators.
- Use `inject()` for dependency injection instead of constructor parameter injection.
- Use the `@if`, `@for`, and `@switch` built-in control flow syntax — never `*ngIf`,
  `*ngFor`, or `*ngSwitch`.

### 2. Adhere to Angular 20 Documentation

- Any pattern, API, or configuration must be verifiable in the **official Angular 20 docs**
  (https://angular.dev).
- Do not introduce patterns that are deprecated or removed in Angular 20.
- When in doubt, prefer the approach recommended in the official documentation over community
  conventions that pre-date Angular 17+.

### 3. Keep Components Lean

- Components are responsible for **view logic only**: displaying data, handling user events,
  and delegating work to services.
- Do not put HTTP calls, data transformations, business rules, or complex state derivations
  directly inside a component class.
- A component method should read like a coordination step, not an implementation:
  `this.trainingService.save(entry)` ✓ vs. writing the HTTP call inline ✗.
- Keep the component class under ~100 lines where possible; if it grows beyond that, it is a
  sign that logic should move to a service.

### 4. HTTP Logic Belongs in Services

- All HTTP calls must be defined in a dedicated **service** (`*.service.ts`).
- Services use Angular's `HttpClient` (injected via `inject(HttpClient)`).
- Services return `Observable` or `Signal`-wrapped data — they do not subscribe internally
  except where a fire-and-forget side effect is intentional and documented.
- One service per domain area (e.g. `training-log.service.ts`, `athlete.service.ts`).

### 5. Facade Services for Multi-Service Orchestration

- If a component needs to coordinate two or more services, create a **facade service**
  (`*.facade.ts`) that encapsulates that orchestration.
- The component calls only the facade; the facade calls the underlying services.
- Facades are named after the feature they serve: e.g. `dashboard.facade.ts`.
- Do not create a facade for a component that only needs a single service.

### 6. Co-locate Private Child Components

- If a component will only ever be rendered by **one specific parent**, create it in the
  **same folder** as that parent.
- Use the `--flat` flag with `ng generate component` to avoid creating a sub-folder:
  ```bash
  ng generate component feature/child-widget --flat
  ```
- Components that are reused across multiple features belong in a `shared/` folder.

---

## Examples

### ✅ Lean component using a facade

```typescript
// dashboard.component.ts
import { Component, inject, OnInit } from '@angular/core';
import { DashboardFacade } from './dashboard.facade';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  templateUrl: './dashboard.component.html',
})
export class DashboardComponent implements OnInit {
  private readonly facade = inject(DashboardFacade);

  readonly summary = this.facade.summary;

  ngOnInit(): void {
    this.facade.load();
  }

  save(entry: TrainingEntry): void {
    this.facade.saveEntry(entry);
  }
}
```

### ✅ Facade orchestrating two services

```typescript
// dashboard.facade.ts
import { Injectable, inject, signal } from '@angular/core';
import { TrainingLogService } from '../training-log/training-log.service';
import { AthleteService } from '../athlete/athlete.service';

@Injectable({ providedIn: 'root' })
export class DashboardFacade {
  private readonly log = inject(TrainingLogService);
  private readonly athlete = inject(AthleteService);

  readonly summary = signal<DashboardSummary | null>(null);

  load(): void {
    // Coordinates two services; keeps component unaware of the details.
    combineLatest([this.log.getRecent(), this.athlete.getProfile()])
      .subscribe(([entries, profile]) => {
        this.summary.set(buildSummary(entries, profile));
      });
  }

  saveEntry(entry: TrainingEntry): void {
    this.log.save(entry).subscribe();
  }
}
```

### ✅ HTTP call in a service, not a component

```typescript
// training-log.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class TrainingLogService {
  private readonly http = inject(HttpClient);

  getRecent(): Observable<TrainingEntry[]> {
    return this.http.get<TrainingEntry[]>('/api/training-log/recent');
  }

  save(entry: TrainingEntry): Observable<void> {
    return this.http.post<void>('/api/training-log', entry);
  }
}
```

### ✅ Co-located private child component (flat)

```
src/app/dashboard/
  dashboard.component.ts         ← parent
  dashboard.component.html
  dashboard.component.scss
  dashboard.component.spec.ts
  summary-card.component.ts      ← child, only used by dashboard
  summary-card.component.html
  summary-card.component.scss
  summary-card.component.spec.ts
  dashboard.facade.ts
```

Generated with:
```bash
ng generate component dashboard/summary-card --flat
```

### ❌ HTTP call inside a component

```typescript
// BAD — HttpClient used directly in a component
export class DashboardComponent {
  private readonly http = inject(HttpClient);

  load(): void {
    this.http.get<TrainingEntry[]>('/api/training-log/recent').subscribe(data => {
      this.entries.set(data);
    });
  }
}
```

### ❌ Component orchestrating services directly

```typescript
// BAD — component knows about multiple services and wires them together
export class DashboardComponent {
  private readonly log = inject(TrainingLogService);
  private readonly athlete = inject(AthleteService);

  load(): void {
    combineLatest([this.log.getRecent(), this.athlete.getProfile()])
      .subscribe(([entries, profile]) => { /* ... */ });
  }
}
```

### ❌ Legacy Angular patterns

```typescript
// BAD — decorator-based I/O, constructor injection, NgModule-era patterns
@Component({ /* ... */ })
export class MyComponent {
  @Input() value!: string;          // use input() signal instead
  @Output() changed = new EventEmitter(); // use output() signal instead

  constructor(private svc: MyService) {} // use inject() instead
}
```

---

## DO's and DON'Ts

### DO's

- **DO** use standalone components and the `inject()` function throughout.
- **DO** use Angular 20 signal APIs (`signal`, `computed`, `effect`, `input`, `output`).
- **DO** use `@if` / `@for` / `@switch` control-flow syntax in templates.
- **DO** place all HTTP calls inside dedicated service files.
- **DO** create a facade when a component needs more than one service.
- **DO** co-locate a child component with its parent when it has only one consumer, using
  `ng generate component <path> --flat`.
- **DO** consult the official Angular 20 docs (https://angular.dev) before introducing any
  new pattern or API.
- **DO** keep component classes under ~100 lines; move excess logic to services.

### DON'Ts

- **DON'T** make HTTP calls directly inside component classes.
- **DON'T** use `@Input()` / `@Output()` decorators — prefer signal-based `input()` /
  `output()`.
- **DON'T** use constructor injection — use `inject()`.
- **DON'T** use `*ngIf`, `*ngFor`, or `*ngSwitch` in templates.
- **DON'T** use NgModules.
- **DON'T** create a facade for a component that only consumes a single service.
- **DON'T** place a component in `shared/` unless it is genuinely used by more than one
  feature.
- **DON'T** introduce deprecated or pre-Angular-17 APIs.
- **DON'T** put business logic, data transformation, or state derivation directly in a
  component class.
