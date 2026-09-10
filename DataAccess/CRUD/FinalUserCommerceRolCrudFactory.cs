using DataAccess.DAO;
using DTOs;
using System;
using System.Collections.Generic;

namespace DataAccess.CRUD
{
    public class FinalUserCommerceRolCrudFactory : CrudFactory
    {
        public FinalUserCommerceRolCrudFactory()
        {
            _sqlDao = SqlDao.GetInstance();
        }

        public override void Create(BaseDTO baseDTO)
        {
            var rel = baseDTO as FinalUserCommerceRol;

            var sqlOperation = new SqlOperation() { ProcedureName = "CRE_FINALUSER_COMMERCE_ROL_PR" };

            sqlOperation.AddIntParam("P_UserId", rel.FinalUserId);
            sqlOperation.AddIntParam("P_CommerceId", rel.CommerceId);
            sqlOperation.AddIntParam("P_RolId", rel.RolId);

            _sqlDao.ExecuteProcedure(sqlOperation);
        }

        public List<T> RetrieveCommercesByUser<T>(int userId)
        {
            var lst = new List<T>();
            var op = new SqlOperation() { ProcedureName = "RET_COMMERCES_BY_USER_PR" };
            op.AddIntParam("P_UserId", userId);

            var results = _sqlDao.ExecuteQueryProcedure(op);

            foreach (var row in results)
            {
                var commerceId = Convert.ToInt32(row["Id"]);
                var commerce = new CommerceCrudFactory().RetrieveById<Commerce>(commerceId);
                lst.Add((T)Convert.ChangeType(commerce, typeof(T)));
            }

            return lst;
        }
        public List<Dictionary<string, object>> RetrieveRolesByCommerce(int commerceId)
        {
            var op = new SqlOperation { ProcedureName = "RET_ROLES_BY_COMMERCE_PR" };
            op.AddIntParam("P_CommerceId", commerceId);
            return _sqlDao.ExecuteQueryProcedure(op);
        }


        public List<T> RetrieveRolesByUser<T>(int userId)
        {
            var lst = new List<T>();
            var op = new SqlOperation() { ProcedureName = "RET_ROLES_BY_USER_PR" };
            op.AddIntParam("P_UserId", userId);

            var results = _sqlDao.ExecuteQueryProcedure(op);

            foreach (var row in results)
            {
                var rel = new FinalUserCommerceRol
                {
                    FinalUserId = userId,
                    CommerceId = Convert.ToInt32(row["CommerceId"]),
                    RolId = Convert.ToInt32(row["RolId"])
                };

                lst.Add((T)Convert.ChangeType(rel, typeof(T)));
            }

            return lst;
        }
        public bool HasRoleInCommerce(int userId, int commerceId, int roleId)
        {
            var op = new SqlOperation { ProcedureName = "RET_USER_ROLE_IN_COMMERCE_PR" };
            op.AddIntParam("P_UserId", userId);
            op.AddIntParam("P_CommerceId", commerceId);
            op.AddIntParam("P_RoleId", roleId);

            var result = _sqlDao.ExecuteQueryProcedure(op);
            return result.Count > 0;
        }

        public override void Delete(BaseDTO baseDTO) => throw new NotImplementedException();
        public override T Retrieve<T>() => throw new NotImplementedException();
        public override List<T> RetrieveAll<T>() => throw new NotImplementedException();
        public override T RetrieveById<T>(int id) => throw new NotImplementedException();
        public override void Update(BaseDTO baseDTO) => throw new NotImplementedException();
    }
}
