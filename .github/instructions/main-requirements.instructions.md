---
name: 'Main Requirements'
description: 'Main requirements for the Front-end of the project'
applyTo: '**/*.tsx  **/*.ts'
---

The React Front-end should be aware of the following requirements, implemented and exposed by the .NET back-end:

1. Cookie-based authentication of the backend REST APIs:
	– with JWT token storage and validation per user’s session
	– with Refresh token storage and validation per user’s session
2. Stores the following entities in relational Sql Db:
	– Entities: Users, Tenants, Roles (user’s roles per tenant), Tenant’s Contacts
3. Role-based authorization includes the following roles:
	– System Admin, Tenant Admin, Power User, and End User
  – Upon fresh startup the System is seeded with one System Admin user
  – sends Verify-Email message to the user’s email for confirmation
4. System Admin API - API for functionalities limited to System Admin user only:
	– can create other System Admin users (user registration)
	– can create Tenants - tenant’s profile with name, BIC, type, etc.
	– can create Users related to Tenant (user registration) with role, either:
    – Tenant Admin, Power User, or End User
    – lists all Tenants in the system
	  – lists all Users in the System and per Tenant
5. Tenant Admin API - API for functionalities specific to Tenant Admin user only:
	– can create Users in their own Tenants (user registration) with role, either:
    – Tenant Admin, Power User, or End User
	– lists all Users in their own Tenants and per Tenant
	– updated their own Tenants - edit tenant profile, deactivate, assign contacts
6. Any User in the System:
	– receives Verify-Email message upon registration
	– verify-email confirmation, forgot/reset password
	– upon authentication, can edit their own user profile, as:
    – first name, last name, phone, email (user receives verify-email message in the new email address), change password
