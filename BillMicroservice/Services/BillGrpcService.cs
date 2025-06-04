using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BillMicroservice.Protos;
using BillMicroservice.src.Application.DTOs;
using BillMicroservice.src.Application.Services.Interfaces;
using BillMicroservice.src.Infrastructure.MessageBroker.Models;
using Grpc.Core;
using Microsoft.VisualBasic;

namespace BillMicroservice.Services
{
    public class BillGrpcService : Protos.BillGrpcService.BillGrpcServiceBase
    {
        private readonly IBillService _billService;

        private readonly IMonitoringEventService _monitoringEventService;

        public BillGrpcService(IBillService billService, IMonitoringEventService monitoringEventService)
        {
            _monitoringEventService = monitoringEventService;
            _billService = billService;
        }

        public override async Task<CreateBillResponse> CreateBill(CreateBillRequest request, ServerCallContext context)
        {
            try
            {
                await _monitoringEventService.PublishActionEventAsync(new ActionEvent
                {
                    ActionMessage = "Crear factura",
                    Service = "BillMicroservice",
                    UserId = request.UserId,
                    UserEmail = request.UserEmail,
                    UrlMethod = "POST/facturas"
                });

                if (request.Amount < 0)
                {
                    throw new ArgumentException("El monto a pagar no puede ser negativo");
                }

                var billDto = new CreateBillDTO
                {
                    UserId = request.UserId,
                    StatusName = request.BillStatus,
                    AmountToPay = request.Amount,
                };

                var createdBill = await _billService.AddBill(billDto);

                if (createdBill == null)
                {
                    throw new KeyNotFoundException("Error al crear la factura");
                }                   

                var response = new Bill
                {
                    BillId = createdBill.Id.ToString(),
                    UserId = createdBill.UserId.ToString(),
                    BillStatus = createdBill.Status,
                    Amount = createdBill.AmountToPay,
                    CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(createdBill.CreatedAt.ToUniversalTime()),
                    PaymentDate = createdBill.PaymentDate.HasValue
                        ? Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(createdBill.PaymentDate.Value.ToUniversalTime())
                        : null,
                };

                return new CreateBillResponse
                {
                    Bill = response,
                };

            }
            catch (KeyNotFoundException ex)
            {
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = ex.Message,
                    Service = "BillMicroservice",
                    UserId = request.UserId,
                    UserEmail = request.UserEmail,
                });
                throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
            }
            catch (ArgumentException ex)
            {
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = ex.Message,
                    Service = "BillMicroservice",
                    UserId = request.UserId,
                    UserEmail = request.UserEmail,
                });
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }

            catch (Exception ex)
            {
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = $"Error al crear la factura: {ex.Message}",
                    Service = "BillMicroservice",
                    UserId = request.UserId,
                    UserEmail = request.UserEmail,
                });
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override async Task<GetBillByIdResponse> GetBillById(GetBillByIdRequest request, ServerCallContext context)
        {
            try
            {
                await _monitoringEventService.PublishActionEventAsync(new ActionEvent
                {
                    ActionMessage = "Obtener factura por ID",
                    Service = "BillMicroservice",
                    UserId = request.UserId,
                    UserEmail = request.UserEmail,
                    UrlMethod = $"GET/facturas/{request.BillId}"
                });
                var bill = await _billService.GetBillById(request.BillId, request.UserId, request.UserRole) ?? throw new KeyNotFoundException("Factura no encontrada");
                var response = new Bill
                {
                    BillId = bill.Id.ToString(),
                    UserId = bill.UserId.ToString(),
                    BillStatus = bill.Status,
                    Amount = bill.AmountToPay,
                    CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(bill.CreatedAt.ToUniversalTime()),
                    PaymentDate = bill.PaymentDate.HasValue
                        ? Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(bill.PaymentDate.Value.ToUniversalTime())
                        : null,
                };

                return new GetBillByIdResponse
                {
                    Bill = response,
                };
            }
            catch (KeyNotFoundException ex)
            {
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = ex.Message,
                    Service = "BillMicroservice",
                    UserId = request.UserId,
                    UserEmail = request.UserEmail,
                });
                throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
            }
            catch (Exception ex)
            {
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = $"Error al obtener la factura con id {request.BillId}: {ex.Message}",
                    Service = "BillMicroservice",
                    UserId = request.UserId,
                    UserEmail = request.UserEmail,
                });
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override async Task<UpdateBillResponse> UpdateBill(UpdateBillRequest request, ServerCallContext context)
        {
            try
            {
                await _monitoringEventService.PublishActionEventAsync(new ActionEvent
                {
                    ActionMessage = "Actualizar estado de factura",
                    Service = "BillMicroservice",
                    UserId = request.UserData.Id,
                    UserEmail = request.UserData.Email,
                    UrlMethod = $"PATCH/facturas/{request.BillId}"
                });

                var updatedBill = await _billService.UpdateBillStatus(request.BillId, request.BillStatus);

                var response = new Bill
                {
                    BillId = updatedBill.Id.ToString(),
                    UserId = updatedBill.UserId.ToString(),
                    BillStatus = updatedBill.Status,
                    Amount = updatedBill.AmountToPay,
                    CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(updatedBill.CreatedAt.ToUniversalTime()),
                    PaymentDate = updatedBill.PaymentDate.HasValue
                        ? Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(updatedBill.PaymentDate.Value.ToUniversalTime())
                        : null,
                };

                return new UpdateBillResponse
                {
                    Bill = response,
                };
            }
            catch (Exception ex)
            {
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = $"Error al actualizar la factura con id {request.BillId}: {ex.Message}",
                    Service = "BillMicroservice",
                    UserId = request.UserData.Id,
                    UserEmail = request.UserData.Email,
                });
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override async Task<DeleteBillResponse> DeleteBill(DeleteBillRequest request, ServerCallContext context)
        {
            try
            {
                await _monitoringEventService.PublishActionEventAsync(new ActionEvent
                {
                    ActionMessage = "Eliminar factura",
                    Service = "BillMicroservice",
                    UserId = request.UserData.Id,
                    UserEmail = request.UserData.Email,
                    UrlMethod = $"DELETE/facturas/{request.BillId}"
                });
                var deletedBill = await _billService.DeleteBill(request.BillId);

                return new DeleteBillResponse
                {

                };
            }
            catch (Exception ex)
            {
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = $"Error al eliminar la factura con id {request.BillId}: {ex.Message}",
                    Service = "BillMicroservice",
                    UserId = request.UserData.Id,
                    UserEmail = request.UserData.Email,
                });
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override async Task<GetAllBillsResponse> GetAllBills(GetAllBillsRequest request, ServerCallContext context)
        {
            try
            {
                await _monitoringEventService.PublishActionEventAsync(new ActionEvent
                {
                    ActionMessage = "Listar facturas por usuario",
                    Service = "BillMicroservice",
                    UserId = request.UserId,
                    UserEmail = request.UserEmail,
                    UrlMethod = "GET/facturas"
                });

                var bills = await _billService.GetBills(request.UserId, request.UserRole, request.BillStatus)
                ?? throw new KeyNotFoundException("No se encontraron facturas");

                var response = new GetAllBillsResponse();

                foreach (var bill in bills)
                {
                    var billResponse = new Bill
                    {
                        BillId = bill.Id.ToString(),
                        UserId = bill.UserId.ToString(),
                        BillStatus = bill.Status,
                        Amount = bill.AmountToPay,
                        CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(bill.CreatedAt.ToUniversalTime()),
                        PaymentDate = bill.PaymentDate.HasValue
                            ? Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(bill.PaymentDate.Value.ToUniversalTime())
                            : null,
                    };

                    response.Bills.Add(billResponse);
                }

                return response;
            }
            catch (KeyNotFoundException ex)
            {
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = ex.Message,
                    Service = "BillMicroservice",
                    UserId = request.UserId,
                    UserEmail = request.UserEmail
                });
                throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
            }
            catch (Exception ex)
            {
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = $"Error al obtener las facturas: {ex.Message}",
                    Service = "BillMicroservice",
                    UserId = request.UserId,
                    UserEmail = request.UserEmail
                });
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }
    }
}