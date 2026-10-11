using Microsoft.EntityFrameworkCore;
using SIGEMAC_ONPE.Data;
using SIGEMAC_ONPE.Services.CU01_ConsultarAsignacionLocal;
using SIGEMAC_ONPE.Services.CU02_VisualizarDescargarMaterial;
using SIGEMAC_ONPE.Services.CU03_AutenticarUsuario;
using SIGEMAC_ONPE.Services.CU04_RegistrarAsistenciaParticipante;
using SIGEMAC_ONPE.Services.CU05_ConsultarPadronSesion;
using SIGEMAC_ONPE.Services.CU06_GestionarSesionesCapacitacion;
using SIGEMAC_ONPE.Services.CU07_AsignarParticipantesCapacitadores;

var builder = WebApplication.CreateBuilder(args);

// Configuración de serialización y controladores.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Evita ciclos al serializar entidades con propiedades de navegación.
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Acceso a SQL Server y servicios de los casos de uso implementados.
builder.Services.AddDbContext<SigemacOnpeContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CadenaSQL")));

builder.Services.AddScoped<ICU01ConsultarAsignacionService, CU01ConsultarAsignacionService>();
builder.Services.AddScoped<ICU02VisualizarDescargarMaterialService, CU02VisualizarDescargarMaterialService>();
builder.Services.AddScoped<ICU03AutenticarUsuarioService, CU03AutenticarUsuarioService>();
builder.Services.AddScoped<ICU04RegistrarAsistenciaParticipanteService, CU04RegistrarAsistenciaParticipanteService>();
builder.Services.AddScoped<ICU05ConsultarPadronSesionService, CU05ConsultarPadronSesionService>();
builder.Services.AddScoped<ICU06_GestionarSesionesCapacitacionService, CU06_GestionarSesionesCapacitacionService>();
builder.Services.AddScoped<ICU07_AsignarParticipantesCapacitadoresService, CU07_AsignarParticipantesCapacitadoresService>();

// Política CORS para permitir las llamadas del cliente web.
builder.Services.AddCors(options =>
{
    options.AddPolicy("NuevaPolitica", app =>
    {
        app.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("NuevaPolitica");
app.UseAuthorization();

app.MapControllers();

app.Run();
