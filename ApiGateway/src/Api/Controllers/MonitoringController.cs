using System;
using System.Collections.Generic;
using System.Linq;
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
                if (User.Identity?.IsAuthenticated == false)
                {
                    return Unauthorized(new { Error = "Se requiere autenticación para acceder a esta información." });
                }
                if (!User.IsInRole("Administrador"))
                {
                    return StatusCode(403, new { Error = "No tienes permisos para acceder a esta información." });
                }
                var request = new Protos.MonitoringService.GetAllActionsRequest();
                var response = await _monitoringGrpcClient.GetAllActionsAsync(request);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("monitoreo/errores")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllErrors()
        {
            try
            {
                if (User.Identity?.IsAuthenticated == false)
                {
                    return Unauthorized(new { Error = "Se requiere autenticación para acceder a esta información." });
                }
                if (!User.IsInRole("Administrador"))
                {
                    return StatusCode(403, new { Error = "No tienes permisos para acceder a esta información." });
                }
                var request = new Protos.MonitoringService.GetAllErrorsRequest();
                var response = await _monitoringGrpcClient.GetAllErrorsAsync(request);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}