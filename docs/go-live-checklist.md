# Go-Live Smoke/UAT Checklist

## Security and Access
- [ ] `Jwt__SecretKey` is configured securely (User Secrets for dev, environment/secret store for prod)
- [ ] Unauthorized access is blocked for protected endpoints (`/api/listings/me`, `/api/payments/*`)
- [ ] Role policies verified:
  - [ ] `Moderator` or `Admin` can call `/api/listings/{id}/moderate`
  - [ ] only `Admin` can call `/api/listings/{id}/featured` and `/api/payments/boost/confirm`

## Authentication and OTP
- [ ] Register creates user as unverified and issues OTP request flow
- [ ] OTP validation accepts only 6-digit codes
- [ ] Expired/invalid OTP is rejected
- [ ] Successful OTP verification marks user as verified

## Payments
- [ ] `POST /api/payments/boost` creates `Pending` transaction
- [ ] Amount is server-calculated (client amount is not accepted)
- [ ] `POST /api/payments/boost/confirm` with matching amount sets `Success`
- [ ] Feature activation is applied only after successful confirm

## Public Search Hardening
- [ ] Public search never returns deleted listings
- [ ] Public search is constrained to approved listing status

## API Error Contract
- [ ] Validation errors return unified `ApiErrorResponse` with `code=validation_failed`
- [ ] Runtime exceptions return unified `ApiErrorResponse` with `traceId`

## Build and Tests
- [ ] `dotnet build` succeeds for entire workspace
- [ ] `dotnet test MaklerWebApp.Tests.Unit` passes (non-skeleton tests)
- [ ] `dotnet test MaklerWebApp.Tests.Integration` passes

## Database and Deployment Prep
- [ ] Latest EF migrations are applied (`dotnet ef database update`)
- [ ] Production appsettings contain no secrets
- [ ] `.env` and local secret files are excluded from source control
