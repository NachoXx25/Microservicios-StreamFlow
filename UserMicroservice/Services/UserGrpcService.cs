using UserMicroservice.Protos;
using Grpc.Core;
using Serilog;
using UserMicroservice.src.Application.Services.Interfaces;
using UserMicroservice.src.Application.DTOs;
using Google.Protobuf.WellKnownTypes;
using UserMicroservice.src.Domain.Models;
using System.Text.RegularExpressions;

namespace UserMicroservice.Services
{
    public class UserGrpcService : Protos.UserGrpcService.UserGrpcServiceBase
    {
        private readonly IUserService _userService;
        private readonly IMonitoringEventService _monitoringEventService;

        public UserGrpcService(IUserService userService, IMonitoringEventService monitoringEventService)
        {
            _userService = userService;
            _monitoringEventService = monitoringEventService;
        }

        public override async Task<GetAllUsersResponse> GetAllUsers(GetAllUsersRequest request, ServerCallContext context)
        {
            Log.Information("recibida petición para obtener todos los usuarios");
            try
            {
                var search = new SearchByDTO
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email
                };

                var users = await _userService.GetAllUsers(search);

                var response = new GetAllUsersResponse();
                response.Users.AddRange(users.Select(u => new User
                {
                    Id = u.Id.ToString(),
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    CreatedAt = Timestamp.FromDateTime(u.CreatedAt.ToUniversalTime()),
                }));
                await _monitoringEventService.PublishActionEventAsync(new ActionEvent
                {
                    ActionMessage = $"Se han obtenido {users.Count()} usuarios",
                    UserId = request.UserId,
                    UserEmail = request.UserEmail,
                    UrlMethod = "GET/usuarios",
                    Service = "UserMicroservice"
                });
                return response;
            }
            catch (Exception ex)
            {
                Log.Error($"Error al obtener todos los usuarios: {ex.Message}");
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = ex.Message,
                    UserId = request.UserId,
                    UserEmail = request.UserEmail,
                    Service = "UserMicroservice"
                });
                throw new RpcException(new Status(StatusCode.Internal, $"Error al obtener todos los usuarios: {ex.Message}"));
            }

        }

        public override async Task<GetUserByIdResponse> GetUserById(GetUserByIdRequest request, ServerCallContext context)
        {
            Log.Information($"Recibida petición para obtener usuario por ID: {request.Id}");
            try
            {
                if (string.IsNullOrEmpty(request.Id)) throw new Exception("El ID no puede estar vacío");
                if (!int.TryParse(request.Id, out int userId)) throw new Exception("ID debe ser un número entero positivo");
                var user = await _userService.GetUserById(userId);
                await _monitoringEventService.PublishActionEventAsync(new ActionEvent
                {
                    ActionMessage = $"Se ha obtenido el usuario con ID: {userId}",
                    UserId = request.UserId,
                    UserEmail = request.UserEmail,
                    UrlMethod = "GET/usuario/{id}",
                    Service = "UserMicroservice"
                });
                return new GetUserByIdResponse
                {
                    User = new User
                    {
                        Id = user.Id.ToString(),
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        CreatedAt = Timestamp.FromDateTime(user.CreatedAt.ToUniversalTime()),
                    }
                };
            }
            catch (Exception ex)
            {
                Log.Error($"Error al obtener usuario por ID: {ex.Message}");
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = ex.Message,
                    UserId = request.UserId,
                    UserEmail = request.UserEmail,
                    Service = "UserMicroservice"
                });
                throw new RpcException(new Status(StatusCode.Internal, $"Error al obtener usuario por ID: {ex.Message}"));
            }
        }

        public override async Task<CreateUserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
        {
            try
            {
                Log.Information("Recibida petición para crear un nuevo usuario");
                var nameRegex = new Regex(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s\-]+$");
                var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
                if (request.FirstName.Length < 2 || request.FirstName.Length > 20) throw new Exception("El Nombre debe tener entre 2 y 20 letras.");
                if (request.LastName.Length < 2 || request.LastName.Length > 20) throw new Exception("El Apellido debe tener entre 2 y 20 letras.");
                if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 8 || request.Password.Length > 20) throw new Exception("La contraseña debe tener entre 8 y 20 caracteres.");
                if (string.IsNullOrEmpty(request.ConfirmPassword) || request.ConfirmPassword != request.Password) throw new Exception("La confirmación de la contraseña no coincide.");
                if (string.IsNullOrEmpty(request.Role) || (request.Role != "Administrador" && request.Role != "Cliente")) throw new Exception("El Rol debe ser 'Administrador' o 'Cliente'.");
                if (string.IsNullOrEmpty(request.FirstName) || string.IsNullOrEmpty(request.LastName) || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.ConfirmPassword) || string.IsNullOrEmpty(request.Role)) throw new Exception("Todos los campos son obligatorios.");
                if (!nameRegex.IsMatch(request.FirstName)) throw new Exception("El Nombre solo puede contener carácteres del abecedario español.");
                if (!nameRegex.IsMatch(request.LastName)) throw new Exception("El Apellido solo puede contener carácteres del abecedario español.");
                if (!emailRegex.IsMatch(request.Email)) throw new Exception("El Correo electrónico no es válido.");
    
                var userDTO = new CreateUserDTO
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Password = request.Password,
                    ConfirmPassword = request.ConfirmPassword,
                    Role = request.Role
                };
                var user = await _userService.CreateUser(userDTO);
                Log.Information("Usuario creado correctamente");
                await _monitoringEventService.PublishActionEventAsync(new ActionEvent
                {
                    ActionMessage = $"Usuario creado con ID: {user.Id}",
                    UserId = request.UserId,
                    UserEmail = request.UserEmail,
                    UrlMethod = "POST/usuarios",
                    Service = "UserMicroservice"
                });
                return new CreateUserResponse
                {
                    Id = user.Id.ToString(),
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    RoleName = user.RoleName,
                    CreatedAt = Timestamp.FromDateTime(user.CreatedAt.ToUniversalTime()),
                    UpdatedAt = Timestamp.FromDateTime(user.UpdatedAt.ToUniversalTime()),
                    IsActive = user.IsActive
                };
            }
            catch (Exception ex)
            {
                Log.Error($"Error al crear usuario: {ex.Message}");
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = ex.Message,
                    UserId = request.UserId,
                    UserEmail = request.UserEmail,
                    Service = "UserMicroservice"
                });
                throw new RpcException(new Status(StatusCode.Internal, $"Error al crear el usuario: {ex.Message}"));
            }
        }

        public override async Task<UpdateUserResponse> UpdateUser(UpdateUserRequest request, ServerCallContext context)
        {
            Log.Information($"Recibida petición para actualizar usuario con ID: {request.Id}");
            try
            {
                if (string.IsNullOrEmpty(request.Id)) throw new Exception("El ID no puede estar vacío");
                if (!int.TryParse(request.Id, out int userId)) throw new Exception("El ID debe ser un número entero positivo");
                if (request.FirstName.Length > 20) throw new Exception("El nombre debe tener máximo 20 letras.");
                if (request.LastName.Length > 20) throw new Exception("El apellido debe tener máximo 20 letras.");
                if (!string.IsNullOrEmpty(request.Email))
                {
                    var regex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
                    if (!regex.IsMatch(request.Email)) throw new Exception("El Correo electrónico no es válido.");
                }
                var updateUser = new UpdateUserDTO
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Password = request.Password
                };
                var updatedUser = await _userService.UpdateUser(updateUser, userId);
                Log.Information("Usuario actualizado correctamente");
                await _monitoringEventService.PublishActionEventAsync(new ActionEvent
                {
                    ActionMessage = $"Usuario actualizado con ID: {updatedUser.Id}",
                    UserId = request.UserId,
                    UserEmail = request.UserEmail,
                    UrlMethod = $"PATCH/usuarios/{userId}",
                    Service = "UserMicroservice"
                });
                return new UpdateUserResponse
                {
                    Id = updatedUser.Id.ToString(),
                    FirstName = updatedUser.FirstName,
                    LastName = updatedUser.LastName,
                    Email = updatedUser.Email,
                    RoleName = updatedUser.RoleName,
                    CreatedAt = Timestamp.FromDateTime(updatedUser.CreatedAt.ToUniversalTime()),
                    UpdatedAt = Timestamp.FromDateTime(updatedUser.UpdatedAt.ToUniversalTime()),
                    IsActive = updatedUser.IsActive
                };
            }
            catch (Exception ex)
            {
                Log.Error($"Error al actualizar usuario: {ex.Message}");
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = ex.Message,
                    UserId = request.UserId,
                    UserEmail = request.UserEmail,
                    Service = "UserMicroservice"
                });
                throw new RpcException(new Status(StatusCode.Internal, $"Error al actualizar el usuario: {ex.Message}"));
            }
        }

        public override async Task<Empty> DeleteUser(DeleteUserRequest request, ServerCallContext context)
        {
            Log.Information($"Recibida petición para eliminar usuario con ID: {request.Id}");
            try
            {
                if (string.IsNullOrEmpty(request.Id)) throw new Exception("El ID no puede estar vacío");
                if (!int.TryParse(request.Id, out int userId)) throw new Exception("ID debe ser un número entero positivo");
                await _userService.DeleteUser(userId);
                Log.Information("Usuario eliminado correctamente");
                await _monitoringEventService.PublishActionEventAsync(new ActionEvent
                {
                    ActionMessage = $"Usuario eliminado con ID: {userId}",
                    UserId = request.UserId,
                    UserEmail = request.UserEmail,
                    UrlMethod = $"DELETE/usuarios/{userId}",
                    Service = "UserMicroservice"
                });
                return new Empty();
            }
            catch (Exception ex)
            {
                Log.Error($"Error al eliminar usuario: {ex.Message}");
                await _monitoringEventService.PublishErrorEventAsync(new ErrorEvent
                {
                    ErrorMessage = ex.Message,
                    UserId = request.UserId,
                    UserEmail = request.UserEmail,
                    Service = "UserMicroservice"
                });
                throw new RpcException(new Status(StatusCode.Internal, $"Error al eliminar el usuario: {ex.Message}"));
            }
        }
    }
}