# P4 Backend Review Report

Date: 2026-05-31
Branch reviewed: `feature/p4-management`
Target branch: `dev`

## Verification Summary

- Static review completed for Transfers, Contracts, Injuries, Dashboard, and Auth/business logic.
- Runtime Swagger verification was not completed because the local environment does not have the `dotnet` CLI available on `PATH`.
- `dotnet build` was attempted and failed before compilation with: `dotnet : The term 'dotnet' is not recognized`.
- `git diff --check` passes.

## Fixes Applied

- Changed P4 POST/PUT authorization to `Manager` only for Transfers, Contracts, and Injuries.
- Kept P4 DELETE authorization as `Admin` only.
- Added P4 authorization regression tests.
- Fixed injury active filtering so active injuries include `Active` and `Recovering`, excluding only `Recovered`.
- Added injury invalid-status handling and pagination max validation.
- Hardened contract pagination, expiring contract filtering, date validation, missing player/club validation, and positive ID validation.
- Fixed contract active enforcement to deactivate all existing active contracts for the same player before creating or activating another one.
- Added contract regression coverage for stale multiple-active rows and missing player/club validation.
- Added transfer service tests for filtering, pagination, player lookup, create validation, update, and delete.
- Fixed test project target from `net10.0` to `net8.0`.
- Fixed injury tests to use the enum DTO type instead of assigning strings.

## PR #1 - Transfers

STATUS: PASS STATIC / RUNTIME NOT VERIFIED

TESTS: Added automated service coverage. Runtime Swagger tests not run because `dotnet` is unavailable.

ISSUES:
- POST/PUT allowed `Admin,Manager`; requirement says Manager only.
- Date filter accepted `fromDate > toDate`.
- Transfer endpoint tests were missing.

FIXES:
- POST/PUT now require `Manager`.
- GET list now rejects invalid date ranges with 400.
- Added transfer service tests for list/filter/sort, get by player, create validation, update, and delete.

READY FOR MERGE: NO, pending successful build/test/Swagger run on a machine with .NET SDK.

## PR #2 - Contracts

STATUS: PASS STATIC / RUNTIME NOT VERIFIED

TESTS: Existing contract tests expanded. Runtime tests not run because `dotnet` is unavailable.

ISSUES:
- POST/PUT allowed `Admin,Manager`; requirement says Manager only.
- Service deactivated only one active contract, leaving risk if bad historical data already had multiple active rows.
- Missing player/club IDs could bubble as database exceptions.
- Active/expiring endpoints lacked service-level pagination validation.
- Expiring contracts could include already-expired active rows.
- Controller did not consistently document 400/401/403/409/500 responses.

FIXES:
- POST/PUT now require `Manager`.
- Active enforcement now deactivates all other active contracts for the player.
- Added player/club existence validation and date validation in service.
- Added pagination/day validation and future-window filtering.
- Added 409 handling for database update conflicts.
- Expanded Swagger response annotations.
- Added regression tests for multiple active rows and missing player/club.

READY FOR MERGE: NO, pending successful build/test/Swagger run on a machine with .NET SDK.

## PR #3 - Injuries

STATUS: PASS STATIC / RUNTIME NOT VERIFIED

TESTS: Existing injury tests updated. Runtime tests not run because `dotnet` is unavailable.

ISSUES:
- Active injuries endpoint returned only `Active`, despite contract/documentation expecting non-recovered injuries.
- POST/PUT allowed `Admin,Manager`; requirement says Manager only.
- Invalid status filters were silently ignored.
- Positive ID and pagination max validation were incomplete.
- Tests assigned string values to an enum DTO property.

FIXES:
- Active injuries now return `Active` and `Recovering`.
- POST/PUT now require `Manager`.
- Invalid status filter returns 400 through `ArgumentException`.
- Added positive ID and page size validation.
- Updated tests to use `InjuryStatus.Recovered`.

READY FOR MERGE: NO, pending successful build/test/Swagger run on a machine with .NET SDK.

## PR #4 - Dashboard

STATUS: PASS STATIC / RUNTIME NOT VERIFIED

TESTS: Existing dashboard service tests cover summary, top scorers, upcoming matches, injured players, expiring contracts, recent transfers, empty state, and invalid query parameters.

ISSUES:
- No code blocker found in static review.
- Runtime Swagger verification could not be performed without .NET SDK.

FIXES:
- No dashboard code changes required.

READY FOR MERGE: NO, pending successful build/test/Swagger run on a machine with .NET SDK.

## PR #5 - Auth / Business Logic

STATUS: PASS STATIC / RUNTIME NOT VERIFIED

TESTS: Added P4 authorization tests. Existing auth registration tests remain in place.

ISSUES:
- P4 write endpoints did not match the requested role policy.
- `FootballClubAPI.Tests` targeted `net10.0`, which is inconsistent with the backend `net8.0` project and likely blocks normal .NET 8 test runs.

FIXES:
- Added reflection tests asserting P4 GET controllers require authentication, P4 POST/PUT endpoints are Manager-only, and P4 DELETE endpoints are Admin-only.
- Changed `FootballClubAPI.Tests` target framework to `net8.0`.

READY FOR MERGE: NO, pending successful build/test/Swagger run on a machine with .NET SDK.

## Required Final Verification

Run these before merge:

```powershell
dotnet build
dotnet test
dotnet run --project BackendAPI/FootballClubAPI.csproj
```

Then verify Swagger at the configured development URL and execute:

- Transfers: GET list, GET filters, GET by player, POST, PUT, DELETE.
- Contracts: GET list, GET specific, GET active, GET expiring, POST create, POST active-contract business logic, PUT, DELETE.
- Injuries: GET list, GET active, POST, PUT.
- Dashboard: summary, top scorers, upcoming matches, injured players, expiring contracts, recent transfers.
- Authorization: unauthenticated 401, non-manager write 403, non-admin delete 403.

## Final Approval

READY FOR MERGE INTO `dev`: NO

Reason: code fixes and static review are complete, but production approval requires build, automated tests, and Swagger runtime verification. Those are blocked in this environment by the missing .NET SDK/CLI.
