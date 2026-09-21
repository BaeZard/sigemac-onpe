# SIGEMAC · ONPE (frontend)
Abrir la carpeta en VS Code → clic derecho en `index.html` → *Open with Live Server* (o doble clic).
Modo demo (`USE_MOCK: true`): miembros de mesa de prueba `72710878`, `45871236`, `40125896`, `71234567` (sin sesión, se le asigna una). Personal: pestaña "Personal ONPE" con cualquier DNI de 8 dígitos + contraseña de 4+ caracteres.

## Arquitecturas del informe → dónde están
- **3 capas**: Presentación (`*.html`, `css/`, `js/views`, `js/controllers`) · Lógica (`js/business` + API C#) · Datos (`js/data` → API → SQL Server).
- **MVC**: `js/models` · `js/views` · `js/controllers`.
- **Cliente-Servidor**: `js/data/ApiClient.js` (Fetch + JWT). El front nunca toca la BD.
- **Microservicios**: un repositorio y una URL por dominio en `js/config.js`.

## Conectar el backend
1. En `js/config.js`: `USE_MOCK: false` y ajustar las URLs.
2. Endpoints esperados (JSON):

| Método | Ruta | Uso |
|---|---|---|
| POST | /api/auth/login `{rol,dni,password}` | personal → `{dni,nombre,rol,token}` |
| POST | /api/auth/member-access `{dni}` | valida DNI en el padrón, asigna capacitación, devuelve token; 404 si no figura |
| GET/POST | /api/sessions | listar / crear sesión `{fecha,hora,sede,modalidad}` |
| GET | /api/members/{dni} | miembro + `sesion`; 404 si no figura |
| GET | /api/members?sesion={id} | participantes de una sesión |
| POST | /api/members/{dni}/assign `{sesionId}` | asignar |
| POST | /api/attendance/{sesionId} `{registros:[{dni,asistio}]}` | guardar asistencia |
| GET/POST | /api/materials | listar / crear `{titulo,tipo,url}` |
| GET | /api/reports/summary | `{miembros,sesiones,cumplimiento,regiones:[{n,p}]}` |

Endpoints adicionales (la API debe filtrar por rol; el miembro de mesa solo ve sus datos):

| Método | Ruta | Uso |
|---|---|---|
| GET/POST | /api/members | listar / registrar miembro |
| PATCH | /api/members/{dni} | actualizar `{cargo,local,region}` |
| PATCH | /api/sessions/{id} | asignar `{capacitador}` o `{materialIds}` |
| PATCH | /api/materials/{id} | actualizar `{estado}` (Disponible/Pendiente/Agotado) |
| GET/POST | /api/incidents | alertas / incidencias `{tipo,detalle}` |

Demo capacitador: DNI `12345678` (tiene la sesión de Lima asignada).

`GET /api/reports/summary` alimenta el dashboard: `{miembros,sesiones,cumplimiento,capacitados,proceso,pendientes,asistencia,regiones:[{n,p,pend}],tendencia:[{n,v}],stock:[{n,disp,dist,pend}]}`.
