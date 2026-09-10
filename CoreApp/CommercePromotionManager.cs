using DataAccess.CRUD;
using DTOs;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreApp
{
    public class CommercePromotionManager : BaseManager
    {
        public CommercePromotionManager() { }

        public List<CommercePromotion> GetAllByCommerce(int commerceId)
        {
            try
            {
                var crud = new CommercePromotionCrudFactory();
                return crud.RetrieveAllByCommerce<CommercePromotion>(commerceId);
            }
            catch (Exception ex)
            {
                ManageException(ex);
                return new List<CommercePromotion>();
            }
        }

        public CommercePromotion? GetById(int id)
        {
            try
            {
                if (id <= 0) throw new ArgumentException("Id inválido.", nameof(id));
                var crud = new CommercePromotionCrudFactory();
                return crud.RetrieveById<CommercePromotion>(id);
            }
            catch (Exception ex)
            {
                ManageException(ex);
                return null;
            }
        }



        // Trae todas las promos vigentes del comercio para un monto dado
        public List<CommercePromotion> GetActiveByCommerce(int commerceId, decimal amount, DateTime? refDate = null)
        {
            try
            {
                var crud = new CommercePromotionCrudFactory();
                return crud.RetrieveActiveByCommerce<CommercePromotion>(commerceId, amount, refDate);
            }
            catch (Exception ex)
            {
                ManageException(ex);
                return new List<CommercePromotion>();
            }
        }

        // Selecciona la mejor promo del comercio (si hay). Reglas:
        // 1) Mayor descuento efectivo sobre el monto
        // 2) Si empata: mayor porcentaje
        // 3) Si empata: mayor descuento fijo
        // 4) Si empata: la que vence primero
        public CommercePromotion? GetBestByCommerce(int commerceId, decimal amount, DateTime? refDate = null)
        {
            var promos = GetActiveByCommerce(commerceId, amount, refDate);
            return SelectBestCommercePromo(promos, amount);
        }

        private static CommercePromotion? SelectBestCommercePromo(List<CommercePromotion> promos, decimal amount)
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
                .ThenByDescending(x => x.Promo.AmountPercentage ?? 0m) 
                .ThenByDescending(x => x.Promo.AmountDiscount ?? 0m)   
                .ThenBy(x => x.Promo.EndDate)
                .Select(x => x.Promo)
                .FirstOrDefault();
        }

        private static decimal ComputeEffectiveDiscount(CommercePromotion p, decimal amount)
        {
            // Salvaguardas por si alguna data viene doblemente informada
            var pct = p.AmountPercentage.HasValue ? Math.Max(0m, p.AmountPercentage.Value) : 0m;
            var fix = p.AmountDiscount.HasValue ? Math.Max(0m, p.AmountDiscount.Value) : 0m;

            decimal fromPct = amount * (pct / 100m);
            decimal effective = Math.Max(fromPct, fix);

            if (effective < 0m) effective = 0m;
            if (effective > amount) effective = amount;

            return Math.Round(effective, 2);
        }

        public CommercePromotion? Create(CommercePromotion promo)
        {
            try
            {
                if (promo == null) throw new ArgumentNullException(nameof(promo));
                ValidateForWrite(promo, isUpdate: false);

                var crud = new CommercePromotionCrudFactory();
                crud.Create(promo); // El SP asigna Created=GETDATE() y devuelve NewId -> crud setea promo.Id
                return promo;
            }
            catch (Exception ex)
            {
                ManageException(ex);
                return null;
            }
        }

        public bool Update(CommercePromotion promo)
        {
            try
            {
                if (promo == null) throw new ArgumentNullException(nameof(promo));
                if (promo.Id <= 0) throw new ArgumentException("Id inválido para actualización.", nameof(promo.Id));
                ValidateForWrite(promo, isUpdate: true);

                var crud = new CommercePromotionCrudFactory();
                crud.Update(promo); // El SP asigna Updated=GETDATE()
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

                var crud = new CommercePromotionCrudFactory();
                // El Delete del CrudFactory espera un DTO con el Id
                crud.Delete(new CommercePromotion { Id = id });
                return true;
            }
            catch (Exception ex)
            {
                ManageException(ex);
                return false;
            }
        }

        private static void ValidateForWrite(CommercePromotion p, bool isUpdate)
        {
            // Requeridos
            if (string.IsNullOrWhiteSpace(p.CoPromoId))
                throw new ArgumentException("CoPromoId es requerido.", nameof(p.CoPromoId));
            if (p.CommerceId <= 0)
                throw new ArgumentException("CommerceId inválido.", nameof(p.CommerceId));
            if (p.StartDate == default)
                throw new ArgumentException("StartDate es requerido.", nameof(p.StartDate));
            if (p.EndDate == default)
                throw new ArgumentException("EndDate es requerido.", nameof(p.EndDate));
            if (!string.Equals(p.Status, "Activa", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(p.Status, "Inactiva", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Status debe ser 'Activa' o 'Inactiva'.", nameof(p.Status));

            // Reglas de negocio alineadas a CHECK constraints
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