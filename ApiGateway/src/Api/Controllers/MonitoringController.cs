using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.src.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class MonitoringController : ControllerBase
    {
        private readonly Services.MonitoringGrpcClient _monitoringGrpcClient;

        public MonitoringController(Services.MonitoringGrpcClient monitoringGrpcClient)
        {
            _monitoringGrpcClient = monitoringGrpcClient;
        }

        [HttpGet("monitoreo/acciones")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllActions()
        {
            try
            {
                var userId = User?.FindFirst("Id")?.Value;
                var userEmail = User?.FindFirst("Email")?.Value;
                var userRole = User?.FindFirst(ClaimTypes.Role)?.Value;

                var request = new Protos.MonitoringService.GetAllActionsRequest()
                {
                    UserData = new Protos.MonitoringService.UserData
                    {
                        Id = userId ?? string.Empty,
                        Email = userEmail ?? string.Empty,
                        Role = userRole ?? string.Empty
                    }
                };
                var response = await _monitoringGrpcClient.GetAllActionsAsync(request);

                return Ok(response);
            }
            catch (RpcException ex)
            {
                var errorMessage = ex.Status.Detail.ToLower();

                if (errorMessage.Contains("no autenticado"))
                {
                    return Unauthorized(new { error = ex.Status.Detail });
                }
                if (errorMessage.Contains("error en el sistema"))
                {
                    return StatusCode(500, new { error = "Error en el sistema, intente más tarde" });
                }
                if (errorMessage.Contains("no encontrado"))
                {
                    return NotFound(new { error = ex.Status.Detail });
                }
                if (errorMessage.Contains("no tienes permisos"))
                {
                    return StatusCode(403, new { error = "No tienes permisos para realizar esta acción" });
                }
                return BadRequest(new { error = ex.Status.Detail });
            }
        }

        [HttpGet("monitoreo/errores")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllErrors()
        {
            try
            {
                var userId = User?.FindFirst("Id")?.Value;
                var userEmail = User?.FindFirst("Email")?.Value;
                var userRole = User?.FindFirst(ClaimTypes.Role)?.Value;

                var request = new Protos.MonitoringService.GetAllErrorsRequest()
                {
                    UserData = new Protos.MonitoringService.UserData
                    {
                        Id = userId ?? string.Empty,
                        Email = userEmail ?? string.Empty,
                        Role = userRole ?? string.Empty
                    }
                };
                var response = await _monitoringGrpcClient.GetAllErrorsAsync(request);

                return Ok(response);
            }
            catch (RpcException ex)
            {
                var errorMessage = ex.Status.Detail.ToLower();

                if (errorMessage.Contains("no autenticado"))
                {
                    return Unauthorized(new { error = ex.Status.Detail });
                }
                if (errorMessage.Contains("error en el sistema"))
                {
                    return StatusCode(500, new { error = "Error en el sistema, intente más tarde" });
                }
                if (errorMessage.Contains("no encontrado"))
                {
                    return NotFound(new { error = ex.Status.Detail });
                }
                if (errorMessage.Contains("no tienes permisos"))
                {
                    return StatusCode(403, new { error = "No tienes permisos para realizar esta acción" });
                }
                return BadRequest(new { error = ex.Status.Detail });
            }
        }
    }
}