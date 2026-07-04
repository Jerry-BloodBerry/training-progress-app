# Agent Rules — training-progress-app

These rules apply to **every agent** operating in this workspace, regardless of task.
Before acting on any task, read this file in full and then load every rule file from
`training-progress-app/docs/` that is relevant to the work you are about to perform.

---

## 1. Project Overview

| Property | Value |
|---|---|
| Framework | Angular 20 (standalone, signal-based) |
| Language | TypeScript 5.9 (strict mode) |
| Styling | SCSS |
| Testing | Karma + Jasmine |
| Formatter | Prettier (config in `package.json`) |
| Package manager | npm |

---

## 2. Mandatory Behaviour for All Agents

### 2.1 Read Before You Write
- Always read the existing file(s) before modifying them.
- Never overwrite or delete code you haven't read and understood.

### 2.2 Load the Relevant Rule Files
Before starting any task, identify which of the following rule files apply and read them:

| Task type | Rule file |
|---|---|
| Frontend Project structure, modules, services, routing | [`docs/frontend-architecture.md`](training-progress-app/docs/frontend-architecture.md) |
| Components, templates, forms, accessibility | [`docs/ui.md`](training-progress-app/docs/ui.md) |
| Input handling, auth, HTTP, data exposure | [`docs/security.md`](training-progress-app/docs/security.md) |
| Unit tests, integration tests, coverage | [`docs/testing.md`](training-progress-app/docs/testing.md) |
| SCSS, theming, naming conventions | [`docs/styling.md`](training-progress-app/docs/styling.md) |

When in doubt, load all rule files.

### 2.3 TypeScript Discipline
- `strict` mode is enabled — never disable it or add `// @ts-ignore`.
- Never use `any`. Prefer `unknown` when the type is genuinely unknown.
- Prefer explicit return types on public methods and functions.
- Use `const` by default; use `let` only when reassignment is required.

### 2.4 Code Style & Formatting
- All code must be Prettier-compliant (`printWidth: 100`, `singleQuote: true`).
- Do not introduce formatting changes unrelated to the task.
- Follow Angular's official style guide naming conventions:
  - Components: `feature-name.component.ts`
  - Services: `feature-name.service.ts`
  - Files use kebab-case; classes use PascalCase.

### 2.5 Minimal, Targeted Changes
- Make only the changes required by the task.
- Do not refactor, reformat, or "improve" code that is outside the scope of the task.
- Do not add comments, docstrings, or annotations to code you did not change.

### 2.6 No Broken Builds
- Every change must leave the project in a compilable, passing-test state.
- Run `ng build` mentally (or literally when a terminal is available) to verify output.
- If a change would break an existing test, fix the test as part of the same task.

### 2.7 Dependency Management
- Do not add new `npm` dependencies without explicit user approval.
- Do not upgrade existing dependencies as a side-effect of a task.
- When a new dependency is genuinely required, state the reason before installing it.

### 2.8 No Destructive Actions Without Confirmation
- Never delete files, branches, or database records without explicit user confirmation.
- Never run `git push`, `git reset --hard`, or `git push --force` autonomously.

### 2.9 Security by Default
- Never hard-code credentials, API keys, tokens, or secrets anywhere in the codebase.
- Always sanitise user-supplied values before rendering or processing.
- Apply the full rules from [`docs/security.md`](training-progress-app/docs/security.md) on any task that touches HTTP, auth, forms, or dynamic content.

### 2.10 Accessibility
- Every UI change must remain keyboard-navigable and screen-reader friendly.
- Full requirements are in [`docs/ui.md`](training-progress-app/docs/ui.md).

---

## 3. Commit Message Convention

Use [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <short summary>
```

Types: `feat`, `fix`, `refactor`, `test`, `chore`, `docs`, `style`, `perf`

Examples:
- `feat(dashboard): add weekly progress chart`
- `fix(auth): handle expired token redirect`
- `test(training-log): add unit tests for entry service`

---

## 4. Definition of Done

A task is complete only when **all** of the following are true:

- [ ] The code compiles without errors (`ng build`).
- [ ] All existing tests pass (`ng test`).
- [ ] New behaviour is covered by tests (see [`docs/testing.md`](training-progress-app/docs/testing.md)).
- [ ] Code is Prettier-formatted.
- [ ] No `any` types or `// @ts-ignore` comments were introduced.
- [ ] Relevant rule files were consulted and followed.
