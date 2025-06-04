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
            var billServiceUrl = configuration["GrpcServices:BillService"] ?? "http://localhost:5026/";
            _channel = GrpcChannel.ForAddress(billServiceUrl);
            _client = new BillGrpcService.BillGrpcServiceClient(_channel);
        }

        public async Task<GetAllBillsResponse> GetAllBillsAsync(GetAllBillsRequest request)
        {
            return await _client.GetAllBillsAsync(request);
        }

        public async Task<GetBillByIdResponse> GetBillByIdAsync(GetBillByIdRequest request)
        {
            return await _client.GetBillByIdAsync(request);
        }

        public async Task<CreateBillResponse> CreateBillAsync(CreateBillRequest request)
        {
            return await _client.CreateBillAsync(request);
        }

        public async Task<UpdateBillResponse> UpdateBillAsync(UpdateBillRequest request)
        {
            return await _client.UpdateBillAsync(request);
        }

        public async Task DeleteBillAsync(DeleteBillRequest request)
        {
            await _client.DeleteBillAsync(request);
        }

        public void Dispose()
        {
            _channel?.Dispose();
        }
    }
}