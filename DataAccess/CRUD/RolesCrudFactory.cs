using DataAccess.DAO;
using DTOs;
using System;
using System.Collections.Generic;

namespace DataAccess.CRUD
{
    public class RolesCrudFactory : CrudFactory
    {
        public RolesCrudFactory()
        {
            _sqlDao = SqlDao.GetInstance();
        }

        public void AddCashierByNationalId(string nationalId, int commerceId, int roleId)
        {
            var op = new SqlOperation { ProcedureName = "ADD_CASHIER_TO_COMMERCE_PR" };
            op.AddStringParameter("P_NationalId", nationalId);
            op.AddIntParam("P_CommerceId", commerceId);
            op.AddIntParam("P_RoleId", roleId);
            _sqlDao.ExecuteProcedure(op);
        }

        public List<Dictionary<string, object>> RetrieveCashiers(int commerceId)
        {
            var op = new SqlOperation { ProcedureName = "RET_CASHIERS_BY_COMMERCE_PR" };
            op.AddIntParam("P_CommerceId", commerceId);
            return _sqlDao.ExecuteQueryProcedure(op);
        }

        public void DeleteCashier(int userId, int commerceId)
        {
            var op = new SqlOperation { ProcedureName = "DEL_CASHIER_FROM_COMMERCE_PR" };
            op.AddIntParam("P_UserId", userId);
            op.AddIntParam("P_CommerceId", commerceId);
            _sqlDao.ExecuteProcedure(op);
        }

        public override void Create(BaseDTO dto) => throw new NotImplementedException();
        public override void Update(BaseDTO dto) => throw new NotImplementedException();
        public override void Delete(BaseDTO dto) => throw new NotImplementedException();
        public override T Retrieve<T>() => throw new NotImplementedException();
        public override List<T> RetrieveAll<T>() => throw new NotImplementedException();
        public override T RetrieveById<T>(int id) => throw new NotImplementedException();
    }
}
