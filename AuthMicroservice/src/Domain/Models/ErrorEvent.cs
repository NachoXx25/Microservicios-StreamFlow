using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserMicroservice.src.Domain.Models
{
    public class ErrorEvent
    {
        public string ErrorMessage { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;
    }
}