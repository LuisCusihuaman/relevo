# Implementation Plan

- [ ] 1. Create database schema and core infrastructure
  - Create new singleton tables with proper indexes and constraints
  - Implement base repository interfaces and domain models
  - Add database migration scripts for table creation
  - _Requirements: 1.1, 1.2, 1.3, 1.4_

- [ ] 1.1 Create singleton database tables
  - Write SQL migration scripts for HANDOVER_PATIENT_DATA, HANDOVER_SITUATION_AWARENESS, and HANDOVER_SYNTHESIS tables
  - Add proper indexes on HANDOVER_ID foreign keys and UPDATED_AT columns
  - Create database constraints to ensure referential integrity with HANDOVERS table
  - _Requirements: 1.1, 1.2, 1.3_

- [ ] 1.2 Implement domain models and DTOs
  - Create HandoverPatientData, HandoverSituationAwareness, and HandoverSynthesis domain models
  - Implement request/response DTOs for each singleton type (Get/Update requests and responses)
  - Add validation attributes and data annotations for proper model binding
  - _Requirements: 1.4, 2.4_

- [ ] 1.3 Create repository interfaces and base implementation
  - Define IHandoverSingletonRepository interface with methods for each singleton type
  - Implement OracleHandoverSingletonRepository with basic CRUD operations
  - Add IsHandoverMutableAsync method to check handover state before updates
  - Write unit tests for repository methods
  - _Requirements: 1.1, 5.1, 5.2_

- [ ] 2. Implement read-only endpoints (Phase 0)
  - Create GET endpoints for each singleton type
  - Update service layer to support singleton data retrieval
  - Populate DTOs from current source of truth during transition
  - _Requirements: 2.1, 2.2, 2.3_

- [ ] 2.1 Add GET endpoints for singleton data
  - Implement GetPatientDataEndpoint with route /api/handovers/{handoverId}/patient-data
  - Implement GetSituationAwarenessEndpoint with route /api/handovers/{handoverId}/situation-awareness
  - Implement GetSynthesisEndpoint with route /api/handovers/{handoverId}/synthesis
  - Add proper error handling and validation for each endpoint
  - _Requirements: 2.1, 2.2, 2.3, 2.4_

- [ ] 2.2 Update service layer for singleton reads
  - Add GetPatientDataAsync, GetSituationAwarenessAsync, and GetSynthesisAsync methods to ISetupService
  - Implement service methods to read from current source of truth (HANDOVER_SECTIONS or HANDOVERS fields)
  - Add proper mapping between legacy data and new domain models
  - Write unit tests for service layer methods
  - _Requirements: 2.1, 2.2, 2.3_

- [ ] 2.3 Create data mapping utilities
  - Implement mappers to convert legacy HANDOVER_SECTIONS data to new domain models
  - Handle section type detection and content parsing from CLOB fields
  - Add fallback logic to read from HANDOVERS table fields when HANDOVER_SECTIONS is empty
  - Write unit tests for mapping logic with various data scenarios
  - _Requirements: 3.3, 3.4_

- [ ] 3. Implement data migration and backfill (Phase 1)
  - Create migration scripts to populate singleton tables from existing data
  - Implement dual-write functionality in legacy endpoints
  - Add monitoring and conflict resolution for migration process
  - _Requirements: 3.1, 3.2, 3.3, 3.4_

- [ ] 3.1 Create data backfill migration script
  - Write SQL script to migrate existing HANDOVER_SECTIONS data to new singleton tables
  - Use UPDATED_AT timestamps to resolve conflicts when multiple entries exist for same section type
  - Handle data validation and cleanup during migration process
  - Add rollback capability for migration script
  - _Requirements: 3.1, 3.3_

- [ ] 3.2 Implement dual-write in legacy endpoint
  - Update UpdateHandoverSectionEndpoint to write to both old and new storage locations
  - Add section type detection to route updates to appropriate singleton table
  - Implement error handling to continue with legacy write if new table write fails
  - Add logging and monitoring for dual-write success/failure rates
  - _Requirements: 3.2, 7.1, 7.2_

- [ ] 3.3 Add immutability enforcement
  - Implement handover state checking in repository layer before allowing updates
  - Add HandoverImmutableException for completed/expired handovers
  - Update all singleton update methods to check mutability before proceeding
  - Write unit tests for immutability enforcement across different handover states
  - _Requirements: 5.1, 5.2, 5.3_

- [ ] 4. Create PUT endpoints for singleton updates
  - Implement update endpoints for each singleton type
  - Add proper validation and error handling
  - Integrate with immutability checks and audit logging
  - _Requirements: 2.1, 2.2, 2.3, 5.1, 5.2_

- [ ] 4.1 Implement PUT endpoints for singleton data
  - Create UpdatePatientDataEndpoint with proper request validation and error handling
  - Create UpdateSituationAwarenessEndpoint with content validation and status management
  - Create UpdateSynthesisEndpoint with proper audit trail and user tracking
  - Add integration tests for each PUT endpoint with various scenarios
  - _Requirements: 2.1, 2.2, 2.3, 2.4_

- [ ] 4.2 Update service layer for singleton writes
  - Add UpdatePatientDataAsync, UpdateSituationAwarenessAsync, and UpdateSynthesisAsync to ISetupService
  - Implement service methods to write directly to new singleton tables
  - Add proper audit logging with user information and timestamps
  - Write unit tests for service layer update methods
  - _Requirements: 2.1, 2.2, 2.3, 1.3_

- [ ] 4.3 Add comprehensive validation and error handling
  - Implement request validation for each singleton update endpoint
  - Add proper HTTP status codes and error messages for different failure scenarios
  - Handle database constraint violations and provide meaningful error responses
  - Write integration tests for error scenarios and edge cases
  - _Requirements: 2.4, 5.1, 5.2_

- [ ] 5. Update frontend to use new typed endpoints (Phase 2)
  - Create typed hooks for each singleton type
  - Replace generic section handling with specific API calls
  - Update React Query caching strategy
  - _Requirements: 4.1, 4.2, 4.3, 4.4_

- [ ] 5.1 Create typed React hooks for singleton data
  - Implement useSituationAwareness and usePutSituationAwareness hooks with proper TypeScript types
  - Implement usePatientData and usePutPatientData hooks with validation
  - Implement useSynthesis and usePutSynthesis hooks with error handling
  - Add proper loading states and error handling to all hooks
  - _Requirements: 4.1, 4.2, 4.3_

- [ ] 5.2 Update API client with new endpoints
  - Add getSituationAwareness, updateSituationAwareness functions to API client
  - Add getPatientData, updatePatientData functions with proper TypeScript interfaces
  - Add getSynthesis, updateSynthesis functions with error handling
  - Update API client to handle new endpoint response formats
  - _Requirements: 4.1, 4.2, 4.3_

- [ ] 5.3 Replace generic section handling in components
  - Remove getSectionByType helper function usage from handover components
  - Update SituationAwareness component to use useSituationAwareness hook
  - Update PatientSummary component to use usePatientData hook
  - Update SynthesisByReceiver component to use useSynthesis hook
  - _Requirements: 4.4, 4.5_

- [ ] 5.4 Update React Query caching strategy
  - Replace generic "sections" cache keys with resource-scoped keys like ["handover", id, "situation-awareness"]
  - Update cache invalidation logic to target specific singleton resources
  - Add proper cache optimization for singleton data fetching
  - Write tests for cache behavior with new key structure
  - _Requirements: 4.4_

- [ ] 6. Add performance optimizations and monitoring
  - Create database indexes for efficient querying
  - Add performance monitoring for new endpoints
  - Implement query optimization for singleton data access
  - _Requirements: 6.1, 6.2, 6.3, 6.4_

- [ ] 6.1 Optimize database performance
  - Add composite indexes on singleton tables for common query patterns
  - Analyze query execution plans for new singleton table queries
  - Add database statistics and monitoring for singleton table performance
  - Compare performance metrics between old CLOB queries and new typed queries
  - _Requirements: 6.1, 6.2, 6.4_

- [ ] 6.2 Implement API performance monitoring
  - Add response time monitoring for new singleton endpoints
  - Create performance dashboards comparing old vs new endpoint performance
  - Add alerting for performance degradation in singleton endpoints
  - Write performance tests to validate response time requirements
  - _Requirements: 6.3, 6.4_

- [ ] 7. Implement legacy cleanup and deprecation (Phase 3)
  - Add deprecation warnings to legacy endpoints
  - Remove dual-write functionality
  - Clean up old database columns and tables
  - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5_

- [ ] 7.1 Add deprecation warnings to legacy endpoints
  - Update UpdateHandoverSectionEndpoint to log deprecation warnings when accessed
  - Add HTTP headers indicating endpoint deprecation with migration timeline
  - Create monitoring dashboard to track legacy endpoint usage
  - Document migration path for any remaining legacy clients
  - _Requirements: 7.1, 7.2_

- [ ] 7.2 Remove dual-write functionality
  - Stop writing to HANDOVER_SECTIONS table from legacy endpoint
  - Remove dual-write logic and associated error handling
  - Update legacy endpoint to return deprecation notices
  - Add final migration verification to ensure data consistency
  - _Requirements: 7.3, 7.4_

- [ ] 7.3 Clean up database schema
  - Remove duplicate content columns (PATIENT_SUMMARY, SYNTHESIS) from HANDOVERS table
  - Mark HANDOVER_SECTIONS table as read-only for grace period
  - Create final cleanup script to drop HANDOVER_SECTIONS table after grace period
  - Update database documentation to reflect new schema structure
  - _Requirements: 7.4, 7.5_

- [ ] 8. Add comprehensive testing and documentation
  - Create end-to-end tests for complete migration flow
  - Add integration tests for new singleton endpoints
  - Update API documentation and developer guides
  - _Requirements: All requirements validation_

- [ ] 8.1 Create comprehensive test suite
  - Write end-to-end tests covering complete handover lifecycle with new endpoints
  - Add integration tests for migration scenarios and data consistency
  - Create performance tests comparing old vs new system performance
  - Add regression tests to ensure existing functionality remains intact
  - _Requirements: All requirements validation_

- [ ] 8.2 Update documentation and developer guides
  - Update API documentation with new singleton endpoint specifications
  - Create migration guide for developers working with handover data
  - Document new frontend patterns and hook usage
  - Add troubleshooting guide for common migration issues
  - _Requirements: 7.3_