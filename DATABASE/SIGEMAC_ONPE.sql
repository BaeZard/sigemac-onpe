CREATE DATABASE SigemacOnpe
GO
Use SigemacOnpe
GO
CREATE TABLE ODPE (
    IdOdpe        INT IDENTITY(1,1) PRIMARY KEY,
    NombreOdpe    VARCHAR(150) NOT NULL,
    Region        VARCHAR(100) NOT NULL,
    Direccion     VARCHAR(200) NULL
);
GO

CREATE TABLE Usuario (
    IdUsuario     INT IDENTITY(1,1) PRIMARY KEY,
    Username      VARCHAR(50)  NOT NULL UNIQUE,
    PasswordHash  VARCHAR(256) NOT NULL,
    Rol           VARCHAR(30)  NOT NULL, 
    Estado        BIT NOT NULL DEFAULT 1
);
GO


CREATE TABLE MiembroMesa (
    Dni                  CHAR(8) PRIMARY KEY,
    Nombres              VARCHAR(100) NOT NULL,
    Apellidos            VARCHAR(100) NOT NULL,
    Cargo                VARCHAR(50)  NOT NULL,
    EstadoCapacitacion   VARCHAR(30)  NOT NULL DEFAULT 'Designado',
    IdOdpe               INT NOT NULL,
    IdUsuario            INT NULL, -- opcional: no todos acceden con cuenta propia

    CONSTRAINT FK_MiembroMesa_Odpe
        FOREIGN KEY (IdOdpe) REFERENCES ODPE(IdOdpe),

    CONSTRAINT FK_MiembroMesa_Usuario
        FOREIGN KEY (IdUsuario) REFERENCES Usuario(IdUsuario)
);
GO

CREATE TABLE Capacitador (
    IdCapacitador       INT IDENTITY(1,1) PRIMARY KEY,
    CodigoCapacitador   VARCHAR(20) NOT NULL UNIQUE,
    Especialidad        VARCHAR(100) NULL,
    IdUsuario           INT NOT NULL,

    CONSTRAINT FK_Capacitador_Usuario
        FOREIGN KEY (IdUsuario) REFERENCES Usuario(IdUsuario)
);
GO


CREATE TABLE SesionCapacitacion (
    IdSesion        INT IDENTITY(1,1) PRIMARY KEY,
    Sede            VARCHAR(150) NOT NULL,
    Direccion       VARCHAR(200) NULL,
    FechaHora       DATETIME NOT NULL,
    Modalidad       VARCHAR(20) NOT NULL CHECK (Modalidad IN ('Presencial','Virtual')),
    IdCapacitador   INT NOT NULL,

    CONSTRAINT FK_Sesion_Capacitador
        FOREIGN KEY (IdCapacitador) REFERENCES Capacitador(IdCapacitador)
);
GO


CREATE TABLE Asistencia (
    IdAsistencia      INT IDENTITY(1,1) PRIMARY KEY,
    DniMiembroMesa    CHAR(8) NOT NULL,
    IdSesion          INT NOT NULL,
    FechaRegistro     DATETIME NOT NULL DEFAULT GETDATE(),
    Asistio           BIT NOT NULL DEFAULT 0,
    Observacion       VARCHAR(250) NULL,

    CONSTRAINT FK_Asistencia_Miembro
        FOREIGN KEY (DniMiembroMesa) REFERENCES MiembroMesa(Dni),

    CONSTRAINT FK_Asistencia_Sesion
        FOREIGN KEY (IdSesion) REFERENCES SesionCapacitacion(IdSesion),

    -- Evita registrar asistencia duplicada del mismo miembro en la misma sesión
    CONSTRAINT UQ_Asistencia_MiembroSesion UNIQUE (DniMiembroMesa, IdSesion)
);
GO


CREATE TABLE MaterialCapacitacion (
    IdMaterial    INT IDENTITY(1,1) PRIMARY KEY,
    Titulo        VARCHAR(150) NOT NULL,
    Tipo          VARCHAR(30)  NOT NULL, -- Manual, Guía, Video
    UrlRecurso    VARCHAR(300) NOT NULL,
    Activo        BIT NOT NULL DEFAULT 1
);
GO


CREATE TABLE SesionMaterial (
    IdSesion          INT NOT NULL,
    IdMaterial        INT NOT NULL,
    FechaAsignacion   DATETIME NOT NULL DEFAULT GETDATE(),

    PRIMARY KEY (IdSesion, IdMaterial),

    CONSTRAINT FK_SesionMaterial_Sesion
        FOREIGN KEY (IdSesion) REFERENCES SesionCapacitacion(IdSesion),

    CONSTRAINT FK_SesionMaterial_Material
        FOREIGN KEY (IdMaterial) REFERENCES MaterialCapacitacion(IdMaterial)
);
GO
