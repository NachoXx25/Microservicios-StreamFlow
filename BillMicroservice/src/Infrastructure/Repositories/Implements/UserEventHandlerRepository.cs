using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BillMicroservice.src.Domain.Models.Bill;
using BillMicroservice.src.Domain.Models.User;
using BillMicroservice.src.Infrastructure.Data;
using BillMicroservice.src.Infrastructure.MessageBroker.Models;
using BillMicroservice.src.Infrastructure.Repositories.Interfaces;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BillMicroservice.src.Infrastructure.Repositories.Implements
{
    public class UserEventHandlerRepository : IUserEventHandlerRepository
    {
        private readonly BillContext _context;

        public UserEventHandlerRepository(BillContext context)
        {
            _context = context;
        }

        public async Task HandleUserCreated(UserCreatedEvent userCreatedEvent)
        {
            try
            {
                Log.Information("Usuario creado: {@UserCreatedEvent}", userCreatedEvent);

                var existingUser = await _context.Users.FindAsync(userCreatedEvent.Id);

                if (existingUser != null)
                {
                    Log.Warning("El usuario con ID {UserId} ya existe en la base de datos.", userCreatedEvent.Id);
                    return;
                }

                await _context.Users.AddAsync(
                    new User
                    {
                        FirstName = userCreatedEvent.FirstName,
                        LastName = userCreatedEvent.LastName,
                        UserName = Guid.NewGuid().ToString(),
                        Email = userCreatedEvent.Email,
                        Status = userCreatedEvent.Status,
                        RoleId = userCreatedEvent.RoleId,
                    }
                );
                await _context.SaveChangesAsync();
                Log.Information("Usuario creado exitosamente: {@UserCreatedEvent}", userCreatedEvent);

                var usersCount = await _context.Users.CountAsync();

                if (usersCount < 100)
                {
                    Log.Information("Se han creado menos de 100 usuarios, no se pueden ejecutar los seeders de facturas.");
                    return;
                }
                else
                {
                    if (!await _context.Bills.AnyAsync())
                    {
                        Log.Information("Se han creado más de 100 usuarios, se ejecutarán los seeders de facturas.");
                        await TriggerSeedersIfNeeded();
                    }
                    else
                    {
                        Log.Information("Ya existen facturas en la base de datos, no se ejecutarán los seeders de facturas.");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al manejar el evento de creación de usuario: {@UserCreatedEvent}", userCreatedEvent);
                throw;
            }
        }

        public async Task HandleUserDeleted(UserDeletedEvent userDeletedEvent)
        {
            try
            {
                Log.Information("Usuario eliminado: {@UserDeletedEvent}", userDeletedEvent);

                var existingUser = await _context.Users.FindAsync(userDeletedEvent.Id) ?? throw new KeyNotFoundException($"Usuario con ID {userDeletedEvent.Id} no encontrado.");

                existingUser.Status = false;

                await _context.SaveChangesAsync();

                Log.Information("Usuario eliminado con ID {VideoId}", userDeletedEvent.Id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al manejar el evento de eliminación de usuario: {@UserDeletedEvent}", userDeletedEvent);
                throw;
            }
        }

        public async Task HandleUserUpdated(UserUpdatedEvent userUpdatedEvent)
        {
            try
            {
                Log.Information("Usuario actualizado: {@UserUpdatedEvent}", userUpdatedEvent);

                var existingUser = await _context.Users.FindAsync(userUpdatedEvent.Id) ?? throw new KeyNotFoundException($"Usuario con ID {userUpdatedEvent.Id} no encontrado.");

                existingUser.FirstName = userUpdatedEvent.FirstName;
                existingUser.LastName = userUpdatedEvent.LastName;
                existingUser.Email = userUpdatedEvent.Email;

                await _context.SaveChangesAsync();
                Log.Information("Usuario actualizado con ID {UserId}", userUpdatedEvent.Id);
            }
            catch (Exception)
            {
                Log.Error("Error al manejar el evento de actualización de usuario: {@UserUpdatedEvent}", userUpdatedEvent);
                throw;
            }
        }
        
        private async Task TriggerSeedersIfNeeded()
        {
            try
            {
                var userIds = await _context.Users.AsNoTracking()
                                    .Select(v => v.Id)
                                    .ToListAsync();
                
                var statusesId = await _context.Statuses.AsNoTracking()
                                    .Select(s => s.Id)
                                    .ToListAsync();

                var paidStatusId = _context.Statuses.First(s => s.Name == "Pagado").Id;

                var faker = new Faker<Bill>()
                    .RuleFor(b => b.UserId, f => f.PickRandom(userIds))
                    .RuleFor(b => b.StatusId, f => f.PickRandom(statusesId))
                    .RuleFor(b => b.AmountToPay, f => (int)f.Finance.Amount(10, 1000))
                    .RuleFor(b => b.CreatedAt, f => f.Date.Past(1))
                    .RuleFor(b => b.PaymentDate, (f, b) =>
                    {
                        if (b.StatusId == paidStatusId)
                            return f.Date.Between(b.CreatedAt, DateTime.Now);
                        else
                            return null;
                    });

                _context.Bills.AddRange(faker.Generate(350));
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error ejecutando seeders de facturas");
            }
        }   
    }
}