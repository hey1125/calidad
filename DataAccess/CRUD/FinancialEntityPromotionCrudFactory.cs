using DataAccess.DAO;
using DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.CRUD
{
    public class FinancialEntityPromotionCrudFactory : CrudFactory
    {
        public FinancialEntityPromotionCrudFactory()
        {
            _sqlDao = SqlDao.GetInstance();
        }

        /// <summary>
        /// Trae TODAS las promos vigentes de entidad financiera resolviendo el banco por la IBAN.
        /// El SP aplica: Status=Activa, fechas válidas, MinAmount (si aplica) y cupones disponibles.
        /// </summary>
        public List<T> RetrieveActiveByIBAN<T>(string iban, decimal amount, DateTime? refDate)
        {
            var list = new List<T>();
            var sqlOperation = new SqlOperation() { ProcedureName = "RET_ACTIVE_FE_PROMOTIONS_BY_IBAN_PR" };

            var dateNow = DateTime.Now.AddDays(1);

            sqlOperation.AddStringParameter("P_IBAN", iban);
            sqlOperation.AddDoubleParam("P_Amount", (double)amount);
            sqlOperation.AddDateTimeParam("P_RefDate", dateNow);

            var results = _sqlDao.ExecuteQueryProcedure(sqlOperation);

            foreach (var row in results)
            {
                var dto = BuildFinancialEntityPromotion(row);
                list.Add((T)Convert.ChangeType(dto, typeof(T)));
            }

            return list;
        }

        // === Overrides no usados aún ===
        public override void Create(BaseDTO baseDTO)
        {
            var promo = baseDTO as FinancialEntityPromotion
                ?? throw new ArgumentException("DTO inválido: se esperaba FinancialEntityPromotion.");

            var op = new SqlOperation { ProcedureName = "CRE_FINANCIAL_ENTITY_PROMOTION_PR" };

            // Requeridos
            op.AddStringParameter("P_FePromoId", promo.FePromoId);
            op.AddIntParam("P_FinancialEntityId", promo.FinancialEntityId);
            op.AddDateTimeParam("P_StartDate", promo.StartDate);
            op.AddDateTimeParam("P_EndDate", promo.EndDate);
            op.AddStringParameter("P_Status", promo.Status);

            // Opcionales (solo si tienen valor)
            if (promo.MinAmount.HasValue) op.AddDecimalParameter("P_MinAmount", promo.MinAmount.Value);
            if (promo.AmountPercentage.HasValue) op.AddDecimalParameter("P_AmountPercentage", promo.AmountPercentage.Value);
            if (promo.AmountDiscount.HasValue) op.AddDecimalParameter("P_AmountDiscount", promo.AmountDiscount.Value);
            if (promo.TotalCoupon.HasValue) op.AddIntParam("P_TotalCoupon", promo.TotalCoupon.Value);
            if (promo.UsedCoupon.HasValue) op.AddIntParam("P_UsedCoupon", promo.UsedCoupon.Value);
            if (!string.IsNullOrWhiteSpace(promo.Description))
                op.AddStringParameter("P_Description", promo.Description);

            var result = _sqlDao.ExecuteQueryProcedure(op);
            if (result.Count > 0 && result[0].ContainsKey("NewId"))
                promo.Id = Convert.ToInt32(result[0]["NewId"]);
        }

        public override T Retrieve<T>() => throw new NotImplementedException();
        public override List<T> RetrieveAll<T>() => throw new NotImplementedException();

        public List<T> RetrieveAllByEntity<T>(int financialEntityId)
        {
            var list = new List<T>();
            var op = new SqlOperation { ProcedureName = "RET_FE_PROMOTIONS_BY_ENTITY_PR" };
            op.AddIntParam("P_FinancialEntityId", financialEntityId);

            var results = _sqlDao.ExecuteQueryProcedure(op);
            foreach (var row in results)
            {
                var dto = BuildFinancialEntityPromotion(row);
                list.Add((T)Convert.ChangeType(dto, typeof(T)));
            }
            return list;
        }

        public override T RetrieveById<T>(int id)
        {
            var op = new SqlOperation { ProcedureName = "RET_FE_PROMOTION_BY_ID_PR" };
            op.AddIntParam("P_Id", id);

            var results = _sqlDao.ExecuteQueryProcedure(op);
            if (results == null || results.Count == 0)
                return default!; // null para tipos referencia

            var dto = BuildFinancialEntityPromotion(results[0]);
            return (T)Convert.ChangeType(dto, typeof(T));
        }

        public override void Update(BaseDTO baseDTO)
        {
            var promo = baseDTO as FinancialEntityPromotion
                ?? throw new ArgumentException("DTO inválido: se esperaba FinancialEntityPromotion.");
            if (promo.Id <= 0) throw new ArgumentException("Id inválido para actualización.");

            var op = new SqlOperation { ProcedureName = "UPD_FINANCIAL_ENTITY_PROMOTION_PR" };

            // Requeridos
            op.AddIntParam("P_Id", promo.Id);
            op.AddStringParameter("P_FePromoId", promo.FePromoId);
            op.AddIntParam("P_FinancialEntityId", promo.FinancialEntityId);
            op.AddDateTimeParam("P_StartDate", promo.StartDate);
            op.AddDateTimeParam("P_EndDate", promo.EndDate);
            op.AddStringParameter("P_Status", promo.Status);

            // Opcionales
            if (promo.MinAmount.HasValue) op.AddDecimalParameter("P_MinAmount", promo.MinAmount.Value);
            if (promo.AmountPercentage.HasValue) op.AddDecimalParameter("P_AmountPercentage", promo.AmountPercentage.Value);
            if (promo.AmountDiscount.HasValue) op.AddDecimalParameter("P_AmountDiscount", promo.AmountDiscount.Value);
            if (promo.TotalCoupon.HasValue) op.AddIntParam("P_TotalCoupon", promo.TotalCoupon.Value);
            if (promo.UsedCoupon.HasValue) op.AddIntParam("P_UsedCoupon", promo.UsedCoupon.Value);
            if (!string.IsNullOrWhiteSpace(promo.Description))
                op.AddStringParameter("P_Description", promo.Description);

            _sqlDao.ExecuteQueryProcedure(op); // Si quieres, lee "UpdatedId" del primer row.
        }
        public override void Delete(BaseDTO baseDTO)
        {
            var promo = baseDTO as FinancialEntityPromotion
                ?? throw new ArgumentException("DTO inválido: se esperaba FinancialEntityPromotion.");
            if (promo.Id <= 0) throw new ArgumentException("Id inválido para eliminación.");

            var op = new SqlOperation { ProcedureName = "DEL_FINANCIAL_ENTITY_PROMOTION_PR" };
            op.AddIntParam("P_Id", promo.Id);

            _sqlDao.ExecuteQueryProcedure(op); // Si quieres, lee "DeletedId" del primer row.
        }

        // === Mapper ===
        private FinancialEntityPromotion BuildFinancialEntityPromotion(Dictionary<string, object> row)
        {
            var dto = new FinancialEntityPromotion
            {
                Id = (int)row["Id"],
                Created = (DateTime)row["Created"],
                Updated = row["Updated"] == null || row["Updated"] == DBNull.Value ? DateTime.MinValue : (DateTime)row["Updated"],

                FePromoId = (string)row["FePromoId"],
                FinancialEntityId = Convert.ToInt32(row["FinancialEntityId"]),

                StartDate = (DateTime)row["StartDate"],
                EndDate = (DateTime)row["EndDate"],

                MinAmount = row["MinAmount"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["MinAmount"]),
                AmountPercentage = row["AmountPercentage"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["AmountPercentage"]),
                AmountDiscount = row["AmountDiscount"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(row["AmountDiscount"]),

                TotalCoupon = row["TotalCoupon"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["TotalCoupon"]),
                UsedCoupon = row["UsedCoupon"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["UsedCoupon"]),

                Status = (string)row["Status"],
                Description = row["Description"] == DBNull.Value ? null : (string)row["Description"]
            };

            // EffectiveDiscountAmount también viene del SP; se ignora en el DTO.
            return dto;
        }
    }
}