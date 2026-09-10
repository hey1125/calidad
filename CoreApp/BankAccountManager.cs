using DataAccess.CRUD;
using DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CoreApp
{
    public class BankAccountManager
    {
        private readonly BankAccountCrudFactory _factory;

        public BankAccountManager()
        {
            _factory = new BankAccountCrudFactory();
        }

        // Valida si el IBAN tiene estructura y dígito de control válidos.

        private bool EsIbanValido(string iban)
        {
            if (string.IsNullOrWhiteSpace(iban))
                return false;

            iban = iban.Replace(" ", "").ToUpper();

            // Validación estructural CR
            if (iban.Length != 22 || !iban.StartsWith("CR"))
                return false;

            if (!int.TryParse(iban.Substring(4, 1), out _)) return false;     // Carácter reservado
            
            if (!long.TryParse(iban.Substring(8, 14), out _)) return false;   // Número de cuenta

            // Reordenar IBAN
            string ibanReordenado = iban.Substring(4) + iban.Substring(0, 4);

            // Convertir letras a números
            string numerico = "";
            foreach (char c in ibanReordenado)
            {
                if (char.IsLetter(c))
                    numerico += (c - 'A' + 10).ToString();
                else
                    numerico += c;
            }

            if (BigInteger.TryParse(numerico, out BigInteger numero))
            {
                var mod = numero % 97;
                Console.WriteLine($" Número IBAN convertido: {numero}");
                Console.WriteLine($" Resultado del módulo 97: {mod}");
                return mod == 1;
            }

            return false;
        }




        // Valida e inserta una cuenta bancaria.

        public void Create(BankAccount account)
        {
            if (!EsIbanValido(account.IBAN))
                throw new Exception("El IBAN ingresado no es válido.");

            var factoryEntidad = new FinancialEntityCrudFactory();
            var entidad = factoryEntidad.RetrieveById<FinancialEntity>(account.BankId);

            if (entidad == null)
                throw new Exception($"No existe entidad financiera con ID = {account.BankId}");


            if (entidad.Status != "Activa")
                throw new Exception("La entidad financiera no está activa.");

            var codigoBancoIBAN = account.IBAN.Substring(5, 3); // caracteres 6-8
            var codigoBancoEntidad = entidad.BankCode;

            if (codigoBancoIBAN != codigoBancoEntidad)
                throw new Exception($"El IBAN no corresponde con el código del banco registrado (esperado: {codigoBancoEntidad})");

            if (CuentaDuplicada(account.UserId, account.IBAN))
                throw new Exception("Ya tienes una cuenta registrada con este IBAN.");

            _factory.Create(account);
        }



       
        private bool CuentaDuplicada(int userId, string iban)
        {
            var factory = new BankAccountCrudFactory();
            var cuenta = factory.RetrieveByUserAndIBAN(userId, iban);
            return cuenta != null;
        }

        public List<BankAccount> GetAccountsByUser(int userId)
        {
            return _factory.RetrieveAllByUser(userId);
        }

        public void Delete(int accountId)
        {
            var account = new BankAccount { Id = accountId };
            _factory.Delete(account);
        }

    }
}