# UI Rules — Angular + PrimeNG

## Summary

This document governs all UI-related decisions in the training-progress-app Angular project.
It defines how components, styling, theming, and data visualisation must be implemented.
All agents performing tasks that touch templates, components, styles, or charts must read and
follow these rules before making any changes.

---

## Description

The application uses **PrimeNG v21** as its sole UI component library, styled with the
**Aura preset** in a **permanent dark theme**. No third-party UI component library other than
PrimeNG should be introduced. Custom components must only be created when PrimeNG does not
already offer an equivalent. All styling is written in **SCSS** — plain CSS files are not
permitted. The visual language prioritises simplicity and clarity: minimal chrome, generous
whitespace, and consistent use of PrimeNG's design-token system for colours and spacing.

---

## Technologies

| Technology | Version | Role |
|---|---|---|
| Angular | 20 (standalone) | Framework |
| PrimeNG | ^21 | UI component library |
| `@primeuix/themes` | latest compatible | Theming / design tokens |
| Chart.js | bundled via PrimeNG | Chart rendering engine |
| `primeng/chart` | same as PrimeNG | Angular wrapper for Chart.js |
| SCSS | — | All component and global styles |

---

## Setup Reference

### Installation

```bash
npm install primeng @primeuix/themes
```

### `app.config.ts` — provider setup (dark theme always on)

```typescript
import { ApplicationConfig } from '@angular/core';
import { providePrimeNG } from 'primeng/config';
import { definePreset } from '@primeuix/themes';
import Aura from '@primeuix/themes/aura';

const AppPreset = definePreset(Aura, {
  semantic: {
    colorScheme: {
      dark: {
        surface: {
          0: '#ffffff',
          50: '{slate.50}',
          100: '{slate.100}',
          200: '{slate.200}',
          300: '{slate.300}',
          400: '{slate.400}',
          500: '{slate.500}',
          600: '{slate.600}',
          700: '{slate.700}',
          800: '{slate.800}',
          900: '{slate.900}',
          950: '{slate.950}',
        },
      },
    },
  },
});

export const appConfig: ApplicationConfig = {
  providers: [
    providePrimeNG({
      theme: {
        preset: AppPreset,
        options: {
          darkModeSelector: '.app-dark', // toggle by adding this class to <html>
        },
      },
    }),
  ],
};
```

Apply the dark class permanently in `index.html`:

```html
<html class="app-dark">
```

---

## Component Usage Rules

### Use PrimeNG first

Before creating any custom component, check whether PrimeNG already provides one.

| Need | PrimeNG component |
|---|---|
| Button / icon button | `ButtonModule` (`p-button`) |
| Data table | `TableModule` (`p-table`) |
| Form inputs | `InputTextModule`, `DropdownModule`, `SelectModule`, etc. |
| Dialog / modal | `DialogModule` (`p-dialog`) |
| Cards | `CardModule` (`p-card`) |
| Navigation tabs | `TabsModule` (`p-tabs`) |
| Charts | `ChartModule` from `primeng/chart` |
| Progress bar | `ProgressBarModule` (`p-progressbar`) |
| Toast notifications | `ToastModule` (`p-toast`) |
| Badges / tags | `BadgeModule`, `TagModule` |
| Menus / sidebars | `MenuModule`, `SidebarModule` |

This list is not exhaustive — always check [https://primeng.org/](https://primeng.org/) before building custom UI.

### Import components individually (tree-shaking)

```typescript
// CORRECT — import only what you use
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';

@Component({
  standalone: true,
  imports: [ButtonModule, TableModule],
  ...
})
```

```typescript
// WRONG — never import the entire PrimeNG bundle
import { PrimeNGModule } from 'primeng/primeng'; // does not exist; shown as anti-pattern
```

---

## Theming & Styling Rules

### SCSS only

Every component style file must use `.scss`. Never create `.css` files.

```
// CORRECT
training-log.component.scss

// WRONG
training-log.component.css
```

### Use design tokens for colours and spacing

Always reference PrimeNG CSS variables (`var(--p-*)`) instead of hard-coded hex values
inside SCSS. This keeps colours consistent with the active theme.

```scss
// CORRECT
.stat-card {
  background: var(--p-surface-800);
  color: var(--p-text-color);
  border: 1px solid var(--p-surface-600);
  border-radius: var(--p-border-radius-md);
}

// WRONG
.stat-card {
  background: #1e293b;
  color: #f8fafc;
  border: 1px solid #334155;
}
```

### Scoped tokens for one-off overrides

When a single component instance needs a visual variant, use the `[dt]` binding
instead of `::ng-deep`.

```typescript
// CORRECT — scoped token override
dangerSwitch = {
  colorScheme: {
    dark: {
      root: { checkedBackground: '{red.600}' },
    },
  },
};
```

```html
<p-toggleswitch [(ngModel)]="flag" [dt]="dangerSwitch" />
```

```scss
// WRONG — avoid ::ng-deep for theme overrides
::ng-deep .p-toggleswitch .p-toggleswitch-slider {
  background: #dc2626;
}
```

### Dark mode is permanent

Do not implement a light/dark toggle. The `app-dark` class is applied to `<html>` at
startup and never removed. Do not write light-mode-specific style rules.

---

## Charts

### Always use `ChartModule` from `primeng/chart`

Do not import or instantiate `Chart.js` directly. Use the `p-chart` component, which
wraps Chart.js and integrates with Angular's change detection.

```typescript
import { ChartModule } from 'primeng/chart';

@Component({
  standalone: true,
  imports: [ChartModule],
  template: `<p-chart type="line" [data]="chartData" [options]="chartOptions" />`,
})
export class ProgressChartComponent {
  chartData = { ... };
  chartOptions = { ... };
}
```

### Style charts to match the dark theme

Pass chart options that use surface/text CSS variables so Chart.js elements respect
the dark palette.

```typescript
chartOptions = {
  plugins: {
    legend: {
      labels: { color: 'var(--p-text-color)' },
    },
  },
  scales: {
    x: {
      ticks: { color: 'var(--p-text-muted-color)' },
      grid: { color: 'var(--p-surface-700)' },
    },
    y: {
      ticks: { color: 'var(--p-text-muted-color)' },
      grid: { color: 'var(--p-surface-700)' },
    },
  },
};
```

### Declare data and options as typed properties

```typescript
import { ChartData, ChartOptions } from 'chart.js';

chartData: ChartData<'line'> = { labels: [], datasets: [] };
chartOptions: ChartOptions<'line'> = { ... };
```

---

## Layout & Visual Design Rules

### Simplicity and clarity first

- Show only the information needed for the current view.
- Avoid decorative elements that do not convey meaning.
- One primary action per screen; secondary actions are visually subordinate.
- Prefer `p-card` as the default content container.

### Spacing

Use PrimeNG's spacing scale via CSS variables or Tailwind-style utility classes where
available. Avoid arbitrary pixel values.

```scss
// Prefer
padding: var(--p-content-padding);

// Over
padding: 13px 17px;
```

### Typography

- Rely on PrimeNG's inherited font settings; do not override the global font family.
- Use semantic heading levels (`h1`–`h3`) inside templates for accessibility.
- Muted secondary text: `color: var(--p-text-muted-color)`.

### Responsive design

- Use CSS Grid or Flexbox for layout — never absolute positioning.
- PrimeNG's `p-fluid` class and responsive grid helpers are preferred.
- Minimum supported viewport: 360 px wide.

---

## Accessibility

- Every interactive PrimeNG component is keyboard-navigable by default — do not
  disable this behaviour.
- Always supply `ariaLabel` or `ariaLabelledBy` when a visual label is absent.
- Icon-only buttons must include an `aria-label`.
- Colour alone must never be the sole means of conveying information.
- Maintain a contrast ratio of at least 4.5 : 1 for normal text against the dark
  surface backgrounds.

---

## Examples

### Correct — reusing a PrimeNG table instead of a custom one

```html
<p-table [value]="sessions" [paginator]="true" [rows]="10">
  <ng-template pTemplate="header">
    <tr>
      <th>Date</th>
      <th>Exercise</th>
      <th pSortableColumn="duration">Duration <p-sortIcon field="duration" /></th>
    </tr>
  </ng-template>
  <ng-template pTemplate="body" let-session>
    <tr>
      <td>{{ session.date | date }}</td>
      <td>{{ session.exercise }}</td>
      <td>{{ session.duration }} min</td>
    </tr>
  </ng-template>
</p-table>
```

### Incorrect — building a custom table when `p-table` is available

```html
<!-- DON'T DO THIS -->
<table class="custom-table">
  <thead><tr><th>Date</th>...</tr></thead>
  <tbody>
    <tr *ngFor="let s of sessions"><td>{{ s.date }}</td></tr>
  </tbody>
</table>
```

### Correct — chart with dark-themed options

```typescript
chartData: ChartData<'bar'> = {
  labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri'],
  datasets: [{
    label: 'Reps',
    data: [12, 15, 10, 18, 14],
    backgroundColor: 'var(--p-primary-color)',
  }],
};

chartOptions: ChartOptions<'bar'> = {
  responsive: true,
  plugins: { legend: { labels: { color: 'var(--p-text-color)' } } },
  scales: {
    x: { ticks: { color: 'var(--p-text-muted-color)' }, grid: { color: 'var(--p-surface-700)' } },
    y: { ticks: { color: 'var(--p-text-muted-color)' }, grid: { color: 'var(--p-surface-700)' } },
  },
};
```

```html
<p-chart type="bar" [data]="chartData" [options]="chartOptions" />
```

---

## DOs and DON'Ts

### DO

- **DO** install and use PrimeNG components for every standard UI element (buttons, tables,
  forms, modals, charts, navigation).
- **DO** write all styles in `.scss` files.
- **DO** use PrimeNG CSS variables (`var(--p-*)`) for colours, spacing, and borders.
- **DO** use `ChartModule` from `primeng/chart` for all data visualisations.
- **DO** type chart `data` and `options` with the appropriate `Chart.js` generic types.
- **DO** keep `darkModeSelector: '.app-dark'` configured and apply the class to `<html>`
  permanently.
- **DO** use `[dt]` scoped tokens for component-level visual variants.
- **DO** import PrimeNG modules individually per component (tree-shaking).
- **DO** keep layouts simple: one primary action per view, clear visual hierarchy.
- **DO** ensure all interactive elements are keyboard accessible and have ARIA labels where
  needed.
- **DO** use `definePreset` + `@primeuix/themes` to extend or customise the theme rather
  than overriding styles with `::ng-deep`.

### DON'T

- **DON'T** build a custom component when an equivalent PrimeNG component exists.
- **DON'T** use plain `.css` files anywhere in the project.
- **DON'T** hard-code hex colour values in SCSS — use design tokens.
- **DON'T** import Chart.js directly; always use `p-chart`.
- **DON'T** implement a light-mode theme or a dark-mode toggle.
- **DON'T** use `::ng-deep` for theme-level overrides; use design tokens or `[dt]` instead.
- **DON'T** add any UI library other than PrimeNG without explicit user approval.
- **DON'T** add unnecessary animations, gradients, or decorative imagery.
- **DON'T** disable keyboard navigation or remove focus rings from PrimeNG components.
- **DON'T** use `any` for chart data or options types.
