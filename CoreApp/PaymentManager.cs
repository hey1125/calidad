using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using DataAccess.CRUD;
using DTOs;
// using DTOs.Requests; // <-- descomenta si moviste ConfirmPaymentRequest a DTOs.Requests

namespace Core.Managers
{
    public class PaymentManager
    {
        private readonly PaymentCrudFactory _crud;

        public PaymentManager()
        {
            _crud = new PaymentCrudFactory();
        }

        // Regla de negocio principal para crear un pago/cobro
        public void Create(Payment payment)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));

            if (string.IsNullOrWhiteSpace(payment.NationalId))
                throw new Exception("La cédula del usuario es obligatoria.");

            if (payment.CommerceId <= 0)
                throw new Exception("Id de comercio inválido.");

            if (payment.GrossAmount <= 0)
                throw new Exception("El monto bruto debe ser mayor a 0.");

            if (string.IsNullOrWhiteSpace(payment.PaymentId))
                payment.GeneratePaymentId();

            if (payment.PaymentDate == default)
                payment.PaymentDate = DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(payment.Status))
                payment.Status = "Pendiente";

            if (payment.Status.Equals("Pagado", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(payment.IBAN))
                    throw new Exception("El IBAN es obligatorio para confirmar el pago.");
                // ✅ regex corregida (un solo \d dentro del string verbatim)
                if (!Regex.IsMatch(payment.IBAN, @"^[A-Z]{2}\d{20}$"))
                    throw new Exception("IBAN inválido. Debe tener 2 letras + 20 dígitos (22 chars).");
            }
            else
            {
                // En 'Pendiente' NO guardamos IBAN
                payment.IBAN = null;
            }

            if (payment.Status.Equals("Pendiente", StringComparison.OrdinalIgnoreCase))
            {
                // En creación del cobro NO hay promo
                payment.CoPromoId = null;
                payment.FePromoId = null;

                // AmountWDiscount = monto del DESCUENTO (sin promo -> 0)
                payment.AmountWDiscount = 0m;

                // Comisión del comercio (si no vino, 0)
                payment.CoCommisionAmount ??= 0m;

                // Comisión financiera NO se calcula en cobro
                payment.FeCommisionAmount = null;
            }

            _crud.Create(payment);
        }

        public Payment ConfirmPayment(ConfirmPaymentRequest req)
        {
            if (req == null) throw new ArgumentNullException(nameof(req));
            if (req.Id <= 0) throw new Exception("Id inválido.");
            if (string.IsNullOrWhiteSpace(req.IBAN)) throw new Exception("IBAN requerido.");
            if (!Regex.IsMatch(req.IBAN, @"^[A-Z]{2}\d{20}$")) throw new Exception("IBAN inválido (CR + 20 dígitos).");
            if (!string.IsNullOrWhiteSpace(req.CoPromoId) && !string.IsNullOrWhiteSpace(req.FePromoId))
                throw new Exception("Solo se permite una promo (comercio o financiera).");

            return _crud.Confirm(req.Id, req.IBAN.ToUpperInvariant(), req.CoPromoId, req.FePromoId);
        }

        /*public void ConfirmPayment(ConfirmPaymentRequest req)
        {
            // Validaciones mínimas
            if (req == null) throw new ArgumentNullException(nameof(req));
            if (req.Id <= 0) throw new Exception("Id inválido.");
            if (string.IsNullOrWhiteSpace(req.IBAN)) throw new Exception("IBAN requerido.");
            // ✅ regex corregida
            if (!Regex.IsMatch(req.IBAN, @"^[A-Z]{2}\d{20}$")) throw new Exception("IBAN inválido (CR + 20 dígitos).");
            if (!string.IsNullOrWhiteSpace(req.CoPromoId) && !string.IsNullOrWhiteSpace(req.FePromoId))
                throw new Exception("Solo se permite una promo (comercio o financiera).");

            // Delega a la BD (cuando implementes el SP de confirmación)
            _crud.Confirm(req.Id, req.IBAN, req.CoPromoId, req.FePromoId);
        }*/

        // Cambiar estado (ej. de Pendiente -> Pagado / Rechazado)
        public void UpdateStatus(int id, string newStatus)
        {
            if (id <= 0) throw new Exception("Id inválido.");
            if (string.IsNullOrWhiteSpace(newStatus)) throw new Exception("El nuevo estado es obligatorio.");

            var payment = new Payment
            {
                Id = id,
                Status = newStatus
            };

            _crud.Update(payment);
        }

        public Payment RetrieveById(int id)
        {
            if (id <= 0) throw new Exception("Id inválido.");
            return _crud.RetrieveById<Payment>(id);
        }

        public List<Payment> RetrieveAll()
        {
            return _crud.RetrieveAll<Payment>();
        }

        // ===== Reportes / Historias de Usuario =====

        // HU 7.1: Historial del usuario final
        public List<Payment> GetUserPaymentHistory(string nationalId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrWhiteSpace(nationalId))
                throw new Exception("NationalId es obligatorio.");

            return _crud.GetUserPaymentHistory(nationalId, startDate, endDate);
        }

        public List<Payment> GetUserPaymentHistoryModule(string nationalId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrWhiteSpace(nationalId))
                throw new Exception("NationalId es obligatorio.");

            return _crud.GetUserPaymentHistoryModule(nationalId, startDate, endDate);
        }

        // HU 7.2: Pagos recibidos por un comercio
        public List<Payment> GetCommerceReceivedPayments(int commerceId, DateTime? startDate = null, DateTime? endDate = null, string status = null)
        {
            if (commerceId <= 0) throw new Exception("CommerceId inválido.");
            return _crud.GetCommerceReceivedPayments(commerceId, startDate, endDate, status);
        }

        // HU 7.3: Pagos por banco + stats
        public List<Payment> GetBankPayments(string bankCode, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrWhiteSpace(bankCode))
                throw new Exception("El código bancario es obligatorio.");

            return _crud.GetBankPayments(bankCode, startDate, endDate);
        }

        public object GetBankStats(string bankCode)
        {
            if (string.IsNullOrWhiteSpace(bankCode))
                throw new Exception("El código bancario es obligatorio.");

            return _crud.GetBankStats(bankCode);
        }

        public void Delete(int id)
        {
            if (id <= 0) throw new Exception("Id inválido.");
            _crud.Delete(new Payment { Id = id });
        }
    }
}