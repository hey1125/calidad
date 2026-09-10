using DataAccess.DAO;
using System;
using System.Collections.Generic;

namespace DataAccess.CRUD
{
    public class RoleCrudFactory : CrudFactory
    {
        public RoleCrudFactory()
        {
            _sqlDao = SqlDao.GetInstance();
        }

        // Ya existentes si quieres listar:
        public List<Dictionary<string, object>> RetrieveAll()
        {
            var op = new SqlOperation { ProcedureName = "RET_ROLES_PR" };
            return _sqlDao.ExecuteQueryProcedure(op);
        }

        // NUEVO: Crear (si no existe) y asignar rol a usuario por cédula
        public Dictionary<string, object> CreateAndAssignToUser(string roleName, string nationalId, int commerceId)
        {
            var op = new SqlOperation { ProcedureName = "CRE_ROLE_AND_ASSIGN_TO_USER_PR" };
            op.AddStringParameter("P_RoleName", roleName);
            op.AddStringParameter("P_NationalId", nationalId);
            op.AddIntParam("P_CommerceId", commerceId);

            var res = _sqlDao.ExecuteQueryProcedure(op);
            return res.Count > 0 ? res[0] : new Dictionary<string, object>();
        }

        // Métodos base no usados en este flujo
        public override void Create(DTOs.BaseDTO dto) => throw new NotImplementedException();
        public override void Update(DTOs.BaseDTO dto) => throw new NotImplementedException();
        public override void Delete(DTOs.BaseDTO dto) => throw new NotImplementedException();
        public override T Retrieve<T>() => throw new NotImplementedException();
        public override List<T> RetrieveAll<T>() => throw new NotImplementedException();
        public override T RetrieveById<T>(int id) => throw new NotImplementedException();
    }
}
