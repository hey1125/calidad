# Guía de entrega para las etapas 2, 3 y 4

Resumen de la Etapa 1 (pruebas programadas): 72 pruebas automatizadas, 23 endpoints, 6 defectos
documentados. Detalle técnico en `README.md`.

## Etapa 2: organizar el código

Ya está hecho en la carpeta `ApiTests/`. Puntos para explicarlo y evaluarlo:

- **Arquitectura por capas + patrón API Object:** `config` → `http` (cliente) → `api` (un objeto por
  controlador) → `tests`. Los tests no conocen URLs ni cabeceras.
- **Separación pedida en la rúbrica:** lógica de pruebas en `tests/`, configuración en `src/config/`,
  datos de prueba en `src/fixtures/`.
- **Código reutilizable:** aserciones en `helpers/assertions.ts`, datos de escritura con
  `helpers/promotionBuilder.ts`, `it.each` para casos repetidos.
- **Seguridad de pruebas:** `http/messagingGuard.ts` impide enviar correos/SMS reales.

## Etapa 3: presentación

| # | Diapositiva | Contenido y evidencia |
|---|---|---|
| 1 | Objetivo y alcance | API BilleTico, 23 endpoints, TypeScript + Vitest + Axios |
| 2 | Arquitectura | Capas config, http, api, tests; patrón API Object (usar el árbol del README) |
| 3 | Estructura del proyecto | Captura de las carpetas de `ApiTests/` en VS Code |
| 4 | Estrategia de pruebas | Status code + body; positivos y negativos; lectura primero; aislamiento de datos; sin correos reales; defectos aparte |
| 5 | Escenarios automatizados | Tabla de cobertura del README (11 + 8 + 19 + 15 + 5 + 13 + 1 = 72) |
| 6 | Evidencia: pruebas en verde | Captura de `npm run test:verbose` y del reporte HTML (72 passed) |
| 7 | Hallazgos | Los 6 defectos (DEF-01 a DEF-06), con la captura del reporte de defectos |
| 8 | Conclusiones | Riesgos: sin autenticación, contraseñas en texto plano, error 500 en estadísticas |

## Etapa 4: demostración en vivo

**Antes de presentar (ensayar una vez):**
1. Base de datos BilleTico en línea y API corriendo con el perfil **https**.
2. `https://localhost:7163/swagger` abre sin errores.
3. Haber corrido una vez `ApiTests/sql/seed_financial_entity.sql`.
4. En `ApiTests/`: `npm test` debe dar **72 passed**.
5. Cerrar cualquier servidor de reporte abierto (Ctrl+C).

**Guion:**
1. Mostrar la estructura de carpetas (1 min).
2. `npm run test:verbose`: 72 casos exitosos, positivos y negativos.
3. `npm run test:report` y luego `npm run report:open`: reporte HTML.
4. `npm run test:defects`: **6 casos fallidos a propósito**. Explicar cada uno:
   DEF-01 contraseña expuesta, DEF-02 sin autenticación, DEF-03 fechas invertidas aceptadas,
   DEF-04 borrado de algo inexistente reporta éxito, DEF-05 error 500,
   DEF-06 registro de banco inválido responde 500.
5. `npm run test:defects:report` y `npm run report:open`: abrir DEF-05 y mostrar el mensaje del error.

**Si algo falla en vivo:** revisar que la API esté corriendo y que la base tenga los datos
(sección "Datos de prueba requeridos" del README).