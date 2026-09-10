using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs
{
    public class ConfirmPaymentRequest
    {
        public int Id { get; set; }                  // Id (int) del pago
        public string IBAN { get; set; } = default!;
        public string? CoPromoId { get; set; }       // opcional
        public string? FePromoId { get; set; }       // opcional
    }
}