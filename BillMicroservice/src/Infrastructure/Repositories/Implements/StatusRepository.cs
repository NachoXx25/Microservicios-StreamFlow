using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BillMicroservice.src.Domain.Models.Bill;
using BillMicroservice.src.Infrastructure.Data;
using BillMicroservice.src.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BillMicroservice.src.Infrastructure.Repositories.Implements
{
    public class StatusRepository : IStatusRepository
    {
        private readonly BillContext _context;

        public StatusRepository(BillContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene todos los estados de factura de la base de datos.
        /// </summary>
        /// <returns>Listado de las facturas con sus ids y nombres</returns>
        public async Task<Status[]> GetAllStatuses()
        {
            var statuses = await _context.Statuses.AsNoTracking().ToArrayAsync() ?? throw new Exception("No se han encontrado estados en la base de datos.");
            return statuses;
        }

        /// <summary>
        /// Obtiene el id del estado de una factura a partir del nombre del estado.
        /// </summary>
        /// <param name="statusName">El nombre del estado</param>
        /// <returns>El id del estado</returns>
        public async Task<int> GetStatusIdByName(string statusName)
        {
            var status = await _context.Statuses.AsNoTracking().FirstOrDefaultAsync(s => s.Name == statusName) ?? throw new Exception("El estado no existe en la base de datos.");
            return status.Id;
        }

        /// <summary>
        /// Obtiene el nombre del estado de una factura a partir del id del estado.
        /// </summary>
        /// <param name="statusId">El id del estado</param>
        /// <returns>El nombre del estado</returns>
        public async Task<string> GetStatusNameById(int statusId)
        {
            var status = await _context.Statuses.AsNoTracking().FirstOrDefaultAsync(s => s.Id == statusId) ?? throw new Exception("El estado no existe en la base de datos.");
            return status.Name;
        }
    }
}