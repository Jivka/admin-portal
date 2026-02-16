---
name: 'Error Handling Conventions'
description: 'Frontend error handling conventions for React and TypeScript files.'
applyTo: '**/*.tsx'
---

Handle the back-end validations as returned from the backend API calls and don’t make them up.
For example, if the backend returns a 400 with a message that the email is invalid, show that message to the user instead of making up your own message. This ensures that the user gets accurate feedback based on the actual validation rules implemented on the backend.