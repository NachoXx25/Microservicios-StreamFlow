using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Net.Client;
using ApiGateway.Protos.BillService;

namespace ApiGateway.Services
{
    public class BillGrpcClient
    {
        private readonly GrpcChannel _channel;

        private readonly BillGrpcService.BillGrpcServiceClient _client;

        public BillGrpcClient(IConfiguration configuration)
        {
            var billServiceUrl = configuration["GrpcServices:BillService"] ?? "http://localhost:5086/";
            _channel = GrpcChannel.ForAddress(billServiceUrl);
            _client = new BillGrpcService.BillGrpcServiceClient(_channel);
        }

        public async Task<GetAllBillsResponse> GetAllBillsAsync(GetAllBillsRequest request)
        {
            try
            {
                return await _client.GetAllBillsAsync(request);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ha ocurrido un error cargando las facturas: {ex.Message}", ex);
            }
        }

        public async Task<GetBillByIdResponse> GetBillByIdAsync(GetBillByIdRequest request)
        {
            try
            {
                return await _client.GetBillByIdAsync(request);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ha ocurrido un error cargando la factura con ID {request.BillId}: {ex.Message}", ex);
            }
        }

        public async Task<CreateBillResponse> CreateBillAsync(CreateBillRequest request)
        {
            try
            {
                return await _client.CreateBillAsync(request);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ha ocurrido un error creando la factura: {ex.Message}", ex);
            }
        }

        public async Task<UpdateBillResponse> UpdateBillAsync(UpdateBillRequest request)
        {
            try
            {
                return await _client.UpdateBillAsync(request);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ha ocurrido un error actualizando la factura con ID {request.BillId}: {ex.Message}", ex);
            }
        }

        public async Task<DeleteBillResponse> DeleteBillAsync(DeleteBillRequest request)
        {
            try
            {
                return await _client.DeleteBillAsync(request);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ha ocurrido un error eliminando la factura con ID {request.BillId}: {ex.Message}", ex);
            }
        }
    }
}