using DataAccess.CRUD;
using DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreApp
{
    public class FinancialEntityPromotionManager : BaseManager
    {
        public FinancialEntityPromotionManager() { }

        public List<FinancialEntityPromotion> GetAllByEntity(int financialEntityId)
        {
            try
            {
                if (financialEntityId <= 0) throw new ArgumentException("financialEntityId inválido.", nameof(financialEntityId));
                var crud = new FinancialEntityPromotionCrudFactory();
                return crud.RetrieveAllByEntity<FinancialEntityPromotion>(financialEntityId);
            }
            catch (Exception ex)
            {
                ManageException(ex);
                return new List<FinancialEntityPromotion>();
            }
        }

        public FinancialEntityPromotion? GetById(int id)
        {
            try
            {
                if (id <= 0) throw new ArgumentException("Id inválido.", nameof(id));
                var crud = new FinancialEntityPromotionCrudFactory();
                return crud.RetrieveById<FinancialEntityPromotion>(id);
            }
            catch (Exception ex)
            {
                ManageException(ex);
                return null;
            }
        }


        // Trae todas las promos vigentes de entidad financiera resolviendo el banco por IBAN
        public List<FinancialEntityPromotion> GetActiveByIBAN(string iban, decimal amount, DateTime? refDate = null)
        {
            try
            {
                var crud = new FinancialEntityPromotionCrudFactory();
                return crud.RetrieveActiveByIBAN<FinancialEntityPromotion>(iban, amount, refDate);
            }
            catch (Exception ex)
            {
                ManageException(ex);
                return new List<FinancialEntityPromotion>();
            }
        }

        // Selecciona la mejor promo del banco (si hay). Mismas reglas de desempate que comercio.
        public FinancialEntityPromotion? GetBestByIBAN(string iban, decimal amount, DateTime? refDate = null)
        {
            var promos = GetActiveByIBAN(iban, amount, refDate);
            return SelectBestFePromo(promos, amount);
        }

        private static FinancialEntityPromotion? SelectBestFePromo(List<FinancialEntityPromotion> promos, decimal amount)
        {
            if (promos == null || promos.Count == 0)
                return null;

            return promos
                .Select(p => new
                {
                    Promo = p,
                    Effective = ComputeEffectiveDiscount(p, amount)
                })
                .OrderByDescending(x => x.Effective)
                .ThenByDescending(x => x.Promo.AmountPercentage ?? 0m) // <— FIX
                .ThenByDescending(x => x.Promo.AmountDiscount ?? 0m)
                .ThenBy(x => x.Promo.EndDate)
                .Select(x => x.Promo)
                .FirstOrDefault();
        }

        private static decimal ComputeEffectiveDiscount(FinancialEntityPromotion p, decimal amount)
        {
            var pct = p.AmountPercentage.HasValue ? Math.Max(0m, p.AmountPercentage.Value) : 0m;
            var fix = p.AmountDiscount.HasValue ? Math.Max(0m, p.AmountDiscount.Value) : 0m;

            decimal fromPct = amount * (pct / 100m);
            decimal effective = Math.Max(fromPct, fix);

            if (effective < 0m) effective = 0m;
            if (effective > amount) effective = amount;

            return Math.Round(effective, 2);
        }

        public FinancialEntityPromotion? Create(FinancialEntityPromotion promo)
        {
            try
            {
                if (promo == null) throw new ArgumentNullException(nameof(promo));
                ValidateForWrite(promo, isUpdate: false);

                var crud = new FinancialEntityPromotionCrudFactory();
                crud.Create(promo); // SP devuelve NewId -> el Crud asigna promo.Id
                return promo;
            }
            catch (Exception ex)
            {
                ManageException(ex);
                return null;
            }
        }

        public bool Update(FinancialEntityPromotion promo)
        {
            try
            {
                if (promo == null) throw new ArgumentNullException(nameof(promo));
                if (promo.Id <= 0) throw new ArgumentException("Id inválido para actualización.", nameof(promo.Id));
                ValidateForWrite(promo, isUpdate: true);

                var crud = new FinancialEntityPromotionCrudFactory();
                crud.Update(promo); // SP setea Updated = GETDATE()
                return true;
            }
            catch (Exception ex)
            {
                ManageException(ex);
                return false;
            }
        }

        public bool Delete(int id)
        {
            try
            {
                if (id <= 0) throw new ArgumentException("Id inválido para eliminación.", nameof(id));

                var crud = new FinancialEntityPromotionCrudFactory();
                crud.Delete(new FinancialEntityPromotion { Id = id });
                return true;
            }
            catch (Exception ex)
            {
                ManageException(ex);
                return false;
            }
        }

        private static void ValidateForWrite(FinancialEntityPromotion p, bool isUpdate)
        {
            // Requeridos
            if (string.IsNullOrWhiteSpace(p.FePromoId))
                throw new ArgumentException("FePromoId es requerido.", nameof(p.FePromoId));
            if (p.FinancialEntityId <= 0)
                throw new ArgumentException("FinancialEntityId inválido.", nameof(p.FinancialEntityId));
            if (p.StartDate == default)
                throw new ArgumentException("StartDate es requerido.", nameof(p.StartDate));
            if (p.EndDate == default)
                throw new ArgumentException("EndDate es requerido.", nameof(p.EndDate));
            if (!string.Equals(p.Status, "Activa", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(p.Status, "Inactiva", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Status debe ser 'Activa' o 'Inactiva'.", nameof(p.Status));

            // Reglas alineadas a CHECK constraints
            if (p.EndDate < p.StartDate)
                throw new ArgumentException("EndDate debe ser mayor o igual que StartDate.", nameof(p.EndDate));

            if (p.UsedCoupon.HasValue)
            {
                if (!p.TotalCoupon.HasValue)
                    throw new ArgumentException("UsedCoupon requiere TotalCoupon.", nameof(p.TotalCoupon));
                if (p.UsedCoupon.Value > p.TotalCoupon.Value)
                    throw new ArgumentException("UsedCoupon no puede ser mayor que TotalCoupon.", nameof(p.UsedCoupon));
            }

            // Salvaguardas: no negativos
            if (p.MinAmount.HasValue && p.MinAmount.Value < 0)
                throw new ArgumentException("MinAmount no puede ser negativo.", nameof(p.MinAmount));
            if (p.AmountPercentage.HasValue && p.AmountPercentage.Value < 0)
                throw new ArgumentException("AmountPercentage no puede ser negativo.", nameof(p.AmountPercentage));
            if (p.AmountDiscount.HasValue && p.AmountDiscount.Value < 0)
                throw new ArgumentException("AmountDiscount no puede ser negativo.", nameof(p.AmountDiscount));
        }

    }
}