using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs
{
    public class Payment : BaseDTO
    {
        #region Campos de la base de datos (estructura real)

        // NOTA: Id (de BaseDTO) es la PK real (INT IDENTITY)

        // PaymentId es el código único del pago (NVARCHAR(20))
        public string PaymentId { get; set; } = string.Empty;

        // Cédula de quien hizo el pago
        public string NationalId { get; set; } = string.Empty;

        // ID del comercio que recibió el pago
        public int CommerceId { get; set; }

        // IBAN de la cuenta bancaria usada
        public string IBAN { get; set; } = string.Empty;

        // IDs de promociones (solo uno puede estar lleno a la vez)
        public string? CoPromoId { get; set; }      // Promoción de comercio
        public string? FePromoId { get; set; }      // Promoción de entidad financiera

        // Fecha específica del pago (diferente de Created)
        public DateTime PaymentDate { get; set; }

        // Monto original antes de descuentos
        public decimal GrossAmount { get; set; }

        // Descuento aplicado (si hay promoción)
        public decimal? AmountWDiscount { get; set; }

        // Comisiones
        public decimal? CoCommisionAmount { get; set; }  // Comisión del comercio
        public decimal? FeCommisionAmount { get; set; }  // Comisión del banco

        // CAMPOS CALCULADOS AUTOMÁTICAMENTE POR LA BD
        public decimal NetAmount { get; set; }           // GrossAmount - AmountWDiscount
        public decimal CommerceProfit { get; set; }      // NetAmount - CoCommisionAmount

        // Estado y descripción
        public string Status { get; set; } = "Pendiente";
        public string? Description { get; set; }

        #endregion

        #region Propiedades auxiliares para reportes (no están en la BD)

        // Nombres para mostrar en los reportes
        public string? PayerName { get; set; }
        public string? PayerEmail { get; set; }
        public string? CommerceName { get; set; }
        public string? CommerceEmail { get; set; }
        public string? BankName { get; set; }
        public string? BankEmail { get; set; }

        // Descripción de la promoción aplicada (viene de los SPs)
        public string? PromotionDescription { get; set; }

        #endregion

        #region Propiedades de conveniencia

        // Para saber rápido si se aplicó algún descuento
        public bool HasPromotion => AmountWDiscount.HasValue && AmountWDiscount.Value > 0;

        // Para saber qué tipo de promoción se aplicó
        public string? PromotionType
        {
            get
            {
                if (!string.IsNullOrEmpty(CoPromoId)) return "Comercio";
                if (!string.IsNullOrEmpty(FePromoId)) return "Banco";
                return null;
            }
        }

        // Porcentaje de descuento
        public decimal DiscountPercentage =>
            GrossAmount > 0 && AmountWDiscount.HasValue ?
            (AmountWDiscount.Value / GrossAmount) * 100 : 0;

        // Estados
        public bool IsCompleted => Status == "Pagado";
        public bool IsPending => Status == "Pendiente";

        // Total de comisiones de BilleTico
        public decimal TotalBilleTicoCommission =>
            (CoCommisionAmount ?? 0) + (FeCommisionAmount ?? 0);

        // Monto final que realmente pagó el cliente (equivalente a NetAmount)
        public decimal FinalAmount => NetAmount;

        // Monto total de descuento aplicado
        public decimal DiscountAmount => AmountWDiscount ?? 0;

        #endregion

        #region Métodos útiles

        // Genera un PaymentId único si no tiene uno
        public void GeneratePaymentId()
        {
            if (string.IsNullOrEmpty(PaymentId))
            {
                PaymentId = $"PAY{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(100, 999)}";
            }
        }

        // Valida que solo tenga una promoción
        public bool IsValidPromotion()
        {
            return !((!string.IsNullOrEmpty(CoPromoId)) && (!string.IsNullOrEmpty(FePromoId)));
        }

        // Para debugging
        public override string ToString()
        {
            return $"Pago {PaymentId} - Usuario: {NationalId}, Comercio: {CommerceId}, " +
                   $"Monto: ₡{NetAmount:N2}, Estado: {Status}, Fecha: {PaymentDate:yyyy-MM-dd}";
        }

        #endregion

        public void CalculateAllAmounts()
        {
            // Si hay descuento aplicado, calcularlo
            if (AmountWDiscount.HasValue && AmountWDiscount > 0)
            {
                NetAmount = GrossAmount - AmountWDiscount.Value;
            }
            else
            {
                NetAmount = GrossAmount;
                AmountWDiscount = 0;
            }

            // Calcular comisiones (asumiendo porcentajes estándar)
            // Estos porcentajes deberían venir de configuración
            const decimal CO_COMMISSION_RATE = 0.025m; // 2.5%
            const decimal FE_COMMISSION_RATE = 0.005m; // 0.5%

            CoCommisionAmount = NetAmount * CO_COMMISSION_RATE;
            FeCommisionAmount = NetAmount * FE_COMMISSION_RATE;

            // Lo que le queda al comercio
            CommerceProfit = NetAmount - CoCommisionAmount.Value - FeCommisionAmount.Value;

            // Establecer fecha si no está definida
            if (PaymentDate == default(DateTime))
            {
                PaymentDate = DateTime.Now;
            }

            // Establecer estado por defecto si no está definido
            if (string.IsNullOrEmpty(Status))
            {
                Status = "Pendiente";
            }
        }
    }
}