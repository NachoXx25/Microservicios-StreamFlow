using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BillMicroservice.Services;
using BillMicroservice.src.Application.DTOs;
using BillMicroservice.src.Application.Services.Interfaces;
using BillMicroservice.src.Domain.Models.Bill;
using BillMicroservice.src.Infrastructure.Repositories.Interfaces;

namespace BillMicroservice.src.Application.Services.Implements
{
    public class BillService : IBillService
    {
        private readonly IBillRepository _billRepository;
        private readonly IStatusRepository _statusRepository;
        private readonly IUserRepository _userRepository;

        private readonly IBillEventService _billEventService;

        public BillService(IBillRepository billRepository, IStatusRepository statusRepository, IUserRepository userRepository, IBillEventService billEventService)
        {
            _billRepository = billRepository;
            _statusRepository = statusRepository;
            _userRepository = userRepository;
            _billEventService = billEventService;
        }

        /// <summary>
        /// Agrega una nueva factura a la base de datos.
        /// </summary>
        /// <param name="bill">La factura a agregar</param>
        /// <returns>La factura creada</returns>
        public async Task<CreatedBillDTO> AddBill(CreateBillDTO bill)
        {
            //Revisar si el id de usuario es válido
            var intUserId = int.Parse(bill.UserId);
            var userExists = await _userRepository.UserExists(intUserId);
            
            if(!userExists){
                throw new KeyNotFoundException("Usuario no encontrado");
            }

            //Obtener el id del estado de la factura según el nombre
            int statusId = await _statusRepository.GetStatusIdByName(bill.StatusName);

            //Crear una nuevo objeto de factura
            var newBill = new Bill
            {
                AmountToPay = bill.AmountToPay,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                PaymentDate = bill.StatusName.Equals("Pagado",StringComparison.OrdinalIgnoreCase) ? DateTime.UtcNow : null, //Agregar la fecha de pago si el estado es "Pagado"
                StatusId = statusId,
                UserId = intUserId
            };

            //Agregar la nueva factura a la base de datos
            var createdBill = await _billRepository.AddBill(newBill);

            //Revisar si la factura fue creada correctamente
            if(createdBill == null){
                throw new KeyNotFoundException("Error al crear la factura");
            }else {
                //Crear un nuevo objeto de DTO para la factura creada

                var normalizedStatusName = await _statusRepository.GetStatusNameById(createdBill.StatusId) 
                    ?? throw new InvalidOperationException("Estado de factura no encontrado");

                var createdBillDTO = new CreatedBillDTO
                {
                    Id = createdBill.Id,
                    AmountToPay = createdBill.AmountToPay,
                    PaymentDate = createdBill.PaymentDate,
                    Status = normalizedStatusName,
                    UserId = createdBill.UserId,
                    CreatedAt = createdBill.CreatedAt
                };

                //Retornar la factura creada con datos mapeados
                return createdBillDTO;
            }
        }

        /// <summary>
        /// Método para borrar una factura de forma lógica.
        /// </summary>
        /// <param name="id">El id de la factura a borrar</param>
        public async Task<CreatedBillDTO> DeleteBill(string id)
        {

            var intId = int.Parse(id);

            //Obtener la factura por su id
            var bill = await _billRepository.GetBillById(intId) ?? throw new KeyNotFoundException("Factura no encontrada");

            //Obtener el nombre del estado de la factura
            var statusName = await _statusRepository.GetStatusNameById(bill.StatusId);

            //Revisar si el estado de la factura es 'Pagado'
            if (statusName.Equals("Pagado",StringComparison.OrdinalIgnoreCase))
            {
                //No permitir la eliminación de una factura pagada
                throw new InvalidOperationException("No se puede eliminar una factura pagada");
            }

            var deletedBill = await _billRepository.DeleteBill(intId) ?? throw new InvalidOperationException("Error al eliminar la factura");
            
            return new CreatedBillDTO
            {
                Id = deletedBill.Id,
                AmountToPay = deletedBill.AmountToPay,
                PaymentDate = deletedBill.PaymentDate,
                Status = statusName,
                UserId = deletedBill.UserId,
                CreatedAt = deletedBill.CreatedAt
            };
        }

        /// <summary>
        /// Método que perite obtener una factura por su id.
        /// </summary>
        /// <param name="id">El id de la factura a obtener</param>
        /// <returns>La factura solicitada</returns>
        public async Task<CreatedBillDTO?> GetBillById(string id, string userId, string userRole)
        {
            //Revisar si el usuario existe
            var userIdExists = int.TryParse(userId, out int userIdInt);
            var userExists = await _userRepository.UserExists(userIdInt);

            //Si el usuario no existe, lanzar una excepción
            if(!userExists){
                throw new KeyNotFoundException("Usuario no encontrado.");
            }
            
            var intId = int.Parse(id);

            //Obtener la factura por su id
            var bill = await _billRepository.GetBillById(intId);

            if(bill == null){
                return null;
            }

            //Obtener el nombre del estado de la factura
            var statusName = await _statusRepository.GetStatusNameById(bill.StatusId);

            //Mapear la factura a un DTO con los datos necesarios
            var mappedBill = new CreatedBillDTO
            {
                Id = bill.Id,
                AmountToPay = bill.AmountToPay,
                PaymentDate = bill.PaymentDate,
                Status = statusName,
                UserId = bill.UserId,
                CreatedAt = bill.CreatedAt
            };

            //Si el usuario no es administrador, verificar que el id de usuario de la factura sea igual al id del usuario que realiza la consulta
            if(userRole != "Administrador" && bill.UserId.ToString() != userId){
                //Si no es así, lanzar una excepción
                throw new UnauthorizedAccessException("No tienes permisos para ver esta factura.");
            }

            return mappedBill;
        }

        /// <summary>
        /// Método que permite obtener todas las facturas de un usuario o todas las facturas del sistema.
        /// </summary>
        /// <param name="userId">El id del usuario que realiza la consulta</param>
        /// <param name="userRole">El rol del usuario que realiza la consulta</param>
        /// <param name="statusName">Filtro opcional por estado de factura</param>
        /// <returns>Listado de las facturas según el usuario y el filtro</returns>
        public async Task<CreatedBillDTO[]?> GetBills(string userId, string userRole, string? statusName)
        {
            Bill[] bills;
            
            if (userRole.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
            {
                bills = await _billRepository.GetAllBills(statusName);
            }
            else if (userRole.Equals("Cliente", StringComparison.OrdinalIgnoreCase))
            {
                var intUserId = int.Parse(userId);
                bills = await _billRepository.GetAllBillsByUserId(intUserId, statusName);
            }
            else
            {
                throw new ArgumentException("El rol de usuario no es válido.");
            }

            if (bills.Length == 0)
            {
                return null;
            }

            var mappedBills = bills.Select(b => new CreatedBillDTO
            {
                Id = b.Id,
                AmountToPay = b.AmountToPay,
                PaymentDate = b.PaymentDate,
                Status = b.Status?.Name ?? "Estado no encontrado",
                UserId = b.UserId,
                CreatedAt = b.CreatedAt
            }).ToArray();

            return mappedBills;
        }

        /// <summary>
        /// Método para modificar el estado de una factura
        /// </summary>
        /// <param name="id">El id de la factura a actualizar</param>
        /// <param name="status">El nuevo estado a asignar</param>
        public async Task<CreatedBillDTO> UpdateBillStatus(string id, string status)
        {
            var intId = int.Parse(id);

            //Obtener la factura por su id
            var bill = await _billRepository.GetBillById(intId) ?? throw new KeyNotFoundException("Factura no encontrada");

            //Revisar si la factura no está eliminada
            if (bill.IsDeleted)
            {
                throw new InvalidOperationException("No puede cambiar el estado de una factura eliminada");
            }

            //Obtener el id del nuevo estado de la factura
            var statusId = await _statusRepository.GetStatusIdByName(status);

            // Determinar la fecha de pago basada en el estado
            DateTime? paymentDate = status.Equals("Pagado",StringComparison.OrdinalIgnoreCase) ? DateTime.UtcNow : null;

            // Actualizar la factura
            var updatedBill = await _billRepository.UpdateBillState(intId, statusId, paymentDate) ?? 
                throw new InvalidOperationException("Error al actualizar la factura");

            var user = await _userRepository.GetUserById(updatedBill.UserId) ?? 
                throw new KeyNotFoundException("Usuario no encontrado");

            var normalizedStatus = await _statusRepository.GetStatusNameById(updatedBill.StatusId) ?? 
                throw new InvalidOperationException("Estado no encontrado");

            await _billEventService.PublishUpdatedBillEvent(new UpdatedBillDTO
            {
                Id = updatedBill.Id,
                UserId = updatedBill.UserId,
                UserEmail = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                StatusName = normalizedStatus,
                Amount = updatedBill.AmountToPay
            });

            // Crear y retornar el DTO
            return new CreatedBillDTO
            {
                Id = updatedBill.Id,
                AmountToPay = updatedBill.AmountToPay,
                PaymentDate = updatedBill.PaymentDate,
                Status = normalizedStatus,
                UserId = updatedBill.UserId,
                CreatedAt = updatedBill.CreatedAt
            };
        }
    }
}