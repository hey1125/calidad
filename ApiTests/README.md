# BilleTico: pruebas automatizadas de la API

Pruebas de API para el curso Calidad, Verificación y Validación de Software.
Escritas en **TypeScript** con **Vitest** y **Axios**. Cubren 23 endpoints con 72 casos de prueba
(positivos y negativos), más 6 pruebas de defectos conocidos que se ejecutan por separado.

## Requisitos

- Node.js 22 o superior
- WebAPI corriendo con el perfil **https** de Visual Studio (`https://localhost:7163`)
- SQL Server local con la base de datos **BilleTico** y sus datos de prueba (ver más abajo)

## Instalación y ejecución

Todos los comandos `npm` se ejecutan dentro de la carpeta `ApiTests/`.

```powershell
cd ApiTests
npm install
npm test
```

Antes de la primera corrida, ejecutar una vez `sql/seed_financial_entity.sql` en SQL Server
(crea el banco de prueba 987 que usan las pruebas de entidades financieras).

| Comando | Qué hace |
|---|---|
| `npm test` | Corre las 72 pruebas principales |
| `npm run test:verbose` | Igual, mostrando el nombre de cada prueba |
| `npm run test:watch` | Vuelve a correr las pruebas al guardar cambios |
| `npm run test:report` | Corre las pruebas y genera el reporte HTML y `reports/main/junit.xml` |
| `npm run test:defects` | Corre las 6 pruebas de defectos conocidos (**se espera que fallen**) |
| `npm run test:defects:report` | Igual, y genera `reports/defects/junit.xml` |
| `npm run report:open` | Abre en el navegador el reporte HTML de la última corrida con reporte |
| `npm run typecheck` | Revisa los tipos de TypeScript sin ejecutar nada |

El reporte HTML se escribe en `.vitest/` y se sobrescribe en cada corrida con reporte.

## Configuración

Se controla con variables de entorno (valores por defecto entre paréntesis):

| Variable | Uso |
|---|---|
| `API_BASE_URL` (`https://localhost:7163`) | URL base de la API |
| `API_TIMEOUT_MS` (`15000`) | Tiempo máximo por petición |
| `ALLOW_MESSAGING` (vacío) | `1` desbloquea los endpoints que envían correo o SMS reales (ver estrategia) |

Ejemplo en PowerShell: `$env:API_BASE_URL="https://localhost:7163"; npm test`

## Arquitectura

Arquitectura por capas con el patrón **API Object** (un objeto por controlador, equivalente a
Page Object) y un **Builder** para los datos de escritura. Cada capa tiene una sola responsabilidad,
así las pruebas no repiten URLs, cabeceras ni datos.

```
ApiTests/
├── src/
│   ├── config/env.ts              Configuración (URL, timeouts, interruptores)
│   ├── http/
│   │   ├── httpClient.ts          Cliente Axios único
│   │   └── messagingGuard.ts      Bloquea endpoints que envían correo/SMS reales
│   ├── api/                       API Objects: una función por endpoint
│   │   ├── finalUser.api.ts
│   │   ├── commerce.api.ts
│   │   ├── commercePromotion.api.ts
│   │   ├── payment.api.ts
│   │   ├── financialEntity.api.ts
│   │   └── financialEntityPromotion.api.ts
│   ├── types/                     Forma de las respuestas (interfaces)
│   ├── fixtures/                  Datos de prueba (usuarios, comercios, promociones, pagos)
│   └── helpers/
│       ├── assertions.ts          Aserciones reutilizables
│       ├── promotionBuilder.ts    Builder de promociones válidas
│       └── fePromotionBuilder.ts  Builder de promociones de banco
├── tests/
│   ├── smoke.test.ts
│   ├── finalUser/                 login.test.ts, retrieveByNationalId.test.ts
│   ├── commerce/                  commerceRead.test.ts
│   ├── commercePromotion/         promotionRead.test.ts, promotionLifecycle.test.ts
│   ├── payment/                   paymentRead.test.ts
│   ├── financialEntity/           financialEntityRead.test.ts
│   ├── financialEntityPromotion/  fePromotionRules.test.ts
│   └── defects/                   security, validation, financialEntity (.defect.ts)
├── sql/seed_financial_entity.sql  Banco de prueba 987
├── vitest.config.ts               Pruebas principales
├── vitest.defects.config.ts       Pruebas de defectos (comando aparte)
└── tsconfig.json
```

## Estrategia de pruebas

- **Cada prueba valida el status code y el body** (campos y valores correctos, o el mensaje de error exacto).
- **Positivos y negativos** en cada grupo: datos válidos, recursos inexistentes (404), parámetros
  inválidos (400), credenciales incorrectas (401).
- **Identificadores trazables:** `TC-LOGIN-xx`, `TC-USER-xx`, `TC-COM-xx`, `TC-PROMO-xx`, `TC-PAY-xx`, `TC-FE-xx`, `TC-FEP-xx`.
- **Lectura primero:** la mayoría de las pruebas solo consulta. Las únicas que escriben son las del
  ciclo crear, actualizar y eliminar de promociones (`promotionLifecycle.test.ts`), que usan el
  comercio 2, y todo lo que crean se elimina al terminar, incluso si una prueba falla.
  Las pruebas de promociones de banco (`fePromotionRules.test.ts`) crean sus promociones en el banco
  987 y las eliminan al terminar; si una corrida anterior se interrumpió, limpian los restos al iniciar.
- **Aislamiento de datos:** los archivos de prueba corren en paralelo. Las lecturas de promociones usan
  los comercios 1 y 3, y las escrituras usan el 2, así que no se afectan entre sí.
- **Sin correos ni SMS reales:** `messagingGuard` bloquea los endpoints que usan SendGrid, Twilio o
  AWS (crear usuario, validar usuario, recuperar contraseña, aprobar/rechazar comercio y entidad,
  crear y confirmar pago). Por eso esos endpoints no están en la suite.
- **Fechas deterministas:** las pruebas de promociones usan `refDate` fija, así no dependen de la fecha
  de hoy. Las pruebas de pagos por rango de fechas dependen de que los datos se cargaran el 2026-10-26.
- **Comparaciones robustas:** cuando el dato puede crecer se compara con `>=` o por relación
  (todos los elementos pertenecen al comercio pedido), no por un total exacto.
- **Defectos conocidos aparte:** `tests/defects` afirma el comportamiento correcto y **falla a propósito**.
  Mantenerlo en una carpeta y un comando distintos deja `npm test` en verde y permite mostrar los
  fallos en la demostración.

- **Reglas de negocio de promociones de banco:** la mejor promoción es la de mayor descuento,
  `max(monto × %, descuento fijo)`; solo aplican las `Activa`, con cupones disponibles y monto mínimo alcanzado.
  Se prueban montos que hacen ganar al porcentaje (20000), al descuento fijo (8000) y montos bajo el mínimo.

## Cobertura

| Controlador | Endpoints probados | Casos |
|---|---|---|
| FinalUser | Login, RetrieveByNationalId | 11 |
| Commerce | RetrieveActive, RetrieveById, RetrieveByLegalId, RetrievePending | 8 |
| CommercePromotion | RetrieveBestByCommerce, RetrieveActiveByCommerce, RetrieveAllByCommerce, RetrieveById, Create, Update, Delete | 19 |
| Payment | GetUserHistoryPayment, GetById, GetCommercePayments, GetBankPayments, GetBankStats | 15 |
| FinancialEntity | RetrieveByBankCode, RetrieveById | 5 |
| FinancialEntityPromotion | Create, RetrieveBestByIBAN, RetrieveActiveByIBAN (reglas de negocio) | 13 |
| Smoke | Swagger disponible | 1 |
| **Total** | **23 endpoints** | **72** |

## Datos de prueba requeridos

La base de datos local **BilleTico** debe contener:

- 3 usuarios activos: `ruizesteban@gmail.com`, `admin@billetico.com`, `prueba1@gmail.com`
- 3 comercios activos (ids 1 a 3) con cédulas jurídicas `3101000001` a `3101000003`
- 1 promoción activa `JMETER-PROMO` en el comercio 1
- 40 pagos en el comercio 1: 5 pagadores por 8 pagos (6 `Pagado` y 2 `Pendiente` cada uno)
- 1 entidad financiera con código bancario `987` (la crea `sql/seed_financial_entity.sql`). Sus promociones
  las crean y eliminan las mismas pruebas

Los valores exactos están en `src/fixtures/`. Los scripts en `PruebasRendimiento/sql` generan parte
de estos datos.

## Hallazgos (pruebas en `tests/defects`)

| ID | Defecto | Evidencia |
|---|---|---|
| DEF-01 | El login devuelve `passwordHash` en texto plano | La respuesta contiene la contraseña |
| DEF-02 | Ningún endpoint exige autenticación | El historial de pagos de un usuario se lee sin credenciales |
| DEF-03 | Un rango de fechas invertido se acepta | Responde 200 con lista vacía en lugar de 400 |
| DEF-04 | Eliminar una promoción inexistente reporta éxito | Responde 200 `true` en lugar de 404 |
| DEF-05 | Estadísticas de un banco sin pagos provocan error 500 | `Object cannot be cast from DBNull to other types.` |
| DEF-06 | Registrar un banco con datos inválidos responde 500 | Correo inválido devuelve 500 en lugar de 400 |

Otros hallazgos de la revisión del código (sin prueba automatizada): llaves de servicios externos
guardadas en `appsettings.json`, cadena de conexión con contraseña escrita en `SqlDao.cs`, el código de entidades financieras lee una columna `NationalId` que
la tabla y sus procedimientos no tienen (toda consulta de un banco existente respondía 500; lo corrige
`sql/seed_financial_entity.sql`), y las
ramas de "no hay comercios activos/pendientes" del controlador de comercios que nunca se ejecutan.

## Convenciones de Git

- Rama de trabajo: `testing` (la rama `main` no se modifica hasta el pull request).
- Mensajes de commit: `test: descripción corta` para pruebas.
- Agregar siempre solo esta carpeta (`git add ApiTests`), para no subir cambios locales de otros proyectos.

## Solución de problemas

- **`npm.ps1 cannot be loaded because running scripts is disabled`:** ejecutar una vez
  `Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned`.
- **Las pruebas fallan de inmediato con error de conexión:** la API no está corriendo. Iniciar el
  perfil **https** en Visual Studio y revisar `https://localhost:7163/swagger`.
- **Fallan pruebas de datos (conteos, ids):** revisar que la base local tenga los datos de la sección
  anterior.
