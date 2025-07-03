### **OpenAPI Specification**

```yaml
openapi: 3.0.1
info:
  title: "RELEVO C# Main API"
  description: |
    API for managing core clinical workflows, patient data, and handovers. 
    All endpoints are protected and require a valid JWT from Clerk.
  version: "v1.1"

servers:
  - url: https://api.relevo.app/v1
    description: Production Server
paths:
  # --------------------------------------------------------------------------
  # Setup & User Context Endpoints
  # --------------------------------------------------------------------------
  /setup/units:
    get:
      tags: [ Setup ]
      summary: "Get Hospital Units"
      description: |
        Fetches a list of available hospital units (e.g., PICU, NICU) to populate selection UIs during the Daily Setup flow.
        This data is cached in-memory on the server to reduce database load.
      responses:
        '200':
          description: "A list of hospital units."
          content:
            application/json:
              schema:
                type: array
                items:
                  $ref: '#/components/schemas/Unit'
              examples:
                example1:
                  value:
                    - id: "unit-1"
                      name: "PICU"
                    - id: "unit-2"
                      name: "NICU"
        '401':
          $ref: '#/components/responses/Unauthorized'

  /setup/shifts:
    get:
      tags: [ Setup ]
      summary: "Get Available Shifts"
      description: "Fetches a list of available shift times (e.g., 'Day', 'Night', 'Evening') for the Daily Setup flow."
      responses:
        '200':
          description: "A list of available shifts."
          content:
            application/json:
              schema:
                type: array
                items:
                  $ref: '#/components/schemas/Shift'
              examples:
                example1:
                  value:
                    - id: "shift-day"
                      name: "Day Shift"
                      startTime: "07:00"
                      endTime: "19:00"
                    - id: "shift-night"
                      name: "Night Shift"
                      startTime: "19:00"
                      endTime: "07:00"
        '401':
          $ref: '#/components/responses/Unauthorized'

  /units/{unitId}/patients:
    get:
      tags: [ Setup ]
      summary: "Get Patients Available for Assignment by Unit"
      description: |
        Retrieves a roster of patients within a specific hospital unit who are available to be assigned to a clinician for a shift.
      parameters:
        - name: unitId
          in: path
          required: true
          schema: { type: string, description: "The ID of the hospital unit." }
        - name: page
          in: query
          description: "Page number for pagination."
          schema:
            type: integer
            default: 1
        - name: pageSize
          in: query
          description: "Number of items per page for pagination."
          schema:
            type: integer
            default: 25
      responses:
        '200':
          description: "A paginated list of patients in the specified unit available for assignment."
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/PaginatedPatientsRoster'
        '401':
          $ref: '#/components/responses/Unauthorized'
        '404':
          $ref: '#/components/responses/NotFound'

  /me/assignments:
    post:
      tags: [ Setup ]
      summary: "Assign Patients for a Shift"
      description: |
        Assigns a list of selected patients to the currently authenticated clinician for their selected shift. This is the final step of the Daily Setup.
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/PatientAssignmentRequest'
            examples:
              example1:
                value:
                  shiftId: "shift-day"
                  patientIds: [ "pat-123", "pat-456" ]
      responses:
        '204':
          description: "Patients assigned successfully."
        '400':
          $ref: '#/components/responses/BadRequest'
        '401':
          $ref: '#/components/responses/Unauthorized'

  /me/patients:
    get:
      tags: [ Patients ]
      summary: "Get Assigned Patients"
      description: |
        Retrieves the list of patients currently assigned to the authenticated clinician, used to populate the main Shift Hub and Patient Management views.
      parameters:
        - name: page
          in: query
          description: "Page number for pagination."
          schema:
            type: integer
            default: 1
        - name: pageSize
          in: query
          description: "Number of items per page for pagination."
          schema:
            type: integer
            default: 25
      responses:
        '200':
          description: "A paginated list of the clinician's patients with their handover status."
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/PaginatedPatientSummaryCards'
        '401':
          $ref: '#/components/responses/Unauthorized'

  /patients/{patientId}:
    get:
      tags: [ Patients ]
      summary: "Get Patient Details"
      description: |
        Fetches comprehensive, read-only information for a single patient to populate the `PatientDetailView`.
      parameters:
        - name: patientId
          in: path
          required: true
          schema: { type: string, format: uuid }
      responses:
        '200':
          description: "Comprehensive details for the specified patient."
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/PatientDetail'
              examples:
                example1:
                  value:
                    id: "pat-789"
                    name: "Charlie Brown"
                    mrn: "MRN0001"
                    dob: "2010-10-10"
                    gender: "Male"
                    admissionDate: "2024-06-20T08:00:00Z"
                    currentUnit: "PICU"
                    roomNumber: "205"
                    diagnosis: "Asthma Exacerbation"
                    allergies: [ "Penicillin" ]
                    medications: [ "Albuterol", "Prednisone" ]
                    notes: "Patient stable, requiring nebulizer treatments every 4 hours."
        '401':
          $ref: '#/components/responses/Unauthorized'
        '404':
          $ref: '#/components/responses/NotFound'
  /patients/{patientId}/handovers:
    get:
      tags: [ Patients ]
      summary: "Get Patient Handover Timeline"
      description: |
        Retrieves a chronological list of all handover sessions associated with a specific patient. 
        This is used to build the interactive Patient Timeline for navigating between historical handovers.
      parameters:
        - name: patientId
          in: path
          required: true
          schema: { type: string, format: uuid }
        - name: page
          in: query
          description: "Page number for pagination."
          schema:
            type: integer
            default: 1
        - name: pageSize
          in: query
          description: "Number of items per page for pagination."
          schema:
            type: integer
            default: 25
      responses:
        '200':
          description: "A paginated list of the patient's handover sessions."
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/PaginatedPatientHandoverTimeline'
        '401':
          $ref: '#/components/responses/Unauthorized'
        '404':
          $ref: '#/components/responses/NotFound'

  /me/profile:
    get:
      tags: [ User ]
      summary: "Get User Profile"
      description: "Retrieves the current authenticated user's preferences and profile settings."
      responses:
        '200':
          description: "User profile data."
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/UserProfile'
              examples:
                example1:
                  value:
                    userId: "user-clerk-123"
                    theme: "dark"
                    notificationsEnabled: true
        '401':
          $ref: '#/components/responses/Unauthorized'
    put:
      tags: [ User ]
      summary: "Update User Profile"
      description: "Updates the current authenticated user's preferences and profile settings."
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/UserProfileUpdate'
            examples:
              example1:
                value:
                  theme: "light"
      responses:
        '204':
          description: "User profile updated successfully."
        '400':
          $ref: '#/components/responses/BadRequest'
        '401':
          $ref: '#/components/responses/Unauthorized'

  /me/notifications:
    get:
      tags: [ User ]
      summary: "Get User Notifications"
      description: "Fetches a list of historical notifications for the authenticated user, supporting the NotificationsView feature."
      parameters:
        - name: page
          in: query
          description: "Page number for pagination."
          schema:
            type: integer
            default: 1
        - name: pageSize
          in: query
          description: "Number of items per page for pagination."
          schema:
            type: integer
            default: 25
      responses:
        '200':
          description: "A paginated list of the user's notifications."
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/PaginatedNotifications'
        '401':
          $ref: '#/components/responses/Unauthorized'

  # --------------------------------------------------------------------------
  # Handover Workflow Endpoints
  # --------------------------------------------------------------------------
  /handovers:
    post:
      tags: [ Handovers ]
      summary: "Initiate a Handover"
      description: |
        Creates a new, stateful handover session for a specific patient, marking the beginning of the I-PASS workflow.
        Upon successful creation, this process also initializes an associated collaborative document for 'Situation Awareness'.
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/HandoverCreationRequest'
            examples:
              example1:
                value:
                  patientId: "pat-123"
      responses:
        '201':
          description: "Handover session created successfully."
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/Handover'
              examples:
                example1:
                  value:
                    id: "hvo-abc-1"
                    patientId: "pat-123"
                    status: "InProgress"
                    illnessSeverity:
                      severity: "Stable"
                    patientSummary:
                      content: "Initial patient summary."
                    actionItems: [ ]
                    situationAwarenessDocId: "hvo-abc-1-sa"
                    synthesis: null
        '400':
          $ref: '#/components/responses/BadRequest'
        '401':
          $ref: '#/components/responses/Unauthorized'

  /handovers/{handoverId}:
    get:
      tags: [ Handovers ]
      summary: "Get Handover Details"
      description: |
        Fetches the complete state of a specific handover session, including all I-PASS section data.
      parameters:
        - name: handoverId
          in: path
          required: true
          schema: { type: string, format: uuid }
      responses:
        '200':
          description: "The full handover object."
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/Handover'
              examples:
                example1:
                  value:
                    id: "hvo-abc-1"
                    patientId: "pat-123"
                    status: "InProgress"
                    illnessSeverity:
                      severity: "Watcher"
                    patientSummary:
                      content: "Patient admitted with pneumonia, responding well to antibiotics."
                    actionItems:
                      - id: "act-1"
                        description: "Check blood cultures"
                        isCompleted: false
                    situationAwarenessDocId: "hvo-abc-1-sa"
                    synthesis: null
        '401':
          $ref: '#/components/responses/Unauthorized'
        '404':
          $ref: '#/components/responses/NotFound'

  /handovers/{handoverId}/history:
    get:
      tags: [ Handovers ]
      summary: "Get Handover Activity History"
      description: |
        Fetches the human-readable activity log for a single handover session.
        This data is read from a pre-generated log populated by a background service to ensure fast response times. This endpoint is intended to populate the 'Updates' or 'Activity Log' tab in the handover's collaboration view.
      parameters:
        - name: handoverId
          in: path
          required: true
          schema: { type: string, format: uuid }
      responses:
        '200':
          description: "A list of historical activity events for the handover."
          content:
            application/json:
              schema:
                type: array
                items:
                  $ref: '#/components/schemas/HandoverHistoryEvent'
        '401':
          $ref: '#/components/responses/Unauthorized'
        '404':
          $ref: '#/components/responses/NotFound'

  /handovers/{handoverId}/patientSummary:
    put:
      tags: [ Handovers ]
      summary: "Update Patient Summary (Static)"
      description: |
        Updates the content of the Patient Summary section. This is a non-real-time, explicit save action.
        Triggers a `PATIENT_SUMMARY_UPDATED` webhook.
      parameters:
        - name: handoverId
          in: path
          required: true
          schema: { type: string, format: uuid }
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/PatientSummaryContent'
            examples:
              example1:
                value:
                  content: "Updated patient summary: Patient improving, fever resolved."
      responses:
        '204':
          description: "Patient Summary updated."
        '401':
          $ref: '#/components/responses/Unauthorized'
        '403':
          $ref: '#/components/responses/Forbidden'
        '404':
          $ref: '#/components/responses/NotFound'

  /handovers/{handoverId}/illnessSeverity:
    put:
      tags: [ Handovers ]
      summary: "Update Illness Severity"
      description: |
        Sets or updates the patient's stability level.
        Triggers the `ILLNESS_SEVERITY_UPDATED` webhook to broadcast the change.
      parameters:
        - name: handoverId
          in: path
          required: true
          schema: { type: string, format: uuid }
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/IllnessSeverity'
            examples:
              example1:
                value:
                  severity: "Unstable"
      responses:
        '204':
          description: "Severity updated. Real-time event triggered."
        '401':
          $ref: '#/components/responses/Unauthorized'
        '404':
          $ref: '#/components/responses/NotFound'

  /handovers/{handoverId}/actionItems:
    post:
      tags: [ Handovers ]
      summary: "Create an Action Item"
      description: |
        Adds a new task to the shared Action List.
        Triggers the `ACTION_ITEM_CREATED` webhook.
      parameters:
        - name: handoverId
          in: path
          required: true
          schema: { type: string, format: uuid }
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/ActionItemCreateRequest'
            examples:
              example1:
                value:
                  description: "Administer next dose of medication."
                  isCompleted: false
      responses:
        '201':
          description: "Action item created."
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ActionItem'
              examples:
                example1:
                  value:
                    id: "act-new-1"
                    description: "Administer next dose of medication."
                    isCompleted: false
        '401':
          $ref: '#/components/responses/Unauthorized'
        '404':
          $ref: '#/components/responses/NotFound'

  /handovers/{handoverId}/actionItems/{itemId}:
    put:
      tags: [ Handovers ]
      summary: "Update an Action Item"
      description: |
        Updates an existing task in the Action List (e.g., marks as complete, edits text).

        Triggers the `ACTION_ITEM_UPDATED` webhook.
      parameters:
        - name: handoverId
          in: path
          required: true
          schema: { type: string, format: uuid }
        - name: itemId
          in: path
          required: true
          schema: { type: string, format: uuid }
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/ActionItem'
            examples:
              example1:
                value:
                  id: "act-1"
                  description: "Check blood cultures (completed)."
                  isCompleted: true
      responses:
        '204':
          description: "Action item updated."
        '401':
          $ref: '#/components/responses/Unauthorized'
        '404':
          $ref: '#/components/responses/NotFound'
    delete:
      tags: [ Handovers ]
      summary: "Delete an Action Item"
      description: |
        Deletes a task from the shared Action List.
        Triggers the `ACTION_ITEM_DELETED` webhook.
      parameters:
        - name: handoverId
          in: path
          required: true
          schema: { type: string, format: uuid }
        - name: itemId
          in: path
          required: true
          schema: { type: string, format: uuid }
      responses:
        '204':
          description: "Action item deleted."
        '401':
          $ref: '#/components/responses/Unauthorized'
        '404':
          $ref: '#/components/responses/NotFound'

  /handovers/{handoverId}/situationAwareness:
    put:
      tags: [ Handovers ]
      summary: "Update Situation Awareness Content"
      description: |
        Updates the content of the collaborative Situation Awareness section. This endpoint is called by the NestJS service to persist auto-saved content to the primary database.
      parameters:
        - name: handoverId
          in: path
          required: true
          schema: { type: string, format: uuid }
      requestBody:
        required: true
        content:
          application/json:
            schema:
              type: object
              properties:
                content:
                  type: string
                  description: "The content of the Situation Awareness document (e.g., Prosemirror JSON or similar)."
              required:
                - content
            examples:
              example1:
                value:
                  content: "{ \"type\": \"doc\", \"content\": [{\"type\": \"paragraph\", \"content\": [{\"type\": \"text\", \"text\": \"Patient condition stable.\"}]}] }"
      responses:
        '204':
          description: "Situation Awareness content updated and persisted."
        '401':
          $ref: '#/components/responses/Unauthorized'
        '404':
          $ref: '#/components/responses/NotFound'

  /handovers/{handoverId}/synthesis:
    put:
      tags: [ Handovers ]
      summary: "Submit Synthesis and Complete Handover"
      description: |
        The receiving clinician submits their summary, completing the I-PASS workflow.
        Triggers `SYNTHESIS_COMPLETED` and `HANDOVER_STATUS_CHANGED` webhooks.
      parameters:
        - name: handoverId
          in: path
          required: true
          schema: { type: string, format: uuid }
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/Synthesis'
            examples:
              example1:
                value:
                  content: "Receiver's synthesis: Patient is stable, all action items addressed."
      responses:
        '204':
          description: "Synthesis submitted and handover completed."
        '401':
          $ref: '#/components/responses/Unauthorized'
        '403':
          $ref: '#/components/responses/Forbidden'
        '404':
          $ref: '#/components/responses/NotFound'

  # --------------------------------------------------------------------------
  # Collaboration & Search Endpoints
  # --------------------------------------------------------------------------
   /patients/{patientId}/messages:
    get:
      tags: [ Collaboration ]
      summary: "Get Chat History"
      description: |
        Retrieves the message history for the 'Discussion' tab in the CollaborationPanel.
      parameters:
        - name: patientId
          in: path
          required: true
          schema: { type: string, format: uuid }
        - name: page
          in: query
          description: "Page number for pagination."
          schema:
            type: integer
            default: 1
        - name: pageSize
          in: query
          description: "Number of items per page for pagination."
          schema:
            type: integer
            default: 25
      responses:
        '200':
          description: "A paginated list of chat messages for the patient."
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/PaginatedChatMessages'
        '401':
          $ref: '#/components/responses/Unauthorized'
        '404':
          $ref: '#/components/responses/NotFound'
    post:
      tags: [ Collaboration ]
      summary: "Persist a Chat Message"
      description: |
        Saves a new chat message to a patient's discussion thread. Called by the NestJS service.
      parameters:
        - name: patientId
          in: path
          required: true
          schema: { type: string, format: uuid }
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/ChatMessageCreateRequest'
            examples:
              example1:
                value:
****                  userName: "Dr. Smith"
                  content: "Okay, I'll review them now."
      responses:
        '201':
          description: "Message persisted."
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ChatMessage'
              examples:
                example1:
                  value:
                    id: "msg-new"
                    userId: "user-clerk-1"
                    userName: "Dr. Smith"
                    content: "Okay, I'll review them now."
                    timestamp: "2024-07-02T14:05:00Z"
        '401':
          $ref: '#/components/responses/Unauthorized'
        '404':
          $ref: '#/components/responses/NotFound'

  /search:
    get:
      tags: [ Search ]
      summary: "Global Search"
      description: |
        Performs a full-text search across patients and action items to power the CommandPalette. Powered by Oracle Text.
      parameters:
        - name: query
          in: query
          required: true
          schema: { type: string }
      examples:
        query: "John Doe"
      responses:
        '200':
          description: "Search results."
          content:
            application/json:
              schema:
                type: array
                items:
                  $ref: '#/components/schemas/SearchResult'
              examples:
                example1:
                  value:
                    - id: "pat-123"
                      type: "Patient"
                      title: "John Doe"
                      description: "MRN: 12345, PICU"
                    - id: "act-456"
                      type: "Action"
                      title: "Administer medication"
                      description: "Handover for Jane Smith"
        '401':
          $ref: '#/components/responses/Unauthorized'

components:
  schemas:
    # REQUEST/RESPONSE BODIES
    Unit:
      type: object
      properties:
        id: { type: string, description: "Unique identifier for the hospital unit." }
        name: { type: string, description: "Display name of the unit (e.g., 'PICU')." }
      required: [ id, name ]
    Shift:
      type: object
      properties:
        id: { type: string, description: "Unique identifier for the shift." }
        name: { type: string, description: "Display name of the shift (e.g., 'Day Shift', 'Night Shift')." }
        startTime: { type: string, format: time, description: "Start time of the shift (e.g., '07:00')." }
        endTime: { type: string, format: time, description: "End time of the shift (e.g., '19:00')." }
      required: [ id, name, startTime, endTime ]
    PatientRosterItem:
      type: object
      properties:
        id: { type: string, description: "Patient ID." }
        name: { type: string, description: "Patient's full name." }
        mrn: { type: string, description: "Medical Record Number." }
        dob: { type: string, format: date, description: "Date of Birth (YYYY-MM-DD)." }
        unitId: { type: string, description: "The ID of the unit the patient is currently in." }
        roomNumber: { type: string, description: "Patient's room number." }
      required: [ id, name, mrn, unitId, roomNumber ]
    PatientDetail:
      type: object
      properties:
        id: { type: string, description: "Patient ID." }
        name: { type: string, description: "Patient's full name." }
        mrn: { type: string, description: "Medical Record Number." }
        dob: { type: string, format: date, description: "Date of Birth (YYYY-MM-DD)." }
        gender: { type: string, enum: [ Male, Female, Other, Unknown ], description: "Patient's gender." }
        admissionDate: { type: string, format: date-time, description: "Date and time of admission." }
        currentUnit: { type: string, description: "Current hospital unit." }
        roomNumber: { type: string, description: "Patient's current room number." }
        diagnosis: { type: string, description: "Primary diagnosis." }
        allergies: { type: array, items: { type: string }, description: "List of known allergies." }
        medications: { type: array, items: { type: string }, description: "List of current medications." }
        notes: { type: string, description: "General clinical notes (mock content)." }
      required: [ id, name, mrn, dob, currentUnit, roomNumber ]
    Notification:
      type: object
      properties:
        id: { type: string, format: uuid }
        handoverId: { type: string, format: uuid, nullable: true }
        title: { type: string }
        message: { type: string }
        isRead: { type: boolean }
        createdAt: { type: string, format: date-time }
      required: [ id, title, message, isRead, createdAt ]
    HandoverHistoryEvent:
      type: object
      properties:
        id: { type: string }
        userId: { type: string, description: "The permanent ID of the user who performed the action." }
        userName: { type: string, description: "The display name of the user at the time the action was performed." }
        action: { type: string }
        timestamp: { type: string, format: date-time }
        details: { type: string }
      required: [ id, userId, userName, action, timestamp ]
    UserProfile:
      type: object
      properties:
        userId: { type: string, description: "The Clerk user ID." }
        roleName: { type: string, description: "The user's display-friendly role name." }
        permissions:
          type: array
          items:
            type: string
          description: "A list of specific permissions granted to the user for conditional UI rendering."
        theme: { type: string, enum: [ light, dark ] }
        notificationsEnabled: { type: boolean }
      required: [ userId, roleName, permissions, theme, notificationsEnabled ]
    UserProfileUpdate:
      type: object
      properties:
        theme: { type: string, enum: [ light, dark ], description: "User's preferred theme." }
        notificationsEnabled: { type: boolean, description: "Whether user has enabled notifications." }
      minProperties: 1
    PatientAssignmentRequest:
      type: object
      properties:
        shiftId:
          type: string
          description: "The ID of the shift for which the patients are being assigned."
        patientIds:
          type: array
          items: { type: string }
          description: "List of patient IDs to assign to the clinician."
      required: [ shiftId, patientIds ]
    PatientSummaryCard:
      type: object
      properties:
        id: { type: string, description: "Patient ID." }
        name: { type: string, description: "Patient's full name." }
        handoverStatus:
          type: string
          enum: [ NotStarted, InProgress, Completed ]
        handoverId:
          type: string
          format: uuid
          nullable: true
          description: "The unique ID of the handover session if one exists. Null otherwise."
      required: [ id, name, handoverStatus ]
    HandoverCreationRequest:
      type: object
      properties:
        patientId: { type: string }
      required: [ patientId ]
    ActionItem:
      type: object
      properties:
        id: { type: string, format: uuid }
        description: { type: string }
        isCompleted: { type: boolean, default: false }
      required: [ id, description, isCompleted ]
    ActionItemCreateRequest:
      type: object
      properties:
        description: { type: string }
        isCompleted: { type: boolean, default: false }
      required: [ description ]
    ChatMessage:
      type: object
      properties:
        id: { type: string, format: uuid }
        userId: { type: string, description: "Clerk user ID of the sender." }
        userName: { type: string, description: "Display name of the sender." }
        content: { type: string }
        timestamp: { type: string, format: date-time }
      required: [ id, userId, userName, content, timestamp ]
    ChatMessageCreateRequest:
      type: object
      properties:
        userName: { type: string, description: "Display name of the sender." }
        content: { type: string }
      required: [ userName, content ]
    SearchResult:
      type: object
      properties:
        id: { type: string, description: "ID of the found resource (e.g., patient ID)." }
        type: { type: string, enum: [ Patient, Action ], description: "The type of resource found." }
        title: { type: string, description: "Main display text for the result (e.g., patient name)." }
        description: { type: string, description: "Secondary text (e.g., patient MRN or unit)." }
      required: [ id, type, title, description ]

    # HANDOVER SUB-COMPONENTS
    IllnessSeverity:
      type: object
      properties:
        severity:
          type: string
          enum: [ Stable, Watcher, Unstable ]
      required: [ severity ]
    PatientSummaryContent:
      type: object
      properties:
        content: { type: string, description: "The markdown or rich text content of the patient summary." }
      required: [ content ]
    Synthesis:
      type: object
      properties:
        content: { type: string, description: "The receiving clinician's summary of the handover." }
      required: [ content ]

    # MAIN HANDOVER RESOURCE
    Handover:
      type: object
      properties:
        id: { type: string, format: uuid, description: "Unique ID for the handover session." }
        patientId: { type: string }
        status:
          type: string
          enum: [ InProgress, Completed ]
        illnessSeverity:
          $ref: '#/components/schemas/IllnessSeverity'
        patientSummary:
          $ref: '#/components/schemas/PatientSummaryContent'
        actionItems:
          type: array
          items: { $ref: '#/components/schemas/ActionItem' }
        situationAwarenessDocId:
          type: string
          description: "The unique ID for the collaborative Situation Awareness document. The client uses this ID to connect to the real-time Hocuspocus service."
        synthesis:
          $ref: '#/components/schemas/Synthesis'
      required: [ id, patientId, status, illnessSeverity, patientSummary, actionItems, situationAwarenessDocId ]
    PatientHandoverTimelineItem:
      type: object
      description: "A summary of a single handover session for display in a patient's timeline."
      properties:
        handoverId:
          type: string
          format: uuid
          description: "The unique ID for the handover session."
        status:
          type: string
          enum: [ InProgress, Completed ]
          description: "The status of the handover."
        createdAt:
          type: string
          format: date-time
          description: "The timestamp when the handover was initiated."
        completedAt:
          type: string
          format: date-time
          nullable: true
          description: "The timestamp when the handover was completed."
        shiftName:
          type: string
          description: "The name of the shift during which the handover occurred."
        illnessSeverity:
          type: string
          enum: [ Stable, Watcher, Unstable ]
          description: "The illness severity at the end of the handover."
      required: [ handoverId, status, createdAt, shiftName, illnessSeverity ]

    # PAGINATION SCHEMAS
    PaginationInfo:
      type: object
      properties:
        totalItems: { type: integer, description: "Total number of items available." }
        totalPages: { type: integer, description: "Total number of pages." }
        currentPage: { type: integer, description: "The current page number." }
        pageSize: { type: integer, description: "Number of items per page." }
    PaginatedPatientsRoster:
      type: object
      properties:
        pagination:
          $ref: '#/components/schemas/PaginationInfo'
        items:
          type: array
          items:
            $ref: '#/components/schemas/PatientRosterItem'
    PaginatedPatientSummaryCards:
      type: object
      properties:
        pagination:
          $ref: '#/components/schemas/PaginationInfo'
        items:
          type: array
          items:
            $ref: '#/components/schemas/PatientSummaryCard'
    PaginatedPatientHandoverTimeline:
      type: object
      properties:
        pagination:
          $ref: '#/components/schemas/PaginationInfo'
        items:
          type: array
          items:
            $ref: '#/components/schemas/PatientHandoverTimelineItem'
    PaginatedNotifications:
      type: object
      properties:
        pagination:
          $ref: '#/components/schemas/PaginationInfo'
        items:
          type: array
          items:
            $ref: '#/components/schemas/Notification'
    PaginatedChatMessages:
      type: object
      properties:
        pagination:
          $ref: '#/components/schemas/PaginationInfo'
        items:
          type: array
          items:
            $ref: '#/components/schemas/ChatMessage'

  responses:
    Unauthorized:
      description: "Unauthorized - JWT is missing or invalid."
    Forbidden:
      description: "Forbidden - User does not have permission to perform this action."
    BadRequest:
      description: "Bad Request - The request body is invalid."
    NotFound:
      description: "Not Found - The specified resource does not exist."

  securitySchemes:
    ClerkAuth:
      type: http
      scheme: bearer
      bearerFormat: JWT
      description: "Clerk-issued JSON Web Token (JWT) is required for all endpoints."

security:
  - ClerkAuth: [ ]
```
### **Webhook & WebSocket Specifications**

The real-time communication specifications now include new events for patient summary updates and action item deletions.

#### **1. Inbound Webhook (C\# API → NestJS Service)**

The C\# API triggers this internal endpoint to notify the NestJS service of a state change that needs broadcasting.

  * **Endpoint:** `POST /internal/v1/broadcast`
  * **Security:** This endpoint must be secured at the network level (e.g., VPC, firewall rules) and/or use a shared secret API key. If using a shared secret API key, it should be sent in the `X-API-Key` HTTP header and validated by the NestJS service.

##### **Webhook Request Body Schema**

```json
{
  "eventType": "ILLNESS_SEVERITY_UPDATED",
  "handoverId": "uuid-for-the-handover",
  "payload": {
    "userId": "uuid-of-user-who-made-change",
    "userName": "Dr. Smith",
    "newSeverity": "Watcher"
  }
}
```

##### **Defined `eventType` values:**

| `eventType` | `payload` Content | Originating Action |
| :--- | :--- | :--- |
| **`PATIENT_SUMMARY_UPDATED`** | `{ userId, userName, content: "..." }` | `PUT /handovers/{id}/patientSummary` |
| **`ILLNESS_SEVERITY_UPDATED`** | `{ userId, userName, newSeverity }` | `PUT /handovers/{id}/illnessSeverity` |
| **`ACTION_ITEM_CREATED`** | `{ userId, userName, actionItem: { ... } }` | `POST /handovers/{id}/actionItems` |
| **`ACTION_ITEM_UPDATED`** | `{ userId, userName, actionItem: { ... } }` | `PUT /handovers/{id}/actionItems/{itemId}` |
| **`ACTION_ITEM_DELETED`** | `{ userId, userName, itemId }` | `DELETE /handovers/{id}/actionItems/{itemId}` |
| **`SYNTHESIS_COMPLETED`** | `{ userId, userName, content: "..." }` | `PUT /handovers/{id}/synthesis` |
| **`HANDOVER_STATUS_CHANGED`**| `{ patientId, newStatus: "Completed" }` | `PUT /handovers/{id}/synthesis` |

Note: `userId` and `userName` values in webhook payloads are derived from the authenticated user's Clerk JWT or user profile data.

-----

#### **2. Outbound WebSocket Events (NestJS Service → Client)**

The NestJS service broadcasts events on specific channels to subscribed clients.

##### **A. Hocuspocus (Collaborative Text)**

This specialized protocol is used for the `Situation Awareness` section.

  * **Connection URI:** `wss://realtime.relevo.app/document/{documentId}`
  * **Example `documentId`:** `handover-uuid-situation-awareness`
  * **Functionality:** Hocuspocus manages real-time text synchronization, cursor tracking, and data persistence.

##### **B. General Purpose Events**

These events power the `CollaborationPanel`, `Shift Hub`, and other live UI elements.

**Channel:** `handover:${handoverId}`

  * **`event: 'handover:activity'`**
      * **Description:** A generic event that populates the "Updates" feed in the `CollaborationPanel`.
      * **Payload:** `{ type: 'ILLNESS_SEVERITY_UPDATED', user: 'Dr. Smith', message: 'updated Illness Severity to Watcher', timestamp: '...' }`
  * **`event: 'presence:update'`**
      * **Description:** Sent when a user joins or leaves the handover channel, populating the avatar list.
      * **Payload:** `{ users: [{ id, name, avatarUrl }, ...] }`
  * **`event: 'chat:message'`**
      * **Description:** A new message for the "Discussion" tab.
      * **Payload:** `{ id, userId, userName, content, timestamp }`
  * **`event: 'chat:typing'`**
      * **Description:** Indicates that a user is actively typing a message.
      * **Payload:** `{ userId, userName, isTyping: boolean }`

**Channel:** `shift-hub:${userId}`

  * **`event: 'handover:status'`**
      * **Description:** Updates the status of a patient card on the main `Shift Hub` dashboard.
      * **Payload:** `{ patientId, handoverId, newStatus: 'Completed' }`



# Mock Hospital Service API Schema

The `hospital-mock-api` (NestJS Mock Hospital Systems) is specifically designed for this purpose, providing predictable data for units, shifts, patient rosters, and clinical details. The `nestjs-service` uses a `Hospital API Module` to call external EMR/Alerts, implying that the mock service should expose similar functionalities.

Here's a schema that the mock service can use, structured to align with the information needed by the C\# service, particularly for the "Daily Setup" and "Patient Management" flows.

**Mock Hospital Service API Schema**

This schema outlines the endpoints and response structures that the `hospital-mock-api` should expose.

```json
{
  "openapi": "3.0.1",
  "info": {
    "title": "RELEVO Mock Hospital Systems API",
    "description": "Mock API to simulate interactions with external hospital EMR/EHR systems for development purposes. No authentication is required for this mock service.",
    "version": "v1.0"
  },
  "servers": [
    {
      "url": "http://localhost:3002/mock-hospital/v1",
      "description": "Local Mock Hospital API Server"
    }
  ],
  "paths": {
    "/units": {
      "get": {
        "tags": ["Setup Data"],
        "summary": "Get All Hospital Units",
        "description": "Retrieves a list of all available hospital units.",
        "responses": {
          "200": {
            "description": "A list of hospital units.",
            "content": {
              "application/json": {
                "schema": {
                  "type": "array",
                  "items": {
                    "$ref": "#/components/schemas/Unit"
                  }
                }
              }
            }
          }
        }
      }
    },
    "/shifts": {
      "get": {
        "tags": ["Setup Data"],
        "summary": "Get Available Shift Times",
        "description": "Fetches a list of available shift times (e.g., 'Day', 'Night', 'Evening').",
        "responses": {
          "200": {
            "description": "A list of available shifts.",
            "content": {
              "application/json": {
                "schema": {
                  "type": "array",
                  "items": {
                    "$ref": "#/components/schemas/Shift"
                  }
                }
              }
            }
          }
        }
      }
    },
    "/units/{unitId}/patients": {
      "get": {
        "tags": ["Patient Data"],
        "summary": "Get Patients Available for Assignment by Unit",
        "description": "Retrieves a roster of patients within a specific hospital unit who are available to be assigned to a clinician for a shift.",
        "parameters": [
          {
            "name": "unitId",
            "in": "path",
            "required": true,
            "schema": {
              "type": "string",
              "description": "The ID of the hospital unit."
            }
          }
        ],
        "responses": {
          "200": {
            "description": "A list of patients in the specified unit available for assignment.",
            "content": {
              "application/json": {
                "schema": {
                  "type": "array",
                  "items": {
                    "$ref": "#/components/schemas/PatientRosterItem"
                  }
                }
              }
            }
          },
          "404": {
            "$ref": "#/components/responses/NotFound"
          }
        }
      }
    },
    "/patients/{patientId}": {
      "get": {
        "tags": ["Patient Data"],
        "summary": "Get Comprehensive Patient Details",
        "description": "Fetches comprehensive, read-only information for a single patient, including medical record number, demographics, and basic clinical overview.",
        "parameters": [
          {
            "name": "patientId",
            "in": "path",
            "required": true,
            "schema": {
              "type": "string",
              "description": "The ID of the patient."
            }
          }
        ],
        "responses": {
          "200": {
            "description": "Comprehensive details for the specified patient.",
            "content": {
              "application/json": {
                "schema": {
                  "$ref": "#/components/schemas/PatientDetail"
                }
              }
            }
          },
          "404": {
            "$ref": "#/components/responses/NotFound"
          }
        }
      }
    }
  },
  "components": {
    "schemas": {
      "Unit": {
        "type": "object",
        "properties": {
          "id": {
            "type": "string",
            "description": "Unique identifier for the hospital unit."
          },
          "name": {
            "type": "string",
            "description": "Display name of the unit (e.g., 'PICU', 'NICU')."
          }
        },
        "required": ["id", "name"]
      },
      "Shift": {
        "type": "object",
        "properties": {
          "id": {
            "type": "string",
            "description": "Unique identifier for the shift."
          },
          "name": {
            "type": "string",
            "description": "Display name of the shift (e.g., 'Day Shift', 'Night Shift')."
          },
          "startTime": {
            "type": "string",
            "format": "time",
            "description": "Start time of the shift (e.g., '07:00')."
          },
          "endTime": {
            "type": "string",
            "format": "time",
            "description": "End time of the shift (e.g., '19:00')."
          }
        },
        "required": ["id", "name", "startTime", "endTime"]
      },
      "PatientRosterItem": {
        "type": "object",
        "properties": {
          "id": {
            "type": "string",
            "description": "Patient ID."
          },
          "name": {
            "type": "string",
            "description": "Patient's full name."
          },
          "mrn": {
            "type": "string",
            "description": "Medical Record Number."
          },
          "dob": {
            "type": "string",
            "format": "date",
            "description": "Date of Birth (YYYY-MM-DD)."
          },
          "unitId": {
            "type": "string",
            "description": "The ID of the unit the patient is currently in."
          },
          "roomNumber": {
            "type": "string",
            "description": "Patient's room number."
          }
        },
        "required": ["id", "name", "mrn", "unitId", "roomNumber"]
      },
      "PatientDetail": {
        "type": "object",
        "properties": {
          "id": {
            "type": "string",
            "description": "Patient ID."
          },
          "name": {
            "type": "string",
            "description": "Patient's full name."
          },
          "mrn": {
            "type": "string",
            "description": "Medical Record Number."
          },
          "dob": {
            "type": "string",
            "format": "date",
            "description": "Date of Birth (YYYY-MM-DD)."
          },
          "gender": {
            "type": "string",
            "enum": ["Male", "Female", "Other", "Unknown"],
            "description": "Patient's gender."
          },
          "admissionDate": {
            "type": "string",
            "format": "date-time",
            "description": "Date and time of admission."
          },
          "currentUnit": {
            "type": "string",
            "description": "Current hospital unit."
          },
          "roomNumber": {
            "type": "string",
            "description": "Patient's current room number."
          },
          "diagnosis": {
            "type": "string",
            "description": "Primary diagnosis."
          },
          "allergies": {
            "type": "array",
            "items": {
              "type": "string"
            },
            "description": "List of known allergies."
          },
          "medications": {
            "type": "array",
            "items": {
              "type": "string"
            },
            "description": "List of current medications."
          },
          "notes": {
            "type": "string",
            "description": "General clinical notes (mock content)."
          }
        },
        "required": ["id", "name", "mrn", "dob", "currentUnit", "roomNumber"]
      }
    },
    "responses": {
      "NotFound": {
        "description": "Not Found - The requested resource was not found."
      }
    }
  }
}
```
