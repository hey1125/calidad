using DataAccess.DAO;
using DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.CRUD
{
    // Este factory maneja todas las consultas de pagos para los reportes
    // Básicamente conecta con la base de datos para traer la info que necesitamos mostrar
    public class PaymentCrudFactory : CrudFactory
    {
        public PaymentCrudFactory()
        {
            _sqlDao = SqlDao.GetInstance();
        }

        #region Métodos CRUD básicos - CORREGIDOS para estructura real

        public override void Create(BaseDTO baseDTO)
        {
            var payment = baseDTO as Payment;
            var sqlOperation = new SqlOperation() { ProcedureName = "CRE_PAYMENT_PR" };

            // Generar PaymentId si no tiene uno
            if (string.IsNullOrEmpty(payment.PaymentId))
                payment.GeneratePaymentId();

            sqlOperation.AddStringParameter("P_PaymentId", payment.PaymentId);
            sqlOperation.AddStringParameter("P_NationalId", payment.NationalId);
            sqlOperation.AddIntParam("P_CommerceId", payment.CommerceId);

            // IBAN puede ser NULL en 'Pendiente' (tu AddStringParameter ya maneja null -> DBNull.Value)
            sqlOperation.AddStringParameter("P_IBAN", payment.IBAN);

            // Promos opcionales: si son null, que viajen como NULL
            sqlOperation.AddStringParameter("P_CoPromoId", payment.CoPromoId);
            sqlOperation.AddStringParameter("P_FePromoId", payment.FePromoId);

            sqlOperation.AddDateTimeParam("P_PaymentDate", payment.PaymentDate);
            sqlOperation.AddDecimalParameter("P_GrossAmount", payment.GrossAmount);

            // Monto de DESCUENTO (no precio con descuento). En 'Pendiente' tu manager lo deja en 0.
            sqlOperation.AddDecimalParameter("P_AmountWDiscount", payment.AmountWDiscount ?? 0m);

            // Comisión del comercio: 0 si no viene
            sqlOperation.AddDecimalParameter("P_CoCommisionAmount", payment.CoCommisionAmount ?? 0m);

            // ⚠️ Banco: si es null NO LO ENVIAMOS para que el SP use el default = NULL
            if (payment.FeCommisionAmount.HasValue)
                sqlOperation.AddDecimalParameter("P_FeCommisionAmount", payment.FeCommisionAmount.Value);
            // si no, no agregamos el parámetro

            sqlOperation.AddStringParameter("P_Status", payment.Status);
            sqlOperation.AddStringParameter("P_Description", payment.Description);

            _sqlDao.ExecuteProcedure(sqlOperation);
        }

        public Payment Confirm(int id, string iban, string coPromoId, string fePromoId)
        {
            var op = new SqlOperation() { ProcedureName = "CONFIRM_PAYMENT_PR" };
            op.AddIntParam("P_Id", id);
            op.AddStringParameter("P_IBAN", iban);
            op.AddStringParameter("P_CoPromoId", coPromoId);
            op.AddStringParameter("P_FePromoId", fePromoId);
            var dateNow = DateTime.Now.AddDays(1);
            op.AddDateTimeParam("P_RefDate", dateNow);

            var result = _sqlDao.ExecuteQueryProcedure(op);
            if (result.Count > 0)
                return BuildPayment(result[0]);   // <-- usa tu mapper

            return null;
        }

        /*public void Confirm(int id, string iban, string? coPromoId, string? fePromoId)
        {
            var op = new SqlOperation() { ProcedureName = "CONFIRM_PAYMENT_PR" };
            op.AddIntParam("P_Id", id);
            op.AddStringParameter("P_IBAN", iban);
            op.AddStringParameter("P_CoPromoId", string.IsNullOrWhiteSpace(coPromoId) ? null : coPromoId);
            op.AddStringParameter("P_FePromoId", string.IsNullOrWhiteSpace(fePromoId) ? null : fePromoId);
            _sqlDao.ExecuteProcedure(op);
        }*/

        public override void Update(BaseDTO baseDTO)
        {
            var payment = baseDTO as Payment;
            var sqlOperation = new SqlOperation() { ProcedureName = "UPD_PAYMENT_PR" };

            sqlOperation.AddIntParam("P_Id", payment.Id);  // Usar Id real, no PaymentId
            sqlOperation.AddStringParameter("P_Status", payment.Status);

            _sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override T RetrieveById<T>(int id)
        {
            var sqlOperation = new SqlOperation() { ProcedureName = "RET_PAYMENT_BY_ID_PR" };
            sqlOperation.AddIntParam("P_Id", id);  // Usar Id real

            var lstResults = _sqlDao.ExecuteQueryProcedure(sqlOperation);

            if (lstResults.Count > 0)
            {
                var payment = BuildPayment(lstResults[0]);
                return (T)Convert.ChangeType(payment, typeof(T));
            }

            return default(T);
        }

        #endregion

        #region Métodos para construir objetos Payment - CORREGIDOS PARA ESTRUCTURA REAL

        // Para el historial básico de pagos - VERSIÓN CORREGIDA PARA ESTRUCTURA REAL
        private Payment BuildPayment(Dictionary<string, object> row)
        {
            return new Payment
            {
                // BaseDTO fields
                Id = Convert.ToInt32(row["Id"]),
                Created = Convert.ToDateTime(row["Created"]),
                Updated = row["Updated"] != DBNull.Value ? Convert.ToDateTime(row["Updated"]) : DateTime.MinValue,

                // Payment fields - NOMBRES REALES DE LA TABLA
                PaymentId = row["PaymentId"]?.ToString() ?? string.Empty,
                NationalId = row["NationalId"]?.ToString() ?? string.Empty,
                CommerceId = Convert.ToInt32(row["CommerceId"]),
                IBAN = row["IBAN"]?.ToString() ?? string.Empty,

                // Promociones (pueden ser null)
                CoPromoId = row["CoPromoId"] != DBNull.Value ? row["CoPromoId"]?.ToString() : null,
                FePromoId = row["FePromoId"] != DBNull.Value ? row["FePromoId"]?.ToString() : null,

                PaymentDate = Convert.ToDateTime(row["PaymentDate"]),
                GrossAmount = Convert.ToDecimal(row["GrossAmount"]),

                // Campos que pueden ser null
                AmountWDiscount = row["AmountWDiscount"] != DBNull.Value ?
                    Convert.ToDecimal(row["AmountWDiscount"]) : null,
                CoCommisionAmount = row["CoCommisionAmount"] != DBNull.Value ?
                    Convert.ToDecimal(row["CoCommisionAmount"]) : null,
                FeCommisionAmount = row["FeCommisionAmount"] != DBNull.Value ?
                    Convert.ToDecimal(row["FeCommisionAmount"]) : null,

                // CAMPOS CALCULADOS AUTOMÁTICAMENTE POR LA BD
                NetAmount = Convert.ToDecimal(row["NetAmount"]),
                CommerceProfit = Convert.ToDecimal(row["CommerceProfit"]),

                Status = row["Status"]?.ToString() ?? "Pendiente",
                Description = row["Description"] != DBNull.Value ? row["Description"]?.ToString() : null
            };
        }

        // Para el historial del usuario final (HU 7.1) - CORREGIDO
        private Payment BuildUserPaymentHistory(Dictionary<string, object> row)
        {
            var payment = BuildPayment(row);

            // Info adicional para el historial del usuario
            payment.CommerceName = row.ContainsKey("CommerceName") ?
                row["CommerceName"]?.ToString() : null;
            payment.PromotionDescription = row.ContainsKey("PromotionDescription") ?
                row["PromotionDescription"]?.ToString() : null;

            return payment;
        }

        // Para los pagos recibidos del comercio (HU 7.2) - CORREGIDO
        private Payment BuildCommerceReceivedPayment(Dictionary<string, object> row)
        {
            var payment = BuildPayment(row);

            // Info adicional para el comercio
            payment.PayerName = row.ContainsKey("PayerName") ?
                row["PayerName"]?.ToString() : null;
            payment.PromotionDescription = row.ContainsKey("PromotionDescription") ?
                row["PromotionDescription"]?.ToString() : null;

            return payment;
        }

        // Para los pagos del banco (HU 7.3) - CORREGIDO
        private Payment BuildBankPayment(Dictionary<string, object> row)
        {
            var payment = BuildPayment(row);

            // Info adicional para el banco
            payment.CommerceName = row.ContainsKey("CommerceName") ?
                row["CommerceName"]?.ToString() : null;

            return payment;
        }

        #endregion
       
      public override void Delete(BaseDTO baseDTO)
        {
            var payment = baseDTO as Payment;
            var sqlOperation = new SqlOperation() { ProcedureName = "DEL_PAYMENT_PR" };
            sqlOperation.AddIntParam("P_Id", payment.Id);

            _sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override List<T> RetrieveAll<T>()
        {
            var lstPayments = new List<T>();
            var sqlOperation = new SqlOperation() { ProcedureName = "RET_ALL_PAYMENTS_PR" };

            var lstResults = _sqlDao.ExecuteQueryProcedure(sqlOperation);

            foreach (var row in lstResults)
            {
                var payment = BuildPayment(row);
                lstPayments.Add((T)Convert.ChangeType(payment, typeof(T)));
            }

            return lstPayments;
        }

        public override T Retrieve<T>()
        {
            throw new NotImplementedException("Usa RetrieveById o los métodos específicos para cada HU");
        }


        /// <summary>
        /// HU (Vista Usuario): Pagos PENDIENTES por nationalId
        /// </summary>
        public List<Payment> GetUserPendingPayments(string nationalId)
        {
            var op = new SqlOperation { ProcedureName = "GET_USER_PENDING_PAYMENTS_PR" };
            op.AddStringParameter("P_NationalId", nationalId);

            var rows = _sqlDao.ExecuteQueryProcedure(op);

            var list = new List<Payment>();
            foreach (var row in rows)
                list.Add(BuildUserPendingRow(row));

            return list;
        }

        /// <summary>
        /// Mapper ligero y tolerante a columnas faltantes para la vista de pendientes
        /// </summary>
        private Payment BuildUserPendingRow(Dictionary<string, object> row)
        {
            // Helpers locales para evitar KeyNotFound/DBNull
            T Get<T>(string key, T defaultValue = default)
            {
                if (!row.ContainsKey(key) || row[key] == null || row[key] == DBNull.Value) return defaultValue;
                return (T)Convert.ChangeType(row[key], typeof(T));
            }

            string Gets(string key) => Get<string>(key, null);

            // OJO: AmountWDiscount = monto de descuento en tu BD (no “precio con descuento”)
            var payment = new Payment
            {
                Id = Get<int>("Id"),
                Created = Get<DateTime>("Created"),
                Updated = Get<DateTime>("Updated", DateTime.MinValue),

                PaymentId = Gets("PaymentId") ?? string.Empty,
                NationalId = Gets("NationalId") ?? string.Empty,
                CommerceId = Get<int>("CommerceId"),
                IBAN = Gets("IBAN"),                       // puede venir NULL en Pendiente
                CoPromoId = Gets("CoPromoId"),
                FePromoId = Gets("FePromoId"),
                PaymentDate = Get<DateTime>("PaymentDate"), // tu esquema lo define NOT NULL

                GrossAmount = Get<decimal>("GrossAmount"),
                AmountWDiscount = row.ContainsKey("AmountWDiscount") && row["AmountWDiscount"] != DBNull.Value
                                    ? Get<decimal>("AmountWDiscount")
                                    : (decimal?)null,
                CoCommisionAmount = row.ContainsKey("CoCommisionAmount") && row["CoCommisionAmount"] != DBNull.Value
                                    ? Get<decimal>("CoCommisionAmount")
                                    : (decimal?)null,
                FeCommisionAmount = row.ContainsKey("FeCommisionAmount") && row["FeCommisionAmount"] != DBNull.Value
                                    ? Get<decimal>("FeCommisionAmount")
                                    : (decimal?)null,

                // calculadas por la BD; las enviamos si vinieron en el SELECT
                NetAmount = row.ContainsKey("NetAmount") && row["NetAmount"] != DBNull.Value
                                ? Get<decimal>("NetAmount")
                                : 0m,
                CommerceProfit = row.ContainsKey("CommerceProfit") && row["CommerceProfit"] != DBNull.Value
                                ? Get<decimal>("CommerceProfit")
                                : 0m,

                Status = Gets("Status") ?? "Pendiente",
                Description = Gets("Description"),

                // Campos “extra” útiles en la vista
                CommerceName = Gets("CommerceName")
            };

            return payment;
        }

        /// <summary>
        /// HU 7.1 - Obtiene historial de pagos COMPLETADOS de un usuario específico
        /// CORREGIDO: Filtra en C# para solo retornar pagos con status "Pagado"
        /// </summary>
        public List<Payment> GetUserPaymentHistory(string nationalId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var sqlOperation = new SqlOperation() { ProcedureName = "GET_USER_PAYMENT_HISTORY_PR" };

            sqlOperation.AddStringParameter("P_NationalId", nationalId);

            if (startDate.HasValue)
                sqlOperation.AddDateTimeParam("P_StartDate", startDate.Value);
            if (endDate.HasValue)
                sqlOperation.AddDateTimeParam("P_EndDate", endDate.Value);

            var result = _sqlDao.ExecuteQueryProcedure(sqlOperation);

            var lstPayments = new List<Payment>();
            foreach (var row in result)
            {
                var payment = BuildUserPaymentHistory(row);

                // FILTRO AGREGADO: Solo agregar pagos completados
                if (payment.Status != null && payment.Status.Equals("Pagado", StringComparison.OrdinalIgnoreCase))
                {
                    lstPayments.Add(payment);
                }
            }

            return lstPayments;
        }

        public List<Payment> GetUserPaymentHistoryModule(string nationalId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var sqlOperation = new SqlOperation() { ProcedureName = "GET_USER_PAYMENT_HISTORY_MODULE_PR" };

            sqlOperation.AddStringParameter("P_NationalId", nationalId);

            if (startDate.HasValue)
                sqlOperation.AddDateTimeParam("P_StartDate", startDate.Value);
            if (endDate.HasValue)
                sqlOperation.AddDateTimeParam("P_EndDate", endDate.Value);

            var result = _sqlDao.ExecuteQueryProcedure(sqlOperation);

            var lstPayments = new List<Payment>();
            foreach (var row in result)
            {
                var payment = BuildUserPaymentHistory(row);
                lstPayments.Add(payment);
            }

            return lstPayments;
        }

        /// <summary>
        /// HU 7.2 - Obtiene pagos recibidos por un comercio específico
        /// </summary>
        public List<Payment> GetCommerceReceivedPayments(int commerceId, DateTime? startDate = null, DateTime? endDate = null, string status = null)
        {
            var sqlOperation = new SqlOperation() { ProcedureName = "GET_COMMERCE_RECEIVED_PAYMENTS_PR" };

            sqlOperation.AddIntParam("P_CommerceId", commerceId);

            if (startDate.HasValue)
                sqlOperation.AddDateTimeParam("P_StartDate", startDate.Value);
            if (endDate.HasValue)
                sqlOperation.AddDateTimeParam("P_EndDate", endDate.Value);
            if (!string.IsNullOrEmpty(status))
                sqlOperation.AddStringParameter("P_Status", status);

            var result = _sqlDao.ExecuteQueryProcedure(sqlOperation);

            var lstPayments = new List<Payment>();
            foreach (var row in result)
            {
                var payment = BuildCommerceReceivedPayment(row);
                lstPayments.Add(payment);
            }

            return lstPayments;
        }

        /// <summary>
        /// HU 7.3 - Obtiene pagos realizados con cuentas de un banco específico
        /// </summary>
        public List<Payment> GetBankPayments(string bankCode, DateTime? startDate = null, DateTime? endDate = null)
        {
            var sqlOperation = new SqlOperation() { ProcedureName = "GET_BANK_PAYMENTS_PR" };

            sqlOperation.AddStringParameter("P_BankCode", bankCode);

            if (startDate.HasValue)
                sqlOperation.AddDateTimeParam("P_StartDate", startDate.Value);
            if (endDate.HasValue)
                sqlOperation.AddDateTimeParam("P_EndDate", endDate.Value);

            var result = _sqlDao.ExecuteQueryProcedure(sqlOperation);

            var lstPayments = new List<Payment>();
            foreach (var row in result)
            {
                var payment = BuildBankPayment(row);
                lstPayments.Add(payment);
            }

            return lstPayments;
        }

        /// <summary>
        /// HU 7.3 - Obtiene estadísticas del banco (totales, conteos, etc.)
        /// </summary>
        public object GetBankStats(string bankCode)
        {
            var sqlOperation = new SqlOperation() { ProcedureName = "GET_BANK_STATS_PR" };

            sqlOperation.AddStringParameter("P_BankCode", bankCode);

            var result = _sqlDao.ExecuteQueryProcedure(sqlOperation);

            if (result.Any())
            {
                var stats = result.First();
                return new
                {
                    TotalPayments = Convert.ToInt32(stats["TotalPayments"]),
                    TotalAmount = Convert.ToDecimal(stats["TotalAmount"]),
                    TotalCommissions = Convert.ToDecimal(stats["TotalCommissions"]),
                    PendingPayments = Convert.ToInt32(stats["PendingPayments"]),
                    CompletedPayments = Convert.ToInt32(stats["CompletedPayments"])
                };
            }

            return new { TotalPayments = 0, TotalAmount = 0m, TotalCommissions = 0m, PendingPayments = 0, CompletedPayments = 0 };
        }
    }
}