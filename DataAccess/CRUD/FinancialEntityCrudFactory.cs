using DataAccess.DAO;
using DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.CRUD
{
    public class FinancialEntityCrudFactory : CrudFactory
    {
        public FinancialEntityCrudFactory()
        {
            _sqlDao = SqlDao.GetInstance();
        }

        public override void Create(BaseDTO baseDTO)
        {
            var financialEntity = baseDTO as FinancialEntity;

            var sqlOperation = new SqlOperation() { ProcedureName = "CRE_FINANCIAL_ENTITY_PR" };
            sqlOperation.AddStringParameter("P_LegalId", financialEntity.LegalId);
            sqlOperation.AddStringParameter("P_BankCode", financialEntity.BankCode);
            sqlOperation.AddStringParameter("P_Name", financialEntity.Name);
            sqlOperation.AddStringParameter("P_Phone", financialEntity.Phone);
            sqlOperation.AddStringParameter("P_Email", financialEntity.Email);
            sqlOperation.AddDoubleParam("P_Latitude", (double)financialEntity.Latitude);
            sqlOperation.AddDoubleParam("P_Longitude", (double)financialEntity.Longitude);
            sqlOperation.AddStringParameter("P_Status", financialEntity.Status);
            sqlOperation.AddStringParameter("P_NationalId", financialEntity.NationalId);

            _sqlDao.ExecuteProcedure(sqlOperation);
        }

        // =============================================
        // MÉTODOS PARA HU 2.2
        // =============================================

        public List<T> RetrieveByStatus<T>(string status)
        {
            var lstFinancialEntities = new List<T>();
            var sqlOperation = new SqlOperation() { ProcedureName = "RET_FINANCIAL_ENTITIES_BY_STATUS_PR" };
            sqlOperation.AddStringParameter("P_Status", status);
            var lstResults = _sqlDao.ExecuteQueryProcedure(sqlOperation);

            if (lstResults.Count > 0)
            {
                foreach (var row in lstResults)
                {
                    var financialEntity = BuildFinancialEntity(row);
                    lstFinancialEntities.Add((T)Convert.ChangeType(financialEntity, typeof(T)));
                }
            }
            return lstFinancialEntities;
        }

        public override T RetrieveById<T>(int id)
        {
            var sqlOperation = new SqlOperation() { ProcedureName = "RET_FINANCIAL_ENTITY_BY_ID_PR" };
            sqlOperation.AddIntParam("P_Id", id);
            var lstResults = _sqlDao.ExecuteQueryProcedure(sqlOperation);

            if (lstResults.Count > 0)
            {
                var row = lstResults[0];
                var financialEntity = BuildFinancialEntity(row);
                return (T)Convert.ChangeType(financialEntity, typeof(T));
            }

            return default(T);
        }

        public void ApproveEntity(int entityId, decimal commissionRate)
        {
            var sqlOperation = new SqlOperation() { ProcedureName = "APPROVE_FINANCIAL_ENTITY_PR" };
            sqlOperation.AddIntParam("P_Id", entityId);
            sqlOperation.AddDoubleParam("P_CommissionRate", (double)commissionRate);

            _sqlDao.ExecuteProcedure(sqlOperation);
        }

        public void RejectEntity(int entityId)
        {
            var sqlOperation = new SqlOperation() { ProcedureName = "REJECT_FINANCIAL_ENTITY_PR" };
            sqlOperation.AddIntParam("P_Id", entityId);

            _sqlDao.ExecuteProcedure(sqlOperation);
        }

        // =============================================
        // MÉTODOS EXISTENTES HU 2.1
        // =============================================

        public T RetrieveByBankCode<T>(FinancialEntity financialEntity)
        {
            var sqlOperation = new SqlOperation() { ProcedureName = "RET_FINANCIAL_ENTITY_BY_BANK_CODE_PR" };
            sqlOperation.AddStringParameter("P_BankCode", financialEntity.BankCode);

            var lstResults = _sqlDao.ExecuteQueryProcedure(sqlOperation);

            if (lstResults.Count > 0)
            {
                var row = lstResults[0];
                financialEntity = BuildFinancialEntity(row);

                return (T)Convert.ChangeType(financialEntity, typeof(T));
            }
            return default(T);
        }

        public T RetrieveByLegalId<T>(FinancialEntity financialEntity)
        {
            var sqlOperation = new SqlOperation() { ProcedureName = "RET_FINANCIAL_ENTITY_BY_LEGAL_ID_PR" };
            sqlOperation.AddStringParameter("P_LegalId", financialEntity.LegalId);

            var lstResults = _sqlDao.ExecuteQueryProcedure(sqlOperation);

            if (lstResults.Count > 0)
            {
                var row = lstResults[0];
                financialEntity = BuildFinancialEntity(row);

                return (T)Convert.ChangeType(financialEntity, typeof(T));
            }
            return default(T);
        }

        public int CreateAndReturnId(FinancialEntity financialEntity)
        {
            var sqlOperation = new SqlOperation() { ProcedureName = "CRE_FINANCIAL_ENTITY_RETURN_ID_PR" };

            sqlOperation.AddStringParameter("P_LegalId", financialEntity.LegalId);
            sqlOperation.AddStringParameter("P_BankCode", financialEntity.BankCode);
            sqlOperation.AddStringParameter("P_Name", financialEntity.Name);
            sqlOperation.AddStringParameter("P_Phone", financialEntity.Phone);
            sqlOperation.AddStringParameter("P_Email", financialEntity.Email);
            sqlOperation.AddDoubleParam("P_Latitude", (double)financialEntity.Latitude);
            sqlOperation.AddDoubleParam("P_Longitude", (double)financialEntity.Longitude);
            sqlOperation.AddStringParameter("P_Status", financialEntity.Status);
            sqlOperation.AddStringParameter("P_NationalId", financialEntity.NationalId);

            var result = _sqlDao.ExecuteQueryProcedure(sqlOperation);

            if (result.Count > 0 && result[0].ContainsKey("ID"))
                return Convert.ToInt32(result[0]["ID"]);
            else
                throw new Exception("No se pudo obtener el ID de la entidad financiera recién creada.");
        }

        //Método que convierte el diccionario en una entidad financiera
        private FinancialEntity BuildFinancialEntity(Dictionary<string, object> row)
        {
            var financialEntity = new FinancialEntity()
            {
                Id = (int)row["Id"],
                Created = (DateTime)row["Created"],
                Updated = row["Updated"] == DBNull.Value || row["Updated"] == null ? DateTime.MinValue : (DateTime)row["Updated"],
                LegalId = row["LegalId"]?.ToString() ?? string.Empty,
                BankCode = row["BankCode"]?.ToString() ?? string.Empty,
                Name = row["Name"]?.ToString() ?? string.Empty,
                Phone = row["Phone"]?.ToString() ?? string.Empty,
                Email = row["Email"]?.ToString() ?? string.Empty,
                Latitude = Convert.ToDecimal(row["Latitude"]),
                Longitude = Convert.ToDecimal(row["Longitude"]),
                Status = row["Status"]?.ToString() ?? string.Empty,
                CommissionRate = row["CommissionRate"] != DBNull.Value ? Convert.ToDecimal(row["CommissionRate"]) : 0.00m,
                NationalId = (string)row["NationalId"]
            };
            return financialEntity;
        }

        // Métodos no implementados para estas HUs
        public override void Update(BaseDTO baseDTO)
        {
            throw new NotImplementedException();
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
    }
}
