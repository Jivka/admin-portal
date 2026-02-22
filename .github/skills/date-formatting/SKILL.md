---
name: date-formatting
description: Frontend conventions for displaying backend UTC dates in the user's local timezone.
---

This skill defines how the React (TypeScript) frontend should display dates.

Core rule: **all dates coming from the backend are UTC**. The UI must **convert to the user's local timezone** before display.

## Preferred approach

- Parse the backend value with `new Date(utcString)` (expects ISO-8601 / RFC3339 strings).
- Format using `toLocaleString()` / `toLocaleDateString()` / `toLocaleTimeString()`.
- Do not manually apply timezone offsets or string-slice ISO timestamps for display.

### Default (date + time)

```ts
const formatted = new Date(utcDateString).toLocaleString();
```

### Date-only

```ts
const formatted = new Date(utcDateString).toLocaleDateString();
```

### Time-only

```ts
const formatted = new Date(utcDateString).toLocaleTimeString();
```

### Stable formatting (explicit options)

Use this when you want consistent output across browsers/locales while still respecting the user's timezone:

```ts
const formatted = new Date(utcDateString).toLocaleString(undefined, {
	year: 'numeric',
	month: '2-digit',
	day: '2-digit',
	hour: '2-digit',
	minute: '2-digit',
});
```

## Null/invalid values

- If the backend returns `null`/empty, render `'-'` (or the page’s existing empty-state convention).
- If parsing fails (`Invalid Date`), render `'-'` and avoid throwing in render.

Example helper pattern:

```ts
export function formatUtcDateTime(value?: string | null): string {
	if (!value) return '-';
	const date = new Date(value);
	if (Number.isNaN(date.getTime())) return '-';
	return date.toLocaleString();
}
```

## What not to do

- Don’t display raw UTC strings (e.g., `2026-02-22T12:34:56Z`) directly.
- Don’t use `Date.toISOString()` for UI display (it forces UTC output).
- Don’t hardcode a timezone; rely on the browser locale/timezone unless the product explicitly requires otherwise.

## Quick checklist

- Backend date is UTC → `new Date(value)` → `toLocale*()` for display.
- Use `toLocaleDateString()` for date-only columns.
- Handle `null` / invalid values without crashing the component.
