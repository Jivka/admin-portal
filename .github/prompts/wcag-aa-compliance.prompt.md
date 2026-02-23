---
name: 'wcag-aa-compliance'
description: 'Audit and remediate [FEATURE NAME] feature UI for WCAG 2.2 Level AA conformance. Apply fixes directly to the source files.'
---

# Make a Feature UI WCAG 2.2 AA Compliant

## Context

You are auditing and remediating the **[FEATURE NAME]** feature (e.g., `client/src/features/tenants/`) for WCAG 2.2 Level AA conformance. The UI is built with **React 19 + TypeScript + MUI v5**. Apply fixes directly to the source files.

---

## Audit Checklist (WCAG 2.2 AA Criteria)

Work through each criterion below. For every issue found, apply the fix immediately.

### 1. Perceivable

- **1.1.1 Non-text Content** — Every `<img>`, icon button, and SVG must have a meaningful `alt` or `aria-label`. Decorative images use `alt=""` and `aria-hidden="true"`.
- **1.3.1 Info and Relationships** — Use semantic HTML. Replace generic `<div>`/`<span>` with `<nav>`, `<main>`, `<section>`, `<ul>`, `<table>`, etc. MUI `<Table>` must include `<caption>` or `aria-label`.
- **1.3.2 Meaningful Sequence** — DOM order must match visual reading order. Do not rely solely on CSS to reorder content.
- **1.3.3 Sensory Characteristics** — Instructions must not rely on shape, colour, size, or position alone (e.g., "click the red button" → "click the Delete button").
- **1.3.4 Orientation** — UI must not lock to portrait or landscape.
- **1.3.5 Identify Input Purpose** — Form inputs must carry `autocomplete` attributes where applicable (e.g., `autoComplete="email"`, `autoComplete="current-password"`).
- **1.4.1 Use of Colour** — Colour must not be the only means of conveying information (e.g., error state needs both red colour *and* an icon or text label).
- **1.4.3 Contrast (Minimum)** — Normal text ≥ 4.5:1, large text (≥ 18 pt / 14 pt bold) ≥ 3:1. Audit MUI `Typography`, `Button`, `Chip`, `Badge`, and custom `sx` colours against the theme palette.
- **1.4.4 Resize Text** — Content must be usable at 200% zoom without horizontal scrolling or content loss.
- **1.4.10 Reflow** — Content must reflow at 320 CSS px width without loss of information or functionality.
- **1.4.11 Non-text Contrast** — UI components (input borders, focus rings, checkbox outlines) must meet 3:1 contrast against adjacent colours.
- **1.4.12 Text Spacing** — UI must tolerate increased letter/word/line spacing without content overlap or truncation.
- **1.4.13 Content on Hover or Focus** — Tooltips and popovers triggered on hover/focus must be dismissible (Esc), hoverable, and persistent.

### 2. Operable

- **2.1.1 Keyboard** — Every interactive element must be reachable and operable by keyboard alone (Tab, Shift+Tab, Enter, Space, Arrow keys). No keyboard traps except in modal dialogs (which must manage focus correctly).
- **2.1.2 No Keyboard Trap** — Focus must never be permanently trapped outside of intentional modal dialogs.
- **2.4.3 Focus Order** — Tab order must be logical and follow the visual/reading order.
- **2.4.4 Link Purpose** — Every link and button label must be descriptive in isolation or via `aria-label`/`aria-describedby`. Avoid "Click here" or "Read more".
- **2.4.6 Headings and Labels** — Headings must be hierarchical (`h1 → h2 → h3`). Form inputs must have a visible `<label>` or `aria-label`.
- **2.4.7 Focus Visible** — Every focusable element must display a visible focus ring. Do not use `outline: none` or `outline: 0` without an equally visible replacement. Extend the MUI theme's `focusVisible` styles if needed.
- **2.4.11 Focus Not Obscured (Minimum)** *(new in WCAG 2.2)* — When an element receives focus, it must not be entirely hidden behind sticky headers, banners, or overlays.
- **2.5.3 Label in Name** — For controls with a visible text label, the accessible name must *contain* that visible text.
- **2.5.7 Dragging Movements** *(new in WCAG 2.2)* — Any drag operation must have a single-pointer alternative (e.g., up/down arrow buttons for sortable lists).
- **2.5.8 Target Size (Minimum)** *(new in WCAG 2.2)* — Interactive targets must be at least **24×24 CSS px**. Prefer **44×44 px** for touch targets.

### 3. Understandable

- **3.1.1 Language of Page** — `<html lang="en">` must be present in `client/index.html`.
- **3.2.1 On Focus** — Receiving focus must not trigger unexpected context changes.
- **3.2.2 On Input** — Changing a control value must not automatically submit forms or navigate away without user intent.
- **3.3.1 Error Identification** — Validation errors must be identified in text, not just by colour, and associated with the relevant input via `aria-describedby`.
- **3.3.2 Labels or Instructions** — Required fields must be labelled (e.g., asterisk + "Required fields are marked *"). Provide format hints where applicable.
- **3.3.7 Redundant Entry** *(new in WCAG 2.2)* — Do not ask users to re-enter information already provided in the same session (pre-fill where possible).

### 4. Robust

- **4.1.2 Name, Role, Value** — All custom interactive components must expose correct ARIA role, name, and state. Verify MUI components pass through `aria-*` props correctly.
- **4.1.3 Status Messages** — Dynamic status messages (toasts, alerts, progress) must use `role="status"` or `role="alert"` so screen readers announce them without moving focus.

---

## MUI-Specific Implementation Notes

- Use `inputProps` / `slotProps` on MUI `TextField` to pass `aria-describedby`, `aria-required`, and `autoComplete`.
- Pass `aria-label` to `IconButton` — MUI does not infer it from the child icon.
- Add `aria-live="polite"` regions for loading states (skeleton/spinner areas).
- Use MUI `<Alert>` with `role="alert"` for error banners rather than plain `<div>`.
- Extend the MUI theme (`theme.components.MuiButtonBase.styleOverrides.root`) to enforce a minimum target size and visible focus ring globally.
- For data tables, add `<caption>` or `aria-label` to `<Table>`, use `scope="col"` on `<TableCell>` header cells, and ensure sort buttons expose `aria-sort`.
- Dialogs (`<Dialog>`) must trap focus, have `aria-labelledby` pointing to the dialog title, and restore focus to the trigger element on close.

---

## Deliverables

1. **Remediated source files** — all issues fixed in-place.
2. **WCAG 2.2 AA Audit Summary** (inline comments or PR description) listing:
   - Each criterion checked.
   - Issues found and the fix applied.
   - Any criterion marked "Not Applicable" with a brief reason.
3. No regressions to existing functionality, routing, or auth flow.
