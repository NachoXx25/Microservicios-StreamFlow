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
            var userExists = await _userRepository.UserExists(bill.UserId);
            
            if(!userExists){
                throw new KeyNotFoundException("El usuario no existe");
            }

            //Obtener el id del estado de la factura según el nombre
            int statusId = await _statusRepository.GetStatusIdByName(bill.StatusName);

            //Crear una nuevo objeto de factura
            var newBill = new Bill
            {
                AmountToPay = bill.AmountToPay,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                PaymentDate = bill.StatusName == "Pagado" ? DateTime.UtcNow : null, //Agregar la fecha de pago si el estado es "Pagado"
                StatusId = statusId,
                UserId = bill.UserId
            };

            //Agregar la nueva factura a la base de datos
            var createdBill = await _billRepository.AddBill(newBill);

            //Revisar si la factura fue creada correctamente
            if(createdBill == null){
                throw new KeyNotFoundException("Error al crear la factura");
            }else {
                //Crear un nuevo objeto de DTO para la factura creada
                var createdBillDTO = new CreatedBillDTO
                {
                    Id = createdBill.Id,
                    AmountToPay = createdBill.AmountToPay,
                    PaymentDate = createdBill.PaymentDate,
                    Status = bill.StatusName,
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
        public async Task<CreatedBillDTO> DeleteBill(int id)
        {
            //Obtener la factura por su id
            var bill = await _billRepository.GetBillById(id) ?? throw new KeyNotFoundException("La factura no existe");

            //Obtener el nombre del estado de la factura
            var statusName = await _statusRepository.GetStatusNameById(bill.StatusId);

            //Revisar si el estado de la factura es 'Pagado'
            if (statusName == "Pagado")
            {
                //No permitir la eliminación de una factura pagada
                throw new InvalidOperationException("No se puede eliminar una factura pagada");
            }

            var deletedBill = await _billRepository.DeleteBill(id) ?? throw new InvalidOperationException("Error al eliminar la factura");
            
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
        public async Task<CreatedBillDTO?> GetBillById(int id, int userId, string userRole)
        {
            //Revisar si el usuario existe
            var userExists = await _userRepository.UserExists(userId);

            //Si el usuario no existe, lanzar una excepción
            if(!userExists){
                throw new KeyNotFoundException("El usuario no existe");
            }
            
            //Obtener la factura por su id
            var bill = await _billRepository.GetBillById(id);

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
            if(userRole != "Administrador" && bill.UserId != userId){
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
        public async Task<CreatedBillDTO[]?> GetBills(int userId, string userRole, string? statusName)
        {
            //Obtener todos los estados de una factura la base de datos
            var statuses = await _statusRepository.GetAllStatuses() ?? throw new InvalidOperationException("No se pudieron cargar los estados.");

            //Si el usuario es administrador, obtener todas las facturas
            if(userRole == "Administrador"){

                var allBills = await _billRepository.GetAllBills();

                //Mapear todas las facturas a un DTO con los datos necesarios
                var mappedBills = allBills.Select( b => new CreatedBillDTO
                {
                    Id = b.Id,
                    AmountToPay = b.AmountToPay,
                    PaymentDate = b.PaymentDate,
                    Status = statuses.FirstOrDefault(s => s.Id == b.StatusId)?.Name ?? "Estado no encontrado",
                    UserId = b.UserId,
                    CreatedAt = b.CreatedAt
                }).ToArray();

                //Si se proporciona un nombre de estado, filtrar las facturas por ese estado
                if(!string.IsNullOrEmpty(statusName)){
                    mappedBills = mappedBills.Where(b => b.Status.Contains(statusName)).ToArray();

                    //Si no se encuentran facturas con ese estado, retornar nulo
                    if(mappedBills.Length == 0){
                        return null;
                    }else{
                        //Si se encuentran facturas, retornar el listado filtrado
                        return mappedBills;
                    }
                }else {
                    //Si no se proporciona un nombre de estado, retornar todas las facturas
                    return mappedBills;
                }

            //Si el usuario es cliente, obtener solo las facturas del usuario
            }else if(userRole == "Cliente"){

                //Obtener todas las facturas del usuario
                var userBills = await _billRepository.GetAllBillsByUserId(userId);

                //Mapear las facturas del usuario a un DTO con los datos necesarios
                var mappedBills = userBills.Select(b => new CreatedBillDTO
                {
                    Id = b.Id,
                    AmountToPay = b.AmountToPay,
                    PaymentDate = b.PaymentDate,
                    Status = statuses.FirstOrDefault(s => s.Id == b.StatusId)?.Name ?? "Estado no encontrado",
                    UserId = b.UserId,
                    CreatedAt = b.CreatedAt
                }).ToArray();

                //Si se proporciona un nombre de estado, filtrar las facturas por ese estado
                if(!string.IsNullOrEmpty(statusName)){
                    mappedBills = mappedBills.Where(b => b.Status.Contains(statusName)).ToArray();

                    //Si no se encuentran facturas con ese estado, retornar nulo
                    if(mappedBills.Length == 0){
                        return null;
                    }else{
                        //Si se encuentran facturas, retornar el listado filtrado
                        return mappedBills;
                    }
                }else {
                    //Si no se proporciona un nombre de estado, retornar todas las facturas del usuario
                    return mappedBills;
                }
            }else {
                throw new ArgumentException("El rol de usuario no es válido.");
            }
        }

        /// <summary>
        /// Método para modificar el estado de una factura
        /// </summary>
        /// <param name="id">El id de la factura a actualizar</param>
        /// <param name="status">El nuevo estado a asignar</param>
        public async Task<CreatedBillDTO> UpdateBillStatus(int id, string status)
        {
            //Obtener la factura por su id
            var bill = await _billRepository.GetBillById(id) ?? throw new KeyNotFoundException("La factura no existe");

            //Revisar si la factura no está eliminada
            if (bill.IsDeleted)
            {
                throw new InvalidOperationException("No puede cambiar el estado de una factura eliminada");
            }

            //Obtener el id del nuevo estado de la factura
            var statusId = await _statusRepository.GetStatusIdByName(status);

            // Determinar la fecha de pago basada en el estado
            DateTime? paymentDate = status == "Pagado" ? DateTime.UtcNow : null;

            // Actualizar la factura
            var updatedBill = await _billRepository.UpdateBillState(id, statusId, paymentDate) ?? 
                throw new InvalidOperationException("Error al actualizar la factura");

            var user = await _userRepository.GetUserById(updatedBill.UserId) ?? 
                throw new KeyNotFoundException("El usuario no existe");

            await _billEventService.PublishUpdatedBillEvent(new UpdatedBillDTO
            {
                Id = updatedBill.Id,
                UserId = updatedBill.UserId,
                UserEmail = user.Email,
                FirstName = user.Email,
                LastName = user.LastName,
                StatusName = status,
                Amount = updatedBill.AmountToPay
            });

            // Crear y retornar el DTO
            return new CreatedBillDTO
            {
                Id = updatedBill.Id,
                AmountToPay = updatedBill.AmountToPay,
                PaymentDate = updatedBill.PaymentDate,
                Status = status,
                UserId = updatedBill.UserId,
                CreatedAt = updatedBill.CreatedAt
            };
        }
    }
}