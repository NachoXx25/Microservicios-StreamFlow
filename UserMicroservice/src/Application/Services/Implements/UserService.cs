using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserMicroservice.Services;
using UserMicroservice.src.Application.DTOs;
using UserMicroservice.src.Application.Services.Interfaces;
using UserMicroservice.src.Domain;
using UserMicroservice.src.Infrastructure.Repositories.Interfaces;

namespace UserMicroservice.src.Application.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IUserRepository _userRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly IUserEventService _userEventService;
        private static readonly Dictionary<string, string> ErrorTranslations = new ()
        {
            {"DuplicateUserName", "El nombre de usuario ya está en uso"},
            {"DuplicateEmail", "El correo electrónico ya está registrado"},
            {"InvalidUserName", "El nombre de usuario contiene caracteres inválidos"}
        };
        public UserService(UserManager<User> userManager, RoleManager<Role> roleManager, IUserRepository userRepository, IServiceProvider serviceProvider, IUserEventService userEventService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userEventService = userEventService;
            _userRepository = userRepository;
            _serviceProvider = serviceProvider;
        }
        

        /// <summary>
        /// Obtiene los datos de un usuario según su Id
        /// </summary>
        /// <param name="Id">Id del usuario</param>
        /// <returns>Datos del usuario</returns>
        public async Task<UserDTO> GetUserById(int Id)
        {
            var user = await _userManager.FindByIdAsync(Id.ToString()) ?? throw new Exception("El usuario especificado no existe.");
            var role = await _roleManager.FindByIdAsync(user.RoleId.ToString());
            var userDTO = new UserDTO(){
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                Role = role?.Name ?? string.Empty,
                CreatedAt = TimeZoneInfo.ConvertTime(user.CreatedAt, TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time"))
            };
            return userDTO;
        }  

        /// <summary>
        /// Hace un borrado lógico del usuario.
        /// </summary>
        /// <param name="Id">Id del usuario.</param>
        public async Task DeleteUser(int Id)
        {
            var user = await _userManager.FindByIdAsync(Id.ToString()) ?? throw new Exception("El usuario especificado no existe.");
            await _userRepository.DeleteUser(user);
            await _userEventService.PublishUserDeletedEvent(user);
        }

        /// <summary>
        /// Crea un nuevo usuario.
        /// </summary>
        /// <param name="userDTO">Datos del nuevo usuario</param>
        /// <returns>Retorna el usuario menos su contraseña</returns>
        public async Task<ReturnUserDTO> CreateUser(CreateUserDTO userDTO)
        {
            try
            {
                var guid = Guid.NewGuid();
                var role = await _roleManager.FindByNameAsync(userDTO.Role) ?? 
                    throw new Exception("El rol especificado no existe.");
                var user = new User(){
                    FirstName = userDTO.FirstName,
                    LastName = userDTO.LastName,
                    Email = userDTO.Email,
                    UserName = guid.ToString(),
                    RoleId = role.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Status = true
                };
                var result = await _userManager.CreateAsync(user, userDTO.Password);
                
                if (result.Succeeded)
                {
                    await _userEventService.PublishUserCreatedEvent(user);
                    return new ReturnUserDTO()
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email ?? string.Empty,
                        RoleName = role.Name ?? string.Empty,
                        CreatedAt = TimeZoneInfo.ConvertTime(user.CreatedAt, 
                            TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time")),
                        UpdatedAt = TimeZoneInfo.ConvertTime(user.UpdatedAt, 
                            TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time")),
                        IsActive = user.Status
                    };
                }
                else
                {
                    throw new Exception(TranslateError(result.Errors.First()));
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error en transacción: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Metodo para traducir los errores de Identity a español.	
        /// </summary>
        /// <param name="error"> Errores de identity</param>
        /// <returns>Errores de identity en español.</returns>
        public static string TranslateError(IdentityError error)
        {
            return ErrorTranslations.TryGetValue(error.Code, out string? translation) ? translation : error.Description;
        }

        /// <summary>
        /// Obtiene todos los usuarios.
        /// </summary>
        /// <param name="search">Parametros de busqueda</param>
        /// <returns>Lista de usuarios</returns>
        public async Task<IEnumerable<UserDTO>> GetAllUsers(SearchByDTO search)
        {
            if(string.IsNullOrWhiteSpace(search.FirstName) && 
               string.IsNullOrWhiteSpace(search.LastName) && 
               string.IsNullOrWhiteSpace(search.Email)) throw new Exception("Debe especificar al menos un parámetro de búsqueda.");
            var users = _userManager.Users.OrderBy( u => u.Id).AsQueryable();
            if(!string.IsNullOrWhiteSpace(search.FirstName))
            {
                users = users.Where(x => x.FirstName.ToLower().Contains(search.FirstName.ToLower()));
            }
            if(!string.IsNullOrWhiteSpace(search.LastName))
            {
                users = users.Where(x => x.LastName.ToLower().Contains(search.LastName.ToLower()));
            }
            if(!string.IsNullOrWhiteSpace(search.Email))
            {
                users = users.Where(x => x.Email != null && x.Email.ToLower().Contains(search.Email.ToLower()));
            }
            var pacificTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time");
            var result = await users.Where(u => u.Status == true).Select(user => new UserDTO()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                Role = user.Role.Name ?? string.Empty,
                CreatedAt = TimeZoneInfo.ConvertTime(user.CreatedAt, pacificTimeZone)
            }).ToListAsync();
            if(result.Count() == 0) throw new Exception("No se encontraron usuarios con los parámetros de búsqueda especificados.");
            return result;
        }

        /// <summary>
        /// Edita algunos parámetros del usuario
        /// </summary>
        /// <param name="updateUser">Atributos a editar</param>
        /// <param name="Id">Id del usuario a editar</param>
        /// <returns>Datos del usuario actualizado</returns>
        public async Task<ReturnUserDTO> UpdateUser(UpdateUserDTO updateUser, int Id)
        {
            var user = await _userManager.FindByIdAsync(Id.ToString()) ?? throw new Exception("El usuario especificado no existe.");
            if(string.IsNullOrWhiteSpace(updateUser.Email) && string.IsNullOrWhiteSpace(updateUser.FirstName) && string.IsNullOrWhiteSpace(updateUser.LastName)) throw new Exception("Debe modificar al menos un campo.");
            bool hasChanges = false;
            if(!string.IsNullOrWhiteSpace(updateUser.Email) && updateUser.Email != user.Email)
            {
                user.Email = updateUser.Email;
                hasChanges = true;
            }
            if(!string.IsNullOrWhiteSpace(updateUser.FirstName) && updateUser.FirstName != user.FirstName)
            {
                user.FirstName = updateUser.FirstName;
                hasChanges= true;

            }
            if(!string.IsNullOrWhiteSpace(updateUser.LastName) && updateUser.LastName != user.LastName)
            {
                user.LastName = updateUser.LastName;
                hasChanges = true;
            }
            user.UpdatedAt = DateTime.UtcNow;
            user.SecurityStamp = Guid.NewGuid().ToString();
            if(!hasChanges) throw new Exception("Debe modificar al menos un campo.");
            var role = await _roleManager.FindByIdAsync(user.RoleId.ToString()) ?? throw new Exception("Error en el sistema, vuelva a intentarlo más tarde.");
            await _userManager.UpdateAsync(user);
            await _userEventService.PublishUserUpdatedEvent(user);
            return new ReturnUserDTO(){
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                RoleName = role.Name ?? string.Empty,
                CreatedAt = TimeZoneInfo.ConvertTime(user.CreatedAt, 
                            TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time")),
                        UpdatedAt = TimeZoneInfo.ConvertTime(user.UpdatedAt, 
                            TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time")),
                        IsActive = user.Status
            };
        }
    }
}