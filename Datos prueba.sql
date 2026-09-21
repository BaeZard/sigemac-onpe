USE SistemaCapacitacion;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.usp_CargarDatosPrueba
    @Reiniciar BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

   --------Reinicio opcional --------
    IF @Reiniciar = 1
    BEGIN
        DELETE FROM dbo.Asistencia;
        DELETE FROM dbo.AsignacionSesion;
        DELETE FROM dbo.SesionMaterial;
        DELETE FROM dbo.SesionCapacitacion;
        DELETE FROM dbo.MaterialCapacitacion;
        DELETE FROM dbo.MiembroMesa;
        DELETE FROM dbo.Capacitador;
        DELETE FROM dbo.Usuario;
        DELETE FROM dbo.ODPE;

        DECLARE @Tabla NVARCHAR(128), @Sql NVARCHAR(400);
        DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
            SELECT OBJECT_NAME(object_id)
            FROM sys.identity_columns
            WHERE OBJECT_SCHEMA_NAME(object_id) = N'dbo'
              AND last_value IS NOT NULL;
        OPEN cur;
        FETCH NEXT FROM cur INTO @Tabla;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            SET @Sql = N'DBCC CHECKIDENT (N''dbo.' + @Tabla + N''', RESEED, 0) WITH NO_INFOMSGS;';
            EXEC sys.sp_executesql @Sql;
            FETCH NEXT FROM cur INTO @Tabla;
        END
        CLOSE cur;
        DEALLOCATE cur;
    END
    ELSE IF EXISTS (SELECT 1 FROM dbo.ODPE)
    BEGIN
        PRINT N'Ya existen datos en ODPE. No se insertó nada. Usa @Reiniciar = 1 para borrar y volver a cargar.';
        RETURN;
    END

    /* ---- Variables ---- */
    DECLARE @Ses1 INT, @Ses2 INT, @Ses3 INT, @Ses4 INT;
    DECLARE @Mat1 INT, @Mat2 INT, @Mat3 INT, @Mat4 INT, @Mat5 INT;
    DECLARE @Hoy DATETIME2(0) = CAST(CAST(SYSDATETIME() AS DATE) AS DATETIME2(0));
    DECLARE @FechaSes1 DATETIME2(0) = DATEADD(HOUR, 9,  DATEADD(DAY, -7, @Hoy));   -- pasada
    DECLARE @FechaSes2 DATETIME2(0) = DATEADD(HOUR, 15, DATEADD(DAY, -3, @Hoy));   -- pasada
    DECLARE @FechaSes3 DATETIME2(0) = DATEADD(HOUR, 9,  DATEADD(DAY,  5, @Hoy));   -- futura
    DECLARE @FechaSes4 DATETIME2(0) = DATEADD(HOUR, 10, DATEADD(DAY,  8, @Hoy));   -- futura

    BEGIN TRY
        BEGIN TRANSACTION;

        -------------------------------- ODPE -------------------------------- 
        INSERT INTO dbo.ODPE (NombreOdpe, Region, Direccion)
        VALUES (N'ODPE LIMA CENTRO 1', N'Lima',     N'Jr. de la Unión 100'),
               (N'ODPE AREQUIPA',      N'Arequipa', N'Av. Ejército 200'),
               (N'ODPE TUMBES',        N'Tumbes',   N'Av. Tumbes Norte 300'),
               (N'ODPE UCAYALI',       N'Ucayali',  N'Jr. Tarapacá 400');

        /* ------------------------------- Usuarios ------------------------------ */
        -- Hash ficticio: la API debe generar el hash real
        INSERT INTO dbo.Usuario (Username, PasswordHash, Rol, Estado)
        VALUES (N'admin01',         N'HASH_DE_PRUEBA_NO_USAR', N'Administrador', N'1'),
               (N'coordinador01',   N'HASH_DE_PRUEBA_NO_USAR', N'Coordinador',   N'1'),
               (N'capacitador01',   N'HASH_DE_PRUEBA_NO_USAR', N'Capacitador',   N'1'),
               (N'capacitador02',   N'HASH_DE_PRUEBA_NO_USAR', N'Capacitador',   N'1'),
               (N'capacitador03',   N'HASH_DE_PRUEBA_NO_USAR', N'Capacitador',   N'1'),
               (N'miembro70000001', N'HASH_DE_PRUEBA_NO_USAR', N'MiembroMesa',   N'1');

        /* ----------------------------- Capacitadores --------------------------- */
        INSERT INTO dbo.Capacitador (CodigoCapacitador, Especialidad, IdUsuario)
        SELECT v.Codigo, v.Especialidad, u.IdUsuario
        FROM (VALUES (N'CAP-001', N'Procesos electorales', N'capacitador01'),
                     (N'CAP-002', N'Material digital',     N'capacitador02'),
                     (N'CAP-003', N'Escrutinio',           N'capacitador03'))
             AS v (Codigo, Especialidad, Username)
        INNER JOIN dbo.Usuario u ON u.Username = v.Username;

        /* ------------- Miembros de mesa (en producción llegan por ETL) --------- */
        -- El estado ya refleja lo que pasó con cada uno (asistencia, reagendamiento, etc.)
        INSERT INTO dbo.MiembroMesa (Dni, Nombres, Apellidos, Cargo, EstadoCapacitacion, IdOdpe, IdUsuario)
        SELECT v.Dni, v.Nombres, v.Apellidos, v.Cargo, v.Estado, o.IdOdpe, u.IdUsuario
        FROM (VALUES
              -- Lima
              (N'70000001', N'Ana María',    N'Quispe Rojas',     N'Presidente', N'Capacitado',     N'ODPE LIMA CENTRO 1', N'miembro70000001'),
              (N'70000002', N'Luis Alberto', N'Ramírez Soto',     N'Secretario', N'Capacitado',     N'ODPE LIMA CENTRO 1', NULL),
              (N'70000003', N'Rosa',         N'Huamán Ñahui',     N'Vocal',      N'AsignadoSesion', N'ODPE LIMA CENTRO 1', NULL),   -- ausente en la sesión 1, reagendada a la 4
              (N'70000006', N'Mario',        N'Torres Díaz',      N'Vocal',      N'Capacitado',     N'ODPE LIMA CENTRO 1', NULL),
              -- Arequipa
              (N'70000004', N'Pedro',        N'Mamani Cutipa',    N'Presidente', N'Capacitado',     N'ODPE AREQUIPA',      NULL),
              (N'70000005', N'Carmen',       N'Salas Vega',       N'Vocal',      N'Capacitado',     N'ODPE AREQUIPA',      NULL),
              (N'70000007', N'Julia',        N'Condori Apaza',    N'Secretario', N'Ausente',        N'ODPE AREQUIPA',      NULL),   -- por reagendar
              (N'70000008', N'Raúl',         N'Flores Quispe',    N'Vocal',      N'EnCapacitacion', N'ODPE AREQUIPA',      NULL),   -- asistencia sin registrar
              -- Tumbes
              (N'70000009', N'Sofía',        N'Ramos Chero',      N'Presidente', N'AsignadoSesion', N'ODPE TUMBES',        NULL),
              (N'70000010', N'Jorge',        N'Peña Zapata',      N'Secretario', N'AsignadoSesion', N'ODPE TUMBES',        NULL),
              (N'70000011', N'Lucía',        N'Ordinola Nima',    N'Vocal',      N'AsignadoSesion', N'ODPE TUMBES',        NULL),
              (N'70000012', N'Diego',        N'Cruz Pingo',       N'Vocal',      N'AsignadoSesion', N'ODPE TUMBES',        NULL),
              -- Ucayali
              (N'70000013', N'Marta',        N'Sánchez Pinedo',   N'Presidente', N'AsignadoSesion', N'ODPE UCAYALI',       NULL),
              (N'70000014', N'Hugo',         N'Vásquez Tuesta',   N'Secretario', N'AsignadoSesion', N'ODPE UCAYALI',       NULL),
              (N'70000015', N'Elena',        N'Rengifo Shahuano', N'Vocal',      N'AsignadoSesion', N'ODPE UCAYALI',       NULL),
              (N'70000016', N'Tomás',        N'Cárdenas Ruiz',    N'Vocal',      N'Designado',      N'ODPE UCAYALI',       NULL)    -- sin sesión
             ) AS v (Dni, Nombres, Apellidos, Cargo, Estado, Odpe, Usuario)
        INNER JOIN dbo.ODPE o ON o.NombreOdpe = v.Odpe
        LEFT JOIN dbo.Usuario u ON u.Username = v.Usuario;

        /* ---------------------- Sesiones de capacitación ----------------------- */
        -- Las dos primeras ya ocurrieron (fecha pasada); las otras dos son futuras.
        INSERT INTO dbo.SesionCapacitacion (Sede, Direccion, FechaHora, Modalidad, IdCapacitador)
        VALUES (N'Colegio Nacional Lima', N'Av. Abancay 500', @FechaSes1, N'Presencial',
                (SELECT IdCapacitador FROM dbo.Capacitador WHERE CodigoCapacitador = N'CAP-001'));
        SET @Ses1 = SCOPE_IDENTITY();

        INSERT INTO dbo.SesionCapacitacion (Sede, Direccion, FechaHora, Modalidad, IdCapacitador)
        VALUES (N'Sala virtual Zoom', NULL, @FechaSes2, N'Virtual',
                (SELECT IdCapacitador FROM dbo.Capacitador WHERE CodigoCapacitador = N'CAP-002'));
        SET @Ses2 = SCOPE_IDENTITY();

        INSERT INTO dbo.SesionCapacitacion (Sede, Direccion, FechaHora, Modalidad, IdCapacitador)
        VALUES (N'I.E. N.° 001 Tumbes', N'Calle Bolognesi 120', @FechaSes3, N'Presencial',
                (SELECT IdCapacitador FROM dbo.Capacitador WHERE CodigoCapacitador = N'CAP-003'));
        SET @Ses3 = SCOPE_IDENTITY();

        INSERT INTO dbo.SesionCapacitacion (Sede, Direccion, FechaHora, Modalidad, IdCapacitador)
        VALUES (N'Sala virtual Zoom', NULL, @FechaSes4, N'Virtual',
                (SELECT IdCapacitador FROM dbo.Capacitador WHERE CodigoCapacitador = N'CAP-002'));
        SET @Ses4 = SCOPE_IDENTITY();

        /* ----------------------------- Materiales ------------------------------ */
        INSERT INTO dbo.MaterialCapacitacion (Titulo, Tipo, UrlRecurso, Activo)
        VALUES (N'Manual del Miembro de Mesa', N'Manual', N'https://ejemplo.onpe.gob.pe/materiales/manual.pdf', N'1');
        SET @Mat1 = SCOPE_IDENTITY();
        INSERT INTO dbo.MaterialCapacitacion (Titulo, Tipo, UrlRecurso, Activo)
        VALUES (N'Guía rápida de instalación', N'Guía', N'https://ejemplo.onpe.gob.pe/materiales/guia.pdf', N'1');
        SET @Mat2 = SCOPE_IDENTITY();
        INSERT INTO dbo.MaterialCapacitacion (Titulo, Tipo, UrlRecurso, Activo)
        VALUES (N'Video: Cómo votar', N'Video', N'https://ejemplo.onpe.gob.pe/materiales/video-votar', N'1');
        SET @Mat3 = SCOPE_IDENTITY();
        INSERT INTO dbo.MaterialCapacitacion (Titulo, Tipo, UrlRecurso, Activo)
        VALUES (N'Manual 2021 (obsoleto)', N'Manual', N'https://ejemplo.onpe.gob.pe/materiales/manual-2021.pdf', N'0');
        SET @Mat4 = SCOPE_IDENTITY();
        INSERT INTO dbo.MaterialCapacitacion (Titulo, Tipo, UrlRecurso, Activo)
        VALUES (N'Cartilla del escrutinio', N'Guía', N'https://ejemplo.onpe.gob.pe/materiales/cartilla.pdf', N'1');
        SET @Mat5 = SCOPE_IDENTITY();   -- queda sin asignar a ninguna sesión a propósito

        INSERT INTO dbo.SesionMaterial (IdSesion, IdMaterial)
        VALUES (@Ses1, @Mat1), (@Ses1, @Mat2), (@Ses1, @Mat4),   -- @Mat4 está inactivo: no debe mostrarse al ciudadano
               (@Ses2, @Mat1), (@Ses2, @Mat3),
               (@Ses3, @Mat1), (@Ses3, @Mat2),
               (@Ses4, @Mat1), (@Ses4, @Mat3);

        /* ------------------- Asignación de miembros a sesiones ------------------ */
        INSERT INTO dbo.AsignacionSesion (IdSesion, DniMiembroMesa)
        VALUES (@Ses1, N'70000001'), (@Ses1, N'70000002'), (@Ses1, N'70000003'), (@Ses1, N'70000006'),
               (@Ses2, N'70000004'), (@Ses2, N'70000005'), (@Ses2, N'70000007'), (@Ses2, N'70000008'),
               (@Ses3, N'70000009'), (@Ses3, N'70000010'), (@Ses3, N'70000011'), (@Ses3, N'70000012'),
               (@Ses4, N'70000013'), (@Ses4, N'70000014'), (@Ses4, N'70000015'),
               (@Ses4, N'70000003');   -- reagendada tras ausentarse en la sesión 1

        /* --------------------------- Asistencia registrada ---------------------- */
        -- Asistio: N'1' = asistió, N'0' = no asistió.
        -- En la sesión 2 queda sin registrar 70000008 (por eso su estado es EnCapacitacion).
        INSERT INTO dbo.Asistencia (DniMiembroMesa, IdSesion, FechaRegistro, Asistio, Observacion)
        VALUES (N'70000001', @Ses1, DATEADD(HOUR, 3, @FechaSes1), N'1', N'Puntual'),
               (N'70000002', @Ses1, DATEADD(HOUR, 3, @FechaSes1), N'1', NULL),
               (N'70000006', @Ses1, DATEADD(HOUR, 3, @FechaSes1), N'1', NULL),
               (N'70000003', @Ses1, DATEADD(HOUR, 3, @FechaSes1), N'0', N'Sin justificar'),
               (N'70000004', @Ses2, DATEADD(HOUR, 3, @FechaSes2), N'1', NULL),
               (N'70000005', @Ses2, DATEADD(HOUR, 3, @FechaSes2), N'1', N'Se conectó tarde'),
               (N'70000007', @Ses2, DATEADD(HOUR, 3, @FechaSes2), N'0', N'Falla de conexión');

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH

    PRINT N'Datos de prueba insertados correctamente.';

    -- Resumen de lo cargado
    SELECT N'ODPE' AS Tabla, COUNT(*) AS Filas FROM dbo.ODPE
    UNION ALL SELECT N'Usuario', COUNT(*) FROM dbo.Usuario
    UNION ALL SELECT N'Capacitador', COUNT(*) FROM dbo.Capacitador
    UNION ALL SELECT N'MiembroMesa', COUNT(*) FROM dbo.MiembroMesa
    UNION ALL SELECT N'SesionCapacitacion', COUNT(*) FROM dbo.SesionCapacitacion
    UNION ALL SELECT N'AsignacionSesion', COUNT(*) FROM dbo.AsignacionSesion
    UNION ALL SELECT N'Asistencia', COUNT(*) FROM dbo.Asistencia
    UNION ALL SELECT N'MaterialCapacitacion', COUNT(*) FROM dbo.MaterialCapacitacion
    UNION ALL SELECT N'SesionMaterial', COUNT(*) FROM dbo.SesionMaterial;
END
GO

--------CARGAR LOS DATOS--------
EXEC dbo.usp_CargarDatosPrueba @Reiniciar = 1;
GO

--------VERIFICACIÓN RÁPIDA (cada consulta se puede ejecutar por separado) Más consultas en 04_Consultas.sql--------
-- Miembros con su ODPE y su estado
SELECT m.Dni, m.Apellidos, m.Nombres, m.Cargo, o.NombreOdpe, m.EstadoCapacitacion
FROM dbo.MiembroMesa m
INNER JOIN dbo.ODPE o ON o.IdOdpe = m.IdOdpe
ORDER BY o.NombreOdpe, m.Apellidos;

-- Sesiones con su capacitador y número de participantes
SELECT s.IdSesion, s.Sede, s.FechaHora, s.Modalidad, c.CodigoCapacitador,
       (SELECT COUNT(*) FROM dbo.AsignacionSesion a WHERE a.IdSesion = s.IdSesion) AS Participantes
FROM dbo.SesionCapacitacion s
INNER JOIN dbo.Capacitador c ON c.IdCapacitador = s.IdCapacitador
ORDER BY s.FechaHora;

-- Participantes de la sesión 1 con su asistencia
SELECT m.Dni, m.Apellidos, m.Nombres,
       CASE x.Asistio WHEN N'1' THEN N'Asistió' WHEN N'0' THEN N'No asistió' ELSE N'Sin registrar' END AS EstadoAsistencia
FROM dbo.AsignacionSesion a
INNER JOIN dbo.MiembroMesa m ON m.Dni = a.DniMiembroMesa
LEFT JOIN dbo.Asistencia x ON x.IdSesion = a.IdSesion AND x.DniMiembroMesa = a.DniMiembroMesa
WHERE a.IdSesion = 1
ORDER BY m.Apellidos;
GO

--------PRUEBAS DE REGLAS DE LA BASE DE DATOS --------
  
-- Error 547 (FOREIGN KEY): asistencia de alguien no asignado a la sesión (HU-03)
-- INSERT INTO dbo.Asistencia (DniMiembroMesa, IdSesion, Asistio) VALUES (N'70000001', 3, N'1');

-- Error 2627 (UNIQUE / PRIMARY KEY): asignación duplicada
-- INSERT INTO dbo.AsignacionSesion (IdSesion, DniMiembroMesa) VALUES (1, N'70000001');

-- Error 547 (CHECK): DNI con formato inválido
-- INSERT INTO dbo.MiembroMesa (Dni, Nombres, Apellidos, IdOdpe) VALUES (N'ABC123', N'Prueba', N'Prueba', 1);

-- Error 547 (CHECK): estado de capacitación inválido
-- UPDATE dbo.MiembroMesa SET EstadoCapacitacion = N'Inexistente' WHERE Dni = N'70000001';

-- Error 547 (FOREIGN KEY): no se puede borrar una ODPE que tiene miembros de mesa
-- DELETE FROM dbo.ODPE WHERE IdOdpe = 1;
GO
