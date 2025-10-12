# Handover State Machine Implementation Progress

## Completed Items

### Sprint 1: Database Integrity & Indexing ✅
- [x] **1.1** Created `08-constraints.sql` with check constraints:
  - `CHK_COMPLETED_REQUIRES_ACCEPTED`
  - `CHK_ACCEPTED_REQUIRES_STARTED`
  - `CHK_STARTED_REQUIRES_READY`
  - `CHK_SINGLE_TERMINAL_STATE`
- [x] **1.2** Created `09-additional-indexes.sql` with performance indexes
- [x] **1.3** Created `HandoverConstraintsTests.cs` with 6 test methods

### Sprint 2: Expiration Automation & Timezone ✅
- [x] **2.1** Created `ExpireHandoversJob.cs` background job
- [x] **2.2** Created `ExpireHandoversBackgroundService.cs` hosted service
- [x] **2.3** Updated `GetById.cs` to format timestamps in UTC with Z suffix
- [x] **2.4** Created `ExpireHandoversJobTests.cs` with 7 test methods
- [x] **2.5** Created `HandoverConstraintTests.cs` with 3 E2E tests

### Sprint 3: Optimistic Locking (Partial) ✅
- [x] **3.1** Created `10-add-version-column.sql`
- [x] **3.2** Created `OptimisticLockException.cs`
- [x] **3.3** Added optimistic locking methods to `OracleSetupRepository.cs`:
  - `StartHandover(handoverId, userId, expectedVersion)`
  - `ReadyHandover(handoverId, userId, expectedVersion)`
  - `AcceptHandover(handoverId, userId, expectedVersion)`
  - `CompleteHandover(handoverId, userId, expectedVersion)`
  - `CancelHandover(handoverId, userId, expectedVersion)`
  - `RejectHandover(handoverId, userId, reason, expectedVersion)`
  - `ExecuteWithOptimisticLock()` helper method
- [x] **3.4** Updated `ISetupRepository` interface with versioned method overloads
- [x] **3.5** Updated `HandoverRecord` to include `Version` field
- [x] **3.6** Updated `GetHandoverByIdResponse` to include `Version` field
- [x] **3.7** Updated `GetHandoverById` SQL query to SELECT VERSION column

## Remaining Items

### Sprint 3: Optimistic Locking & E2E Tests (Remaining)

#### 3.8: Update API Endpoints ❌
Need to update these endpoints to accept and pass version parameter:
- `AcceptHandover.cs`
- `CompleteHandover.cs`
- `StartHandover.cs`
- `Ready.Post.cs`

Files to modify:
```
relevo-api/src/Relevo.Web/Handovers/AcceptHandover.cs
relevo-api/src/Relevo.Web/Handovers/CompleteHandover.cs
relevo-api/src/Relevo.Web/Handovers/StartHandover.cs
relevo-api/src/Relevo.Web/Handovers/Ready.Post.cs
```

#### 3.9: Create Cancel/Reject Endpoints ❌
Need to create these new endpoints:
- `CancelHandover.cs`
- `RejectHandover.cs`

#### 3.10-3.15: E2E Test Files ❌
Create these test files:
- `HandoverCancellationTests.cs` (4 tests)
- `HandoverRejectionTests.cs` (2 tests)
- `HandoverIdempotencyTests.cs` (2 tests)
- `HandoverConcurrencyTests.cs` (1 test)
- `MultipleHandoversTests.cs` (2 tests)

#### 3.16: Register Background Service ❌
Add to `Program.cs`:
```csharp
services.AddSingleton<ExpireHandoversJob>();
services.AddHostedService<ExpireHandoversBackgroundService>();
```

#### 3.17: Run SQL Scripts ❌
Execute in order:
1. `08-constraints.sql`
2. `09-additional-indexes.sql`
3. `10-add-version-column.sql`

## Notes

- All files are compatible with C# 12 and .NET 8
- Uses Oracle 11g compatible SQL syntax
- Follows hexagonal architecture pattern
- All timestamps now returned in UTC format with Z suffix
- Optimistic locking provides concurrency protection via VERSION column
- Background job runs hourly to expire old handovers

## Next Steps

1. Update API endpoints to accept version parameter (backwards compatible)
2. Create cancel/reject endpoints
3. Implement comprehensive E2E test suite
4. Run SQL migration scripts
5. Register background service
6. Run full test suite to verify implementation

