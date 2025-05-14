using UserMicroservice.Protos;
using Grpc.Core;
using Serilog;

namespace UserMicroservice.Services
{
    public class UserNotificationService : Protos.UserNotificationService.UserNotificationServiceBase
    {
        /*
        public override Task<UserNotificationResponse> NotifyUserCreated(UserCreatedEvent request, ServerCallContext context)
        {
            Log.Information($"Nuevo usuario creado - Email: {request.Email}, Nombre: {request.FirstName} {request.LastName}");
            return Task.FromResult(new UserNotificationResponse { Success = true, Message = $"Notificación de usuario creado recibida para {request.Email}" });
        }

        public override Task<UserNotificationResponse> NotifyRoleCreated(RoleCreatedEvent request, ServerCallContext context)
        {
            Log.Information($"Nuevo rol creado - Nombre: {request.RoleName}");
            return Task.FromResult(new UserNotificationResponse { Success = true, Message = "Notificación de rol creado recibida." });
        }
        */
    }
}