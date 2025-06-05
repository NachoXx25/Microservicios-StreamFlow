using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.Protos.SocialInteractionsService;
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
            catch (Exception ex)
            {
                throw new Exception("Error cargando los likes y comentarios del video", ex);
            }
        }

        public async Task<GiveLikeResponse> GiveLikeAsync(GiveLikeRequest request)
        {
            try
            {
                return await _client.GiveLikeAsync(request);
            }
            catch (Exception ex)
            {
                throw new Exception("Error agregando like al video", ex);
            }
        }

        public async Task<MakeCommentResponse> MakeCommentAsync(MakeCommentRequest request)
        {
            try
            {
                return await _client.MakeCommentAsync(request);
            }
            catch (Exception ex)
            {
                throw new Exception("Error agregando comentario al video", ex);
            }
        }
    }
}