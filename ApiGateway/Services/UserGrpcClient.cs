using ApiGateway.Protos.UserService;
using Grpc.Net.Client;
namespace ApiGateway.Services
{
    public class UserGrpcClient
    {
        private readonly GrpcChannel _channel;
        private readonly UserGrpcService.UserGrpcServiceClient _client;

        public UserGrpcClient(IConfiguration configuration)
        {
            var userServiceUrl = configuration["GrpcServices:UserService"] ?? "http://localhost:5136/";
            _channel = GrpcChannel.ForAddress(userServiceUrl);
            _client = new UserGrpcService.UserGrpcServiceClient(_channel);
        }

        public async Task<GetAllUsersResponse> GetAllUsersAsync()
        {
            return await _client.GetAllUsersAsync(new GetAllUsersRequest());
        }

        public async Task<GetUserByIdResponse> GetUserByIdAsync(string userId)
        {
            return await _client.GetUserByIdAsync(new GetUserByIdRequest { Id = userId });
        }

        public async Task<CreateUserResponse> CreateUserAsync(CreateUserRequest request)
        {
            return await _client.CreateUserAsync(request);
        }

        public async Task<UpdateUserResponse> UpdateUserAsync(UpdateUserRequest request)
        {
            return await _client.UpdateUserAsync(request);
        }

        public async Task DeleteUserAsync(DeleteUserRequest request)
        {
            await _client.DeleteUserAsync(request);
        }

        public void Dispose()
        {
            _channel?.Dispose();
        }
    }
}