using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BillMicroservice.src.Application.DTOs
{
    public class CreateBillDTO
    {
        [Required(ErrorMessage = "El id de usuario es requerido.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "El id de usuario debe ser un número entero positivo.")]
        public required int UserId { get; set; }

        [Required(ErrorMessage = "El estado de la factura es requerido.")]
        [RegularExpression(@"^(Pagado|Pendiente|Vencido)$", ErrorMessage = "El estado de la factura debe ser sólo 'Pagado', 'Pendiente' o 'Vencido'.")]
        public required string StatusName { get; set; }

        [Required(ErrorMessage = "El monto a pagar es requerido.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "El monto a pagar debe ser un número entero positivo.")]
        [Range(0, int.MaxValue, ErrorMessage = "El monto a pagar debe ser mayor que cero.")]
        public required int AmountToPay { get; set; }
    }
}