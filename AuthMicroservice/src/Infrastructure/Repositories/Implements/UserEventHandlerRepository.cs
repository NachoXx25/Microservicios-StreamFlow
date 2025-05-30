using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthMicroservice.src.Domain.Models;
using AuthMicroservice.src.Infrastructure.Data;
using AuthMicroservice.src.Infrastructure.MessageBroker.Models;
using AuthMicroservice.src.Infrastructure.Repositories.Interfaces;
using Serilog;

namespace AuthMicroservice.src.Infrastructure.Repositories.Implements
{
    public class UserEventHandlerRepository : IUserEventHandlerRepository
    {
        private readonly DataContext _context;
        public UserEventHandlerRepository(DataContext context)
        {
            _context = context;
        }
        public async Task HandleUserCreatedEvent(UserCreatedEvent userEvent)
        {
            try
            {
                Log.Information("Usuario creado: {@UserEvent}", userEvent);

                var existingUser = await _context.Users.FindAsync(userEvent.Id);
                if (existingUser != null)
                {
                    Log.Warning("El usuario con ID {UserId} ya existe.", userEvent.Id);
                    return;
                }
                await _context.Users.AddAsync(new User
                {
                    Id = userEvent.Id,
                    FirstName = userEvent.FirstName,
                    LastName = userEvent.LastName,
                    UserName = Guid.NewGuid().ToString(),
                    Email = userEvent.Email,
                    RoleId = userEvent.RoleId,
                    Status = userEvent.Status,
                    PasswordHash = userEvent.PasswordHash,
                });
                await _context.SaveChangesAsync();
                Log.Information("Usuario creado con ID {UserId}", userEvent.Id);
            }catch (Exception ex)
            {
                Log.Error(ex, "Error al manejar el evento de creación de usuario: {@UserEvent}", userEvent);
                throw;
            }
            
        }

        public async Task HandleUserDeletedEvent(UserDeletedEvent userEvent)
        {
            try
            {
                Log.Information("Usuario eliminado: {@UserEvent}", userEvent);

                var existingUser = await _context.Users.FindAsync(userEvent.Id);
                if (existingUser == null)
                {
                    Log.Warning("El usuario con ID {UserId} no existe.", userEvent.Id);
                    return;
                }

                existingUser.Status = userEvent.Status;
                existingUser.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                Log.Information("Usuario eliminado con ID {UserId}", userEvent.Id);
            }catch (Exception ex)
            {
                Log.Error(ex, "Error al manejar el evento de eliminación de usuario: {@UserEvent}", userEvent);
                throw;
            }
        }

        public async Task HandleUserUpdatedEvent(UserUpdatedEvent userEvent)
        {
            try
            {
                Log.Information("Usuario actualizado: {@UserEvent}", userEvent);

                var existingUser = await _context.Users.FindAsync(userEvent.Id);
                if (existingUser == null)
                {
                    Log.Warning("El usuario con ID {UserId} no existe.", userEvent.Id);
                    return;
                }

                existingUser.FirstName = userEvent.FirstName;
                existingUser.LastName = userEvent.LastName;
                existingUser.Email = userEvent.Email;
                existingUser.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                Log.Information("Usuario actualizado con ID {UserId}", userEvent.Id);
            }catch (Exception ex)
            {
                Log.Error(ex, "Error al manejar el evento de actualización de usuario: {@UserEvent}", userEvent);
                throw;
            }
        }
    }
}