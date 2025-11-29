# Relevo API Migration & Endpoints

## Migration Checklist

*(Use the checklist below to track the migration progress if needed)*

## API Endpoints

Generated from `swagger.json`.

### Units
| Done | Method | Endpoint | Description |
| :---: | :--- | :--- | :--- |
| [x] | `GET` | `/units/{unitId}/patients` | Get patients by unit |

### Shift-Check-In
| Done | Method | Endpoint | Description |
| :---: | :--- | :--- | :--- |
| [x] | `GET` | `/shift-check-in/shifts` | Get shifts |
| [x] | `GET` | `/shift-check-in/units` | Get units |

### Patients
| Done | Method | Endpoint | Description |
| :---: | :--- | :--- | :--- |
| [x] | `GET` | `/patients` | Get all patients |
| [x] | `GET` | `/patients/{patientId}` | Get patient by ID |
| [x] | `GET` | `/patients/{patientId}/handovers` | Get patient handovers |
| [x] | `GET` | `/handovers/{handoverId}` | Get handover by ID |
| [x] | `GET` | `/handovers/{handoverId}/patient` | Get patient data for handover |
| [x] | `POST` | `/handovers` | Create handover |
| [x] | `GET` | `/patients/{patientId}/summary` | Get patient summary |
| [x] | `POST` | `/patients/{patientId}/summary` | Create patient summary |
| [x] | `PUT` | `/patients/{patientId}/summary` | Update patient summary |
| [x] | `GET` | `/patients/{patientId}/action-items` | Get patient action items |


### Handovers
| Done | Method | Endpoint | Description |
| :---: | :--- | :--- | :--- |
| [x] | `POST` | `/handovers/{handoverId}/accept` | Accept handover |
| [x] | `POST` | `/handovers/{handoverId}/cancel` | Cancel handover |
| [x] | `POST` | `/handovers/{handoverId}/complete` | Complete handover |
| [x] | `GET` | `/handovers/pending` | Get pending handovers |
| [x] | `POST` | `/handovers/{handoverId}/reject` | Reject handover |
| [x] | `POST` | `/handovers/{handoverId}/start` | Start handover |
| [x] | `POST` | `/handovers` | Create handover |
| [x] | `GET` | `/handovers/{handoverId}` | Get handover by ID |
| [x] | `GET` | `/handovers/{handoverId}/patient` | Get patient handover data (demographics, physicians, status) |
| [x] | `GET` | `/handovers/{handoverId}/patient-data` | Get patient clinical data |
| [x] | `PUT` | `/handovers/{handoverId}/patient-data` | Update patient clinical data |
| [x] | `DELETE` | `/handovers/{handoverId}/contingency-plans/{contingencyId}` | Delete contingency plan |
| [x] | `GET` | `/handovers/{handoverId}/contingency-plans` | Get contingency plans |
| [x] | `POST` | `/handovers/{handoverId}/contingency-plans` | Create contingency plan |
| [x] | `POST` | `/handovers/{id}/ready` | Mark handover as ready |
| [x] | `GET` | `/handovers/{handoverId}/situation-awareness` | Get situation awareness |
| [x] | `PUT` | `/handovers/{handoverId}/situation-awareness` | Update situation awareness |
| [x] | `GET` | `/handovers/{handoverId}/synthesis` | Get synthesis |
| [x] | `PUT` | `/handovers/{handoverId}/synthesis` | Update synthesis |


### Me (User Context)
| Done | Method | Endpoint | Description |
| :---: | :--- | :--- | :--- |
| [x] | `POST` | `/me/assignments` | Post assignments |
| [x] | `DELETE` | `/me/handovers/{handoverId}/action-items/{itemId}` | Delete handover action item |
| [x] | `PUT` | `/me/handovers/{handoverId}/action-items/{itemId}` | Update handover action item |
| [x] | `GET` | `/me/handovers/{handoverId}/action-items` | Get handover action items |
| [x] | `POST` | `/me/handovers/{handoverId}/action-items` | Create handover action item |
| [x] | `GET` | `/me/handovers/{handoverId}/activity` | Get handover activity |
| [x] | `GET` | `/me/handovers/{handoverId}/checklists` | Get handover checklists |
| [x] | `PUT` | `/me/handovers/{handoverId}/checklists/{itemId}` | Update checklist item |
| [x] | `GET` | `/me/handovers/{handoverId}/contingency-plans` | Get handover contingency plans |
| [x] | `POST` | `/me/handovers/{handoverId}/contingency-plans` | Create contingency plan |
| [x] | `GET` | `/me/handovers/{handoverId}/messages` | Get handover messages |
| [x] | `POST` | `/me/handovers/{handoverId}/messages` | Create handover message |
| [x] | `GET` | `/me/handovers` | Get my handovers |
| [x] | `GET` | `/me/patients` | Get my patients |
| [x] | `GET` | `/me/profile` | Get my profile |
