# Especificación de Casos de Uso del Sistema (SIGEMAC-ONPE)

A continuación se detalla la especificación de los casos de uso del sistema, organizados según la estructura y flujo de actores del diagrama general. Se han integrado las descripciones de negocio a sus equivalentes en el sistema.

---

### 1. CUS-01: Consultar Asignación y Local por DNI

| Campo | Descripción |
| :--- | :--- |
| **ID** | CUS-01 |
| **Nombre** | Consultar Asignación y Local por DNI |
| **Actor principal** | Miembro de Mesa (Ciudadano) |
| **Objetivo** | Permitir al ciudadano consultar el estado de su asignación, sede y materiales de capacitación. |
| **Descripción** | El ciudadano consulta su estado de capacitación ingresando su número de DNI en la plataforma web institucional. El sistema valida la existencia del registro y presenta la información de la jornada. |
| **Precondición** | La información del padrón debe estar previamente cargada en la base de datos. |
| **Flujo principal** | 1. El ciudadano ingresa su DNI.<br>2. Presiona "Consultar".<br>3. El sistema valida el formato e identidad del documento.<br>4. El sistema consulta y retorna la información asignada.<br>5. El ciudadano visualiza sus datos y accesos a la capacitación. |
| **Poscondición** | El ciudadano visualiza la información correspondiente a su DNI. |

---

### 2. CUS-02: Visualizar y Descargar Material Educativo

| Campo | Descripción |
| :--- | :--- |
| **ID** | CUS-02 |
| **Nombre** | Visualizar y Descargar Material Educativo |
| **Actor principal** | Miembro de Mesa (Ciudadano) |
| **Objetivo** | Permitir al miembro de mesa acceder a los recursos educativos (guías, manuales, videos) vinculados a su rol. |
| **Descripción** | El sistema muestra los materiales disponibles según la capacitación asignada, permitiendo su visualización en línea o descarga directa. |
| **Precondición** | El miembro de mesa debe haber consultado su asignación (CUS-01) y los materiales deben estar previamente cargados. |
| **Flujo principal** | 1. El miembro de mesa selecciona la opción de materiales.<br>2. El sistema lista los recursos disponibles (PDFs, enlaces, videos).<br>3. El usuario selecciona un recurso para ver o descargar.<br>4. El sistema entrega el archivo solicitado. |
| **Poscondición** | El miembro de mesa obtiene el material educativo en su dispositivo. |

---

### 3. CUS-04: Registrar Asistencia de Participantes

| Campo | Descripción |
| :--- | :--- |
| **ID** | CUS-04 |
| **Nombre** | Registrar Asistencia de Participantes |
| **Actor principal** | Capacitador Electoral |
| **Objetivo** | Registrar la participación de los miembros de mesa en las sesiones. |
| **Descripción** | El capacitador registra la asistencia de los participantes durante la sesión presencial o virtual. |
| **Precondición** | El participante debe estar asignado a una sesión y el capacitador debe haber iniciado sesión (CUS-03). |
| **Flujo principal** | 1. El capacitador selecciona la sesión correspondiente.<br>2. Consulta la lista de participantes.<br>3. Registra la asistencia.<br>4. El sistema almacena la información. |
| **Poscondición** | La asistencia queda registrada y disponible para seguimiento por parte de los coordinadores. |

---

### 4. CUS-05: Consultar Padrón de Sesión Asignada

| Campo | Descripción |
| :--- | :--- |
| **ID** | CUS-05 |
| **Nombre** | Consultar Padrón de Sesión Asignada |
| **Actor principal** | Capacitador Electoral |
| **Objetivo** | Permitir al capacitador revisar la lista de miembros de mesa que asistirán a sus sesiones programadas. |
| **Descripción** | El sistema lista los participantes agrupados por la sesión específica a cargo del capacitador en turno. |
| **Precondición** | El capacitador debe estar autenticado (CUS-03) y tener sesiones asignadas. |
| **Flujo principal** | 1. El capacitador ingresa a sus sesiones.<br>2. Selecciona una sesión de su lista.<br>3. El sistema retorna el padrón de ciudadanos asignados a esa sesión. |
| **Poscondición** | El capacitador visualiza la información completa de los asistentes esperados. |

---

### 5. CUS-03: Autenticar Usuario (Gestión de Sesión)

| Campo | Descripción |
| :--- | :--- |
| **ID** | CUS-03 |
| **Nombre** | Autenticar Usuario (Gestión de Sesión) |
| **Actor principal** | Capacitador Electoral, Coordinador, Administrador (Inclusión `<<include>>`) |
| **Objetivo** | Validar la identidad de los usuarios internos del sistema para permitir el acceso a sus funciones específicas. |
| **Descripción** | El sistema verifica las credenciales (DNI/Username y contraseña) y genera un token (JWT) de sesión según el rol. |
| **Precondición** | El usuario debe estar registrado en la base de datos de personal. |
| **Flujo principal** | 1. El usuario ingresa credenciales.<br>2. El sistema valida contra la base de datos.<br>3. Se emite autorización por rol.<br>4. El usuario accede a su dashboard. |
| **Poscondición** | El usuario obtiene acceso seguro a los módulos correspondientes a su perfil. |

---

### 6. CUS-06: Gestionar Sesiones de Capacitación

| Campo | Descripción |
| :--- | :--- |
| **ID** | CUS-06 |
| **Nombre** | Gestionar Sesiones de Capacitación |
| **Actor principal** | Coordinador / Personal ODPE |
| **Objetivo** | Organizar las sesiones de capacitación de acuerdo con fechas, horarios, sedes y modalidades disponibles. |
| **Descripción** | El coordinador registra, modifica o elimina las sesiones que serán desarrolladas para los miembros de mesa. |
| **Precondición** | El coordinador debe estar autenticado en el sistema (CUS-03). |
| **Flujo principal** | 1. El coordinador ingresa al módulo de planificaciones.<br>2. Selecciona la opción de nueva sesión.<br>3. Registra fecha, horario, sede y modalidad.<br>4. El sistema valida los datos.<br>5. Se registra la sesión en la base de datos. |
| **Poscondición** | La sesión queda disponible para la asignación de participantes y capacitadores. |

---

### 7. CUS-07: Asignar Participantes y Capacitadores

| Campo | Descripción |
| :--- | :--- |
| **ID** | CUS-07 |
| **Nombre** | Asignar Participantes y Capacitadores |
| **Actor principal** | Coordinador / Personal ODPE |
| **Objetivo** | Asignar automáticamente o mediante criterios definidos a los miembros de mesa y capacitadores a las sesiones. |
| **Descripción** | El sistema utiliza la información almacenada para vincular a los participantes y responsables con una sesión específica (CRUD de asignaciones). |
| **Precondición** | Deben existir sesiones previamente creadas y datos de miembros de mesa en la base de datos. |
| **Flujo principal** | 1. El coordinador selecciona una sesión.<br>2. El sistema consulta los miembros disponibles.<br>3. Se realiza la asignación (por DNI).<br>4. El sistema valida posibles duplicidades.<br>5. Se confirma y guarda la asignación. |
| **Poscondición** | Los miembros de mesa y el capacitador quedan vinculados de manera efectiva a una sesión de capacitación. |

---

### 8. CUS-08: Administrar Repositorio de Materiales

| Campo | Descripción |
| :--- | :--- |
| **ID** | CUS-08 |
| **Nombre** | Administrar Repositorio de Materiales |
| **Actor principal** | Coordinador / Personal ODPE |
| **Objetivo** | Centralizar y facilitar la distribución de materiales educativos. |
| **Descripción** | El responsable registra, actualiza y pone a disposición manuales, guías y recursos digitales para las capacitaciones. |
| **Precondición** | El usuario debe contar con permisos de gestión y estar autenticado. |
| **Flujo principal** | 1. Ingresa al módulo de materiales.<br>2. Registra o actualiza un recurso (título, tipo, URL).<br>3. El sistema valida la información.<br>4. El material queda disponible para los usuarios autorizados (asignado a sesiones). |
| **Poscondición** | El material puede ser consultado digitalmente por los capacitadores y ciudadanos. |

---

### 9. CUS-09: Generar Reportes y Dashboards

| Campo | Descripción |
| :--- | :--- |
| **ID** | CUS-09 |
| **Nombre** | Generar Reportes y Dashboards |
| **Actor principal** | Coordinador / Personal ODPE |
| **Objetivo** | Obtener información estadística sobre el avance de las capacitaciones. |
| **Descripción** | El sistema procesa la información almacenada y presenta indicadores de cobertura, asistencia y cumplimiento en formato visual (SVG) o exportable (PDF/CSV). |
| **Precondición** | Deben existir registros de capacitaciones y control de asistencia procesados. |
| **Flujo principal** | 1. El usuario solicita acceder al panel de seguimiento / reportes.<br>2. El sistema consulta los datos transaccionales.<br>3. Procesa la información (agrupaciones por ODPE, asistencia).<br>4. Genera indicadores gráficos.<br>5. Presenta el dashboard. |
| **Poscondición** | El usuario dispone de información analítica actualizada para la toma de decisiones. |

---

### 10. CUS-10: Ejecutar Ingesta Masiva de Datos ETL

| Campo | Descripción |
| :--- | :--- |
| **ID** | CUS-10 |
| **Nombre** | Ejecutar Ingesta Masiva de Datos ETL |
| **Actor principal** | Administrador / Personal de TI |
| **Objetivo** | Migrar e integrar datos a gran escala hacia el sistema desde fuentes externas (padrones, listados maestros). |
| **Descripción** | Permite ejecutar procesos de Extracción, Transformación y Carga (SSIS/ETL) para poblar las tablas de ciudadanos, locales y personal en la base de datos SQL Server. |
| **Precondición** | Deben existir archivos origen o conexiones habilitadas hacia los sistemas fuente. |
| **Flujo principal** | 1. El administrador inicia el proceso ETL.<br>2. El sistema lee la fuente de datos.<br>3. Se aplican reglas de transformación y validación.<br>4. Los datos son insertados en la base de datos principal de SIGEMAC. |
| **Poscondición** | El padrón de ciudadanos y datos maestros quedan listos para su uso transaccional. |

---

### 11. CUS-11: Gestionar Cuentas y Accesos

| Campo | Descripción |
| :--- | :--- |
| **ID** | CUS-11 |
| **Nombre** | Gestionar Cuentas y Accesos |
| **Actor principal** | Administrador / Personal de TI |
| **Objetivo** | Administrar las credenciales, roles y permisos de los usuarios internos del sistema. |
| **Descripción** | Módulo de seguridad para la creación, actualización, bloqueo y eliminación de perfiles de usuarios (Coordinadores, Capacitadores, Logística). |
| **Precondición** | El administrador debe estar validado en el sistema con privilegios máximos. |
| **Flujo principal** | 1. El administrador accede al gestor de cuentas.<br>2. Selecciona crear o modificar un usuario.<br>3. Asigna un rol (ej. Capacitador) y credenciales (DNI/Password).<br>4. Guarda los cambios en la base de datos. |
| **Poscondición** | Las políticas de seguridad y accesos del personal son actualizadas y entran en vigor de inmediato. |
```eof

He renombrado los identificadores de "CUN" (Negocio) a "CUS" (Sistema) tal como está en el diagrama para mantener la coherencia y he añadido los casos que aparecían ilustrados pero que no tenían tabla de especificación en el texto original. ¡Cualquier ajuste me avisas!