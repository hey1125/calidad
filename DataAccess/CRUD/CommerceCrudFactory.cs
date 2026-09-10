using DataAccess.DAO;
using DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.CRUD
{
    public class CommerceCrudFactory : CrudFactory
    {

        public CommerceCrudFactory()
        {
            _sqlDao = SqlDao.GetInstance();
        }

        public override void Create(BaseDTO baseDTO)
        {
            var commerce = baseDTO as Commerce;
            var sqlOperation = new SqlOperation() { ProcedureName = "CRE_COMMERCE_PR" };

            sqlOperation.AddStringParameter("P_LegalId", commerce.LegalId);
            sqlOperation.AddStringParameter("P_Name", commerce.Name);
            sqlOperation.AddStringParameter("P_Phone", commerce.Phone);
            sqlOperation.AddStringParameter("P_Email", commerce.Email);
            sqlOperation.AddDoubleParam("P_Latitude", (double)commerce.Latitude);
            sqlOperation.AddDoubleParam("P_Longitude", (double)commerce.Longitude);
            sqlOperation.AddStringParameter("P_Status", commerce.Status);
            sqlOperation.AddStringParameter("P_IBAN", commerce.IBAN);
            sqlOperation.AddStringParameter("P_NationalId", commerce.NationalId);

            _sqlDao.ExecuteProcedure(sqlOperation);
        }

        public List<T> RetrieveByStatus<T>(string status)
        {
            var lstCommerce = new List<T>();
            var sqlOperation = new SqlOperation() { ProcedureName = "RET_COMMERCE_BY_STATUS_PR" };
            sqlOperation.AddStringParameter("P_Status", status);
            var lstResults = _sqlDao.ExecuteQueryProcedure(sqlOperation);

            if (lstResults.Count > 0)
            {
                foreach (var row in lstResults)
                {
                    var commerce = BuildCommerce(row);
                    lstCommerce.Add((T)Convert.ChangeType(commerce, typeof(T)));
                }
            }
            return lstCommerce;
        }

        public override T RetrieveById<T>(int id)
        {
            var sqlOperation = new SqlOperation() { ProcedureName = "RET_COMMERCE_BY_ID_PR" };
            sqlOperation.AddIntParam("P_Id", id);
            var lstResults = _sqlDao.ExecuteQueryProcedure(sqlOperation);

            if (lstResults.Count > 0)
            {
                var row = lstResults[0];
                var commerce = BuildCommerce(row);
                return (T)Convert.ChangeType(commerce, typeof(T));
            }

            return default(T);
        }

        public void ApproveCommerce(int commerceId, decimal commissionRate)
        {
            var sqlOperation = new SqlOperation() { ProcedureName = "APPROVE_COMMERCE_PR" };
            sqlOperation.AddIntParam("P_Id", commerceId);
            sqlOperation.AddDoubleParam("P_CommissionRate", (double)commissionRate);

            _sqlDao.ExecuteProcedure(sqlOperation);
        }

        public void RejectCommerce(int commerceId)
        {
            var sqlOperation = new SqlOperation() { ProcedureName = "REJECT_COMMERCE_PR" };
            sqlOperation.AddIntParam("P_Id", commerceId);

            _sqlDao.ExecuteProcedure(sqlOperation);
        }

        public T RetrieveByLegalId<T>(Commerce commerce)
        {
            var sqlOperation = new SqlOperation() { ProcedureName = "RET_COMMERCE_BY_LEGAL_ID_PR" };
            sqlOperation.AddStringParameter("P_LegalId", commerce.LegalId);

            var lstResults = _sqlDao.ExecuteQueryProcedure(sqlOperation);

            if (lstResults.Count > 0)
            {
                var row = lstResults[0];
                commerce = BuildCommerce(row);

                return (T)Convert.ChangeType(commerce, typeof(T));
            }
            return default(T);
        }

        public T RetrieveByIBAN<T>(Commerce commerce)
        {
            var sqlOperation = new SqlOperation() { ProcedureName = "RET_COMMERCE_BY_IBAN_PR" };
            sqlOperation.AddStringParameter("P_IBAN", commerce.IBAN);

            var lstResults = _sqlDao.ExecuteQueryProcedure(sqlOperation);

            if (lstResults.Count > 0)
            {
                var row = lstResults[0];
                commerce = BuildCommerce(row);

                return (T)Convert.ChangeType(commerce, typeof(T));
            }
            return default(T);
        }

        public override void Delete(BaseDTO baseDTO)
        {
            throw new NotImplementedException();
        }

        public override T Retrieve<T>()
        {
            throw new NotImplementedException();
        }

        public override List<T> RetrieveAll<T>()
        {
            throw new NotImplementedException();
        }

        public override void Update(BaseDTO baseDTO)
        {
            throw new NotImplementedException();
        }

        public int CreateAndReturnId(Commerce commerce)
        {
            var sqlOperation = new SqlOperation() { ProcedureName = "CRE_COMMERCE_RETURN_ID_PR" };

            sqlOperation.AddStringParameter("P_LegalId", commerce.LegalId);
            sqlOperation.AddStringParameter("P_Name", commerce.Name);
            sqlOperation.AddStringParameter("P_Phone", commerce.Phone);
            sqlOperation.AddStringParameter("P_Email", commerce.Email);
            sqlOperation.AddDoubleParam("P_Latitude", (double)commerce.Latitude);
            sqlOperation.AddDoubleParam("P_Longitude", (double)commerce.Longitude);
            sqlOperation.AddStringParameter("P_Status", commerce.Status);
            sqlOperation.AddStringParameter("P_IBAN", commerce.IBAN);
            sqlOperation.AddStringParameter("P_NationalId", commerce.NationalId);

            var result = _sqlDao.ExecuteQueryProcedure(sqlOperation);

            if (result.Count > 0 && result[0].ContainsKey("ID"))
                return Convert.ToInt32(result[0]["ID"]);
            else
                throw new Exception("No se pudo obtener el ID del comercio recién creado.");
        }


        private Commerce BuildCommerce(Dictionary<string, object> row)
        {
            var commerce = new Commerce()
            {
                Id = (int)row["Id"],
                Created = (DateTime)row["Created"],
                Updated = row["Updated"] == DBNull.Value || row["Updated"] == null ? DateTime.MinValue : (DateTime)row["Updated"],
                LegalId = (string)row["LegalId"],
                Name = (string)row["Name"],
                Phone = (string)row["Phone"],
                Email = (string)row["Email"],
                Latitude = Convert.ToDecimal(row["Latitude"]),
                Longitude = Convert.ToDecimal(row["Longitude"]),
                Status = (string)row["Status"],
                IBAN = (string)row["IBAN"],
                CommissionRate = row["CommissionRate"] != DBNull.Value ? Convert.ToDecimal(row["CommissionRate"]) : 0.00m,
                NationalId = (string)row["NationalId"]
            };
            return commerce;
        }
            
    }
}
