```plaintext
sigemac-onpe/
├── .gitignore                      # Configuración para ignorar archivos (binarios, CSVs, contraseñas)
├── README.md                       # Documentación general y guía de instalación del proyecto
│
├── etl/                            # [FASE PREVIA]: Procesamiento e Ingesta de Datos
│   ├── notebooks/
│   │   └── limpieza_datos.ipynb    # Tu notebook de Google Colab (exportado desde Colab)
│   ├── ssis/
│   │   └── Proyecto_SSIS.sln       # Solución de Visual Studio para SSIS (Integration Services)
│   └── data_sample/
│       └── muestra_miembros.csv    # Archivo con 5 a 10 filas de muestra (NO poner CSVs pesados aquí)
│
├── database/                       # [CAPA 3]: Scripts y Esquema de SQL Server
│   └── scripts/
│       ├── 01_schema_miembros.sql  # Script DDL para crear la tabla 'MiembrosMesa'
│       ├── 02_schema_usuarios.sql  # Script DDL para tablas de Login (Usuarios, Roles, Credenciales)
│       ├── 03_schema_materiales.sql# Script DDL para tablas de Materiales de Capacitación
│       └── 04_seed_data.sql        # Datos iniciales/prueba para usuarios y materiales
│
├── backend/                        # [CAPA 2]: Lógica de Negocio y Web API (.NET / C#)
│   ├── src/
│   │   ├── Controllers/            # Controladores API (ej: AuthController.cs, MaterialesController.cs)
│   │   ├── Services/               # Lógica de negocio (Hash de contraseñas, tokens JWT)
│   │   ├── Models/                 # Modelos de datos y DTOs para consultas T-SQL
│   │   ├── Program.cs              # Punto de entrada de la Minimal API / Web API
│   │   └── appsettings.Example.json# Plantilla de configuración (cadena de conexión de ejemplo)
│   └── BackendApi.sln              # Solución de .NET (Visual Studio)
│
└── frontend/                       # [CAPA 1]: Presentación (HTML5, CSS3, JS)
    ├── css/
    │   └── styles.css              # Estilos visuales de la plataforma
    ├── js/
    │   ├── auth.js                 # Lógica JS de login/logout y gestión de sesiones
    │   ├── materiales.js           # Peticiones Fetch API para consultar y mostrar materiales
    │   └── main.js                 # Interacción general de la interfaz
    ├── login.html                  # Pantalla de autenticación (usuario y contraseña)
    └── dashboard.html              # Panel principal para consultar/gestionar el material
```