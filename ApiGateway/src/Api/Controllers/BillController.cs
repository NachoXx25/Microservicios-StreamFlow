using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.src.Application.DTOs.Bill;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.src.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class BillController : ControllerBase
    {

        private readonly Services.BillGrpcClient _billGrpcClient;

        public BillController(Services.BillGrpcClient billGrpcClient)
        {
            _billGrpcClient = billGrpcClient;
        }

        [HttpGet("facturas")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllBillsAsync([FromQuery] string? status)
        {
            try
            {
                if (!User.Identity?.IsAuthenticated == true)
                {
                    return Unauthorized(new { Error = "Se requiere autenticación para acceder a esta información." });
                }

                var userId = User.FindFirst("Id")?.Value;
                var userEmail = User.FindFirst("Email")?.Value;
                var userRole = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

                if (userRole != "Administrador" && userRole != "Cliente")
                {
                    return StatusCode(403, "No tienes permisos para acceder a esta información.");
                }

                var request = new Protos.BillService.GetAllBillsRequest
                {
                    UserId = userId ?? "",
                    UserRole = userRole ?? "",
                    UserEmail = userEmail ?? ""
                };

                if (!string.IsNullOrWhiteSpace(status))
                {
                    request.BillStatus = status;
                }

                var response = await _billGrpcClient.GetAllBillsAsync(request);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ha ocurrido un error cargando las facturas: {ex.Message}");
            }
        }

        [HttpGet("facturas/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBillByIdAsync(string id)
        {
            try
            {
                if (!User.Identity?.IsAuthenticated == true)
                {
                    return Unauthorized(new { Error = "Se requiere autenticación para acceder a esta información." });
                }

                var userId = User.FindFirst("Id")?.Value;
                var userEmail = User.FindFirst("Email")?.Value;
                var userRole = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

                if (userRole != "Administrador" && userRole != "Cliente")
                {
                    return StatusCode(403, "No tienes permisos para acceder a esta información.");
                }

                var request = new Protos.BillService.GetBillByIdRequest
                {
                    BillId = id,
                    UserId = userId ?? "",
                    UserRole = userRole ?? "",
                    UserEmail = userEmail ?? ""
                };

                var response = await _billGrpcClient.GetBillByIdAsync(request);

                if (response.Bill == null)
                {
                    return NotFound(new { Message = "Factura no encontrada." });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ha ocurrido un error cargando la factura: {ex.Message}");
            }
        }

        [HttpPost("facturas")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateBillAsync([FromBody] CreateBillDTO createBill)
        {
            try
            {
                if (!User.Identity?.IsAuthenticated == true)
                {
                    return Unauthorized(new { Error = "Se requiere autenticación para crear una factura." });
                }

                var userId = User.FindFirst("Id")?.Value;
                var userEmail = User.FindFirst("Email")?.Value;
                var userRole = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

                if (userRole != "Administrador")
                {
                    return StatusCode(403, "No tienes permisos para crear una factura.");
                }

                var request = new Protos.BillService.CreateBillRequest
                {
                    UserId = createBill.UserId,
                    BillStatus = createBill.BillStatus,
                    Amount = createBill.Amount,
                    UserEmail = userEmail ?? "",
                };

                var response = await _billGrpcClient.CreateBillAsync(request);

                return StatusCode(201, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ha ocurrido un error creando la factura: {ex.Message}");
            }
        }

        [HttpPatch("facturas/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateBillAsync(string id, [FromBody] string billStatus)
        {
            try
            {
                if (!User.Identity?.IsAuthenticated == true)
                {
                    return Unauthorized(new { Error = "Se requiere autenticación para actualizar una factura." });
                }

                var userId = User.FindFirst("Id")?.Value;
                var userEmail = User.FindFirst("Email")?.Value;
                var userRole = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

                if (userRole != "Administrador")
                {
                    return StatusCode(403, "No tienes permisos para actualizar una factura.");
                }

                var request = new Protos.BillService.UpdateBillRequest
                {
                    BillId = id,
                    BillStatus = billStatus,
                    UserData = new Protos.BillService.UserData
                    {
                        Id = userId ?? "",
                        Email = userEmail ?? "",
                    }
                };

                var response = await _billGrpcClient.UpdateBillAsync(request);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ha ocurrido un error actualizando la factura: {ex.Message}");
            }
        }

        [HttpDelete("facturas/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteBillAsync(string id)
        {
            try
            {
                if (!User.Identity?.IsAuthenticated == true)
                {
                    return Unauthorized(new { Error = "Se requiere autenticación para eliminar una factura." });
                }

                var userId = User.FindFirst("Id")?.Value;
                var userEmail = User.FindFirst("Email")?.Value;
                var userRole = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

                if (userRole != "Administrador")
                {
                    return StatusCode(403, "No tienes permisos para eliminar una factura.");
                }

                var request = new Protos.BillService.DeleteBillRequest
                {
                    BillId = id,
                    UserData = new Protos.BillService.UserData
                    {
                        Id = userId ?? "",
                        Email = userEmail ?? "",
                    }
                };

                var response = await _billGrpcClient.DeleteBillAsync(request);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ha ocurrido un error eliminando la factura: {ex.Message}");
            }
        }
    }
}