using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BillMicroservice.src.Application.DTOs;
using BillMicroservice.src.Domain.Models.Bill;

namespace BillMicroservice.src.Infrastructure.Repositories.Interfaces
{
    public interface IBillRepository
    {
        /// <summary>
        /// Agrega una factura a la base de datos
        /// </summary>
        /// <param name="bill">Factura a agregar.</param>
        Task<Bill> AddBill(Bill bill);

        /// <summary>
        /// Obtiene una factura por su id
        /// </summary>
        /// <param name="id">El id de la factura a buscar</param>
        Task<Bill?> GetBillById(int id);

        /// <summary>
        /// Actualiza el estado de una factura
        /// </summary>
        /// <param name="id">El id de la factura a actualizar</param>
        /// <param name="statusId">El id del nuevo estado de la factura</param>
        Task<Bill?> UpdateBillState(int id, int statusId, DateTime? paymentDate);

        /// <summary>
        /// Hace un borrado lógico de una factura
        /// </summary>
        /// <param name="bill">Factura a borrar.</param>
        Task<Bill?> DeleteBill(int id);

        /// <summary>
        /// Obtiene todas las facturas
        /// </summary>
        /// <returns>Listado de facturas</returns>
        Task<Bill[]> GetAllBills(string? statusFilter);

        /// <summary>
        /// Obtiene todas las facturas de un usuario
        /// </summary>
        /// <param name="userId">El id del usuario al que le corresponden las facturas</param>
        /// <returns>Listado de facturas del usuario</returns>
        Task<Bill[]> GetAllBillsByUserId(int userId, string? statusFilter);
    }
}