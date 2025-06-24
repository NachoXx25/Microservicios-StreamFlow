using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Net.Client;
using ApiGateway.Protos.BillService;
using Grpc.Core;
using DotNetEnv;

namespace ApiGateway.Services
{
    public class BillGrpcClient
    {
        private readonly GrpcChannel _channel;

        private readonly BillGrpcService.BillGrpcServiceClient _client;

        public BillGrpcClient(IConfiguration configuration)
        {
            var billServiceUrl = Env.GetString("GrpcServices__BillService") ?? "http://localhost:5086/";
            _channel = GrpcChannel.ForAddress(billServiceUrl);
            _client = new BillGrpcService.BillGrpcServiceClient(_channel);
        }

        public async Task<GetAllBillsResponse> GetAllBillsAsync(GetAllBillsRequest request)
        {
            try
            {
                return await _client.GetAllBillsAsync(request);
            }
            catch (RpcException)
            {
                throw;
            }
        }

        public async Task<GetBillByIdResponse> GetBillByIdAsync(GetBillByIdRequest request)
        {
            try
            {
                return await _client.GetBillByIdAsync(request);
            }
            catch (RpcException)
            {
                throw;
            }
        }

        public async Task<CreateBillResponse> CreateBillAsync(CreateBillRequest request)
        {
            try
            {
                return await _client.CreateBillAsync(request);
            }
            catch (RpcException)
            {
                throw;
            }
        }

        public async Task<UpdateBillResponse> UpdateBillAsync(UpdateBillRequest request)
        {
            try
            {
                return await _client.UpdateBillAsync(request);
            }
            catch (RpcException)
            {
                throw;
            }
        }

        public async Task<DeleteBillResponse> DeleteBillAsync(DeleteBillRequest request)
        {
            try
            {
                return await _client.DeleteBillAsync(request);
            }
            catch (RpcException)
            {
                throw;
            }
        }
    }
}