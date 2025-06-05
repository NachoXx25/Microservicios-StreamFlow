using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.Protos.MonitoringService;
using Grpc.Net.Client;

namespace ApiGateway.Services
{
    public class MonitoringGrpcClient
    {
        private readonly GrpcChannel _channel;

        private readonly MonitoringGrpcService.MonitoringGrpcServiceClient _client;

        public MonitoringGrpcClient(IConfiguration configuration)
        {
            var monitoringServiceUrl = configuration["GrpcServices:MonitoringService"] ?? "http://localhost:5038/";
            _channel = GrpcChannel.ForAddress(monitoringServiceUrl);
            _client = new MonitoringGrpcService.MonitoringGrpcServiceClient(_channel);
        }

        public async Task<GetAllActionsResponse> GetAllActionsAsync(GetAllActionsRequest request)
        {
            try
            {
                return await _client.GetAllActionsAsync(request);
            }   
            catch (Exception ex)
            {
                throw new Exception($"Error cargando las acciones: {ex.Message}", ex);
            }
        }

        public async Task<GetAllErrorsResponse> GetAllErrorsAsync(GetAllErrorsRequest request)
        {
            try
            {
                return await _client.GetAllErrorsAsync(request);
            }
            catch (Exception ex)
            {
                
                throw new Exception($"Error cargando los errores: {ex.Message}", ex);
            }
        }

    }
}