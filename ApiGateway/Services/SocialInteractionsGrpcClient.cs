using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.Protos.SocialInteractionsService;
using Grpc.Core;
using Grpc.Net.Client;

namespace ApiGateway.Services
{
    public class SocialInteractionsGrpcClient
    {
        private readonly GrpcChannel _channel;
        private readonly SocialInteractionsGrpcService.SocialInteractionsGrpcServiceClient _client;

        public SocialInteractionsGrpcClient(IConfiguration configuration)
        {
            var socialInteractionsServiceUrl = configuration["GrpcServices:SocialInteractionsService"] ?? "http://localhost:5217/";
            _channel = GrpcChannel.ForAddress(socialInteractionsServiceUrl);
            _client = new SocialInteractionsGrpcService.SocialInteractionsGrpcServiceClient(_channel);
        }

        public async Task<GetVideoLikesAndCommentsResponse> GetVideoLikesAndCommentsAsync(GetVideoLikesAndCommentsRequest request)
        {
            try
            {
                return await _client.GetVideoLikesAndCommentsAsync(request);
            }
            catch (RpcException)
            {
                throw;
            }
        }

        public async Task<GiveLikeResponse> GiveLikeAsync(GiveLikeRequest request)
        {
            try
            {
                return await _client.GiveLikeAsync(request);
            }
            catch (RpcException)
            {
                throw;
            }
        }

        public async Task<MakeCommentResponse> MakeCommentAsync(MakeCommentRequest request)
        {
            try
            {
                return await _client.MakeCommentAsync(request);
            }
            catch (RpcException)
            {
                throw;
            }
        }
    }
}