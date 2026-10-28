# Arquitectura de las pruebas API de BilleTico

## Objetivo

La carpeta `ApiTests/` contiene pruebas funcionales automatizadas para la API REST de BilleTico. El proyecto usa TypeScript, Vitest y Axios. La organización separa las pruebas, las llamadas HTTP, la configuración y los datos para reducir duplicación y hacer que los casos sean más fáciles de mantener.

## Diagrama de capas

```text
                    tests/
       (escenarios y aserciones de cada prueba)
                       |
             usa API Objects y módulos
                 de soporte de prueba
                       |
                 src/api/
       (un objeto por controlador; define métodos
             para llamar a cada endpoint)
                       |
                 src/http/
       (cliente Axios único y protección contra
             llamadas que enviarían correo/SMS)
                       |
                src/config/
          (URL base, timeout y variables
                 de entorno)

Módulos de soporte utilizados por las capas:
  src/fixtures/  -> datos de prueba reutilizables
  src/helpers/   -> aserciones y builders de datos
  src/types/     -> interfaces para describir respuestas
```

## Responsabilidad de cada parte

| Parte | Responsabilidad |
|---|---|
| `tests/` | Describe escenarios positivos y negativos, ejecuta las llamadas y comprueba el código HTTP y el cuerpo de respuesta. |
| `src/api/` | Agrupa las operaciones de cada controlador en un API Object. Los tests usan estos métodos en lugar de repetir rutas HTTP. |
| `src/http/httpClient.ts` | Configura el cliente Axios compartido, el tiempo de espera, las cabeceras y el tratamiento del certificado local. |
| `src/http/messagingGuard.ts` | Bloquea por defecto las operaciones que podrían enviar correo o SMS reales. |
| `src/config/env.ts` | Lee la URL base, el timeout y el interruptor para permitir mensajería. |
| `src/fixtures/` | Conserva los datos de prueba conocidos, como usuarios, comercios, promociones y pagos sembrados en la base local. |
| `src/helpers/` | Ofrece utilidades reutilizables, como la aserción de status HTTP y el builder para crear promociones de prueba. |
| `src/types/` | Declara los tipos de los objetos que devuelve la API para mejorar la comprobación estática del código. |

## Regla de dependencias

**Cada capa utiliza la capa inferior que le corresponde; una capa inferior no debe importar ni depender de una capa superior.** En concreto:

- Los archivos de `tests/` llaman a los métodos de `src/api/` y pueden utilizar fixtures y helpers de prueba.
- Los API Objects de `src/api/` envían solicitudes mediante `src/http/httpClient.ts` y usan tipos de `src/types/`.
- El cliente HTTP utiliza la configuración de `src/config/` y el guard de mensajería.
- Los fixtures contienen datos y no llaman a la API ni a la base de datos.
- Los helpers centralizan utilidades y no dependen de los archivos de prueba.

De esta forma, las pruebas no necesitan conocer la configuración interna de Axios ni repetir URLs, cabeceras o la lógica común de validación.

## Recorrido de una prueba

1. Vitest ejecuta un caso de un archivo dentro de `tests/`.
2. El caso obtiene los datos desde `src/fixtures/` y llama a un método de `src/api/`.
3. El API Object construye la solicitud y la envía mediante `request()` en `src/http/httpClient.ts`.
4. El cliente aplica la configuración, verifica que la operación no esté bloqueada por `messagingGuard` y envía la solicitud a BilleTico.
5. La prueba comprueba tanto el status HTTP como los datos devueltos, usando helpers cuando corresponde.

## Ejecución y verificación

Desde la carpeta `ApiTests/`:

```bash
npm run typecheck
npm test
```

`npm run typecheck` valida los tipos TypeScript, incluyendo ambas configuraciones de Vitest. `npm test` ejecuta la suite principal. Para las pruebas de defectos conocidos se utiliza por separado `npm run test:defects`; esos casos documentan comportamientos defectuosos que se espera que fallen hasta que se corrija la API.
