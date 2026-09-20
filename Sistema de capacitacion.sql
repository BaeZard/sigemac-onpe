USE master;
GO

IF DB_ID(N'SistemaCapacitacion') IS NULL
	CREATE DATABASE SistemaCapacitacion;
GO 

USE SistemaCapacitacion;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

--------TABLA DE ODPE--------
IF OBJECT_ID(N'dbo.ODPE',N'U') IS NULL
BEGIN
	CREATE TABLE dbo.ODPE
	(
		IdOdpe INT IDENTITY(1,1) NOT NULL,
		NombreOdpe NVARCHAR(50) NOT NULL,
		Region NVARCHAR(50) NOT NULL,
		Direccion NVARCHAR(50) NULL,

		CONSTRAINT PK_ODPE PRIMARY KEY CLUSTERED (IdOdpe),
		CONSTRAINT UQ_ODPE_NombreOdpe UNIQUE (NombreOdpe)
	);
END 
GO

--------TABLA DE USUARIO--------
IF OBJECT_ID(N'dbo.Usuario', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuario
    (
        IdUsuario INT IDENTITY(1,1) NOT NULL,
        Username NVARCHAR(50) NOT NULL,
        PasswordHash NVARCHAR(255) NOT NULL,   -- ampliado: 50 no alcanza para un hash
        Rol NVARCHAR(50) NOT NULL,
        Estado NVARCHAR(1) NOT NULL
            CONSTRAINT DF_Usuario_Estado DEFAULT (N'1'),

        CONSTRAINT PK_Usuario PRIMARY KEY CLUSTERED (IdUsuario),
        CONSTRAINT UQ_Usuario_Username UNIQUE (Username),
        CONSTRAINT CK_Usuario_Estado CHECK (Estado IN (N'0', N'1')),
        CONSTRAINT CK_Usuario_Rol CHECK (Rol IN (N'Administrador', N'Coordinador', N'Capacitador', N'PersonalODPE',  N'PersonalTI',  N'MiembroMesa'))
    );
END
GO

--------TABLA DE MIEMBRO DE MESA--------
IF OBJECT_ID(N'dbo.MiembroMesa', N'U') IS NULL
BEGIN
	CREATE TABLE DBO.MiembroMesa
	(
		Dni NVARCHAR(10) NOT NULL,
        Nombres NVARCHAR(50) NOT NULL,
        Apellidos NVARCHAR(50) NOT NULL,
        Cargo NVARCHAR(50) NULL,
        EstadoCapacitacion  NVARCHAR(50)  NOT NULL
            CONSTRAINT DF_MiembroMesa_EstadoCapacitacion DEFAULT (N'Designado'),
        IdOdpe INT NOT NULL,
        IdUsuario INT NULL, 

        CONSTRAINT PK_MiembroMesa PRIMARY KEY CLUSTERED (Dni),
        CONSTRAINT FK_MiembroMesa_ODPE
            FOREIGN KEY (IdOdpe) REFERENCES dbo.ODPE (IdOdpe),
        CONSTRAINT FK_MiembroMesa_Usuario
            FOREIGN KEY (IdUsuario) REFERENCES dbo.Usuario (IdUsuario),
        CONSTRAINT CK_MiembroMesa_Dni
            CHECK (LEN(Dni) = 8 AND Dni NOT LIKE N'%[^0-9]%'),
        CONSTRAINT CK_MiembroMesa_EstadoCapacitacion CHECK (EstadoCapacitacion IN
            (N'Designado', N'AsignadoSesion', N'EnCapacitacion', N'Capacitado', N'Ausente'))
	);
END 
GO

--------TABLA DE CAPACITADOR--------
IF OBJECT_ID(N'dbo.Capacitador', N'U') IS NULL
BEGIN
	CREATE TABLE dbo.Capacitador
	(
		IdCapacitador INT IDENTITY(1,1) NOT NULL,
        CodigoCapacitador NVARCHAR(50) NOT NULL,
        Especialidad NVARCHAR(50) NULL,
        IdUsuario INT NOT NULL,

        CONSTRAINT PK_Capacitador PRIMARY KEY CLUSTERED (IdCapacitador),
        CONSTRAINT UQ_Capacitador_Codigo UNIQUE (CodigoCapacitador),
        CONSTRAINT UQ_Capacitador_IdUsuario UNIQUE (IdUsuario),   
        CONSTRAINT FK_Capacitador_Usuario
			FOREIGN KEY (IdUsuario) REFERENCES dbo.Usuario (IdUsuario)
	);
END 
GO

--------TABLA SESION DE CAPACITACION--------
IF OBJECT_ID(N'dbo.SesionCapacitacion', N'U') IS NULL
BEGIN
	CREATE TABLE dbo.SesionCapacitacion
	(
		IdSesion INT IDENTITY(1,1) NOT NULL,
        Sede NVARCHAR(50) NOT NULL,
        Direccion NVARCHAR(50) NULL,
        FechaHora DATETIME2(0) NOT NULL,
        Modalidad NVARCHAR(50) NOT NULL,
        IdCapacitador INT NOT NULL,

        CONSTRAINT PK_SesionCapacitacion PRIMARY KEY CLUSTERED (IdSesion),
        CONSTRAINT FK_SesionCapacitacion_Capacitador
            FOREIGN KEY (IdCapacitador) REFERENCES dbo.Capacitador (IdCapacitador),
        CONSTRAINT CK_SesionCapacitacion_Modalidad
            CHECK (Modalidad IN (N'Presencial', N'Virtual'))
	);
    CREATE NONCLUSTERED INDEX IX_SesionCapacitacion_IdCapacitador
        ON dbo.SesionCapacitacion (IdCapacitador, FechaHora);
END 
GO

--------TABLA ASIGNACION DE SESION--------
IF OBJECT_ID(N'dbo.AsignacionSesion', N'U') IS NULL
BEGIN
	CREATE TABLE dbo.AsignacionSesion
	(
		IdSesion         INT           NOT NULL,
        DniMiembroMesa   NVARCHAR(10)  NOT NULL,
        FechaAsignacion  DATETIME2(0)  NOT NULL
            CONSTRAINT DF_AsignacionSesion_Fecha DEFAULT (SYSDATETIME()),

        CONSTRAINT PK_AsignacionSesion PRIMARY KEY CLUSTERED (IdSesion, DniMiembroMesa),
        CONSTRAINT FK_AsignacionSesion_Sesion
            FOREIGN KEY (IdSesion) REFERENCES dbo.SesionCapacitacion (IdSesion),
        CONSTRAINT FK_AsignacionSesion_MiembroMesa
            FOREIGN KEY (DniMiembroMesa) REFERENCES dbo.MiembroMesa (Dni)
	);
	CREATE NONCLUSTERED INDEX IX_AsignacionSesion_Dni
        ON dbo.AsignacionSesion (DniMiembroMesa);
END 
GO

--------TABLA DE ASISTENCIA--------
IF OBJECT_ID(N'dbo.Asistencia', N'U') IS NULL
BEGIN
	CREATE TABLE dbo.Asistencia 
	(
		IdAsistencia INT IDENTITY(1,1) NOT NULL,
        DniMiembroMesa NVARCHAR(10) NOT NULL,
        IdSesion INT NOT NULL,
        FechaRegistro DATETIME2(0) NOT NULL
            CONSTRAINT DF_Asistencia_FechaRegistro DEFAULT (SYSDATETIME()),
        Asistio NVARCHAR(1) NOT NULL,
        Observacion NVARCHAR(50) NULL,

        CONSTRAINT PK_Asistencia PRIMARY KEY CLUSTERED (IdAsistencia),
        CONSTRAINT UQ_Asistencia_Miembro_Sesion UNIQUE (DniMiembroMesa, IdSesion),
        CONSTRAINT CK_Asistencia_Asistio CHECK (Asistio IN (N'0', N'1')),
        CONSTRAINT FK_Asistencia_MiembroMesa
            FOREIGN KEY (DniMiembroMesa) REFERENCES dbo.MiembroMesa (Dni),
        CONSTRAINT FK_Asistencia_SesionCapacitacion
            FOREIGN KEY (IdSesion) REFERENCES dbo.SesionCapacitacion (IdSesion),
        -- Solo se puede registrar asistencia de quien está asignado a la sesión (HU-03)
        CONSTRAINT FK_Asistencia_AsignacionSesion
            FOREIGN KEY (IdSesion, DniMiembroMesa)
            REFERENCES dbo.AsignacionSesion (IdSesion, DniMiembroMesa)
    );
	    CREATE NONCLUSTERED INDEX IX_Asistencia_IdSesion
        ON dbo.Asistencia (IdSesion);
END 
GO

--------TABLA DE MATERIAL DE CAPACITACION--------
IF OBJECT_ID(N'dbo.MaterialCapacitacion', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MaterialCapacitacion
    (
        IdMaterial INT IDENTITY(1,1) NOT NULL,
        Titulo NVARCHAR(50) NOT NULL,
        Tipo NVARCHAR(50) NOT NULL,-- manual, guía, video, etc.
        UrlRecurso NVARCHAR(500) NOT NULL,
        Activo NVARCHAR(1) NOT NULL
            CONSTRAINT DF_MaterialCapacitacion_Activo DEFAULT (N'1'),

        CONSTRAINT PK_MaterialCapacitacion PRIMARY KEY CLUSTERED (IdMaterial),
        CONSTRAINT CK_MaterialCapacitacion_Activo CHECK (Activo IN (N'0', N'1'))
    );
END
GO

--------TABLA DE SESION Y MATERIAL--------
IF OBJECT_ID(N'dbo.SesionMaterial', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SesionMaterial
    (
        IdSesion INT NOT NULL,
        IdMaterial INT NOT NULL,
        FechaAsignacion DATETIME2(0) NOT NULL
            CONSTRAINT DF_SesionMaterial_Fecha DEFAULT (SYSDATETIME()),

        CONSTRAINT PK_SesionMaterial PRIMARY KEY CLUSTERED (IdSesion, IdMaterial),
        CONSTRAINT FK_SesionMaterial_SesionCapacitacion
            FOREIGN KEY (IdSesion) REFERENCES dbo.SesionCapacitacion (IdSesion),
        CONSTRAINT FK_SesionMaterial_MaterialCapacitacion
            FOREIGN KEY (IdMaterial) REFERENCES dbo.MaterialCapacitacion (IdMaterial)
    );
        CREATE NONCLUSTERED INDEX IX_SesionMaterial_IdMaterial
        ON dbo.SesionMaterial (IdMaterial);
END
GO

PRINT N'Esquema SistemaCapacitacion creado correctamente.';
GO

