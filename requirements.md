# Requirements Document

## Introduction

This feature involves refactoring the current handover sections system from a generic, CLOB-based approach to a normalized database schema with specific endpoints for singleton data types. The current system stores all handover section content in a single `HANDOVER_SECTIONS` table with a generic `CONTENT` CLOB field, which creates data integrity issues, makes querying difficult, and leads to duplication between the `HANDOVERS` and `HANDOVER_SECTIONS` tables.

The refactoring will establish clear separation between singleton data (illness severity, patient summary, situation awareness, synthesis) and collection data (action items, contingency plans, messages), providing type-safe endpoints and better data integrity while maintaining backward compatibility during migration.

## Requirements

### Requirement 1: Database Schema Normalization

**User Story:** As a system architect, I want handover singleton data stored in normalized tables with proper typing, so that I can ensure data integrity and enable efficient querying.

#### Acceptance Criteria

1. WHEN the system stores handover singleton data THEN it SHALL use separate tables for each singleton type (patient data, situation awareness, synthesis)
2. WHEN a handover singleton table is created THEN it SHALL have `HANDOVER_ID` as primary key/foreign key to maintain 1:1 relationship
3. WHEN singleton data is stored THEN it SHALL include proper audit fields (last_edited_by, updated_at, status)
4. WHEN the new schema is implemented THEN it SHALL maintain referential integrity with existing handover records
5. WHEN querying singleton data THEN the system SHALL support indexed queries on typed columns instead of CLOB content

### Requirement 2: API Endpoint Restructuring

**User Story:** As a frontend developer, I want specific endpoints for each handover singleton type, so that I can have clear contracts and type safety when updating handover data.

#### Acceptance Criteria

1. WHEN accessing patient data THEN the system SHALL provide `GET|PUT /api/handovers/{id}/patient-data` endpoint
2. WHEN accessing situation awareness THEN the system SHALL provide `GET|PUT /api/handovers/{id}/situation-awareness` endpoint  
3. WHEN accessing synthesis data THEN the system SHALL provide `GET|PUT /api/handovers/{id}/synthesis` endpoint
4. WHEN using singleton endpoints THEN each SHALL have explicit request/response contracts with proper validation
5. WHEN collection endpoints are accessed THEN they SHALL maintain existing plural patterns (`/action-items`, `/contingency-plans`, `/messages`)
6. WHEN the generic `/sections/{sectionId}` endpoint is called THEN it SHALL continue to work during migration phase for backward compatibility

### Requirement 3: Data Migration and Dual-Write Strategy

**User Story:** As a system administrator, I want seamless migration from the old system to the new one without downtime, so that users can continue working while the system is being upgraded.

#### Acceptance Criteria

1. WHEN migration begins THEN the system SHALL backfill new singleton tables from existing `HANDOVER_SECTIONS` data using latest `UPDATED_AT` timestamps
2. WHEN the old endpoint receives updates THEN it SHALL dual-write to both old and new storage locations during transition period
3. WHEN data conflicts occur during migration THEN the system SHALL use `UPDATED_AT` timestamp to determine the authoritative version
4. WHEN migration is complete THEN all historical data SHALL be preserved and accessible through new endpoints
5. WHEN rollback is needed THEN the system SHALL be able to revert to the old system without data loss

### Requirement 4: Frontend Integration Updates

**User Story:** As a frontend developer, I want to replace generic section handling with typed hooks and API calls, so that I can have better type safety and clearer code organization.

#### Acceptance Criteria

1. WHEN updating situation awareness THEN the frontend SHALL use `useSituationAwareness()` and `usePutSituationAwareness()` hooks instead of generic section calls
2. WHEN updating patient data THEN the frontend SHALL use `usePatientData()` and `usePutPatientData()` hooks
3. WHEN updating synthesis THEN the frontend SHALL use `useSynthesis()` and `usePutSynthesis()` hooks
4. WHEN caching data THEN React Query keys SHALL be resource-scoped (e.g., `["handover", id, "situation-awareness"]`) instead of generic sections cache
5. WHEN the migration is complete THEN the `getSectionByType()` helper function SHALL be removed from the codebase

### Requirement 5: State Management and Immutability

**User Story:** As a clinical user, I want completed handovers to remain immutable, so that I can maintain accurate historical records for compliance and audit purposes.

#### Acceptance Criteria

1. WHEN a handover status is "Completed" or "Expired" THEN the system SHALL prevent any updates to singleton or collection data
2. WHEN attempting to modify completed handovers THEN the system SHALL return appropriate error responses with clear messaging
3. WHEN state transitions occur THEN the system SHALL use existing transition endpoints (`/ready`, `/start`, `/accept`, `/complete`, `/cancel`, `/reject`) to enforce immutability rules
4. WHEN creating new handovers THEN they SHALL capture current truth while preserving historical snapshots
5. WHEN enforcing immutability THEN the system SHALL implement checks at both repository and database constraint levels

### Requirement 6: Performance and Queryability

**User Story:** As a system administrator, I want to efficiently query handover data by specific criteria, so that I can generate reports and monitor system performance.

#### Acceptance Criteria

1. WHEN querying by illness severity THEN the system SHALL support indexed queries like "find all handovers with Critical severity"
2. WHEN searching handover content THEN the system SHALL use typed columns instead of CLOB text searches
3. WHEN accessing singleton data THEN response times SHALL be comparable or better than current generic approach
4. WHEN the new schema is active THEN database query plans SHALL show efficient index usage for common queries
5. WHEN generating reports THEN the system SHALL support complex queries across normalized singleton tables

### Requirement 7: Backward Compatibility and Deprecation

**User Story:** As a system maintainer, I want a clear deprecation path for the old generic sections approach, so that I can safely remove legacy code after successful migration.

#### Acceptance Criteria

1. WHEN the new endpoints are deployed THEN the old `/sections/{sectionId}` endpoint SHALL remain functional for a defined deprecation period
2. WHEN legacy endpoints are accessed THEN the system SHALL log deprecation warnings for monitoring
3. WHEN the deprecation period ends THEN the system SHALL provide clear migration documentation for any remaining clients
4. WHEN removing legacy code THEN the `HANDOVER_SECTIONS` table SHALL be kept readable for a grace period before final cleanup
5. WHEN the migration is complete THEN all duplicate content columns SHALL be removed from the `HANDOVERS` table