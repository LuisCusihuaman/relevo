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


### Handovers
| Done | Method | Endpoint | Description |
| :---: | :--- | :--- | :--- |
| [ ] | `POST` | `/handovers/{handoverId}/accept` | Accept handover |
| [ ] | `POST` | `/handovers/{handoverId}/cancel` | Cancel handover |
| [ ] | `POST` | `/handovers/{handoverId}/complete` | Complete handover |
| [ ] | `DELETE` | `/handovers/{handoverId}/contingency-plans/{contingencyId}` | Delete contingency plan |
| [ ] | `GET` | `/handovers/{handoverId}/contingency-plans` | Get contingency plans |
| [ ] | `POST` | `/handovers/{handoverId}/contingency-plans` | Create contingency plan |
| [ ] | `POST` | `/handovers` | Create handover |
| [ ] | `GET` | `/handovers/{handoverId}` | Get handover by ID |
| [ ] | `GET` | `/handovers/{handoverId}/patient` | Get patient handover data (demographics, physicians, status) |
| [ ] | `GET` | `/handovers/pending` | Get pending handovers |
| [ ] | `GET` | `/handovers/{handoverId}/patient-data` | Get patient clinical data |
| [ ] | `PUT` | `/handovers/{handoverId}/patient-data` | Update patient clinical data |
| [ ] | `POST` | `/handovers/{id}/ready` | Mark handover as ready |
| [ ] | `POST` | `/handovers/{handoverId}/reject` | Reject handover |
| [ ] | `GET` | `/handovers/{handoverId}/situation-awareness` | Get situation awareness |
| [ ] | `PUT` | `/handovers/{handoverId}/situation-awareness` | Update situation awareness |
| [ ] | `POST` | `/handovers/{handoverId}/start` | Start handover |
| [ ] | `GET` | `/handovers/{handoverId}/synthesis` | Get synthesis |
| [ ] | `PUT` | `/handovers/{handoverId}/synthesis` | Update synthesis |


### Me (User Context)
| Done | Method | Endpoint | Description |
| :---: | :--- | :--- | :--- |
| [ ] | `POST` | `/me/assignments` | Post assignments |
| [ ] | `DELETE` | `/me/handovers/{handoverId}/action-items/{itemId}` | Delete handover action item |
| [ ] | `PUT` | `/me/handovers/{handoverId}/action-items/{itemId}` | Update handover action item |
| [ ] | `GET` | `/me/handovers/{handoverId}/action-items` | Get handover action items |
| [ ] | `POST` | `/me/handovers/{handoverId}/action-items` | Create handover action item |
| [ ] | `GET` | `/me/handovers/{handoverId}/activity` | Get handover activity |
| [ ] | `GET` | `/me/handovers/{handoverId}/checklists` | Get handover checklists |
| [ ] | `PUT` | `/me/handovers/{handoverId}/checklists/{itemId}` | Update checklist item |
| [ ] | `GET` | `/me/handovers/{handoverId}/contingency-plans` | Get handover contingency plans |
| [ ] | `POST` | `/me/handovers/{handoverId}/contingency-plans` | Create contingency plan |
| [ ] | `GET` | `/me/handovers/{handoverId}/messages` | Get handover messages |
| [ ] | `POST` | `/me/handovers/{handoverId}/messages` | Create handover message |
| [ ] | `GET` | `/me/handovers` | Get my handovers |
| [ ] | `GET` | `/me/patients` | Get my patients |
| [ ] | `GET` | `/me/profile` | Get my profile |
