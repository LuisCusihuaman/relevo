-- =============================================
-- Handovers para el paciente pat-001 (John Doe)
-- =============================================

-- Connect as RELEVO_APP user
CONNECT RELEVO_APP/TuPass123;
INSERT INTO HANDOVERS (ID, ASSIGNMENT_ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS_DOC_ID, SYNTHESIS, SHIFT_NAME, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_DOCTOR_ID, TO_DOCTOR_ID, CREATED_BY, HANDOVER_WINDOW_DATE, READY_AT)
VALUES ('h-001', 'assign-001', 'pat-001', 'Ready', 'Unstable',
    'John Doe, M, 68. Antecedentes de HTA, DM2, y cardiopatía isquémica con FEVI 35%. Ingresó por SCA sin elevación del ST hace 3 días, manejado con AAS, clopidogrel, enoxaparina y estatinas. Coronariografía ayer mostró enfermedad de 3 vasos no revascularizable percutáneamente. Se discutió con cirugía cardíaca y se aceptó para CRM programada. Durante la noche, presentó episodio de DPN que respondió a furosemida IV. Actualmente con dolor torácico leve, intermitente.',
    'doc-awareness-001',
    'Paciente con enfermedad coronaria severa en espera de cirugía de revascularización. Requiere monitorización hemodinámica estricta y manejo de insuficiencia cardíaca congestiva. Alto riesgo de isquemia recurrente y arritmias ventriculares.',
    'Cardiology Day Shift Handover', 'shift-day', 'shift-night', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123457', 'user_demo12345678901234567890123456', TO_DATE('2023-10-27', 'YYYY-MM-DD'), SYSTIMESTAMP - INTERVAL '1' HOUR
);

-- Planificación de Contingencia para handover-001 (Paciente con neumonía - pat-001)
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-092', 'h-001', 'Si el paciente desarrolla dificultad respiratoria aguda o saturación de oxígeno < 92%', 'Administrar oxígeno suplementario, llamar a terapia respiratoria, considerar BIPAP, contactar al médico tratante inmediatamente', 'high', 'active', 'user_demo12345678901234567890123456', SYSTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-093', 'h-001', 'Si la temperatura axilar supera los 38.5°C', 'Administrar antipiréticos según protocolo, evaluar foco infeccioso, contactar médico si persiste fiebre', 'high', 'active', 'user_demo12345678901234567890123457', SYSTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-094', 'h-001', 'Si aparecen signos de insuficiencia respiratoria (taquipnea > 30/min, tiraje)', 'Aumentar FiO2, preparar para posible intubación, llamar a intensivista, contactar médico tratante', 'high', 'planned', 'user_demo12345678901234567890123458', SYSTIMESTAMP - INTERVAL '20' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-095', 'h-001', 'Si el paciente presenta dolor torácico > 5/10 no controlado con NTG', 'Realizar EKG de 12 derivaciones, administrar morfina, y notificar a cardiología de guardia inmediatamente.', 'high', 'active', 'user_demo12345678901234567890123456', SYSTIMESTAMP - INTERVAL '15' MINUTE);

-- Handovers para el paciente pat-002 (Jane Smith)
INSERT INTO HANDOVERS (ID, ASSIGNMENT_ID, PATIENT_ID, STATUS, ILLNESS_SEVERITY, PATIENT_SUMMARY, SITUATION_AWARENESS_DOC_ID, SYNTHESIS, SHIFT_NAME, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_DOCTOR_ID, TO_DOCTOR_ID, CREATED_BY, HANDOVER_WINDOW_DATE, READY_AT)
VALUES ('h-002', 'assign-002', 'pat-002', 'InProgress', 'Watcher',
    'Jane Smith, F, 45. Antecedentes de cáncer de mama estadio III, actualmente en quimioterapia adyuvante con paclitaxel. Presenta neutropenia grado 2, último recuento de neutrófilos 1.2 x 10^9/L. Recibe GCSF profiláctico. Durante la noche presentó episodio de náuseas que respondió a ondansetrón. Actualmente estable, sin fiebre.',
    'doc-awareness-002',
    'Paciente oncológica en tratamiento quimioterápico con riesgo de neutropenia febril. Requiere monitorización estrecha de signos vitales y recuentos hematológicos. Mantener precauciones de aislamiento por neutropenia.',
    'Oncology Night Shift Handover', 'shift-night', 'shift-day', 'user_demo12345678901234567890123458', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123458', TO_DATE('2023-10-27', 'YYYY-MM-DD'), SYSTIMESTAMP - INTERVAL '45' MINUTE
);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-096', 'h-002', 'Si la paciente presenta fiebre neutropénica (T > 38.3°C o > 38.0°C por 1h)', 'Iniciar protocolo de sepsis, tomar hemocultivos y urocultivo, administrar antibióticos de amplio espectro en la primera hora', 'high', 'active', 'user_demo12345678901234567890123458', SYSTIMESTAMP - INTERVAL '10' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-097', 'h-002', 'Si el recuento de plaquetas cae por debajo de 20,000/µL', 'Preparar para transfusión de plaquetas, evitar procedimientos invasivos, monitorizar signos de sangrado', 'high', 'active', 'user_demo12345678901234567890123458', SYSTIMESTAMP - INTERVAL '5' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-098', 'h-002', 'Si la paciente presenta náuseas y vómitos incontrolables (>3 episodios en 1h)', 'Administrar antieméticos de rescate (e.g., olanzapina), asegurar hidratación IV, consultar con farmacia clínica', 'medium', 'active', 'user_demo12345678901234567890123456', SYSTIMESTAMP - INTERVAL '3' MINUTE);
-- ... existing code ...
