-- ========================================
-- PLANIFICACIÓN DE CONTINGENCIA EN ESPAÑOL PARA TODOS LOS PACIENTES
-- ========================================

-- Planificación de Contingencia para handover-001 (Paciente con neumonía - pat-001)
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-001', 'handover-001', 'Si el paciente desarrolla dificultad respiratoria aguda o saturación de oxígeno < 92%', 'Administrar oxígeno suplementario, llamar a terapia respiratoria, considerar BIPAP, contactar al médico tratante inmediatamente', 'high', 'active', 'user_demo12345678901234567890123456', SYSTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-002', 'handover-001', 'Si la temperatura axilar supera los 38.5°C', 'Administrar antipiréticos según protocolo, evaluar foco infeccioso, contactar médico si persiste fiebre', 'high', 'active', 'user_demo12345678901234567890123457', SYSTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-003', 'handover-001', 'Si aparecen signos de insuficiencia respiratoria (taquipnea > 30/min, tiraje)', 'Aumentar FiO2, preparar para posible intubación, llamar a intensivista, contactar médico tratante', 'high', 'planned', 'user_demo12345678901234567890123458', SYSTIMESTAMP - INTERVAL '20' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-004', 'handover-001', 'Si el paciente presenta reacción alérgica a penicilina', 'Suspender antibiótico inmediatamente, administrar epinefrina si anafilaxia, contactar alergólogo', 'high', 'active', 'user_demo12345678901234567890123456', SYSTIMESTAMP - INTERVAL '15' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-005', 'handover-001', 'Si no hay mejoría radiológica en 48-72 horas', 'Repetir radiografía de tórax, considerar cambio de antibiótico, consultar infectólogo', 'medium', 'planned', 'user_demo12345678901234567890123457', SYSTIMESTAMP - INTERVAL '10' MINUTE);

-- Planificación de Contingencia para handover-002 (Paciente con sepsis - pat-002)
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-006', 'handover-002', 'Si la presión arterial sistólica < 90 mmHg o requiere aumento de vasopresores', 'Aumentar fluidos IV, incrementar dosis de vasopresores, llamar a intensivista, monitoreo continuo', 'high', 'active', 'user_demo12345678901234567890123456', SYSTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-007', 'handover-002', 'Si el paciente presenta oliguria (< 1ml/kg/hora) o anuria', 'Evaluar estado de hidratación, ecografía renal, considerar catéter vesical, ajustar fluidos', 'high', 'active', 'user_demo12345678901234567890123457', SYSTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-008', 'handover-002', 'Si aparecen nuevos focos infecciosos o empeoramiento clínico', 'Cultivos adicionales, evaluación por imágenes, escalar antibióticos, consultar infectólogo', 'high', 'planned', 'user_demo12345678901234567890123458', SYSTIMESTAMP - INTERVAL '20' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-009', 'handover-002', 'Si el paciente presenta alteración del estado mental', 'Evaluar causas metabólicas, infección SNC, ajustar sedación, consultar neurología', 'medium', 'active', 'user_demo12345678901234567890123456', SYSTIMESTAMP - INTERVAL '15' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-010', 'handover-002', 'Si persiste fiebre > 48 horas con tratamiento antibiótico', 'Repetir hemocultivos, evaluación de foco oculto, cambio empírico de antibióticos', 'medium', 'planned', 'user_demo12345678901234567890123457', SYSTIMESTAMP - INTERVAL '10' MINUTE);

-- Planificación de Contingencia para handover-003 (Paciente con asma - pat-003)
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-011', 'handover-003', 'Si el paciente presenta crisis asmática severa con pobre respuesta a tratamiento', 'Administrar adrenalina subcutánea, preparar para intubación, llamar a intensivista', 'high', 'active', 'user_demo12345678901234567890123456', SYSTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-012', 'handover-003', 'Si requiere oxígeno suplementario > 2L/min persistentemente', 'Aumentar dosis de corticoides, considerar terapia inhalatoria adicional, evaluación intensiva', 'high', 'active', 'user_demo12345678901234567890123457', SYSTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-013', 'handover-003', 'Si aparecen signos de fatiga muscular respiratoria', 'Preparar para ventilación no invasiva, evaluar necesidad de intubación, monitoreo continuo', 'high', 'planned', 'user_demo12345678901234567890123458', SYSTIMESTAMP - INTERVAL '20' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-014', 'handover-003', 'Si el paciente presenta taquicardia > 150/min persistente', 'Evaluar causa (hipoxemia, ansiedad, efectos medicamentosos), ajustar tratamiento', 'medium', 'active', 'user_demo12345678901234567890123456', SYSTIMESTAMP - INTERVAL '15' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-015', 'handover-003', 'Si no hay mejoría clínica en 24 horas', 'Reevaluar tratamiento, considerar inmunomoduladores, consultar alergólogo/inmunólogo', 'medium', 'planned', 'user_demo12345678901234567890123457', SYSTIMESTAMP - INTERVAL '10' MINUTE);

-- Planificación de Contingencia para handover-004 (Paciente con trauma craneoencefálico - pat-004)
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-016', 'handover-004', 'Si disminución del nivel de conciencia (Glasgow < 12)', 'Evaluar urgencia neuroquirúrgica, tomografía cerebral inmediata, preparar para cirugía', 'high', 'active', 'user_demo12345678901234567890123456', SYSTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-017', 'handover-004', 'Si anisocoria o midriasis unilateral', 'Medición de presión intraocular, evaluación neuroquirúrgica urgente, preparar para cirugía', 'high', 'active', 'user_demo12345678901234567890123457', SYSTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-018', 'handover-004', 'Si convulsiones o movimientos anormales', 'Administrar anticonvulsivantes, evaluación neurológica, monitoreo EEG continuo', 'high', 'planned', 'user_demo12345678901234567890123458', SYSTIMESTAMP - INTERVAL '20' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-019', 'handover-004', 'Si hipertensión endocraneana (presión > 20 mmHg)', 'Aumentar dosis de manitol, hiperventilación, consultar neurocirugía para drenaje', 'high', 'active', 'user_demo12345678901234567890123456', SYSTIMESTAMP - INTERVAL '15' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-020', 'handover-004', 'Si náuseas o vómitos incoercibles', 'Administrar antieméticos, evaluar aumento de presión endocraneana, monitoreo neurológico', 'medium', 'planned', 'user_demo12345678901234567890123457', SYSTIMESTAMP - INTERVAL '10' MINUTE);

-- Planificación de Contingencia para handover-005 (Paciente con insuficiencia respiratoria - pat-005)
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-021', 'handover-005', 'Si desaturación < 88% o desconexión accidental del ventilador', 'Reintubar inmediatamente, verificar parámetros ventilatorios, llamar a intensivista', 'high', 'active', 'user_demo12345678901234567890123456', SYSTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-022', 'handover-005', 'Si neumotórax o deterioro respiratorio agudo', 'Drenaje pleural urgente, ajuste ventilatorio, radiografía de tórax inmediata', 'high', 'active', 'user_demo12345678901234567890123457', SYSTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-023', 'handover-005', 'Si hemorragia por vía aérea artificial', 'Verificar posición del tubo, aspirar, evaluar sangrado, preparar cambio de tubo', 'high', 'planned', 'user_demo12345678901234567890123458', SYSTIMESTAMP - INTERVAL '20' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-024', 'handover-005', 'Si agitación o dolor no controlado con sedación actual', 'Aumentar dosis de sedantes, evaluar causa (neumotórax, etc.), consulta anestésica', 'medium', 'active', 'user_demo12345678901234567890123456', SYSTIMESTAMP - INTERVAL '15' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-025', 'handover-005', 'Si infección nosocomial o neumonía asociada a ventilador', 'Cultivos bronquiales, antibióticos de amplio espectro, consultar infectólogo', 'medium', 'planned', 'user_demo12345678901234567890123457', SYSTIMESTAMP - INTERVAL '10' MINUTE);

-- Planificación de Contingencia para handover-006 (Paciente con choque séptico - pat-006)
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-026', 'handover-006', 'Si hipotensión refractaria (< 65 mmHg) a pesar de vasopresores', 'Escalar vasopresores, evaluar necesidad de balón intraaórtico, consultar cardiología', 'high', 'active', 'user_demo12345678901234567890123456', SYSTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-027', 'handover-006', 'Si anuria persistente > 6 horas', 'Iniciar hemodiálisis urgente, evaluar necesidad de catéter venoso central', 'high', 'active', 'user_demo12345678901234567890123457', SYSTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-028', 'handover-006', 'Si acidosis láctica > 4 mmol/L', 'Aumentar soporte ventilatorio, evaluar perfusión tisular, considerar bicarbonato', 'high', 'planned', 'user_demo12345678901234567890123458', SYSTIMESTAMP - INTERVAL '20' MINUTE);

-- Planificación de Contingencia para handover-007 (Paciente con meningitis - pat-007)
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-029', 'handover-007', 'Si deterioro neurológico agudo (disminución Glasgow > 2 puntos)', 'Repetir tomografía cerebral urgente, preparar para intervención neuroquirúrgica', 'high', 'active', 'user_demo12345678901234567890123456', SYSTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-030', 'handover-007', 'Si convulsiones recurrentes a pesar de tratamiento', 'Administrar fenitoína en carga, monitoreo EEG continuo, consultar neurología', 'high', 'active', 'user_demo12345678901234567890123457', SYSTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-031', 'handover-007', 'Si hipertensión endocraneana (presión > 20 mmHg)', 'Administrar manitol, hiperventilación controlada, preparar para drenaje ventricular', 'high', 'planned', 'user_demo12345678901234567890123458', SYSTIMESTAMP - INTERVAL '20' MINUTE);

-- Planificación de Contingencia para handover-008 (Paciente con quemaduras - pat-008)
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-032', 'handover-008', 'Si signos de infección en quemaduras (fiebre, eritema, secreción purulenta)', 'Cultivos locales, antibióticos intravenosos, evaluación quirúrgica urgente', 'high', 'active', 'user_demo12345678901234567890123456', SYSTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-033', 'handover-008', 'Si dolor incontrolable a pesar de analgesia multimodal', 'Consultar servicio de dolor, evaluar necesidad de sedación profunda', 'medium', 'active', 'user_demo12345678901234567890123457', SYSTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-034', 'handover-008', 'Si hipovolemia por pérdidas insensibles', 'Aumentar fluidos IV según fórmula de Parkland, monitoreo hemodinámico', 'medium', 'planned', 'user_demo12345678901234567890123458', SYSTIMESTAMP - INTERVAL '20' MINUTE);

-- Planificación de Contingencia para handover-009 (Paciente con convulsiones - pat-009)
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-035', 'handover-009', 'Si convulsión prolongada (> 5 minutos)', 'Administrar diazepam IV, preparar para intubación, monitoreo post-ictal', 'high', 'active', 'user_demo12345678901234567890123456', SYSTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-036', 'handover-009', 'Si estado post-ictal prolongado (> 30 minutos)', 'Evaluar causa metabólica, tomografía cerebral, consulta neurológica', 'medium', 'active', 'user_demo12345678901234567890123457', SYSTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-037', 'handover-009', 'Si recurrencia de convulsiones febriles', 'Repetir estudios etiológicos, considerar profilaxis antiepiléptica', 'medium', 'planned', 'user_demo12345678901234567890123458', SYSTIMESTAMP - INTERVAL '20' MINUTE);

-- Planificación de Contingencia para handover-010 (Paciente con intoxicación - pat-010)
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-038', 'handover-010', 'Si deterioro del nivel de conciencia', 'Evaluar Glasgow, soporte ventilatorio, antídoto específico según tóxico', 'high', 'active', 'user_demo12345678901234567890123456', SYSTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-039', 'handover-010', 'Si insuficiencia hepática aguda', 'Factor VII recombinante, plasma fresco congelado, consulta hepatología', 'high', 'active', 'user_demo12345678901234567890123457', SYSTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-040', 'handover-010', 'Si arritmias cardíacas por toxicidad', 'Antiarrítmicos específicos, monitoreo continuo, consulta cardiología', 'high', 'planned', 'user_demo12345678901234567890123458', SYSTIMESTAMP - INTERVAL '20' MINUTE);

-- Planificación de Contingencia para handover-011 (Paciente con hipoglucemia - pat-011)
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-041', 'handover-011', 'Si hipoglucemia recurrente (< 60 mg/dL)', 'Ajustar esquema insulínico, evaluar adherencia, consulta endocrinología', 'medium', 'active', 'user_demo12345678901234567890123456', SYSTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-042', 'handover-011', 'Si cetosis o cetoacidosis', 'Insulina intravenosa continua, fluidos, monitoreo gasométrico frecuente', 'high', 'active', 'user_demo12345678901234567890123457', SYSTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-043', 'handover-011', 'Si convulsiones por hipoglucemia', 'Glucosa hipertónica IV, benzodiazepinas si convulsiones, evaluación neurológica', 'high', 'planned', 'user_demo12345678901234567890123458', SYSTIMESTAMP - INTERVAL '20' MINUTE);

-- Planificación de Contingencia para handover-012 (Paciente con trauma abdominal - pat-012)
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-044', 'handover-012', 'Si signos de peritonitis (rigidez abdominal, rebote)', 'Cirugía urgente, antibióticos de amplio espectro, soporte hemodinámico', 'high', 'active', 'user_demo12345678901234567890123456', SYSTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-045', 'handover-012', 'Si hipotensión por sangrado interno', 'Fluidos cristaloides, sangre tipo específico, evaluación radiológica urgente', 'high', 'active', 'user_demo12345678901234567890123457', SYSTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-046', 'handover-012', 'Si dolor abdominal incontrolable', 'Analgesia multimodal, evaluación por dolor agudo, consulta cirugía', 'medium', 'planned', 'user_demo12345678901234567890123458', SYSTIMESTAMP - INTERVAL '20' MINUTE);

-- Planificación de Contingencia para handover-013 (Paciente con bronquiolitis - pat-013)
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-047', 'handover-013', 'Si insuficiencia respiratoria progresiva', 'Oxígeno suplementario, preparar para ventilación no invasiva, consulta intensivista', 'high', 'active', 'user_demo12345678901234567890123456', SYSTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-048', 'handover-013', 'Si deshidratación por dificultad respiratoria', 'Fluidos IV, monitoreo electrolitos, ajuste respiratorio para minimizar trabajo', 'medium', 'active', 'user_demo12345678901234567890123457', SYSTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-049', 'handover-013', 'Si apnea o bradicardia', 'Estimulación, ventilación con ambú, preparar para intubación', 'high', 'planned', 'user_demo12345678901234567890123458', SYSTIMESTAMP - INTERVAL '20' MINUTE);

-- Planificación de Contingencia para handover-014 (Paciente con gastroenteritis - pat-014)
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-050', 'handover-014', 'Si deshidratación severa (pérdida > 10% peso)', 'Fluidos IV isotónicos, monitoreo electrolitos, evaluación cardíaca', 'high', 'active', 'user_demo12345678901234567890123456', SYSTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-051', 'handover-014', 'Si vómitos incoercibles a pesar de ondansetrón', 'Antieméticos parenterales, evaluación causa, posible sonda nasogástrica', 'medium', 'active', 'user_demo12345678901234567890123457', SYSTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-052', 'handover-014', 'Si sangre en deposiciones', 'Evaluación endoscópica urgente, antibióticos, monitoreo hemodinámico', 'high', 'planned', 'user_demo12345678901234567890123458', SYSTIMESTAMP - INTERVAL '20' MINUTE);

-- Planificación de Contingencia para handover-015 (Paciente con otitis media - pat-015)
INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-053', 'handover-015', 'Si complicación intracraneal (meningitis, absceso)', 'Antibióticos intravenosos de amplio espectro, evaluación neurológica urgente', 'high', 'active', 'user_demo12345678901234567890123456', SYSTIMESTAMP - INTERVAL '30' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-054', 'handover-015', 'Si mastoiditis aguda', 'Antibióticos intravenosos, evaluación otorrinolaringológica urgente', 'high', 'active', 'user_demo12345678901234567890123457', SYSTIMESTAMP - INTERVAL '25' MINUTE);

INSERT INTO HANDOVER_CONTINGENCY (ID, HANDOVER_ID, CONDITION_TEXT, ACTION_TEXT, PRIORITY, STATUS, CREATED_BY, CREATED_AT)
VALUES ('contingency-055', 'handover-015', 'Si dolor persistente a pesar de analgesia', 'Reevaluar causa, posible paracentesis, consulta otorrinolaringología', 'medium', 'planned', 'user_demo12345678901234567890123458', SYSTIMESTAMP - INTERVAL '20' MINUTE);

COMMIT;
