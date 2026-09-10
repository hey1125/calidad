using DataAccess.DAO;
using DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.CRUD
{
    public class BankAccountCrudFactory: CrudFactory

    {
        public BankAccountCrudFactory()
        {
            _sqlDao = SqlDao.GetInstance();
        }
        
        // Inserta una nueva cuenta bancaria usando el procedimiento almacenado CRE_BANK_ACCOUNT_PR.
        
        public override void Create(BaseDTO baseDTO)
        {
            var account = baseDTO as BankAccount;
            var sqlOperation = new SqlOperation() { ProcedureName = "CRE_BANK_ACCOUNT_PR" };

            sqlOperation.AddIntParam("UserId", account.UserId);
            sqlOperation.AddIntParam("BankId", account.BankId);
            sqlOperation.AddStringParameter("Iban", account.IBAN);

            _sqlDao.ExecuteProcedure(sqlOperation);
        }
        public override void Update(BaseDTO baseDTO)
        {
            throw new NotImplementedException();
        }
        public override void Delete(BaseDTO baseDTO)
        {
            var account = baseDTO as BankAccount;
            var sqlOperation = new SqlOperation() { ProcedureName = "DEL_BANK_ACCOUNT_BY_ID_PR" };
            sqlOperation.AddIntParam("Id", account.Id);

            _sqlDao.ExecuteProcedure(sqlOperation);
        }

        public override T Retrieve<T>()
        {
            throw new NotImplementedException();
        }
        public override T RetrieveById<T>(int id)
        {
            throw new NotImplementedException();
        }
        public override List<T> RetrieveAll<T>()
        {
            throw new NotImplementedException();

        }
        public BankAccount RetrieveByUserAndIBAN(int userId, string iban)
        {
            var operation = new SqlOperation
            {
                ProcedureName = "RET_BANK_ACCOUNT_BY_IBAN_AND_USER_PR"
            };

            operation.AddIntParam("UserId", userId);
            operation.AddStringParameter("IBAN", iban);

            var results = _sqlDao.ExecuteQueryProcedure(operation);

            if (results.Count > 0)
            {
                return BuildBankAccount(results[0]);
            }

            return null;
        }
        private BankAccount BuildBankAccount(Dictionary<string, object> row)
        {
            return new BankAccount
            {
                Id = Convert.ToInt32(row["Id"]),
                UserId = Convert.ToInt32(row["UserId"]),
                BankId = Convert.ToInt32(row["BankId"]),
                IBAN = Convert.ToString(row["IBAN"])
            };



       
        }
        public List<BankAccount> RetrieveAllByUser(int userId)
        {
            var operation = new SqlOperation
            {
                ProcedureName = "RET_ALL_BANK_ACCOUNTS_BY_USER_PR"
            };

            operation.AddIntParam("UserId", userId);

            var results = _sqlDao.ExecuteQueryProcedure(operation);
            var list = new List<BankAccount>();

            foreach (var row in results)
            {
                list.Add(BuildBankAccount(row));
            }

            return list;
        }

    }
}
