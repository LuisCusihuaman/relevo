-- ============================================================================
-- Script: 03-alter-constraints.sql
-- Description: Modificaciones a las reglas de negocio de HANDOVERS
-- Author: Sistema Relevo
-- Date: 2025-12-19
-- ============================================================================
-- 
-- CAMBIOS EN REGLAS DE NEGOCIO:
-- - Se permite que el sender pueda iniciar su propio handover
-- - Se permite que el sender pueda completar su propio handover
-- 
-- CONSTRAINTS ELIMINADOS:
-- - CHK_HO_STARTED_NE_SENDER: Evitaba que STARTED_BY_USER_ID = SENDER_USER_ID
-- - CHK_HO_COMPLETED_NE_SENDER: Evitaba que COMPLETED_BY_USER_ID = SENDER_USER_ID
-- ============================================================================

-- Conectar como usuario de aplicación
CONNECT RELEVO_APP/TuPass123@localhost:1521/XE;

-- Eliminar constraint que impedía que el sender iniciara su propio handover
BEGIN
    EXECUTE IMMEDIATE 'ALTER TABLE HANDOVERS DROP CONSTRAINT CHK_HO_STARTED_NE_SENDER';
    DBMS_OUTPUT.PUT_LINE('✓ Constraint CHK_HO_STARTED_NE_SENDER eliminado correctamente');
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE = -2443 THEN
            DBMS_OUTPUT.PUT_LINE('ℹ Constraint CHK_HO_STARTED_NE_SENDER ya no existe');
        ELSE
            DBMS_OUTPUT.PUT_LINE('✗ Error al eliminar CHK_HO_STARTED_NE_SENDER: ' || SQLERRM);
            RAISE;
        END IF;
END;
/

-- Eliminar constraint que impedía que el sender completara su propio handover
BEGIN
    EXECUTE IMMEDIATE 'ALTER TABLE HANDOVERS DROP CONSTRAINT CHK_HO_COMPLETED_NE_SENDER';
    DBMS_OUTPUT.PUT_LINE('✓ Constraint CHK_HO_COMPLETED_NE_SENDER eliminado correctamente');
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE = -2443 THEN
            DBMS_OUTPUT.PUT_LINE('ℹ Constraint CHK_HO_COMPLETED_NE_SENDER ya no existe');
        ELSE
            DBMS_OUTPUT.PUT_LINE('✗ Error al eliminar CHK_HO_COMPLETED_NE_SENDER: ' || SQLERRM);
            RAISE;
        END IF;
END;
/

-- Verificar que los constraints fueron eliminados
SELECT 'Verificación de constraints eliminados:' AS STATUS FROM DUAL;

SELECT 
    CONSTRAINT_NAME,
    CONSTRAINT_TYPE,
    SEARCH_CONDITION
FROM USER_CONSTRAINTS
WHERE TABLE_NAME = 'HANDOVERS'
    AND CONSTRAINT_NAME IN ('CHK_HO_STARTED_NE_SENDER', 'CHK_HO_COMPLETED_NE_SENDER');

-- Si no hay resultados, los constraints fueron eliminados correctamente
SELECT 
    CASE 
        WHEN COUNT(*) = 0 THEN '✓ Todos los constraints objetivo fueron eliminados correctamente'
        ELSE '✗ Aún existen ' || COUNT(*) || ' constraint(s) que deberían haber sido eliminados'
    END AS RESULTADO
FROM USER_CONSTRAINTS
WHERE TABLE_NAME = 'HANDOVERS'
    AND CONSTRAINT_NAME IN ('CHK_HO_STARTED_NE_SENDER', 'CHK_HO_COMPLETED_NE_SENDER');

COMMIT;

-- Mostrar mensaje final
SELECT '═══════════════════════════════════════════════════════════' AS MSG FROM DUAL
UNION ALL
SELECT '  REGLAS DE NEGOCIO ACTUALIZADAS CORRECTAMENTE' FROM DUAL
UNION ALL
SELECT '  - Sender puede iniciar su propio handover' FROM DUAL
UNION ALL
SELECT '  - Sender puede completar su propio handover' FROM DUAL
UNION ALL
SELECT '═══════════════════════════════════════════════════════════' FROM DUAL;
