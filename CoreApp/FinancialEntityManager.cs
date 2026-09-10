using DataAccess.CRUD;
using DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoreApp
{
    public class FinancialEntityManager : BaseManager
    {
        public FinancialEntityManager() { }

        // =============================================
        // MÉTODOS HU 2.1 - REGISTRO
        // =============================================

        public void Create(FinancialEntity financialEntity)
        {
            try
            {
                if (!ValidateRequiredFields(financialEntity))
                    throw new Exception("Todos los campos obligatorios deben estar completos");

                if (!ValidateEmailFormat(financialEntity.Email))
                    throw new Exception("Ingrese un correo electrónico válido");

                if (!ValidatePhoneFormat(financialEntity.Phone))
                    throw new Exception("El teléfono debe tener 8 dígitos numéricos");

                if (!ValidateLegalIdFormat(financialEntity.LegalId))
                    throw new Exception("La cédula jurídica debe contener 10 dígitos numéricos");

                if (!ValidateBankCodeFormat(financialEntity.BankCode))
                    throw new Exception("El código bancario debe contener 3 dígitos numéricos");

                if (!ValidateNationalIdFormat(financialEntity.NationalId))
                    throw new Exception("La cédula debe contener 9 dígitos numéricos");


                var feCrud = new FinancialEntityCrudFactory();

                // 1. Verificar si la entidad YA EXISTE por código bancario
                var existingByBankCode = feCrud.RetrieveByBankCode<FinancialEntity>(financialEntity);

                if (existingByBankCode != null)
                {
                    // La entidad YA EXISTE
                    throw new Exception($"Entidad financiera ya existe con ID: {existingByBankCode.Id}");
                }

                // 2. Verificar si existe por cédula jurídica (pero no por código bancario)
                var existingByLegalId = feCrud.RetrieveByLegalId<FinancialEntity>(financialEntity);
                if (existingByLegalId != null)
                {
                    throw new Exception("Ya existe una entidad financiera con esta cédula jurídica pero diferente código bancario");
                }

                // 3. La entidad NO EXISTE - crear nueva entidad y asociar usuario
                financialEntity.Status = "Pendiente";
                financialEntity.CommissionRate = 0.00m;

                feCrud.Create(financialEntity);

            }
            catch (Exception ex)
            {
                ManageException(ex);
            }
        }

        public FinancialEntity RetrieveByBankCode(string bankCode)
        {
            var feCrud = new FinancialEntityCrudFactory();
            var dto = new FinancialEntity { BankCode = bankCode };
            return feCrud.RetrieveByBankCode<FinancialEntity>(dto);
        }

        public List<FinancialEntity> GetEntitiesByUser(int userId)
        {
            var relCrud = new FinalUserFinancialEntityRolCrudFactory();
            return relCrud.RetrieveEntitiesByUser<FinancialEntity>(userId);
        }

        // =============================================
        // MÉTODOS HU 2.2 - APROBACIÓN
        // =============================================

        public List<FinancialEntity> GetPendingEntities()
        {
            var feCrud = new FinancialEntityCrudFactory();
            return feCrud.RetrieveByStatus<FinancialEntity>("Pendiente");
        }

        public FinancialEntity GetEntityDetails(int entityId)
        {
            var feCrud = new FinancialEntityCrudFactory();
            return feCrud.RetrieveById<FinancialEntity>(entityId);
        }

        public void ApproveEntity(int entityId, decimal commissionRate)
        {
            if (!ValidateCommissionRate(commissionRate))
            {
                throw new Exception("La comisión debe estar entre 0.01% y 99.99%");
            }

            var feCrud = new FinancialEntityCrudFactory();

            var entity = feCrud.RetrieveById<FinancialEntity>(entityId);
            if (entity == null)
            {
                throw new Exception("La entidad financiera no existe");
            }

            if (entity.Status != "Pendiente")
            {
                throw new Exception("Solo se pueden aprobar entidades en estado pendiente");
            }

            feCrud.ApproveEntity(entityId, commissionRate);
        }

        public void RejectEntity(int entityId)
        {
            var feCrud = new FinancialEntityCrudFactory();

            var entity = feCrud.RetrieveById<FinancialEntity>(entityId);
            if (entity == null)
            {
                throw new Exception("La entidad financiera no existe");
            }

            if (entity.Status != "Pendiente")
            {
                throw new Exception("Solo se pueden rechazar entidades en estado pendiente");
            }

            feCrud.RejectEntity(entityId);
        }

        public bool CanEntityOperate(int entityId)
        {
            var feCrud = new FinancialEntityCrudFactory();
            var entity = feCrud.RetrieveById<FinancialEntity>(entityId);

            return entity != null && entity.Status == "Activa";
        }

        public List<FinancialEntity> GetActiveEntities()
        {
            var feCrud = new FinancialEntityCrudFactory();
            return feCrud.RetrieveByStatus<FinancialEntity>("Activa");
        }

        public List<FinancialEntity> GetRejectedEntities()
        {
            var feCrud = new FinancialEntityCrudFactory();
            return feCrud.RetrieveByStatus<FinancialEntity>("Rechazada");
        }

        // =============================================
        // MÉTODOS DE VALIDACIÓN PRIVADOS
        // =============================================

        private bool ValidateRequiredFields(FinancialEntity financialEntity)
        {
            return !string.IsNullOrEmpty(financialEntity.LegalId) &&
                   !string.IsNullOrEmpty(financialEntity.BankCode) &&
                   !string.IsNullOrEmpty(financialEntity.Name) &&
                   !string.IsNullOrEmpty(financialEntity.Phone) &&
                   !string.IsNullOrEmpty(financialEntity.Email) &&
                   financialEntity.Latitude != 0 &&
                   financialEntity.Longitude != 0 &&
                   !string.IsNullOrEmpty(financialEntity.NationalId);
        }

        private bool ValidateEmailFormat(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }

        private bool ValidatePhoneFormat(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return false;

            string phonePattern = @"^[0-9]{8}$";
            return Regex.IsMatch(phone, phonePattern);
        }

        private bool ValidateLegalIdFormat(string legalId)
        {
            if (string.IsNullOrEmpty(legalId))
                return false;

            string legalIdPattern = @"^[0-9]{10}$";
            return Regex.IsMatch(legalId, legalIdPattern);
        }

        private bool ValidateBankCodeFormat(string bankCode)
        {
            if (string.IsNullOrEmpty(bankCode))
                return false;

            string bankCodePattern = @"^[0-9]{3}$";
            return Regex.IsMatch(bankCode, bankCodePattern);
        }

        private bool ValidateCommissionRate(decimal commissionRate)
        {
            return commissionRate >= 0.01m && commissionRate <= 99.99m;
        }

        private bool ValidateNationalIdFormat(string nationalId)
        {
            if (string.IsNullOrEmpty(nationalId))
                return false;

            // Debe tener exactamente 9 dígitos numéricos
            string nationalIdPattern = @"^[0-9]{9}$";
            return Regex.IsMatch(nationalId, nationalIdPattern);
        }

        public List<FinancialEntity> GetActiveEntitiesByUser(int userId)
        {
            var relCrud = new FinalUserFinancialEntityRolCrudFactory();
            var allUserEntities = relCrud.RetrieveEntitiesByUser<FinancialEntity>(userId);

            // Filtrar solo las que están en estado "Activa"
            return allUserEntities.Where(e => e.Status == "Activa").ToList();
        }

    }
}