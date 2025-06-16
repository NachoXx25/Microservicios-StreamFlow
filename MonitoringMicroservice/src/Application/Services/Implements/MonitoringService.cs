using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonitoringMicroservice.src.Application.DTOs;
using MonitoringMicroservice.src.Application.Services.Interfaces;
using MonitoringMicroservice.src.Domain.Models;
using MonitoringMicroservice.src.Infrastructure.Repositories.Interfaces;

namespace MonitoringMicroservice.src.Application.Services.Implements
{
    public class MonitoringService : IMonitoringService
    {
        private readonly IActionRepository _actionRepository;
        private readonly IErrorRepository _errorRepository;
        
        public MonitoringService(IActionRepository actionRepository, IErrorRepository errorRepository)
        {
            _actionRepository = actionRepository;
            _errorRepository = errorRepository;
        }

        public async Task<List<ActionDTO>> GetAllActions()
        {
            var actions = await _actionRepository.GetAllActions();
           
            return actions.Select(a => new ActionDTO
            {
                Id = a.Id.ToString(),
                Name = a.Name,
                UserId = a.UserId,
                UserEmail = a.UserEmail,
                MethodUrl = a.MethodUrl,
                Timestamp = a.Timestamp
            }).ToList();
        }

        public async Task<List<ErrorDTO>> GetAllErrors()
        {
            var errors = await _errorRepository.GetAllErrors();

            return errors.Select(e => new ErrorDTO
            {
                Id = e.Id.ToString(),
                Message = e.Message,
                UserId = e.UserId,
                UserEmail = e.UserEmail,
                Timestamp = e.Timestamp
            }).ToList();
        }
    }
}