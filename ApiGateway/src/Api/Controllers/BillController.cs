using System.Security.Claims;
using ApiGateway.src.Application.DTOs.Bill;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
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
                var userId = User.FindFirst("Id")?.Value;
                var userEmail = User.FindFirst("Email")?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

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
                if (errorMessage.Contains("no encontrada"))
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

        [HttpGet("facturas/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBillByIdAsync(string id)
        {
            try
            {
                var userId = User.FindFirst("Id")?.Value;
                var userEmail = User.FindFirst("Email")?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var request = new Protos.BillService.GetBillByIdRequest
                {
                    BillId = id,
                    UserId = userId ?? "",
                    UserRole = userRole ?? "",
                    UserEmail = userEmail ?? ""
                };

                var response = await _billGrpcClient.GetBillByIdAsync(request);

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
                if (errorMessage.Contains("no encontrada"))
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

        [HttpPost("facturas")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateBillAsync([FromBody] CreateBillDTO createBill)
        {
            try
            {
                var userId = User.FindFirst("Id")?.Value;
                var userEmail = User.FindFirst("Email")?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var request = new Protos.BillService.CreateBillRequest
                {
                    UserId = createBill.UserId,
                    BillStatus = createBill.BillStatus,
                    Amount = createBill.Amount,
                    UserEmail = userEmail ?? "",
                    UserRole = userRole ?? "",
                };

                var response = await _billGrpcClient.CreateBillAsync(request);

                return StatusCode(201, response);
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
                if (errorMessage.Contains("no encontrada"))
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

        [HttpPatch("facturas/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateBillAsync(string id, [FromBody] string billStatus)
        {
            try
            {
                var userId = User.FindFirst("Id")?.Value;
                var userEmail = User.FindFirst("Email")?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var request = new Protos.BillService.UpdateBillRequest
                {
                    BillId = id,
                    BillStatus = billStatus,
                    UserData = new Protos.BillService.UserData
                    {
                        Id = userId ?? "",
                        Email = userEmail ?? "",
                        Role = userRole ?? ""
                    }
                };

                var response = await _billGrpcClient.UpdateBillAsync(request);

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
                if (errorMessage.Contains("no encontrada"))
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

        [HttpDelete("facturas/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteBillAsync(string id)
        {
            try
            {
                var userId = User.FindFirst("Id")?.Value;
                var userEmail = User.FindFirst("Email")?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var request = new Protos.BillService.DeleteBillRequest
                {
                    BillId = id,
                    UserData = new Protos.BillService.UserData
                    {
                        Id = userId ?? "",
                        Email = userEmail ?? "",
                        Role = userRole ?? ""
                    }
                };

                var response = await _billGrpcClient.DeleteBillAsync(request);

                return NoContent();
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
                if (errorMessage.Contains("no encontrada"))
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