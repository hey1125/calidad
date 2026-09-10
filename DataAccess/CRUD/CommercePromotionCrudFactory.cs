using DataAccess.DAO;
using DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.CRUD
{
    public class CommercePromotionCrudFactory : CrudFactory
    {
        public CommercePromotionCrudFactory()
        {
            _sqlDao = SqlDao.GetInstance();
        }

        /// <summary>
        /// Trae TODAS las promociones vigentes de comercio para un CommerceId y monto dado.
        /// El SP aplica: Status=Activa, fechas válidas, MinAmount (si aplica) y cupones disponibles.
        /// </summary>
        public List<T> RetrieveActiveByCommerce<T>(int commerceId, decimal amount, DateTime? refDate = null)
        {
            var list = new List<T>();
            var sqlOperation = new SqlOperation() { ProcedureName = "RET_ACTIVE_COMMERCE_PROMOTIONS_BY_COMMERCE_PR" };

            sqlOperation.AddIntParam("P_CommerceId", commerceId);
            sqlOperation.AddDoubleParam("P_Amount", (double)amount);
            if (refDate.HasValue)
            {
                // Si tu SqlOperation no tiene AddDateTimeParam, omití este parámetro y el SP usará el default en SQL.
                sqlOperation.AddDateTimeParam("P_RefDate", refDate.Value);
            }

            var results = _sqlDao.ExecuteQueryProcedure(sqlOperation);

            foreach (var row in results)
            {
                var dto = BuildCommercePromotion(row);
                list.Add((T)Convert.ChangeType(dto, typeof(T)));
            }

            return list;
        }

        // === Overrides no usados aún ===
        public override void Create(BaseDTO baseDTO)
        {
            var promo = baseDTO as CommercePromotion ?? throw new ArgumentException("DTO inválido: se esperaba CommercePromotion.");

            var op = new SqlOperation { ProcedureName = "CRE_COMMERCE_PROMOTION_PR" };

            // Requeridos
            op.AddStringParameter("P_CoPromoId", promo.CoPromoId);
            op.AddIntParam("P_CommerceId", promo.CommerceId);
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

            var result = _sqlDao.ExecuteQueryProcedure(op);
            if (result.Count > 0 && result[0].ContainsKey("NewId"))
                promo.Id = Convert.ToInt32(result[0]["NewId"]);
        }

        public override T Retrieve<T>() => throw new NotImplementedException();
        public override List<T> RetrieveAll<T>() => throw new NotImplementedException();
        public override T RetrieveById<T>(int id)
        {
            var op = new SqlOperation { ProcedureName = "RET_COMMERCE_PROMOTION_BY_ID_PR" };
            op.AddIntParam("P_Id", id);

            var results = _sqlDao.ExecuteQueryProcedure(op);
            if (results == null || results.Count == 0)
                return default!; // null para tipos referencia

            var dto = BuildCommercePromotion(results[0]);
            return (T)Convert.ChangeType(dto, typeof(T));
        }

        public override void Update(BaseDTO baseDTO)
        {
            var promo = baseDTO as CommercePromotion ?? throw new ArgumentException("DTO inválido: se esperaba CommercePromotion.");
            if (promo.Id <= 0) throw new ArgumentException("Id inválido para actualización.");

            var op = new SqlOperation { ProcedureName = "UPD_COMMERCE_PROMOTION_PR" };

            // Requeridos
            op.AddIntParam("P_Id", promo.Id);
            op.AddStringParameter("P_CoPromoId", promo.CoPromoId);
            op.AddIntParam("P_CommerceId", promo.CommerceId);
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

            _sqlDao.ExecuteQueryProcedure(op); // El SP devuelve UpdatedId si lo necesitas leer.
        }
        public override void Delete(BaseDTO baseDTO)
        {
            var promo = baseDTO as CommercePromotion ?? throw new ArgumentException("DTO inválido: se esperaba CommercePromotion.");
            if (promo.Id <= 0) throw new ArgumentException("Id inválido para eliminación.");

            var op = new SqlOperation { ProcedureName = "DEL_COMMERCE_PROMOTION_PR" };
            op.AddIntParam("P_Id", promo.Id);

            _sqlDao.ExecuteQueryProcedure(op); // El SP devuelve DeletedId si lo necesitas leer.
        }

        public List<T> RetrieveAllByCommerce<T>(int commerceId)
        {
            var list = new List<T>();
            var op = new SqlOperation { ProcedureName = "RET_COMMERCE_PROMOTIONS_BY_COMMERCE_PR" };
            op.AddIntParam("P_CommerceId", commerceId);

            var results = _sqlDao.ExecuteQueryProcedure(op);
            foreach (var row in results)
            {
                var dto = BuildCommercePromotion(row);
                list.Add((T)Convert.ChangeType(dto, typeof(T)));
            }
            return list;
        }


        // === Mapper ===
        private CommercePromotion BuildCommercePromotion(Dictionary<string, object> row)
        {
            var dto = new CommercePromotion
            {
                Id = (int)row["Id"],
                Created = (DateTime)row["Created"],
                Updated = row["Updated"] == null || row["Updated"] == DBNull.Value ? DateTime.MinValue : (DateTime)row["Updated"],

                CoPromoId = (string)row["CoPromoId"],
                CommerceId = Convert.ToInt32(row["CommerceId"]),

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

            // El SP también devuelve EffectiveDiscountAmount, pero el DTO no lo incluye. Se ignora aquí.
            return dto;
        }
    }
}
