using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BillMicroservice.Protos;
using BillMicroservice.src.Application.DTOs;
using BillMicroservice.src.Application.Services.Interfaces;
using Grpc.Core;

namespace BillMicroservice.Services
{
    public class BillGrpcService : Protos.BillGrpcService.BillGrpcServiceBase
    {
        private readonly IBillService _billService;

        public BillGrpcService(IBillService billService)
        {
            _billService = billService;
        }

        public override async Task<CreateBillResponse> CreateBill(CreateBillRequest request, ServerCallContext context)
        {
            try
            {
                Console.WriteLine($"Monto a pagar: {request.Amount}");
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

                var response = new Bill
                {
                    BillId = createdBill.Id,
                    UserId = createdBill.UserId,
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
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override async Task<GetBillByIdResponse> GetBillById(GetBillByIdRequest request, ServerCallContext context)
        {
            try
            {
                var bill = await _billService.GetBillById(request.BillId, request.UserId, request.UserRole) ?? throw new RpcException(new Status(StatusCode.NotFound, "Factura no encontrada"));
                var response = new Bill
                {
                    BillId = bill.Id,
                    UserId = bill.UserId,
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
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override async Task<UpdateBillResponse> UpdateBill(UpdateBillRequest request, ServerCallContext context)
        {
            try
            {
                var updatedBill = await _billService.UpdateBillStatus(request.BillId, request.BillStatus);

                var response = new Bill
                {
                    BillId = updatedBill.Id,
                    UserId = updatedBill.UserId,
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
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override async Task<DeleteBillResponse> DeleteBill(DeleteBillRequest request, ServerCallContext context)
        {
            try
            {
                var deletedBill = await _billService.DeleteBill(request.BillId);

                return new DeleteBillResponse
                {

                };
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override async Task<GetAllBillsResponse> GetAllBills(GetAllBillsRequest request, ServerCallContext context)
        {
            try
            {
                var bills = await _billService.GetBills(request.UserId, request.UserRole, request.BillStatus)
                ?? throw new RpcException(new Status(StatusCode.NotFound, "No se encontraron facturas"));

                var response = new GetAllBillsResponse();

                foreach (var bill in bills)
                {
                    var billResponse = new Bill
                    {
                        BillId = bill.Id,
                        UserId = bill.UserId,
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
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }
    }
}