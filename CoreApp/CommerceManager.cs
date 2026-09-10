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
    public class CommerceManager : BaseManager
    {
        public CommerceManager() { }

        public void Create(Commerce commerce)
        {
            try
            {
                if (!ValidateRequiredFields(commerce))
                    throw new Exception("Todos los campos obligatorios deben estar completos");

                if (!ValidateEmailFormat(commerce.Email))
                    throw new Exception("Ingrese un correo electrónico válido");

                if (!ValidatePhoneFormat(commerce.Phone))
                    throw new Exception("El teléfono debe tener 8 dígitos numéricos");

                if (!ValidateLegalIdFormat(commerce.LegalId))
                    throw new Exception("La cédula jurídica debe contener 10 dígitos numéricos");

                if (!ValidateIBAN(commerce.IBAN))
                    throw new Exception("Formato de IBAN inválido. Debe comenzar con 'CR' seguido de 20 dígitos");

                if (!ValidateNationalIdFormat(commerce.NationalId))
                    throw new Exception("La cédula debe contener 9 dígitos numéricos");

                var cCrud = new CommerceCrudFactory();

                var existingByIBAN = cCrud.RetrieveByIBAN<Commerce>(commerce);
                if (existingByIBAN != null)
                    throw new Exception("Este IBAN ya está siendo utilizado en otro usuario, entidad o comercio");

                var existingByLegalId = cCrud.RetrieveByLegalId<Commerce>(commerce);
                if (existingByLegalId != null)
                    throw new Exception("Esta cédula jurídica ya está registrada");

                // Asignar estado y comisión inicial
                commerce.Status = "Pendiente";
                commerce.CommissionRate = 0.00m;

                cCrud.Create(commerce);
            }
            catch (Exception ex)
            {
                ManageException(ex);
            }
        }

        // =============================================
        // MÉTODOS HU 3.2 - APROBACIÓN
        // =============================================

        /*
         * T24: Obtener lista de comercios pendientes de aprobación
         */

        public List<Commerce> GetPendingCommerce()
        {
            var cCrud = new CommerceCrudFactory();
            return cCrud.RetrieveByStatus<Commerce>("Pendiente");
        }

        /*
         * T25: Vista detallada de una solicitud específica
         */

        public Commerce GetEntityDetails(int commerceId)
        {
            var cCrud = new CommerceCrudFactory();
            return cCrud.RetrieveById<Commerce>(commerceId);
        }

        /*
         * T26: Aprobar comercio y agregar con comisión
         */

        public void ApproveCommerce(int commerceId, decimal commissionRate)
        {
            // Validar que la comisión sea válida
            if (!ValidateCommissionRate(commissionRate))
            {
                throw new Exception("La comisión debe estar entre 0.01% y 99.99%");
            }

            var cCrud = new CommerceCrudFactory();

            // Verificar que el comercio existe y está pendiente
            var commerce = cCrud.RetrieveById<Commerce>(commerceId);
            if (commerce == null)
            {
                throw new Exception("El comercio no existe.");
            }

            if (commerce.Status != "Pendiente")
            {
                throw new Exception("Solo se pueden aprobar comercios en estado pendiente");
            }

            // Aprobar el comercio y establecer la comisión
            cCrud.ApproveCommerce(commerceId, commissionRate);
        }

        /*
         * T26: Rechazar comercio
         */

        public void RejectCommerce(int commerceId)
        {
            var cCrud = new CommerceCrudFactory();

            // Verificar que el comercio existe y está pendiente
            var commerce = cCrud.RetrieveById<Commerce>(commerceId);
            if (commerce == null)
            {
                throw new Exception("El comercio no existe.");
            }

            if (commerce.Status != "Pendiente")
            {
                throw new Exception("Solo se pueden rechazar comercios en estado pendiente");
            }

            // Rechazar el comercio
            cCrud.RejectCommerce(commerceId);
        }

        /*
         * T27: Notificar al comercio que fue aprobado o rechazado
         */

        /*
         * T28: Validar que solo comercios activos puedan operar
         */

        public bool CanCommerceOperate(int commerceId)
        {
            var cCrud = new CommerceCrudFactory();
            var commerce = cCrud.RetrieveById<Commerce>(commerceId);

            return commerce != null && commerce.Status == "Activo";
        }

        public List<Commerce> GetActiveCommerces()
        {
            var cCrud = new CommerceCrudFactory();
            return cCrud.RetrieveByStatus<Commerce>("Activo");
        }

        public List<Commerce> GetPendingCommerces()
        {
            var cCrud = new CommerceCrudFactory();
            return cCrud.RetrieveByStatus<Commerce>("Pendiente");
        }

        public List<Commerce> GetRejectedCommerces()
        {
            var cCrud = new CommerceCrudFactory();
            return cCrud.RetrieveByStatus<Commerce>("Rechazado");
        }

        public Commerce RetrieveById(int id)
        {
            var cCrud = new CommerceCrudFactory();
            return cCrud.RetrieveById<Commerce>(id);
        }

        public Commerce RetrieveByLegalId(string legalId)
        {
            var cCrud = new CommerceCrudFactory();
            var cdto = new Commerce { LegalId = legalId };
            return cCrud.RetrieveByLegalId<Commerce>(cdto);
        }

        public List<Commerce> GetCommercesByUser(int userId)
        {
            var relCrud = new FinalUserCommerceRolCrudFactory();
            return relCrud.RetrieveCommercesByUser<Commerce>(userId);
        }

        public List<Commerce> GetActiveCommercesByUser(int userId)
        {
            var relCrud = new FinalUserCommerceRolCrudFactory();
            var allUserCommerce = relCrud.RetrieveCommercesByUser<Commerce>(userId);

            // Filtrar solo las que están en estado "Activa"
            return allUserCommerce.Where(e => e.Status == "Activo").ToList();
        }

        // =============================================
        // MÉTODOS DE VALIDACIÓN PRIVADOS
        // =============================================


        private bool ValidateRequiredFields(Commerce commerce)
        {
            return !string.IsNullOrEmpty(commerce.LegalId) &&
                   !string.IsNullOrEmpty(commerce.Name) &&
                   !string.IsNullOrEmpty(commerce.Phone) &&
                   !string.IsNullOrEmpty(commerce.Email) &&
                   commerce.Latitude != 0 &&
                   commerce.Longitude != 0 &&
                   !string.IsNullOrEmpty(commerce.IBAN) &&
                   !string.IsNullOrEmpty(commerce.NationalId);
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

            // Debe tener exactamente 8 dígitos numéricos
            string phonePattern = @"^[0-9]{8}$";
            return Regex.IsMatch(phone, phonePattern);
        }

        private bool ValidateLegalIdFormat(string legalId)
        {
            if (string.IsNullOrEmpty(legalId))
                return false;

            // Debe tener exactamente 10 dígitos numéricos
            string legalIdPattern = @"^[0-9]{10}$";
            return Regex.IsMatch(legalId, legalIdPattern);
        }

        private bool ValidateIBAN(string iban)
        {
            if (string.IsNullOrWhiteSpace(iban))
                return false;

            const string pattern = @"^CR\d{20}$";
            return Regex.IsMatch(iban, pattern, RegexOptions.IgnoreCase);
        }

        private bool ValidateCommissionRate(decimal commissionRate)
        {
            // Comisión debe estar entre 0.01% y 99.99%
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

    }
}
