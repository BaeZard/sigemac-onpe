USE SistemaCapacitacion;
GO

SET NOCOUNT ON;

--------CONSULTAS BÁSICAS--------
--Miembros de mesa de una ODPE
SELECT m.Dni, m.Nombres, m.Apellidos, m.Cargo, m.EstadoCapacitacion
FROM dbo.MiembroMesa m
INNER JOIN dbo.ODPE o ON o.IdOdpe = m.IdOdpe
WHERE o.NombreOdpe = N'ODPE LIMA CENTRO 1'
ORDER BY m.Apellidos, m.Nombres;

--Cantidad de miembros según su estado de capacitación
SELECT EstadoCapacitacion, COUNT(*) AS Total
FROM dbo.MiembroMesa
GROUP BY EstadoCapacitacion
ORDER BY Total DESC;

--Búsqueda por DNI (prefijo) o por apellido/nombre
SELECT Dni, Nombres, Apellidos, Cargo, EstadoCapacitacion
FROM dbo.MiembroMesa
WHERE Dni LIKE N'7000%'
   OR Apellidos LIKE N'%Quispe%'
   OR Nombres LIKE N'%Quispe%'
ORDER BY Apellidos, Nombres;

--Sesiones de capacitación programadas a futuro
SELECT IdSesion, Sede, Direccion, FechaHora, Modalidad
FROM dbo.SesionCapacitacion
WHERE FechaHora >= SYSDATETIME()
ORDER BY FechaHora;

--Materiales vigentes agrupados por tipo (HU-04)
SELECT Tipo, COUNT(*) AS TotalMateriales
FROM dbo.MaterialCapacitacion
WHERE Activo = N'1'
GROUP BY Tipo
ORDER BY Tipo;

--------CONSULTAS CON JOIN--------
--datos del miembro de mesa por DNI
SELECT m.Dni, m.Nombres, m.Apellidos, m.Cargo, m.EstadoCapacitacion,
       o.NombreOdpe, o.Region
FROM dbo.MiembroMesa m
INNER JOIN dbo.ODPE o ON o.IdOdpe = m.IdOdpe
WHERE m.Dni = N'70000001';

--sesiones asignadas a un DNI, con su capacitador
SELECT s.IdSesion, s.Sede, s.Direccion, s.FechaHora, s.Modalidad,
       c.CodigoCapacitador, u.Username AS UsuarioCapacitador
FROM dbo.AsignacionSesion a
INNER JOIN dbo.SesionCapacitacion s ON s.IdSesion = a.IdSesion
INNER JOIN dbo.Capacitador c ON c.IdCapacitador = s.IdCapacitador
INNER JOIN dbo.Usuario u ON u.IdUsuario = c.IdUsuario
WHERE a.DniMiembroMesa = N'70000001'
ORDER BY s.FechaHora;

--materiales vigentes de las sesiones de un DNI
SELECT DISTINCT mat.IdMaterial, mat.Titulo, mat.Tipo, mat.UrlRecurso
FROM dbo.AsignacionSesion a
INNER JOIN dbo.SesionMaterial sm ON sm.IdSesion = a.IdSesion
INNER JOIN dbo.MaterialCapacitacion mat ON mat.IdMaterial = sm.IdMaterial
WHERE a.DniMiembroMesa = N'70000001'
  AND mat.Activo = N'1'
ORDER BY mat.Tipo, mat.Titulo;

--participantes de una sesión con su estado de asistencia
SELECT m.Dni, m.Apellidos, m.Nombres, m.Cargo,
       CASE x.Asistio
            WHEN N'1' THEN N'Asistió'
            WHEN N'0' THEN N'No asistió'
            ELSE N'Sin registrar'
       END AS EstadoAsistencia,
       x.FechaRegistro, x.Observacion
FROM dbo.AsignacionSesion a
INNER JOIN dbo.MiembroMesa m ON m.Dni = a.DniMiembroMesa
LEFT JOIN dbo.Asistencia x
       ON x.IdSesion = a.IdSesion AND x.DniMiembroMesa = a.DniMiembroMesa
WHERE a.IdSesion = 1
ORDER BY m.Apellidos, m.Nombres;

--Capacitadores con su usuario y las sesiones que dictan
SELECT c.CodigoCapacitador, c.Especialidad, u.Username, u.Estado,
       s.IdSesion, s.Sede, s.FechaHora, s.Modalidad
FROM dbo.Capacitador c
INNER JOIN dbo.Usuario u ON u.IdUsuario = c.IdUsuario
LEFT JOIN dbo.SesionCapacitacion s ON s.IdCapacitador = c.IdCapacitador
ORDER BY c.CodigoCapacitador, s.FechaHora;

--Miembros de mesa que tienen usuario para iniciar sesión
SELECT m.Dni, m.Apellidos, m.Nombres, u.Username, u.Rol, u.Estado
FROM dbo.MiembroMesa m
INNER JOIN dbo.Usuario u ON u.IdUsuario = m.IdUsuario
ORDER BY m.Apellidos;

--Materiales asignados a cada sesión
SELECT s.IdSesion, s.Sede, s.FechaHora,
       mat.Titulo, mat.Tipo, mat.Activo, sm.FechaAsignacion
FROM dbo.SesionCapacitacion s
INNER JOIN dbo.SesionMaterial sm ON sm.IdSesion = s.IdSesion
INNER JOIN dbo.MaterialCapacitacion mat ON mat.IdMaterial = sm.IdMaterial
ORDER BY s.FechaHora, mat.Tipo;

--------CONSULTAS DE AGREGACIÓN (REPORTES Y DASHBOARDS)--------
--Miembros de mesa por región y ODPE, con subtotales y total nacional
SELECT CASE WHEN GROUPING(o.Region) = 1 THEN N'TOTAL NACIONAL' ELSE o.Region END AS Region,
       CASE WHEN GROUPING(o.NombreOdpe) = 1 THEN N'Subtotal' ELSE o.NombreOdpe END AS Odpe,
       COUNT(m.Dni) AS TotalMiembros
FROM dbo.ODPE o
LEFT JOIN dbo.MiembroMesa m ON m.IdOdpe = o.IdOdpe
GROUP BY ROLLUP (o.Region, o.NombreOdpe)
ORDER BY GROUPING(o.Region), o.Region, GROUPING(o.NombreOdpe), o.NombreOdpe;

--Cumplimiento de capacitación por ODPE (% de miembros capacitados)
SELECT o.Region, o.NombreOdpe,
       COUNT(m.Dni) AS TotalMiembros,
       SUM(CASE WHEN m.EstadoCapacitacion = N'Capacitado' THEN 1 ELSE 0 END) AS Capacitados,
       SUM(CASE WHEN m.EstadoCapacitacion = N'Ausente' THEN 1 ELSE 0 END) AS Ausentes,
       CAST(100.0 * SUM(CASE WHEN m.EstadoCapacitacion = N'Capacitado' THEN 1 ELSE 0 END)
            / NULLIF(COUNT(m.Dni), 0) AS DECIMAL(5,2)) AS PorcentajeCumplimiento
FROM dbo.ODPE o
LEFT JOIN dbo.MiembroMesa m ON m.IdOdpe = o.IdOdpe
GROUP BY o.Region, o.NombreOdpe
ORDER BY PorcentajeCumplimiento DESC, o.NombreOdpe;

--Asistencia por sesión (asignados, asistieron, no asistieron, sin registrar y %)
SELECT s.IdSesion, s.Sede, s.FechaHora, s.Modalidad,
       COUNT(*) AS TotalAsignados,
       SUM(CASE WHEN x.Asistio = N'1' THEN 1 ELSE 0 END) AS Asistieron,
       SUM(CASE WHEN x.Asistio = N'0' THEN 1 ELSE 0 END) AS NoAsistieron,
       SUM(CASE WHEN x.IdAsistencia IS NULL THEN 1 ELSE 0 END) AS SinRegistrar,
       CAST(100.0 * SUM(CASE WHEN x.Asistio = N'1' THEN 1 ELSE 0 END)
            / NULLIF(COUNT(*), 0) AS DECIMAL(5,2)) AS PorcentajeAsistencia
FROM dbo.SesionCapacitacion s
INNER JOIN dbo.AsignacionSesion a ON a.IdSesion = s.IdSesion
LEFT JOIN dbo.Asistencia x
       ON x.IdSesion = a.IdSesion AND x.DniMiembroMesa = a.DniMiembroMesa
GROUP BY s.IdSesion, s.Sede, s.FechaHora, s.Modalidad
ORDER BY s.FechaHora;

--Asistencia por región y ODPE
SELECT o.Region, o.NombreOdpe,
       COUNT(*) AS TotalAsignados,
       SUM(CASE WHEN x.Asistio = N'1' THEN 1 ELSE 0 END) AS Asistieron,
       CAST(100.0 * SUM(CASE WHEN x.Asistio = N'1' THEN 1 ELSE 0 END)
            / NULLIF(COUNT(*), 0) AS DECIMAL(5,2)) AS PorcentajeAsistencia
FROM dbo.AsignacionSesion a
INNER JOIN dbo.MiembroMesa m ON m.Dni = a.DniMiembroMesa
INNER JOIN dbo.ODPE o ON o.IdOdpe = m.IdOdpe
LEFT JOIN dbo.Asistencia x
       ON x.IdSesion = a.IdSesion AND x.DniMiembroMesa = a.DniMiembroMesa
GROUP BY o.Region, o.NombreOdpe
ORDER BY o.Region, PorcentajeAsistencia DESC;

--sesiones con baja participación (asistencia menor a 60 %)
SELECT s.IdSesion, s.Sede, s.FechaHora,
       COUNT(*) AS TotalAsignados,
       SUM(CASE WHEN x.Asistio = N'1' THEN 1 ELSE 0 END) AS Asistieron,
       CAST(100.0 * SUM(CASE WHEN x.Asistio = N'1' THEN 1 ELSE 0 END)
            / COUNT(*) AS DECIMAL(5,2)) AS PorcentajeAsistencia
FROM dbo.SesionCapacitacion s
INNER JOIN dbo.AsignacionSesion a ON a.IdSesion = s.IdSesion
LEFT JOIN dbo.Asistencia x
       ON x.IdSesion = a.IdSesion AND x.DniMiembroMesa = a.DniMiembroMesa
GROUP BY s.IdSesion, s.Sede, s.FechaHora
HAVING 100.0 * SUM(CASE WHEN x.Asistio = N'1' THEN 1 ELSE 0 END) / COUNT(*) < 60
ORDER BY PorcentajeAsistencia;

--Materiales más utilizados (en cuántas sesiones se asignó cada uno)
SELECT mat.IdMaterial, mat.Titulo, mat.Tipo, mat.Activo,
       COUNT(sm.IdSesion) AS SesionesQueLoUsan
FROM dbo.MaterialCapacitacion mat
LEFT JOIN dbo.SesionMaterial sm ON sm.IdMaterial = mat.IdMaterial
GROUP BY mat.IdMaterial, mat.Titulo, mat.Tipo, mat.Activo
ORDER BY SesionesQueLoUsan DESC, mat.Titulo;

--Carga de trabajo por capacitador (sesiones y participantes)
SELECT c.CodigoCapacitador, c.Especialidad,
       COUNT(DISTINCT s.IdSesion) AS Sesiones,
       COUNT(a.DniMiembroMesa) AS Participantes
FROM dbo.Capacitador c
LEFT JOIN dbo.SesionCapacitacion s ON s.IdCapacitador = c.IdCapacitador
LEFT JOIN dbo.AsignacionSesion a ON a.IdSesion = s.IdSesion
GROUP BY c.CodigoCapacitador, c.Especialidad
ORDER BY Sesiones DESC, c.CodigoCapacitador;

--Sesiones por modalidad y mes
SELECT s.Modalidad, YEAR(s.FechaHora) AS Anio, MONTH(s.FechaHora) AS Mes,
       COUNT(*) AS TotalSesiones
FROM dbo.SesionCapacitacion s
GROUP BY s.Modalidad, YEAR(s.FechaHora), MONTH(s.FechaHora)
ORDER BY Anio, Mes, s.Modalidad;

--------SUBCONSULTAS, CTE Y FUNCIONES DE VENTANA--------
--Miembros de mesa sin ninguna sesión asignada
SELECT m.Dni, m.Apellidos, m.Nombres, o.NombreOdpe, m.EstadoCapacitacion
FROM dbo.MiembroMesa m
INNER JOIN dbo.ODPE o ON o.IdOdpe = m.IdOdpe
WHERE NOT EXISTS (SELECT 1 FROM dbo.AsignacionSesion a WHERE a.DniMiembroMesa = m.Dni)
ORDER BY o.NombreOdpe, m.Apellidos;

--Asistencias pendientes de registrar en sesiones que ya ocurrieron
SELECT s.IdSesion, s.Sede, s.FechaHora, m.Dni, m.Apellidos, m.Nombres
FROM dbo.AsignacionSesion a
INNER JOIN dbo.SesionCapacitacion s ON s.IdSesion = a.IdSesion
INNER JOIN dbo.MiembroMesa m ON m.Dni = a.DniMiembroMesa
LEFT JOIN dbo.Asistencia x
       ON x.IdSesion = a.IdSesion AND x.DniMiembroMesa = a.DniMiembroMesa
WHERE s.FechaHora < SYSDATETIME()
  AND x.IdAsistencia IS NULL
ORDER BY s.FechaHora, m.Apellidos;

--Miembros ausentes que aún no tienen una nueva sesión futura (por reagendar)
SELECT m.Dni, m.Apellidos, m.Nombres, o.NombreOdpe, o.Region
FROM dbo.MiembroMesa m
INNER JOIN dbo.ODPE o ON o.IdOdpe = m.IdOdpe
WHERE m.EstadoCapacitacion = N'Ausente'
  AND NOT EXISTS (SELECT 1
                  FROM dbo.AsignacionSesion a
                  INNER JOIN dbo.SesionCapacitacion s ON s.IdSesion = a.IdSesion
                  WHERE a.DniMiembroMesa = m.Dni
                    AND s.FechaHora > SYSDATETIME())
ORDER BY o.Region, o.NombreOdpe, m.Apellidos;

--Sesiones que todavía no tienen material asignado
SELECT s.IdSesion, s.Sede, s.FechaHora, s.Modalidad
FROM dbo.SesionCapacitacion s
WHERE NOT EXISTS (SELECT 1 FROM dbo.SesionMaterial sm WHERE sm.IdSesion = s.IdSesion)
ORDER BY s.FechaHora;

--Materiales vigentes que no se han asignado a ninguna sesión
SELECT mat.IdMaterial, mat.Titulo, mat.Tipo
FROM dbo.MaterialCapacitacion mat
WHERE mat.Activo = N'1'
  AND NOT EXISTS (SELECT 1 FROM dbo.SesionMaterial sm WHERE sm.IdMaterial = mat.IdMaterial);

--Ranking de ODPE por porcentaje de asistencia (función de ventana RANK)
WITH Asistencia_Odpe AS
(
    SELECT o.Region, o.NombreOdpe,
           COUNT(*) AS TotalAsignados,
           CAST(100.0 * SUM(CASE WHEN x.Asistio = N'1' THEN 1 ELSE 0 END)
                / NULLIF(COUNT(*), 0) AS DECIMAL(5,2)) AS PorcentajeAsistencia
    FROM dbo.AsignacionSesion a
    INNER JOIN dbo.MiembroMesa m ON m.Dni = a.DniMiembroMesa
    INNER JOIN dbo.ODPE o ON o.IdOdpe = m.IdOdpe
    LEFT JOIN dbo.Asistencia x
           ON x.IdSesion = a.IdSesion AND x.DniMiembroMesa = a.DniMiembroMesa
    GROUP BY o.Region, o.NombreOdpe
)
SELECT RANK() OVER (ORDER BY PorcentajeAsistencia DESC) AS Posicion,
       Region, NombreOdpe, TotalAsignados, PorcentajeAsistencia
FROM Asistencia_Odpe
ORDER BY Posicion, NombreOdpe;

--Miembros asignados a más de una sesión
SELECT m.Dni, m.Apellidos, m.Nombres, COUNT(*) AS SesionesAsignadas
FROM dbo.AsignacionSesion a
INNER JOIN dbo.MiembroMesa m ON m.Dni = a.DniMiembroMesa
GROUP BY m.Dni, m.Apellidos, m.Nombres
HAVING COUNT(*) > 1
ORDER BY SesionesAsignadas DESC, m.Apellidos;

--Zonas rezagadas: ODPE con cumplimiento menor al promedio nacional
WITH Cumplimiento AS
(
    SELECT o.IdOdpe, o.Region, o.NombreOdpe,
           COUNT(m.Dni) AS TotalMiembros,
           CAST(100.0 * SUM(CASE WHEN m.EstadoCapacitacion = N'Capacitado' THEN 1 ELSE 0 END)
                / NULLIF(COUNT(m.Dni), 0) AS DECIMAL(5,2)) AS PorcentajeCumplimiento
    FROM dbo.ODPE o
    LEFT JOIN dbo.MiembroMesa m ON m.IdOdpe = o.IdOdpe
    GROUP BY o.IdOdpe, o.Region, o.NombreOdpe
)
SELECT Region, NombreOdpe, TotalMiembros, PorcentajeCumplimiento
FROM Cumplimiento
WHERE PorcentajeCumplimiento <
      (SELECT 100.0 * SUM(CASE WHEN EstadoCapacitacion = N'Capacitado' THEN 1 ELSE 0 END)
              / NULLIF(COUNT(*), 0)
       FROM dbo.MiembroMesa)
ORDER BY PorcentajeCumplimiento;

--Última asistencia registrada de cada miembro (ROW_NUMBER)
WITH Ultima AS
(
    SELECT x.DniMiembroMesa, x.IdSesion, x.Asistio, x.FechaRegistro,
           ROW_NUMBER() OVER (PARTITION BY x.DniMiembroMesa
                              ORDER BY x.FechaRegistro DESC, x.IdAsistencia DESC) AS Orden
    FROM dbo.Asistencia x
)
SELECT m.Dni, m.Apellidos, m.Nombres, u.IdSesion, u.Asistio, u.FechaRegistro
FROM Ultima u
INNER JOIN dbo.MiembroMesa m ON m.Dni = u.DniMiembroMesa
WHERE u.Orden = 1
ORDER BY u.FechaRegistro DESC;

--------VALIDACIÓN DE CALIDAD DE DATOS (control posterior a la carga ETL)--------
--DNI con formato inválido (debe tener 8 dígitos numéricos)
SELECT Dni, Nombres, Apellidos
FROM dbo.MiembroMesa
WHERE LEN(Dni) <> 8 OR Dni LIKE N'%[^0-9]%';

--Nombres o apellidos vacíos
SELECT Dni, Nombres, Apellidos
FROM dbo.MiembroMesa
WHERE LTRIM(RTRIM(Nombres)) = N'' OR LTRIM(RTRIM(Apellidos)) = N'';

--ODPE sin miembros de mesa cargados
SELECT o.IdOdpe, o.NombreOdpe, o.Region
FROM dbo.ODPE o
WHERE NOT EXISTS (SELECT 1 FROM dbo.MiembroMesa m WHERE m.IdOdpe = o.IdOdpe);

--Miembros marcados como Capacitado sin ninguna asistencia positiva registrada
SELECT m.Dni, m.Apellidos, m.Nombres
FROM dbo.MiembroMesa m
WHERE m.EstadoCapacitacion = N'Capacitado'
  AND NOT EXISTS (SELECT 1 FROM dbo.Asistencia x
                  WHERE x.DniMiembroMesa = m.Dni AND x.Asistio = N'1');

--Conciliación de conteos por tabla (comparar con las filas del archivo de origen)
SELECT N'ODPE' AS Tabla, COUNT(*) AS Filas FROM dbo.ODPE
UNION ALL SELECT N'Usuario', COUNT(*) FROM dbo.Usuario
UNION ALL SELECT N'MiembroMesa', COUNT(*) FROM dbo.MiembroMesa
UNION ALL SELECT N'Capacitador', COUNT(*) FROM dbo.Capacitador
UNION ALL SELECT N'SesionCapacitacion', COUNT(*) FROM dbo.SesionCapacitacion
UNION ALL SELECT N'AsignacionSesion', COUNT(*) FROM dbo.AsignacionSesion
UNION ALL SELECT N'Asistencia', COUNT(*) FROM dbo.Asistencia
UNION ALL SELECT N'MaterialCapacitacion', COUNT(*) FROM dbo.MaterialCapacitacion
UNION ALL SELECT N'SesionMaterial', COUNT(*) FROM dbo.SesionMaterial;

--Usuarios activos con rol Capacitador o MiembroMesa sin registro vinculado
SELECT u.IdUsuario, u.Username, u.Rol
FROM dbo.Usuario u
WHERE u.Estado = N'1'
  AND ((u.Rol = N'Capacitador'
        AND NOT EXISTS (SELECT 1 FROM dbo.Capacitador c WHERE c.IdUsuario = u.IdUsuario))
    OR (u.Rol = N'MiembroMesa'
        AND NOT EXISTS (SELECT 1 FROM dbo.MiembroMesa m WHERE m.IdUsuario = u.IdUsuario)));
GO

--------VISTAS PARA DASHBOARDS Y CONSULTA PÚBLICA--------
-- Vista de consulta pública: solo los datos mínimos (Ley N.° 29733)
CREATE OR ALTER VIEW dbo.vw_ConsultaMiembro
AS
SELECT m.Dni, m.Nombres, m.Apellidos, m.Cargo, m.EstadoCapacitacion,
       o.NombreOdpe, o.Region
FROM dbo.MiembroMesa m
INNER JOIN dbo.ODPE o ON o.IdOdpe = m.IdOdpe;
GO

-- Participantes de cada sesión con su estado de asistencia
CREATE OR ALTER VIEW dbo.vw_ParticipantesSesion
AS
SELECT a.IdSesion, s.Sede, s.FechaHora, s.Modalidad,
       m.Dni, m.Apellidos, m.Nombres, m.Cargo,
       x.IdAsistencia, x.Asistio, x.FechaRegistro, x.Observacion
FROM dbo.AsignacionSesion a
INNER JOIN dbo.SesionCapacitacion s ON s.IdSesion = a.IdSesion
INNER JOIN dbo.MiembroMesa m ON m.Dni = a.DniMiembroMesa
LEFT JOIN dbo.Asistencia x
       ON x.IdSesion = a.IdSesion AND x.DniMiembroMesa = a.DniMiembroMesa;
GO

-- Indicadores de asistencia por sesión
CREATE OR ALTER VIEW dbo.vw_AsistenciaPorSesion
AS
SELECT s.IdSesion, s.Sede, s.FechaHora, s.Modalidad,
       COUNT(*) AS TotalAsignados,
       SUM(CASE WHEN x.Asistio = N'1' THEN 1 ELSE 0 END) AS Asistieron,
       SUM(CASE WHEN x.Asistio = N'0' THEN 1 ELSE 0 END) AS NoAsistieron,
       SUM(CASE WHEN x.IdAsistencia IS NULL THEN 1 ELSE 0 END) AS SinRegistrar,
       CAST(100.0 * SUM(CASE WHEN x.Asistio = N'1' THEN 1 ELSE 0 END)
            / NULLIF(COUNT(*), 0) AS DECIMAL(5,2)) AS PorcentajeAsistencia
FROM dbo.SesionCapacitacion s
INNER JOIN dbo.AsignacionSesion a ON a.IdSesion = s.IdSesion
LEFT JOIN dbo.Asistencia x
       ON x.IdSesion = a.IdSesion AND x.DniMiembroMesa = a.DniMiembroMesa
GROUP BY s.IdSesion, s.Sede, s.FechaHora, s.Modalidad;
GO

-- Cumplimiento de capacitación por ODPE
CREATE OR ALTER VIEW dbo.vw_CumplimientoPorOdpe
AS
SELECT o.Region, o.IdOdpe, o.NombreOdpe,
       COUNT(m.Dni) AS TotalMiembros,
       SUM(CASE WHEN m.EstadoCapacitacion = N'Capacitado' THEN 1 ELSE 0 END) AS Capacitados,
       SUM(CASE WHEN m.EstadoCapacitacion = N'Ausente' THEN 1 ELSE 0 END) AS Ausentes,
       CAST(100.0 * SUM(CASE WHEN m.EstadoCapacitacion = N'Capacitado' THEN 1 ELSE 0 END)
            / NULLIF(COUNT(m.Dni), 0) AS DECIMAL(5,2)) AS PorcentajeCumplimiento
FROM dbo.ODPE o
LEFT JOIN dbo.MiembroMesa m ON m.IdOdpe = o.IdOdpe
GROUP BY o.Region, o.IdOdpe, o.NombreOdpe;
GO

-- Materiales vigentes por sesión (lo que ve el ciudadano)
CREATE OR ALTER VIEW dbo.vw_MaterialVigentePorSesion
AS
SELECT sm.IdSesion, mat.IdMaterial, mat.Titulo, mat.Tipo, mat.UrlRecurso
FROM dbo.SesionMaterial sm
INNER JOIN dbo.MaterialCapacitacion mat ON mat.IdMaterial = sm.IdMaterial
WHERE mat.Activo = N'1';
GO

-- Ejemplos de uso de las vistas
SELECT * FROM dbo.vw_ConsultaMiembro WHERE Dni = N'70000001';
SELECT * FROM dbo.vw_ParticipantesSesion WHERE IdSesion = 1 ORDER BY Apellidos;
SELECT * FROM dbo.vw_AsistenciaPorSesion ORDER BY PorcentajeAsistencia;
SELECT * FROM dbo.vw_CumplimientoPorOdpe ORDER BY PorcentajeCumplimiento DESC;
SELECT * FROM dbo.vw_MaterialVigentePorSesion WHERE IdSesion = 1;
GO

--------MEDICIÓN DE RENDIMIENTO--------
SET STATISTICS IO ON;
SET STATISTICS TIME ON;

-- Consulta por DNI: datos del miembro y sus sesiones asignadas
SELECT * FROM dbo.vw_ConsultaMiembro WHERE Dni = N'70000001';

SELECT s.IdSesion, s.Sede, s.Direccion, s.FechaHora, s.Modalidad
FROM dbo.AsignacionSesion a
INNER JOIN dbo.SesionCapacitacion s ON s.IdSesion = a.IdSesion
WHERE a.DniMiembroMesa = N'70000001'
ORDER BY s.FechaHora;

SET STATISTICS IO OFF;
SET STATISTICS TIME OFF;
GO

--------ANÁLISIS OPERATIVO Y DE GESTIÓN--------
--Indicadores nacionales en una sola fila (tarjetas del dashboard)
WITH M AS
(
    SELECT COUNT(*) AS TotalMiembros,
           SUM(CASE WHEN EstadoCapacitacion = N'Capacitado' THEN 1 ELSE 0 END) AS Capacitados
    FROM dbo.MiembroMesa
),
S AS (SELECT COUNT(*) AS TotalSesiones FROM dbo.SesionCapacitacion),
A AS (SELECT COUNT(*) AS TotalAsignaciones FROM dbo.AsignacionSesion),
X AS (SELECT SUM(CASE WHEN Asistio = N'1' THEN 1 ELSE 0 END) AS AsistenciasPositivas
      FROM dbo.Asistencia)
SELECT M.TotalMiembros,
       ISNULL(M.Capacitados, 0) AS Capacitados,
       CAST(100.0 * ISNULL(M.Capacitados, 0) / NULLIF(M.TotalMiembros, 0) AS DECIMAL(5,2)) AS PorcentajeCumplimiento,
       S.TotalSesiones,
       A.TotalAsignaciones,
       ISNULL(X.AsistenciasPositivas, 0) AS AsistenciasPositivas,
       CAST(100.0 * ISNULL(X.AsistenciasPositivas, 0) / NULLIF(A.TotalAsignaciones, 0) AS DECIMAL(5,2)) AS PorcentajeAsistencia
FROM M CROSS JOIN S CROSS JOIN A CROSS JOIN X;

--Asistencia según modalidad (presencial frente a virtual)
SELECT s.Modalidad,
       COUNT(*) AS TotalAsignados,
       SUM(CASE WHEN x.Asistio = N'1' THEN 1 ELSE 0 END) AS Asistieron,
       CAST(100.0 * SUM(CASE WHEN x.Asistio = N'1' THEN 1 ELSE 0 END)
            / NULLIF(COUNT(*), 0) AS DECIMAL(5,2)) AS PorcentajeAsistencia
FROM dbo.SesionCapacitacion s
INNER JOIN dbo.AsignacionSesion a ON a.IdSesion = s.IdSesion
LEFT JOIN dbo.Asistencia x
       ON x.IdSesion = a.IdSesion AND x.DniMiembroMesa = a.DniMiembroMesa
GROUP BY s.Modalidad
ORDER BY s.Modalidad;

--Asistencia según el cargo del miembro (Presidente, Secretario, Vocal, etc.)
SELECT ISNULL(m.Cargo, N'Sin cargo') AS Cargo,
       COUNT(*) AS TotalAsignados,
       SUM(CASE WHEN x.Asistio = N'1' THEN 1 ELSE 0 END) AS Asistieron,
       CAST(100.0 * SUM(CASE WHEN x.Asistio = N'1' THEN 1 ELSE 0 END)
            / NULLIF(COUNT(*), 0) AS DECIMAL(5,2)) AS PorcentajeAsistencia
FROM dbo.AsignacionSesion a
INNER JOIN dbo.MiembroMesa m ON m.Dni = a.DniMiembroMesa
LEFT JOIN dbo.Asistencia x
       ON x.IdSesion = a.IdSesion AND x.DniMiembroMesa = a.DniMiembroMesa
GROUP BY ISNULL(m.Cargo, N'Sin cargo')
ORDER BY PorcentajeAsistencia DESC;

--Estado de capacitación por ODPE 
SELECT o.Region, o.NombreOdpe,
       SUM(CASE WHEN m.EstadoCapacitacion = N'Designado'      THEN 1 ELSE 0 END) AS Designado,
       SUM(CASE WHEN m.EstadoCapacitacion = N'AsignadoSesion' THEN 1 ELSE 0 END) AS AsignadoSesion,
       SUM(CASE WHEN m.EstadoCapacitacion = N'EnCapacitacion' THEN 1 ELSE 0 END) AS EnCapacitacion,
       SUM(CASE WHEN m.EstadoCapacitacion = N'Capacitado'     THEN 1 ELSE 0 END) AS Capacitado,
       SUM(CASE WHEN m.EstadoCapacitacion = N'Ausente'        THEN 1 ELSE 0 END) AS Ausente,
       COUNT(m.Dni) AS Total
FROM dbo.ODPE o
LEFT JOIN dbo.MiembroMesa m ON m.IdOdpe = o.IdOdpe
GROUP BY o.Region, o.NombreOdpe
ORDER BY o.Region, o.NombreOdpe;

--Conflictos de horario de un capacitador (dos sesiones a menos de 120 minutos)
--     Ajusta 120 a la duración real de la sesión.
SELECT c.CodigoCapacitador,
       s1.IdSesion AS Sesion1, s1.FechaHora AS FechaHora1,
       s2.IdSesion AS Sesion2, s2.FechaHora AS FechaHora2
FROM dbo.SesionCapacitacion s1
INNER JOIN dbo.SesionCapacitacion s2
        ON s2.IdCapacitador = s1.IdCapacitador
       AND s2.IdSesion > s1.IdSesion
       AND ABS(DATEDIFF(MINUTE, s1.FechaHora, s2.FechaHora)) < 120
INNER JOIN dbo.Capacitador c ON c.IdCapacitador = s1.IdCapacitador
ORDER BY s1.FechaHora;

--Sesiones presenciales en la misma sede con horarios cercanos (verificar disponibilidad)
SELECT s1.Sede,
       s1.IdSesion AS Sesion1, s1.FechaHora AS FechaHora1,
       s2.IdSesion AS Sesion2, s2.FechaHora AS FechaHora2
FROM dbo.SesionCapacitacion s1
INNER JOIN dbo.SesionCapacitacion s2
        ON s2.Sede = s1.Sede
       AND s2.IdSesion > s1.IdSesion
       AND ABS(DATEDIFF(MINUTE, s1.FechaHora, s2.FechaHora)) < 120
WHERE s1.Modalidad = N'Presencial' AND s2.Modalidad = N'Presencial'
ORDER BY s1.Sede, s1.FechaHora;

--Miembros asignados a dos sesiones con horarios superpuestos
SELECT m.Dni, m.Apellidos, m.Nombres,
       s1.IdSesion AS Sesion1, s1.FechaHora AS FechaHora1,
       s2.IdSesion AS Sesion2, s2.FechaHora AS FechaHora2
FROM dbo.AsignacionSesion a1
INNER JOIN dbo.AsignacionSesion a2
        ON a2.DniMiembroMesa = a1.DniMiembroMesa AND a2.IdSesion > a1.IdSesion
INNER JOIN dbo.SesionCapacitacion s1 ON s1.IdSesion = a1.IdSesion
INNER JOIN dbo.SesionCapacitacion s2 ON s2.IdSesion = a2.IdSesion
INNER JOIN dbo.MiembroMesa m ON m.Dni = a1.DniMiembroMesa
WHERE ABS(DATEDIFF(MINUTE, s1.FechaHora, s2.FechaHora)) < 120
ORDER BY m.Apellidos;

--Alerta operativa: sesiones de los próximos 7 días con su número de asignados y materiales
SELECT s.IdSesion, s.Sede, s.FechaHora, s.Modalidad,
       COUNT(DISTINCT a.DniMiembroMesa) AS Asignados,
       COUNT(DISTINCT sm.IdMaterial) AS MaterialesAsignados
FROM dbo.SesionCapacitacion s
LEFT JOIN dbo.AsignacionSesion a ON a.IdSesion = s.IdSesion
LEFT JOIN dbo.SesionMaterial sm ON sm.IdSesion = s.IdSesion
WHERE s.FechaHora >= SYSDATETIME()
  AND s.FechaHora < DATEADD(DAY, 7, SYSDATETIME())
GROUP BY s.IdSesion, s.Sede, s.FechaHora, s.Modalidad
ORDER BY s.FechaHora;

--Observaciones más frecuentes registradas en la asistencia
SELECT Observacion, COUNT(*) AS Veces
FROM dbo.Asistencia
WHERE Observacion IS NOT NULL
GROUP BY Observacion
ORDER BY Veces DESC, Observacion;

--historial de un miembro: sus sesiones y el resultado de cada asistencia
SELECT m.Dni, m.Apellidos, m.Nombres,
       s.IdSesion, s.Sede, s.FechaHora, s.Modalidad,
       CASE x.Asistio
            WHEN N'1' THEN N'Asistió'
            WHEN N'0' THEN N'No asistió'
            ELSE N'Sin registrar'
       END AS EstadoAsistencia,
       x.Observacion
FROM dbo.MiembroMesa m
INNER JOIN dbo.AsignacionSesion a ON a.DniMiembroMesa = m.Dni
INNER JOIN dbo.SesionCapacitacion s ON s.IdSesion = a.IdSesion
LEFT JOIN dbo.Asistencia x
       ON x.IdSesion = a.IdSesion AND x.DniMiembroMesa = a.DniMiembroMesa
WHERE m.Dni = N'70000001'
ORDER BY s.FechaHora;

--Avance diario del registro de asistencia (serie de tiempo)
SELECT CAST(FechaRegistro AS DATE) AS Dia,
       COUNT(*) AS Registros,
       SUM(CASE WHEN Asistio = N'1' THEN 1 ELSE 0 END) AS Asistieron,
       SUM(CASE WHEN Asistio = N'0' THEN 1 ELSE 0 END) AS NoAsistieron
FROM dbo.Asistencia
GROUP BY CAST(FechaRegistro AS DATE)
ORDER BY Dia;

--------USUARIOS Y SEGURIDAD--------

--Usuarios por rol y estado
SELECT Rol,
       SUM(CASE WHEN Estado = N'1' THEN 1 ELSE 0 END) AS Activos,
       SUM(CASE WHEN Estado = N'0' THEN 1 ELSE 0 END) AS Inactivos,
       COUNT(*) AS Total
FROM dbo.Usuario
GROUP BY Rol
ORDER BY Rol;

--Usuarios inactivos (eliminación lógica)
SELECT IdUsuario, Username, Rol
FROM dbo.Usuario
WHERE Estado = N'0'
ORDER BY Username;

--Capacitadores que todavía no tienen sesiones asignadas
SELECT c.IdCapacitador, c.CodigoCapacitador, c.Especialidad, u.Username
FROM dbo.Capacitador c
INNER JOIN dbo.Usuario u ON u.IdUsuario = c.IdUsuario
WHERE NOT EXISTS (SELECT 1 FROM dbo.SesionCapacitacion s WHERE s.IdCapacitador = c.IdCapacitador);

--Datos de sesión de un usuario activo (sin devolver PasswordHash; el hash lo valida la API)
SELECT u.IdUsuario, u.Username, u.Rol,
       c.IdCapacitador, m.Dni AS DniMiembroMesa
FROM dbo.Usuario u
LEFT JOIN dbo.Capacitador c ON c.IdUsuario = u.IdUsuario
LEFT JOIN dbo.MiembroMesa m ON m.IdUsuario = u.IdUsuario
WHERE u.Username = N'coordinador01'
  AND u.Estado = N'1';

--Contraseñas que no parecen un hash real (menos de 32 caracteres).
--     Con los datos de prueba aparecerán a propósito; en producción debe devolver 0 filas.
SELECT IdUsuario, Username, LEN(PasswordHash) AS LongitudHash
FROM dbo.Usuario
WHERE LEN(PasswordHash) < 32;
GO

--------VERIFICACIÓN DEL ESQUEMA Y DE LAS REGLAS  (evidencia para el informe)--------
--Relaciones entre tablas (claves foráneas): útil para documentar el modelo físico
SELECT fk.name AS Restriccion,
       OBJECT_NAME(fk.parent_object_id) AS TablaHija,
       COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS ColumnaHija,
       OBJECT_NAME(fk.referenced_object_id) AS TablaPadre,
       COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS ColumnaPadre
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
ORDER BY TablaHija, Restriccion, fkc.constraint_column_id;

--Diccionario de datos: columnas, tipos y obligatoriedad de cada tabla
SELECT c.TABLE_NAME AS Tabla, c.ORDINAL_POSITION AS Orden, c.COLUMN_NAME AS Columna,
       c.DATA_TYPE AS Tipo, c.CHARACTER_MAXIMUM_LENGTH AS Longitud, c.IS_NULLABLE AS AceptaNulos
FROM INFORMATION_SCHEMA.COLUMNS c
INNER JOIN INFORMATION_SCHEMA.TABLES t
        ON t.TABLE_SCHEMA = c.TABLE_SCHEMA
       AND t.TABLE_NAME = c.TABLE_NAME
       AND t.TABLE_TYPE = N'BASE TABLE'
WHERE c.TABLE_SCHEMA = N'dbo'
ORDER BY c.TABLE_NAME, c.ORDINAL_POSITION;

--Requisito de caracteres Unicode
SELECT c.TABLE_NAME AS Tabla, c.COLUMN_NAME AS Columna, c.DATA_TYPE AS Tipo
FROM INFORMATION_SCHEMA.COLUMNS c
INNER JOIN INFORMATION_SCHEMA.TABLES t
        ON t.TABLE_SCHEMA = c.TABLE_SCHEMA
       AND t.TABLE_NAME = c.TABLE_NAME
       AND t.TABLE_TYPE = N'BASE TABLE'
WHERE c.TABLE_SCHEMA = N'dbo'
  AND c.DATA_TYPE IN (N'char', N'varchar', N'text');

--Restricciones CHECK y UNIQUE definidas (reglas de negocio en la base de datos)
SELECT OBJECT_NAME(cc.parent_object_id) AS Tabla, cc.name AS Restriccion,
       N'CHECK' AS Tipo, cc.[definition] AS Definicion
FROM sys.check_constraints cc
UNION ALL
SELECT OBJECT_NAME(kc.parent_object_id), kc.name, N'UNIQUE', NULL
FROM sys.key_constraints kc
WHERE kc.type = N'UQ'
ORDER BY Tabla, Tipo, Restriccion;

-- no debe existir asistencia de personas no asignadas a la sesión (0 filas)
SELECT x.IdAsistencia, x.DniMiembroMesa, x.IdSesion
FROM dbo.Asistencia x
WHERE NOT EXISTS (SELECT 1 FROM dbo.AsignacionSesion a
                  WHERE a.IdSesion = x.IdSesion AND a.DniMiembroMesa = x.DniMiembroMesa);

--No debe haber asistencia duplicada para un mismo miembro y sesión (0 filas)
SELECT DniMiembroMesa, IdSesion, COUNT(*) AS Registros
FROM dbo.Asistencia
GROUP BY DniMiembroMesa, IdSesion
HAVING COUNT(*) > 1;
GO
