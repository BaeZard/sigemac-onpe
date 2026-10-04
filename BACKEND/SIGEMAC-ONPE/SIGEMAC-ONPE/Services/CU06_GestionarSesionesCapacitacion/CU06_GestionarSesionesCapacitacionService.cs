using Microsoft.EntityFrameworkCore;
using SIGEMAC_ONPE.Data;
using SIGEMAC_ONPE.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SIGEMAC_ONPE.Services.CU06_GestionarSesionesCapacitacion
{
    public class CU06_GestionarSesionesCapacitacionService : ICU06_GestionarSesionesCapacitacionService
    {
        private readonly SigemacOnpeContext _dbContext;

        public CU06_GestionarSesionesCapacitacionService(SigemacOnpeContext dbContext)
        {
            _dbContext = dbContext;
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

        public async Task<bool> ActualizarSesionAsync(int idSesion, SesionCapacitacionDTO dto)
        {
            var sesion = await _dbContext.SesionCapacitacions.FindAsync(idSesion);
            if (sesion == null) return false;

            // Si llega un DNI de capacitador (desde la pestaña Asignar capacitadores)
            if (!string.IsNullOrEmpty(dto.Capacitador))
            {
                var capacitador = await _dbContext.Capacitadors
                    .Include(c => c.IdUsuarioNavigation)
                    .FirstOrDefaultAsync(c => c.IdUsuarioNavigation.Username == dto.Capacitador);

                if (capacitador != null)
                {
                    sesion.IdCapacitador = capacitador.IdCapacitador;
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

            _dbContext.SesionCapacitacions.Update(sesion);
            await _dbContext.SaveChangesAsync();
            return true;
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