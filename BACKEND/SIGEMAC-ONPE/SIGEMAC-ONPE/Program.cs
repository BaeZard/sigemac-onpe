using Microsoft.EntityFrameworkCore;
using SIGEMAC_ONPE.Data;
using SIGEMAC_ONPE.Services.CU01_ConsultarAsignacionLocal;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Esta línea corta los bucles infinitos en las respuestas JSON
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<SigemacOnpeContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("CadenaSQL"));
});

// Registro de Inyección de Dependencias del CU01
builder.Services.AddScoped<ICU01ConsultarAsignacionService, CU01ConsultarAsignacionService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("NuevaPolitica", app =>
    {
        app.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

//Registro de Inyeccion de dependencia del CU02
builder.Services.AddScoped<SIGEMAC_ONPE.Services.CU02_VisualizarDescargarMaterial.ICU02VisualizarDescargarMaterialService, SIGEMAC_ONPE.Services.CU02_VisualizarDescargarMaterial.CU02VisualizarDescargarMaterialService>();

builder.Services.AddScoped<SIGEMAC_ONPE.Services.CU03_AutenticarUsuario.ICU03AutenticarUsuarioService, SIGEMAC_ONPE.Services.CU03_AutenticarUsuario.CU03AutenticarUsuarioService>();

builder.Services.AddScoped<SIGEMAC_ONPE.Services.CU04_RegistrarAsistenciaParticipante.ICU04RegistrarAsistenciaParticipanteService, SIGEMAC_ONPE.Services.CU04_RegistrarAsistenciaParticipante.CU04RegistrarAsistenciaParticipanteService>();

builder.Services.AddScoped<SIGEMAC_ONPE.Services.CU05_ConsultarPadronSesion.ICU05ConsultarPadronSesionService, SIGEMAC_ONPE.Services.CU05_ConsultarPadronSesion.CU05ConsultarPadronSesionService>();
var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("NuevaPolitica");
app.UseAuthorization();

app.MapControllers();

app.Run();