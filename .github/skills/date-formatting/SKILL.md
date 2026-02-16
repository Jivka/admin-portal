---
name: date-formatting
description: On the UI convert and format dates to the user's local timezone and user's browser settings before displaying them.
---

As all dates are strored in UTC in the backend, the frontend should always convert dates to the user's local timezone before displaying them. Use the `toLocaleString()` method for date formatting, which automatically handles timezone conversion and localization based on the user's browser settings. For example:

```typescript
const date = new Date(utcDateString);
const formattedDate = date.toLocaleString(); // This will display the date in the user's local timezone and format
```