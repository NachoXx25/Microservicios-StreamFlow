using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Identity.Client;
using MonitoringMicroservice.Protos;
using MonitoringMicroservice.src.Application.Services.Interfaces;
using Serilog;

namespace MonitoringMicroservice.Services
{
    public class MonitoringGrpcService : Protos.MonitoringGrpcService.MonitoringGrpcServiceBase
    {

        private readonly IMonitoringService _monitoringService;

        public MonitoringGrpcService(IMonitoringService monitoringService)
        {
            _monitoringService = monitoringService;
        }

        public override async Task<Protos.GetAllActionsResponse> GetAllActions(Protos.GetAllActionsRequest request, ServerCallContext context)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.UserData.Id))
                {
                    throw new Exception("No autenticado: se requiere un usuario autenticado para listar las acciones.");
                }

                if (request.UserData.Role.ToLower() != "administrador")
                {
                    throw new Exception("No autorizado: no tienes permisos para listar las acciones.");
                }

                var actions = await _monitoringService.GetAllActions();
                var response = new Protos.GetAllActionsResponse();

                foreach (var action in actions)
                {
                    response.Actions.Add(new Protos.Action
                    {
                        Id = action.Id,
                        UserId = action.UserId,
                        MethodUrl = action.MethodUrl,
                        UserEmail = action.UserEmail,
                        Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(action.Timestamp.ToUniversalTime()),
                        Name = action.Name,
                    });
                }

                return response;
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }

        public override async Task<Protos.GetAllErrorsResponse> GetAllErrors(Protos.GetAllErrorsRequest request, ServerCallContext context)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.UserData.Id))
                {
                    throw new Exception("No autenticado: se requiere un usuario autenticado para listar los errores.");
                }

                if (request.UserData.Role.ToLower() != "administrador")
                {
                    throw new Exception("No autorizado: no tienes permisos para listar los errores.");
                }

                var errors = await _monitoringService.GetAllErrors();
                var response = new Protos.GetAllErrorsResponse();

                foreach (var error in errors)
                {
                    response.Errors.Add(new Protos.Error
                    {
                        Id = error.Id,
                        UserId = error.UserId,
                        UserEmail = error.UserEmail,
                        Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(error.Timestamp.ToUniversalTime()),
                        Message = error.Message,
                    });
                }

                return response;
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }
        
        public override Task<CheckHealthResponse> CheckHealth(Empty request, ServerCallContext context)
        {
            try
            {
                return Task.FromResult(new CheckHealthResponse { IsRunning = true });
            }
            catch (Exception ex)
            {
                Log.Error($"Error al verificar la salud del servicio: {ex.Message}");
                throw new RpcException(new Status(StatusCode.Internal, "Error al verificar la salud del servicio de monitoreo."));
            }
        }
    }
}