# SIGEMAC-ONPE — Guía para exposición técnica

Este documento resume la arquitectura y el comportamiento de los casos de uso a partir de `BACKEND/SIGEMAC-ONPE/SIGEMAC-ONPE/CasosDeUso.md`, el backend actual y las llamadas que realiza el frontend.

> **Criterio de lectura:** se distingue entre el comportamiento definido en el caso de uso y lo que está implementado hoy. Las carpetas y controladores vacíos de los CU pendientes se conservan intencionalmente como estructura de trabajo; no significan que esa funcionalidad ya esté desarrollada.

## 1. Presentación breve del proyecto

SIGEMAC-ONPE es una aplicación web para gestionar la capacitación electoral de miembros de mesa. El sistema permite consultar asignaciones, organizar sesiones, registrar participantes y asistencia, consultar materiales y visualizar indicadores. También contempla la carga de datos mediante ETL y la administración de cuentas.

El cliente está desarrollado con HTML, CSS y JavaScript nativo. El backend es una API REST en ASP.NET Core 8 que consulta SQL Server mediante Entity Framework Core. No son microservicios: el backend se ejecuta como una única aplicación, organizada internamente por casos de uso.

## 2. Arquitectura y flujo entre capas

La organización combina **módulos por caso de uso** con una separación en capas:

```text
Interacción del usuario
  → Controller JavaScript y Views.js
  → Services.js (reglas y coordinación en el cliente)
  → Repositories.js / ApiClient.js (HTTP y JSON)
  → Controller ASP.NET Core del CU
  → interfaz y servicio del CU
  → SigemacOnpeContext / Entity Framework Core
  → SQL Server
```

En el backend, los controladores reciben y responden solicitudes HTTP; los servicios implementan la lógica del caso de uso; las interfaces permiten inyectar las implementaciones; y `SigemacOnpeContext` realiza el acceso a las entidades de la base de datos.

En el frontend, `AuthController.js` procesa el acceso inicial y `AppController.js` coordina navegación, formularios y acciones. `Views.js` presenta las pantallas y `Charts.js` dibuja gráficos SVG. `DataService.bundle()` solicita en paralelo los datos que el panel necesita.

### Decisiones de diseño que se pueden explicar

- **Organización por CU:** facilita ubicar la ruta, el servicio y el contrato correspondientes a una funcionalidad.
- **Service Layer:** mantiene las reglas y operaciones de base de datos fuera de los controladores HTTP.
- **Inyección de dependencias:** `Program.cs` registra servicios con ciclo `Scoped`; las clases reciben sus dependencias por constructor.
- **DTO y adaptación de JSON:** convierten formatos del formulario y las entidades en contratos que el frontend consume.
- **Entity Framework Core:** mapea las entidades y relaciones de SQL Server. El modelo de `Models/` representa tablas y relaciones de persistencia; no debe confundirse con los DTO de una solicitud o respuesta.

## 3. Cómo interactúa la persona usuaria con el sistema

### Consulta pública de miembro de mesa

1. En `index.html`, la persona ingresa su DNI.
2. `AuthController.js` comprueba que el DNI tenga ocho caracteres y llama a `MemberRepository.byDni()`.
3. El cliente consulta `GET /api/cus01/{dni}`.
4. CU01 busca al miembro y su relación de sesión/asistencia.
5. La vista muestra sus datos y, cuando existe, la información de la sesión. La persona puede abrir el panel como miembro de mesa.

### Acceso de personal

1. El usuario selecciona uno de los roles mostrados por la interfaz e ingresa DNI y contraseña.
2. `AuthService` valida los campos y llama a `POST /api/Auth/login`.
3. CU03 consulta la tabla de usuarios y responde con DNI, nombre, rol y un token.
4. El cliente guarda los datos de sesión en `sessionStorage` y navega a `app.html`.

**Alcance de seguridad actual:** el token que genera CU03 es simulado; no es un JWT validado por el servidor. La selección de rol y los menús del frontend organizan la interfaz, pero no sustituyen autorización real en la API.

### Carga del panel

Al navegar por el panel, `DataService.bundle()` solicita datos a varios servicios mediante `Promise.all`: sesiones, miembros según el rol, materiales, incidencias y resumen de reportes. Si una solicitud falla, el panel muestra un error. Esto explica por qué deben conservarse las rutas de reportes e incidencias mientras el frontend las siga consultando.

## 4. Casos de uso

### CUS-01 — Consultar asignación y local por DNI

**Actor:** miembro de mesa (ciudadano). **Objetivo:** consultar sus datos y la información de capacitación asociada.

- **Interfaz:** ingreso del DNI en la portada.
- **API:** `GET /api/cus01/{dni}`; para el panel administrativo también existe `GET /api/cus01/a`.
- **Backend:** `CU01Controller` delega en `ICU01ConsultarAsignacionService`; el servicio consulta `MiembroMesa`, ODPE y la relación con sesiones/asistencia. El JSON incluye el DNI, nombre, cargo, local, región y, si existe, sesión y estado de asistencia.
- **Estado:** implementado. La asignación se obtiene del vínculo guardado en `Asistencia`, no de un ID fijo.
- **Consideración:** el modelo admite más de una relación miembro-sesión. Para el contrato que muestra una sesión singular se devuelve la relación más reciente por fecha de registro.

### CUS-02 — Visualizar y descargar material educativo

**Actor:** miembro de mesa. **Objetivo:** ver los materiales de capacitación disponibles.

- **Interfaz:** panel de miembro, sección Materiales.
- **API:** `GET /api/cus02`.
- **Backend:** CU02 lee `MaterialCapacitacion` y adapta los campos al JSON (`id`, `titulo`, `tipo`, `url`, `estado`).
- **Comportamiento actual:** el cliente filtra los recursos disponibles y presenta sus enlaces. El backend entrega URLs; no almacena ni transmite directamente el contenido binario de los archivos.
- **Estado:** consulta implementada. La administración CRUD de materiales pertenece a CUS-08 y sigue pendiente; la asociación/filtrado por sesión debe considerarse al completar esos casos.

### CUS-03 — Autenticar usuario

**Actor:** personal interno, como coordinador, capacitador o personal logístico. **Objetivo:** validar credenciales y abrir el panel correspondiente al rol.

- **Interfaz:** diálogo de acceso del personal en `index.html`.
- **API:** `POST /api/Auth/login`, con `{ rol, dni, password }`.
- **Backend:** CU03 busca un usuario activo que coincida con DNI/username, rol y contraseña.
- **Estado:** el flujo visual y la consulta existen; la emisión y validación de JWT real están pendientes. Actualmente se devuelve un token constante y la comparación de contraseña no verifica un hash seguro.
- **Nota para la exposición:** presentar el login como flujo implementado en su forma actual, no como autenticación JWT terminada.

### CUS-04 — Registrar asistencia de participantes

**Actor:** capacitador. **Objetivo:** guardar qué participantes asistieron a una sesión.

- **Interfaz:** se selecciona una sesión; `AppController.js` carga el padrón y muestra casillas por participante. Al enviar, recopila `{ dni, asistio }` para cada fila.
- **API alineada:** `POST /api/Asistencia/{sesionId}`, con `{ "registros": [{ "dni": "...", "asistio": true }] }`.
- **Backend:** CU04 valida sesión, DNI, duplicados y pertenencia de los participantes al padrón de esa sesión; actualiza el lote y persiste los cambios con una sola operación de `SaveChangesAsync`.
- **Respuesta:** `200` si registra; `400` ante solicitud inválida; `404` si la sesión no existe; `409` si un participante no está asignado.
- **Estado:** ruta y formato alineados con `AttendanceRepository`. Se verificaron respuestas de error; no se realizó una escritura de asistencia de prueba en la base real.

### CUS-05 — Consultar padrón de sesión asignada

**Actor:** capacitador. **Objetivo:** ver participantes de las sesiones programadas.

- **Interfaz:** lista de sesiones y padrón de participantes; para asistencia, el cliente obtiene el padrón por sesión.
- **API:** `GET /api/SesionCapacitacion` consulta sesiones y `GET /api/cus01?sesion={id}` obtiene los participantes de una sesión.
- **Backend:** el servicio CU05 carga sesiones y sus relaciones de asistencia; CU07 provee el padrón por sesión desde los mismos vínculos persistidos.
- **Estado:** consulta implementada. La API de CU05 actualmente devuelve las sesiones disponibles; el filtrado por capacitador no se aplica mediante identidad autenticada en el servidor.

### CUS-06 — Gestionar sesiones de capacitación

**Actor:** coordinador o personal ODPE. **Objetivo:** crear, actualizar y eliminar sesiones.

- **Interfaz:** el formulario envía fecha, hora, sede, dirección y modalidad. La pestaña de capacitadores actualiza una sesión por su ID.
- **API:** `POST /api/SesionCapacitacion/Nuevo`, `POST /api/SesionCapacitacion/Editar/{id}` y `DELETE /api/SesionCapacitacion/Eliminar/{id}`.
- **Backend:** CU06 recibe `SesionCapacitacionDTO`; combina fecha y hora al crear, persiste mediante EF Core y delega la asignación de capacitador al servicio CU07. La actualización de materiales también pasa por el servicio de sesiones.
- **Estado:** creación y eliminación disponibles en API; la actualización cubre asignaciones de capacitador/materiales, no un formulario completo de edición de todos los campos. El cliente tiene método de borrado en su repositorio, pero no se encontró una acción de pantalla conectada a él. El borrado también puede ser rechazado por SQL Server cuando existen registros de asistencia vinculados a la sesión.
- **Dato provisional:** al crear se usa `IdCapacitador = 1` para satisfacer la relación obligatoria. Es una decisión temporal y debe existir ese registro en la base.

### CUS-07 — Asignar participantes y capacitadores

**Actor:** coordinador o personal ODPE. **Objetivo:** relacionar miembros y capacitadores con una sesión.

- **Interfaz:** el formulario de participantes envía DNI y sesión; el de capacitadores envía DNI y sesión.
- **Contrato conservado del frontend:** `POST /api/cus01/{dni}/assign` con `{ sesionId }`; la lista se obtiene con `GET /api/cus01?sesion={id}`. La asignación de capacitador conserva `POST /api/SesionCapacitacion/Editar/{id}` con `{ capacitador: dni }` para no modificar la UI.
- **Backend:** la lógica vive en `Services/CU07_AsignarParticipantesCapacitadores`. Valida formato y existencia de miembro/sesión y evita asignaciones duplicadas. La relación se persiste en `Asistencia` con asistencia inicial en falso. El índice único de la base `(DniMiembroMesa, IdSesion)` protege también frente a duplicados concurrentes. La asignación del capacitador valida el registro del capacitador y su rol.
- **Estado:** implementación central terminada e integrada con el contrato actual. La prueba de alta exitosa debe ejecutarse en una base de pruebas para no alterar los datos reales.
- **Decisión de modelo:** `MiembroMesa` no contiene `SesionId`; por eso se utiliza la relación `Asistencia` existente, que también es el padrón consumido por CU04/CU05.

### CUS-08 — Administrar repositorio de materiales

**Actor:** coordinador o personal ODPE. **Objetivo:** registrar y mantener manuales, guías y recursos digitales.

- **Interfaz prevista:** formularios de alta, actualización de disponibilidad y distribución.
- **Backend actual:** CU02 permite consultar materiales; CU06 procesa asignaciones de materiales a sesiones. El controlador y servicio de administración CU08 permanecen como scaffolding intencional.
- **Pendiente:** las operaciones `POST/Editar/Eliminar` que declara el repositorio frontend no tienen endpoints implementados en CU08. Por tanto, no se debe exponer como CRUD terminado.

### CUS-09 — Generar reportes y dashboards

**Actor:** coordinador o personal ODPE. **Objetivo:** mostrar indicadores para seguimiento.

- **Interfaz:** `Views.js` presenta tarjetas y gráficos SVG de `Charts.js`; el usuario también puede exportar datos o imprimir.
- **API:** `GET /api/Reports/summary`. El controlador se ubica ahora dentro de la carpeta CU09 y mantiene la ruta para compatibilidad con el frontend.
- **Comportamiento actual:** responde un resumen de ejemplo con cifras fijas. El dashboard puede renderizarlo, pero no representa un cálculo analítico real de la base de datos.
- **Estado:** contrato/punto de integración presente; consultas analíticas pendientes.

### CUS-10 — Ejecutar ingesta masiva ETL

**Actor:** administrador o personal de TI. **Objetivo:** cargar padrones y datos maestros mediante extracción, transformación y carga.

- **Componentes disponibles:** existe un proyecto SSIS con `Package.dtsx` bajo `ETL/SIGEMAC-ONPE_SSIS`.
- **Backend actual:** no se encontró una ruta que inicie o supervise el paquete SSIS desde la API.
- **Estado:** infraestructura ETL en el repositorio; integración operativa con la API pendiente.

### CUS-11 — Gestionar cuentas y accesos

**Actor:** administrador o personal de TI. **Objetivo:** gestionar cuentas, roles, credenciales y bloqueos.

- **Interfaz/backend:** la carpeta CU11 y su controlador son scaffolding para avanzar el caso. No hay CRUD de cuentas implementado.
- **Relación con CUS-03:** el login de personal existe, pero no reemplaza la administración de cuentas. JWT real, hash de contraseña y autorización de endpoints siguen pendientes.
- **Estado:** caso planificado.

## 5. Rutas principales para demostrar el recorrido

| Acción demostrable | Método y ruta | Caso |
|---|---|---|
| Consultar miembro por DNI | `GET /api/cus01/{dni}` | CUS-01 |
| Login de personal | `POST /api/Auth/login` | CUS-03 |
| Ver materiales | `GET /api/cus02` | CUS-02 |
| Listar sesiones/padrón | `GET /api/SesionCapacitacion` | CUS-05 |
| Crear sesión | `POST /api/SesionCapacitacion/Nuevo` | CUS-06 |
| Asignar participante | `POST /api/cus01/{dni}/assign` | CUS-07 |
| Consultar participantes de sesión | `GET /api/cus01?sesion={id}` | CUS-05/CUS-07 |
| Asignar capacitador | `POST /api/SesionCapacitacion/Editar/{id}` | CUS-07, conservando la ruta del cliente |
| Registrar asistencia por lote | `POST /api/Asistencia/{sesionId}` | CUS-04 |
| Ver resumen del dashboard | `GET /api/Reports/summary` | CUS-09, datos demostrativos |

## 6. Estado técnico y alcance que conviene declarar

- Los roles visibles en el frontend son coordinador, capacitador, logístico y miembro de mesa. El rol administrador descrito en algunos casos aún no tiene una interfaz correspondiente.
- `IncidentsController` permanece en la raíz porque el frontend consulta `GET /api/Incidents`, pero no existe un caso de uso específico de incidencias en `CasosDeUso.md`. Actualmente devuelve una lista vacía; el alta de incidencias del frontend no tiene endpoint `POST` implementado.
- La consulta de reportes devuelve datos fijos; debe presentarse como integración/demostración, no como analítica final.
- La autenticación actual no aplica JWT real ni protección de roles en el servidor. No presentar el token simulado como mecanismo de seguridad terminado.
- La compilación del backend tras las últimas modificaciones fue exitosa: cero errores y cero advertencias. Las pruebas HTTP cubrieron respuestas de validación y error, sin ejecutar escrituras exitosas en la base local.
- `Models/` contiene entidades de persistencia de SQL Server. Para adaptar contratos se usan DTO, sin eliminar esas entidades.

## 7. Guion breve de exposición

> “SIGEMAC-ONPE es una aplicación web para gestionar la capacitación de miembros de mesa. El frontend está hecho con JavaScript nativo y separa vistas, controladores, servicios y repositorios. El backend es una Web API ASP.NET Core organizada por casos de uso; cada controlador recibe la petición y delega en un servicio, que usa Entity Framework Core para trabajar con SQL Server. Actualmente están implementadas consultas de miembros, login funcional en modo demostrativo, lectura de materiales, consulta de sesiones, gestión básica de sesiones, asignaciones y registro de asistencia por lote. Los CU08 a CU11 tienen partes pendientes: el CRUD de materiales, reportes calculados, ejecución de ETL desde la API y gestión de cuentas/JWT. Así se distingue lo que ya funciona de la evolución prevista.”
