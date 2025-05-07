using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserMicroservice.src.Application.DTOs;

namespace UserMicroservice.src.Application.Services.Interfaces
{
    public interface IUserService
    {
        /// <summary>
        /// Obtiene todos los usuarios.
        /// </summary>
        /// <param name="search">Parámetros de búsqueda</param>
        /// <returns>Lista de usuarios</returns>
        Task<IEnumerable<UserDTO>> GetAllUsers(SearchByDTO search);

        /// <summary>
        /// Obtiene los datos de un usuario según su Id
        /// </summary>
        /// <param name="Id">Id del usuario</param>
        /// <returns>Datos del usuario</returns>
        Task<UserDTO> GetUserById(int Id);

        /// <summary>
        /// Crea un nuevo usuario.
        /// </summary>
        /// <param name="userDTO">Datos del nuevo usuario</param>
        /// <returns>Retorna el usuario menos su contraseña</returns>
        Task<ReturnUserDTO> CreateUser(CreateUserDTO userDTO);

        /// <summary>
        /// Edita algunos parámetros del usuario
        /// </summary>
        /// <param name="updateUser">Atributos a editar</param>
        /// <param name="Id">Id del usuario a editar</param>
        /// <returns>Datos del usuario actualizado</returns>
        Task<ReturnUserDTO> UpdateUser(UpdateUserDTO updateUser, int Id);

        /// <summary>
        /// Hace un borrado lógico del usuario.
        /// </summary>
        /// <param name="Id">Id del usuario.</param>
        Task DeleteUser(int Id);
    }
}