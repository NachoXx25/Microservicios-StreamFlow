using ApiGateway.Protos.UserService;
using DotNetEnv;
using Grpc.Core;
using Grpc.Net.Client;
using Serilog;
namespace ApiGateway.Services
{
    public class UserGrpcClient
    {
        private readonly GrpcChannel _channel;
        private readonly UserGrpcService.UserGrpcServiceClient _client;

        public UserGrpcClient(IConfiguration configuration)
        {
            var userServiceUrl = Env.GetString("GrpcServices__UserService") ?? "http://localhost:5136/";
            _channel = GrpcChannel.ForAddress(userServiceUrl);
            _client = new UserGrpcService.UserGrpcServiceClient(_channel);
        }

        public async Task<GetAllUsersResponse> GetAllUsersAsync(GetAllUsersRequest request)
        {
            try
            {
                return await _client.GetAllUsersAsync(request);
            }
            catch (RpcException ex)
            {
                Log.Error(ex, "Error gRPC obteniendo todos los usuarios");
                throw new InvalidOperationException(ex.Status.Detail);
            }
        }

        public async Task<GetUserByIdResponse> GetUserByIdAsync(GetUserByIdRequest request)
        {
            try
            {
                return await _client.GetUserByIdAsync(request);
            }
            catch (RpcException ex)
            {
                Log.Error(ex, "Error gRPC obteniendo usuario por ID");
                throw new InvalidOperationException(ex.Status.Detail);
            }
        }

        public async Task<CreateUserResponse> CreateUserAsync(CreateUserRequest request)
        {
            try
            {
                return await _client.CreateUserAsync(request);
            }
            catch (RpcException ex)
            {
                Log.Error(ex, "Error gRPC creando usuario");
                throw new InvalidOperationException(ex.Status.Detail);
            }
        }

        public async Task<UpdateUserResponse> UpdateUserAsync(UpdateUserRequest request)
        {
            try
            {
                return await _client.UpdateUserAsync(request);
            }
            catch (RpcException ex)
            {
                Log.Error(ex, "Error gRPC actualizando usuario");
                throw new InvalidOperationException(ex.Status.Detail);
            }
        }

        public async Task DeleteUserAsync(DeleteUserRequest request)
        {
            try
            {
                await _client.DeleteUserAsync(request);
            }
            catch (RpcException ex)
            {
                Log.Error(ex, "Error gRPC eliminando usuario");
                throw new InvalidOperationException(ex.Status.Detail);
            }
        }

        public void Dispose()
        {
            _channel?.Dispose();
        }
    }
}