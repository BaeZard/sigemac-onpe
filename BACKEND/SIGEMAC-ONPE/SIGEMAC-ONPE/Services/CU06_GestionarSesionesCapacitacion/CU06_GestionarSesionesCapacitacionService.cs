using Microsoft.EntityFrameworkCore;
using SIGEMAC_ONPE.Data;
using SIGEMAC_ONPE.Models;
using SIGEMAC_ONPE.Services.CU07_AsignarParticipantesCapacitadores;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SIGEMAC_ONPE.Services.CU06_GestionarSesionesCapacitacion
{
    public class CU06_GestionarSesionesCapacitacionService : ICU06_GestionarSesionesCapacitacionService
    {
        private readonly SigemacOnpeContext _dbContext;
        private readonly ICU07_AsignarParticipantesCapacitadoresService _asignacionService;

        public CU06_GestionarSesionesCapacitacionService(
            SigemacOnpeContext dbContext,
            ICU07_AsignarParticipantesCapacitadoresService asignacionService)
        {
            _dbContext = dbContext;
            _asignacionService = asignacionService;
        }

        public async Task<bool> CrearSesionAsync(SesionCapacitacionDTO dto)
        {
            try
            {
                // Combinamos la fecha y hora que envía tu formulario JS
                DateTime fechaHoraReal = DateTime.Parse($"{dto.Fecha} {dto.Hora}:00");

                var nuevaSesion = new SesionCapacitacion
                {
                    Sede = dto.Sede ?? "Sede Principal",
                    Direccion = dto.Direccion ?? "Por definir",
                    FechaHora = fechaHoraReal,
                    Modalidad = dto.Modalidad ?? "Presencial",
                    IdCapacitador = 1 // Asignación inicial por defecto para cumplir con la llave foránea
                };

                _dbContext.SesionCapacitacions.Add(nuevaSesion);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<CU06_ResultadoActualizacion> ActualizarSesionAsync(int idSesion, SesionCapacitacionDTO dto)
        {
            var sesion = await _dbContext.SesionCapacitacions.FindAsync(idSesion);
            if (sesion == null) return CU06_ResultadoActualizacion.SesionNoEncontrada;

            // Si llega un DNI de capacitador (desde la pestaña Asignar capacitadores)
            if (!string.IsNullOrEmpty(dto.Capacitador))
            {
                var resultado = await _asignacionService.AsignarCapacitadorAsync(idSesion, dto.Capacitador);
                if (resultado == CU07_ResultadoAsignacion.SolicitudInvalida)
                {
                    return CU06_ResultadoActualizacion.SolicitudInvalida;
                }
                if (resultado == CU07_ResultadoAsignacion.CapacitadorNoEncontrado)
                {
                    return CU06_ResultadoActualizacion.CapacitadorNoEncontrado;
                }
                if (resultado != CU07_ResultadoAsignacion.Exitoso)
                {
                    return CU06_ResultadoActualizacion.SesionNoEncontrada;
                }
            }

            // Si se asignan materiales
            if (dto.MaterialIds != null && dto.MaterialIds.Any())
            {
                var materialesViejos = _dbContext.SesionMaterials.Where(sm => sm.IdSesion == idSesion);
                _dbContext.SesionMaterials.RemoveRange(materialesViejos);

                var nuevosMateriales = dto.MaterialIds.Select(idMat => new SesionMaterial
                {
                    IdSesion = idSesion,
                    IdMaterial = idMat,
                    FechaAsignacion = DateTime.Now
                });
                _dbContext.SesionMaterials.AddRange(nuevosMateriales);
            }

            await _dbContext.SaveChangesAsync();
            return CU06_ResultadoActualizacion.Actualizado;
        }

        public async Task<bool> EliminarSesionAsync(int idSesion)
        {
            var sesion = await _dbContext.SesionCapacitacions.FindAsync(idSesion);
            if (sesion == null) return false;

            var materiales = _dbContext.SesionMaterials.Where(sm => sm.IdSesion == idSesion);
            _dbContext.SesionMaterials.RemoveRange(materiales);

            _dbContext.SesionCapacitacions.Remove(sesion);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
