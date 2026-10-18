# BilleTico: pruebas de rendimiento con JMeter

Caso de Estudio #2 de BISOFT-17 (Calidad, Verificación y Validación de Software).

## Contenido

| Archivo | Para qué sirve |
|---|---|
| `BilleTico_Rendimiento.jmx` | Plan de prueba de JMeter 5.6.3 |
| `data/usuarios.csv` | Credenciales para el login (**reemplazar por usuarios reales activos**) |
| `data/parametros.csv` | Montos y rangos de fechas para las consultas |
| `ejecutar.bat` | Corre la prueba en modo consola y genera el reporte HTML |
| `resultados/` | Carpeta que se crea al ejecutar (está en `.gitignore`) |

## Servicios bajo prueba (flujo de un usuario final)

| # | Método | Endpoint | Datos de entrada | Dato que se obtiene de la respuesta |
|---|---|---|---|---|
| 01 | POST | `/api/FinalUser/Login` | `email`, `password` (CSV) | `userId`, `nationalId` (JSON Extractor) |
| 02 | GET | `/api/FinalUser/RetrieveByNationalId` | `nationalId` (extraído en 01) | `nombreUsuario` (RegEx Extractor), con verificación de que `id` sea igual a `userId` |
| 03 | GET | `/api/Payment/GetUserHistoryPayment` | `nationalId` (extraído en 01), `fechaInicio` y `fechaFin` (CSV) | `pagoId_1..N` y la cantidad en `pagoId_matchNr` (JSON Extractor, match -1) |
| 04 | GET | `/api/Commerce/RetrieveActive` | — | `commerceId` aleatorio de la lista (JSON Extractor) |
| 05 | GET | `/api/CommercePromotion/RetrieveBestByCommerce` | `commerceId` (extraído en 04), `monto` (CSV) | `promocionId` (JSON Extractor) |

La transacción `00 - Flujo completo usuario` mide el tiempo de los 5 pasos juntos.

Se eligieron servicios de **lectura** (más el login) a propósito. `Payment/Create`, `Commerce/Approve` y `FinalUser/Create` envían correos o SMS por SendGrid y Twilio, y una prueba de carga contra ellos mandaría cientos de mensajes reales y crearía registros basura en la base de datos.

## Cómo se cubre cada criterio de evaluación

**1. Parametrización y configuración (2%)**
- El elemento *Configuracion global* define `PROTOCOL`, `HOST`, `PORT`, `USUARIOS`, `RAMPUP`, `DURACION`, `THINK_MIN`, `THINK_RANGO`, `SLA_MS` y `DATA_DIR`.
- Cada variable usa `${__P(nombre,valorPorDefecto)}`, así que se puede cambiar sin abrir el script: `-Jusuarios=50 -Jduracion=300 -Jhost=billetico-api-....azurewebsites.net -Jport=443`.
- *HTTP Request Defaults* centraliza el host, el puerto, el protocolo y los timeouts. *HTTP Header Manager* envía `Content-Type: application/json`.
- El Thread Group corre por duración (scheduler), con ramp-up y un *Uniform Random Timer* que simula la pausa del usuario.

**2. Gestión de datos, CSV Data Set Config (2%)**
- `usuarios.csv` → `email,password`
- `parametros.csv` → `monto,fechaInicio,fechaFin`
- Los dos archivos se comparten entre todos los hilos y se reciclan al llegar al final.

**3. Obtención de datos de las respuestas (2%)**
- JSON Extractor: `$.id`, `$.nationalId`, `$[*].id` (con match aleatorio) y `$.id` de la promoción.
- Regular Expression Extractor: `"firstName"\s*:\s*"([^"]*)"`
- JSON Extractor con `$[*].id` y match -1: extrae todos los ids de pagos; `pagoId_matchNr` da la cantidad.
- Los datos extraídos se **encadenan**: lo que se obtiene en 01 se usa en 02 y 03, y lo que se obtiene en 04 se usa en 05.
- Aserciones: código HTTP, JSON Assertion (comprueba que el usuario devuelto sea el del login) y Duration Assertion (SLA).

**4. Reporte (2%)**
- En el GUI: *Summary Report* y *Aggregate Report*.
- En consola: `ejecutar.bat` genera el `.jtl` y el **dashboard HTML** de JMeter (`-e -o`), con APDEX, percentiles, throughput, errores y gráficos en el tiempo.

**5. Análisis y conclusiones (2%)**: ver la plantilla al final de este documento.

## Paso a paso

1. Instalar Java 17 o 21 y descomprimir JMeter 5.6.3 en `C:\apache-jmeter-5.6.3`. Si lo descomprime en otra ruta, cambie `JMETER_HOME` en `ejecutar.bat`.
2. Levantar el WebAPI desde Visual Studio con el perfil **https**, que queda en `https://localhost:7163`. Para confirmar que está arriba, abra `https://localhost:7163/swagger`.
3. Poner en `data/usuarios.csv` usuarios **reales con Status = Active**. Lo ideal son 5 o más usuarios con historial de pagos.
4. **Depurar en el GUI.** Abra `bin\jmeter.bat`, luego File → Open → `BilleTico_Rendimiento.jmx`. Para la depuración, cambie `usuarios` a 1 y `duracion` a 10 en *Configuracion global*. Ejecute y revise *View Results Tree*: los 5 requests deben salir en verde.
5. **Ejecutar la carga real en consola**, desactivando antes el *View Results Tree*:
   ```
   ejecutar.bat 10 60 10      (línea base)
   ejecutar.bat 25 120 20     (carga normal)
   ejecutar.bat 50 180 30     (carga alta / estrés)
   ```
   Cada corrida crea `resultados\<fecha>_<N>usuarios\reporte\index.html`.
6. Si quiere probar contra Azure en lugar de local:
   ```
   jmeter -n -t BilleTico_Rendimiento.jmx -Jhost=billetico-api-baeecxbkd0asb4fb.centralus-01.azurewebsites.net -Jport=443 -Jusuarios=10 -l r.jtl -e -o reporte
   ```

## Cómo leer las métricas

| Métrica | Qué significa | Referencia |
|---|---|---|
| Average | Tiempo promedio de respuesta | Se distorsiona con valores atípicos; no conviene usarlo solo |
| Median / 90% / 95% / 99% Line | Percentiles: el 95% de las peticiones tardó lo mismo o menos que ese valor | El p95 es la métrica principal contra el SLA (2000 ms) |
| Min / Max | Extremos | Un Max muy alto con un p95 bajo indica picos aislados |
| Error % | Porcentaje de peticiones fallidas (por código HTTP o por aserciones) | Meta: < 1% |
| Throughput | Peticiones por segundo que atendió el API | Debería crecer con más usuarios; si se estanca, se llegó al límite |
| Received/Sent KB/sec | Ancho de banda | Sirve para detectar respuestas muy pesadas, como `RetrieveActive` |
| APDEX (dashboard) | Satisfacción entre 0 y 1 | ≥ 0.94 excelente, ≥ 0.85 bueno, < 0.70 deficiente |

## Plantilla del informe de resultados

**Ambiente:** WebAPI .NET 8 en (local / Azure App Service), BD (…), máquina generadora de carga (CPU, RAM), JMeter 5.6.3.

**Escenarios ejecutados**

| Escenario | Usuarios | Ramp-up | Duración | Total de muestras | Throughput (req/s) | Error % | p95 flujo (ms) | APDEX |
|---|---|---|---|---|---|---|---|---|
| Línea base | 10 | 10 s | 60 s | | | | | |
| Carga normal | 25 | 20 s | 120 s | | | | | |
| Estrés | 50 | 30 s | 180 s | | | | | |

**Resultados por servicio (escenario de carga normal)**

| Servicio | Promedio | Mediana | p90 | p95 | Máx | Error % | ¿Cumple SLA? |
|---|---|---|---|---|---|---|---|
| 01 Login | | | | | | | |
| 02 RetrieveByNationalId | | | | | | | |
| 03 GetUserHistory | | | | | | | |
| 04 Commerce RetrieveActive | | | | | | | |
| 05 RetrieveBestByCommerce | | | | | | | |

**Análisis.** Preguntas que conviene responder:
- ¿Qué servicio es el más lento y por qué? Revise el código. Por ejemplo, `GetUserHistory` filtra por fechas en la BD y `RetrieveActive` devuelve la lista completa sin paginar.
- Cuando se duplican los usuarios, ¿el throughput crece en proporción o se estanca? ¿En qué punto empieza a subir el tiempo de respuesta (punto de saturación)?
- ¿Aparecen errores bajo carga, como 500 o timeouts? Revise `jmeter.log` y la pestaña *Errors* del dashboard.
- ¿El primer minuto es más lento (calentamiento del JIT de .NET o cold start de Azure)? Véalo en el gráfico *Response Times Over Time*.

**Conclusiones y recomendaciones** (ejemplos para adaptar a sus resultados reales):
- Indique cuántos usuarios concurrentes soporta el API sin pasar el SLA de 2 s en p95.
- Proponga paginar `Commerce/RetrieveActive` si el tamaño de la respuesta lo justifica.
- Proponga agregar índices en la BD a las columnas que se consultan (`NationalId`, `Status`, fechas de pago).
- Proponga usar caché para catálogos que cambian poco, como comercios activos y promociones.
- Señale el riesgo de que el login compare la contraseña en texto plano: es un problema de seguridad, aunque no afecte el rendimiento.
