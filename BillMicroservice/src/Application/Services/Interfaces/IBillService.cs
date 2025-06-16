using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BillMicroservice.src.Application.DTOs;

namespace BillMicroservice.src.Application.Services.Interfaces
{
    public interface IBillService
    {
        /// <summary>
        /// Agrega una nueva factura a la base de datos.
        /// </summary>
        /// <param name="bill">La nueva factura a agregar</param>
        Task<CreatedBillDTO> AddBill(CreateBillDTO  bill);

        /// <summary>
        /// Obtener una factura por su id.
        /// </summary>
        /// <param name="id">El id de la factura a obtener</param>
        Task<CreatedBillDTO?> GetBillById(string id, string userId, string userRole);

        /// <summary>
        /// Cambiar el estado de una factura.
        /// </summary>
        /// <param name="id">El id de la factura a actualizar</param>
        /// <param name="status">El nuevo estado a asignar</param>
        Task<CreatedBillDTO> UpdateBillStatus(string id, string status);

        /// <summary>
        /// Obtener todas las facturas.
        /// </summary>
        /// <param name="search">Búsqueda de una factura</param>
        /// <returns></returns>
        Task<CreatedBillDTO[]?> GetBills(string userId, string userRole, string? statusName);

        /// <summary>
        /// Hace un borrado lógico de una factura.
        /// </summary>
        /// <param name="id">El id de la factura a borrar</param>
        /// <returns></returns>
        Task<CreatedBillDTO> DeleteBill(string id);
    }
}